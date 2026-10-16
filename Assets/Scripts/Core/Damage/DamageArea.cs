using System;
using UnityEngine;
using System.Collections.Generic;
/// <summary>範囲内の対象へ一度または一定間隔でダメージを適用する</summary>

public class DamageArea : DamageBase
{
    [SerializeField]
    private DamageAreaMode mode;

    [SerializeField]
    private float interval = 1f;

    /// <summary>1対象の接触Colliderと解除通知元をまとめて保持する</summary>
    private sealed class Contact
    {
        public readonly HashSet<Collider> Colliders = new();
        public IDamageable Target;
        public DamageTargetBinding Binding;

        /// <summary>終了した活動期間の参照を解放して次の接触に備える</summary>
        public void Reset()
        {
            Target = null;
            Binding = null;
            Colliders.Clear();
        }
    }

    private readonly Dictionary<IDamageable, Contact> contacts = new();
    private readonly Dictionary<Collider, Contact> colliderContacts = new();
    private readonly Stack<Contact> reusableContacts = new();
    private readonly List<IDamageable> tickBuffer = new();
    private Action<IDamageable> unavailableHandler;
    private float tickTimer;
    private float spawnTime;
    private int damagePerSecond;
    [SerializeField] private DamageAreaScaleController scaleController;

    /// <summary>侵入したColliderへ単発モードのダメージを適用する</summary>
    private void OnTriggerEnter(Collider other)
    {
        if (mode == DamageAreaMode.OnEnterOnce)
        {
            TryDamage(other, out _);
            return;
        }

        if (!DamageTargetRegistry.TryResolveBinding(other, out DamageTargetBinding binding))
            return;
        IDamageable damageable = binding.Target;

        if ((damageable.TeamType & targetTeam) == 0)
            return;

        ResolveContact(other, damageable, binding);
    }

    private void FixedUpdate()
    {
        if (mode != DamageAreaMode.Interval)
            return;

        tickTimer += Time.deltaTime;

        if (tickTimer < interval)
            return;

        tickTimer = 0f;
        DamageContacts();
    }

    private void DamageContacts()
    {
        tickBuffer.Clear();
        tickBuffer.AddRange(contacts.Keys);

        foreach (IDamageable target in tickBuffer)
        {
            if (!contacts.ContainsKey(target))
                continue;

            target.TakeDamage(CurrentDamage, knockbackForce);
        }

        tickBuffer.Clear();
    }

    /// <summary>継続通知の状態を再利用し、新規Colliderを対象の接触状態へ登録する</summary>
    private Contact ResolveContact(Collider collider, IDamageable target, DamageTargetBinding binding)
    {
        if (colliderContacts.TryGetValue(collider, out Contact contact) && ReferenceEquals(contact.Target, target))
            return contact;
        if (!contacts.TryGetValue(target, out contact))
        {
            unavailableHandler ??= ForgetTarget;
            contact = reusableContacts.Count > 0 ? reusableContacts.Pop() : new Contact();
            contact.Target = target;
            contact.Binding = binding;
            contacts.Add(target, contact);
            binding.OnUnavailable += unavailableHandler;
        }
        contact.Colliders.Add(collider);
        colliderContacts[collider] = contact;
        return contact;
    }
    /// <summary>退出したColliderの接触状態を解除する</summary>

    private void OnTriggerExit(Collider other)
    {
        if (!colliderContacts.Remove(other, out Contact contact))
            return;

        contact.Colliders.Remove(other);
        if (contact.Colliders.Count == 0)
            ForgetTarget(contact.Target);
    }
    /// <summary>取得した個体へ今回の攻撃設定を渡す</summary>
    public override void Initialize(int damage, float knockback)
    {
        base.Initialize(damage, knockback);

        // 新しいダメージ値が設定された後に拡縮
        scaleController?.SetScaleByDamage(damage);
    }

    public override void ApplyAreaModifiers(float extraLifetime, int damagePerSecond)
    {
        this.damagePerSecond = damagePerSecond;
        if (lifetime > 0f && extraLifetime > 0f)
            LifetimeRunner.Register(this, lifetime + extraLifetime);
    }

    protected override int CurrentDamage =>
        damagePerSecond > 0 ? damage + Mathf.FloorToInt(Time.time - spawnTime) * damagePerSecond : damage;
    /// <summary>新しい活動期間の状態を準備する</summary>

    public override void OnGet()
    {
        base.OnGet();

        ClearTargets();
        tickTimer = 0f;
        spawnTime = Time.time;
        damagePerSecond = 0;

        // 前回のスケールを初期化
        scaleController?.ResetScale();
    }
    /// <summary>返却する個体の一時状態と購読を解除する</summary>

    public override void OnRelease()
    {
        ClearTargets();

        scaleController?.ResetScale();

        base.OnRelease();
    }

    /// <summary>返却や無効化した対象の接触状態と購読を破棄する</summary>
    private void ForgetTarget(IDamageable target)
    {
        if (!contacts.TryGetValue(target, out Contact contact)) return;
        contacts.Remove(target);
        ReturnContact(contact);
    }

    /// <summary>通知の購読とColliderの対応を解除して接触状態を再利用へ戻す</summary>
    private void ReturnContact(Contact contact)
    {
        if (contact.Binding != null) contact.Binding.OnUnavailable -= unavailableHandler;
        foreach (Collider collider in contact.Colliders) colliderContacts.Remove(collider);
        contact.Reset();
        reusableContacts.Push(contact);
    }

    /// <summary>範囲攻撃の再利用前にすべての接触参照を解除する</summary>
    private void ClearTargets()
    {
        // 対象のDictionaryを列挙している間は変更せず、最後にまとめて解除する
        foreach (Contact contact in contacts.Values) ReturnContact(contact);
        contacts.Clear();
        colliderContacts.Clear();
    }

    /// <summary>無効化時に寿命管理と接触の購読を解除する</summary>
    protected override void OnDisable()
    {
        ClearTargets();
        base.OnDisable();
    }
}

/// <summary>範囲攻撃のダメージを適用するタイミング</summary>
public enum DamageAreaMode
{
    OnEnterOnce, // 入った瞬間に一度だけダメージ
    Interval,      // 入っている間、一定間隔でダメージ
}
