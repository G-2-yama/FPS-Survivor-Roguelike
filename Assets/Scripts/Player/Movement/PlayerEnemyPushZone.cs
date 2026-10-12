using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレイヤーの接触領域に入った敵へダメージを与え、水平方向に押し出す
/// </summary>
[RequireComponent(typeof(CapsuleCollider))]
public sealed class PlayerEnemyPushZone : MonoBehaviour
{
    private readonly Dictionary<Enemy, HashSet<Collider>> overlappingColliders = new();
    private readonly Dictionary<Enemy, float> enemyHorizontalRadii = new();
    private readonly List<Enemy> staleEnemies = new();
    private CapsuleCollider pushCollider;
    private CharacterController playerController;
    private Player player;

    /// <summary>プレイヤーと接触領域を関連付け、Colliderを初期化する</summary>
    /// <param name="targetPlayer">接触領域を所有するプレイヤー</param>
    /// <param name="controller">領域の寸法を決めるCharacterController</param>
    /// <param name="zoneLayer">敵の接触検知に使う専用レイヤー</param>
    public void Initialize(Player targetPlayer, CharacterController controller, int zoneLayer)
    {
        player = targetPlayer;
        playerController = controller;
        pushCollider = GetComponent<CapsuleCollider>();
        gameObject.layer = zoneLayer;
        pushCollider.isTrigger = true;
        pushCollider.direction = 1;
        SyncCollider();
    }

    /// <summary>接触領域を更新し、重なった敵をプレイヤーの外側へ押し出す</summary>
    private void FixedUpdate()
    {
        if (playerController == null || pushCollider == null)
            return;

        SyncCollider();

        staleEnemies.Clear();
        foreach (Enemy enemy in overlappingColliders.Keys)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy)
                staleEnemies.Add(enemy);
            else
                PushOutsidePlayer(enemy);
        }

        foreach (Enemy enemy in staleEnemies)
        {
            overlappingColliders.Remove(enemy);
            enemyHorizontalRadii.Remove(enemy);
        }
    }

    /// <summary>侵入した敵を記録し、接触ダメージを一度だけ与えて押し出す</summary>
    /// <param name="other">押し出し領域に入ったCollider</param>
    private void OnTriggerEnter(Collider other)
    {
        Enemy enemy = other.GetComponentInParent<Enemy>();
        if (enemy == null)
            return;

        bool firstCollider = !overlappingColliders.TryGetValue(enemy, out HashSet<Collider> colliders);
        if (firstCollider)
        {
            colliders = new HashSet<Collider>();
            overlappingColliders.Add(enemy, colliders);
            enemyHorizontalRadii[enemy] = GetHorizontalRadius(enemy);
            ApplyContactDamage(enemy);
        }

        colliders.Add(other);
        PushOutsidePlayer(enemy);
    }

    /// <summary>敵の最後のColliderが領域を出た時点で追跡を終了する</summary>
    /// <param name="other">押し出し領域から出たCollider</param>
    private void OnTriggerExit(Collider other)
    {
        Enemy enemy = other.GetComponentInParent<Enemy>();
        if (enemy == null || !overlappingColliders.TryGetValue(enemy, out HashSet<Collider> colliders))
            return;

        colliders.Remove(other);
        if (colliders.Count == 0)
        {
            overlappingColliders.Remove(enemy);
            enemyHorizontalRadii.Remove(enemy);
        }
    }

    /// <summary>領域が無効になったとき接触状態を消去する</summary>
    private void OnDisable()
    {
        overlappingColliders.Clear();
        enemyHorizontalRadii.Clear();
        staleEnemies.Clear();
    }

    /// <summary>CharacterControllerの現在の寸法をTriggerカプセルへ反映する</summary>
    private void SyncCollider()
    {
        pushCollider.radius = playerController.radius;
        pushCollider.height = Mathf.Max(playerController.height, playerController.radius * 2f);
        pushCollider.center = playerController.center;
    }

    /// <summary>敵に設定された接触ダメージをプレイヤーへ適用する</summary>
    /// <param name="enemy">接触ダメージを与える敵</param>
    private void ApplyContactDamage(Enemy enemy)
    {
        if (player == null || enemy == null)
            return;

        ToutchDamageController touchDamage = enemy.GetComponentInChildren<ToutchDamageController>();
        if (touchDamage != null)
            player.TakeDamage(touchDamage.DamageAmount, 0f);
    }

    /// <summary>高さを保ったまま、重なった敵を水平方向へ押し出す</summary>
    /// <param name="enemy">プレイヤーから離す敵</param>
    private void PushOutsidePlayer(Enemy enemy)
    {
        if (enemy == null || !enemy.TryGetComponent(out EnemyBrain brain) || brain.Rb == null)
            return;

        Rigidbody enemyBody = brain.Rb;
        Vector3 playerCenter = transform.parent.TransformPoint(playerController.center);
        Vector3 enemyPosition = enemyBody.position;
        Vector3 offset = enemyPosition - playerCenter;
        offset.y = 0f;

        float distance = offset.magnitude;
        if (!enemyHorizontalRadii.TryGetValue(enemy, out float enemyRadius))
            enemyRadius = GetHorizontalRadius(enemy);

        float minimumDistance = playerController.radius + enemyRadius;
        if (distance >= minimumDistance)
            return;

        Vector3 direction = distance > 0.0001f
            ? offset / distance
            : GetFallbackDirection(enemy);

        enemyPosition += direction * (minimumDistance - distance);
        enemyBody.MovePosition(enemyPosition);
    }

    /// <summary>敵の通常Colliderから水平方向の半径を推定する</summary>
    /// <param name="enemy">Colliderの境界を測定する敵</param>
    /// <returns>最大の水平半径、通常Colliderがない場合は0</returns>
    private static float GetHorizontalRadius(Enemy enemy)
    {
        float radius = 0f;
        foreach (Collider enemyCollider in enemy.GetComponentsInChildren<Collider>())
        {
            if (enemyCollider == null || enemyCollider.isTrigger)
                continue;

            Bounds bounds = enemyCollider.bounds;
            radius = Mathf.Max(radius, Mathf.Max(bounds.extents.x, bounds.extents.z));
        }

        return radius;
    }

    /// <summary>敵とプレイヤーの中心が一致した場合に使う水平方向を返す</summary>
    /// <param name="enemy">向きを代替方向に使う敵</param>
    /// <returns>正規化した水平方向</returns>
    private Vector3 GetFallbackDirection(Enemy enemy)
    {
        Vector3 direction = enemy.transform.forward;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f)
            direction = Vector3.right;

        return direction.normalized;
    }
}
