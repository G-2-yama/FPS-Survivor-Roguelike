using UnityEngine;

public abstract class MovementPattern : ScriptableObject
{
    [Min(0f)] public float ChaseSpeed = 3f;
    

    public abstract void Move(
        Rigidbody rb,
        Transform self,
        Transform target);
}
