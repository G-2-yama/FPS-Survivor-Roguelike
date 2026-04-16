using UnityEngine;

public interface IEnemyAttack
{
    void Execute(moveenemy actor, Transform shotPoint);
    void Cancel(moveenemy actor); // actorを指定して特定のコルーチン等を止める
}

public abstract class EnemyAttackBase : ScriptableObject, IEnemyAttack
{
    public abstract void Execute(moveenemy actor, Transform shotPoint);
    public abstract void Cancel(moveenemy actor);
}