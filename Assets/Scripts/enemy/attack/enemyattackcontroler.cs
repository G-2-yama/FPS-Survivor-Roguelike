using UnityEngine;
using System.Collections;

public class EnemyAttackController : MonoBehaviour
{
    [SerializeField] private EnemyConfig config;
    [SerializeField] private EnemyTargetProvider targetProvider;
    [SerializeField] private ProjectileLauncher launcher;
    [SerializeField] private Transform shotPoint;

    private Coroutine currentAttackRoutine;
    private float nextAttackTime;
    private float attackMultiplier = 1f;

    public Transform CurrentTarget => targetProvider != null ? targetProvider.CurrentTarget : null;
    public Transform ShotPoint => shotPoint;
    public ProjectileLauncher Launcher => launcher;
    public int AttackPower => config != null ? EnemyStatScaling.Scale(config.AttackPower, attackMultiplier) : 0;

    public void SetAttackMultiplier(float multiplier) => attackMultiplier = multiplier;

    public bool CanAttack()
    {
        return config != null
            && config.AttackPattern != null
            && shotPoint != null
            && launcher != null
            && CurrentTarget != null
            && currentAttackRoutine == null
            && Time.time >= nextAttackTime;
    }

    public void TryAttack()
    {
        if (!CanAttack())
            return;

        nextAttackTime = Time.time + config.AttackInterval;
        currentAttackRoutine = StartCoroutine(RunPattern());
    }

    public void CancelAttack()
    {
        if (currentAttackRoutine == null)
            return;

        StopCoroutine(currentAttackRoutine);
        currentAttackRoutine = null;
    }

    private IEnumerator RunPattern()
    {
        yield return config.AttackPattern.Execute(new AttackContext(this));
        currentAttackRoutine = null;
    }
}
