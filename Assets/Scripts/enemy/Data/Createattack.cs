using UnityEngine;
using System.Collections; 
[CreateAssetMenu(menuName = "enemy/AttackLogic/BurstShot")]
public class BurstShotLogic : EnemyAttackBase
{
    [SerializeField] private enemybulletData bulletData;
    [SerializeField] private int shotCount = 3;
    [SerializeField] private float interval = 0.2f;

    public override void Execute(moveenemy actor, Transform shotPoint)
    {
        // 実行はactorのCoroutineとして開始（複数個体での競合を防ぐ）
        actor.StartCoroutine(PerformAttack(actor, shotPoint));
    }

    private IEnumerator PerformAttack(moveenemy actor, Transform shotPoint)
    {
        for (int i = 0; i < shotCount; i++)
        {
            if (actor.Target == null) yield break;

            Vector3 dir = (actor.Target.position - shotPoint.position).normalized;
            bulletData.Shot(shotPoint, dir);

            yield return new WaitForSeconds(interval);
        }
    }

    public override void Cancel(moveenemy actor)
    {
        // 攻撃動作（コルーチン）を強制停止
        actor.StopAllCoroutines();
    }
}