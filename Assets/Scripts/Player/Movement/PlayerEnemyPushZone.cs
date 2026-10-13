using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>敵の接触を対象単位で管理し、接触ダメージと水平押し出しを仲介する</summary>
[RequireComponent(typeof(CapsuleCollider))]
public sealed class PlayerEnemyPushZone : MonoBehaviour
{
    /// <summary>1体の対象が領域に持つColliderと推定半径を保持する</summary>
    private sealed class Contact
    {
        public readonly HashSet<Collider> Colliders = new();
        public PlayerContactTarget Target;
        public float Radius;
        public int Version;

        /// <summary>次の接触へ再利用できるよう参照を解放する</summary>
        public void Reset()
        {
            Version++;
            Target = null;
            Radius = 0f;
            Colliders.Clear();
        }
    }

    private readonly Dictionary<PlayerContactTarget, Contact> contacts = new();
    private readonly Dictionary<Collider, Contact> colliderContacts = new();
    private readonly Stack<Contact> reusableContacts = new();
    private Action<PlayerContactTarget> unavailableHandler;
    private readonly List<PlayerContactTarget> staleTargets = new();
    private readonly List<Collider> staleColliders = new();
    private CapsuleCollider pushCollider;
    private CharacterController controller;
    private Player player;
    private PlayerCollisionSettings settings;

    /// <summary>所有プレイヤー、現在の本体、開始時の設定を接触領域へ関連付ける</summary>
    public void Initialize(Player owner, CharacterController characterController, PlayerCollisionSettings collisionSettings, CapsuleCollider zoneCollider)
    {
        ClearContacts();
        unavailableHandler ??= ForgetTarget;
        player = owner;
        controller = characterController;
        settings = collisionSettings;
        pushCollider = zoneCollider;
        pushCollider.isTrigger = true;
        pushCollider.direction = 1;
        // 他のレイヤーの除外設定を含め、接触領域は敵の検知だけに使用する
        gameObject.layer = settings.PushZoneLayer;
        pushCollider.includeLayers = settings.EnemyLayers;
        pushCollider.excludeLayers = ~settings.EnemyLayers.value;
        pushCollider.layerOverridePriority = 20;
        AttackCollisionFilter.Register(pushCollider);
        SyncCollider();
    }

    /// <summary>姿勢変更を反映し、無効なColliderを清掃して接触中の対象を押し出す</summary>
    private void FixedUpdate()
    {
        if (controller == null || settings == null) return;
        SyncCollider();
        staleTargets.Clear();
        foreach (var pair in contacts)
        {
            PlayerContactTarget target = pair.Key;
            if (target == null || !target.IsAvailable) { staleTargets.Add(target); continue; }
            staleColliders.Clear();
            foreach (Collider collider in pair.Value.Colliders)
                if (collider == null || !collider.enabled || !collider.gameObject.activeInHierarchy)
                    staleColliders.Add(collider);
            foreach (Collider collider in staleColliders)
            {
                pair.Value.Colliders.Remove(collider);
                colliderContacts.Remove(collider);
            }
            if (pair.Value.Colliders.Count == 0) staleTargets.Add(target);
            else PushOutsidePlayer(target, pair.Value.Radius);
        }
        foreach (PlayerContactTarget target in staleTargets) ForgetTarget(target);
    }

    /// <summary>侵入したColliderを登録し、最初の接触だけダメージを適用する</summary>
    private void OnTriggerEnter(Collider other) => TrackContact(other);

    /// <summary>Collider再有効化などで侵入通知を逃した場合も追跡を復元する</summary>
    private void OnTriggerStay(Collider other) => TrackContact(other);

    /// <summary>対象単位でColliderを集約し、同じ対象への二重ダメージを防ぐ</summary>
    private void TrackContact(Collider other)
    {
        if (settings == null || player == null || player.Health == null || player.Health.IsDead) return;
        if ((settings.EnemyLayers.value & (1 << other.gameObject.layer)) == 0) return;
        Rigidbody targetBody = other.attachedRigidbody;
        if (!PlayerContactTarget.TryResolve(targetBody, out PlayerContactTarget target)) return;
        if (target == null || !target.IsAvailable) return;
        Contact contact = ResolveContact(other, target);
        if (contact == null) return;
        if (target.RadiusOverride <= 0f)
        {
            Bounds bounds = other.bounds;
            contact.Radius = Mathf.Max(contact.Radius, Mathf.Max(bounds.extents.x, bounds.extents.z));
        }
        // 継続通知とFixedUpdateの補正タイミングは現在の挙動に合わせて維持する
        PushOutsidePlayer(target, contact.Radius);
    }


    /// <summary>対象の所有者を確認済みのColliderを接触状態へ関連付ける</summary>
    private Contact ResolveContact(Collider collider, PlayerContactTarget target)
    {
        if (colliderContacts.TryGetValue(collider, out Contact contact) && contact.Target == target)
            return contact;
        if (!contacts.TryGetValue(target, out contact))
        {
            contact = reusableContacts.Count > 0 ? reusableContacts.Pop() : new Contact();
            contact.Target = target;
            contact.Radius = target.RadiusOverride;
            contacts.Add(target, contact);
            target.OnUnavailable += unavailableHandler;
            int version = contact.Version;
            ApplyContactDamage(target);
            // ダメージ適用から返却や無効化が発生しても、終了した状態を再登録しない
            if (contact.Version != version || !target.IsAvailable) return null;
        }
        contact.Colliders.Add(collider);
        colliderContacts[collider] = contact;
        return contact;
    }
    /// <summary>最後のColliderが離れた時点で対象への追跡を終了する</summary>
    private void OnTriggerExit(Collider other)
    {
        // 親が変わった場合も侵入時に保存した所有者へ退出を通知する
        if (!colliderContacts.TryGetValue(other, out Contact contact)) return;
        colliderContacts.Remove(other);
        contact.Colliders.Remove(other);
        if (contact.Colliders.Count == 0) ForgetTarget(contact.Target);
    }

    /// <summary>プール返却、無効化、退出時に対象のすべての接触参照を解放する</summary>
    private void ForgetTarget(PlayerContactTarget target)
    {
        if (!contacts.TryGetValue(target, out Contact contact)) return;
        contacts.Remove(target);
        ReturnContact(contact);
    }

    /// <summary>購読とColliderの対応を解除して接触状態を再利用へ戻す</summary>
    private void ReturnContact(Contact contact)
    {
        if (contact.Target != null) contact.Target.OnUnavailable -= unavailableHandler;
        foreach (Collider collider in contact.Colliders) colliderContacts.Remove(collider);
        contact.Reset();
        reusableContacts.Push(contact);
    }

    /// <summary>無効化時に対象のイベント購読も含めて清掃する</summary>
    private void OnDisable()
    {
        ClearContacts();
        AttackCollisionFilter.Unregister(pushCollider);
    }

    /// <summary>有効化時に攻撃除外対象として登録する</summary>
    private void OnEnable() => AttackCollisionFilter.Register(pushCollider);

    /// <summary>現在のすべての接触状態を破棄する</summary>
    private void ClearContacts()
    {
        // ReturnContactは対象のDictionaryを変更せず、最後にまとめてクリアする
        foreach (Contact contact in contacts.Values) ReturnContact(contact);
        contacts.Clear();
        colliderContacts.Clear();
        staleTargets.Clear();
        staleColliders.Clear();
    }

    /// <summary>現在の本体寸法と追加距離から検知カプセルを同期する</summary>
    private void SyncCollider()
    {
        pushCollider.radius = controller.radius + settings.PushRadiusPadding;
        pushCollider.height = Mathf.Max(controller.height, pushCollider.radius * 2f);
        pushCollider.center = controller.center;
        pushCollider.enabled = settings.PushEnabled || settings.ContactDamageEnabled;
    }

    /// <summary>設定が有効な場合だけ対象の接触ダメージを所有プレイヤーへ適用する</summary>
    private void ApplyContactDamage(PlayerContactTarget target)
    {
        if (settings.ContactDamageEnabled) player.TakeDamage(target.ContactDamage, 0f);
    }

    /// <summary>通常の敵AIを維持し、重なりによる水平位置だけを補正する</summary>
    private void PushOutsidePlayer(PlayerContactTarget target, float targetRadius)
    {
        if (!settings.PushEnabled || player.Health.IsDead || !target.IsAvailable || !target.CanBePushed) return;
        Vector3 scale = controller.transform.lossyScale;
        float radius = (controller.radius + settings.PushRadiusPadding) * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
        Vector3 position = target.Body.position;
        Vector3 corrected = PlayerPushSolver.CorrectPosition(
            controller.transform.TransformPoint(controller.center), position, radius + targetRadius, target.transform.forward);
        if ((corrected - position).sqrMagnitude > 0.00000001f)
            target.Body.MovePosition(corrected);
    }
}
