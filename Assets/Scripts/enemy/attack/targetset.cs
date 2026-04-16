using UnityEngine;

public class EnemyTargetProvider : MonoBehaviour
{
    [SerializeField] private Transform currentTarget;
    [SerializeField] private string playerTag = "Player";

    public Transform CurrentTarget => currentTarget;

    private void Awake()
    {
        if (currentTarget != null)
            return;

        GameObject found = GameObject.FindWithTag(playerTag);
        if (found != null)
        {
            currentTarget = found.transform;
        }
    }

    public void SetTarget(Transform target)
    {
        currentTarget = target;
    }
}

