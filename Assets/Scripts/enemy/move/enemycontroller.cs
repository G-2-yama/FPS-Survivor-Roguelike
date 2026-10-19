using UnityEngine;

public class EnemyBrain : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private EnemyConfig config;
    [SerializeField] private EnemyTargetProvider targetProvider;
    [SerializeField] private EnemyAttackController attack;
    [SerializeField] private Animator animator;
    [SerializeField] private EnemyAnimationEffect uniqueAnimation;
    [SerializeField] private EnemyPoseSwitcher poseSwitcher;
    private StateMachine<IState> stateMachine;
    private Transform cachedTransform;

    private ChaseState chaseState;
    private CombatState combatState;
    private KnockbackState knockbackState;

    public Rigidbody Rb => rb;
    public Transform CachedTransform => cachedTransform;
    public EnemyConfig Config => config;
    /// <summary>この個体の戦闘開始距離。共有Configは初期値としてのみ使用する</summary>
    public float EngageDistance { get; private set; }
    public Transform Target => targetProvider?.CurrentTarget;
    public EnemyAttackController Attack => attack;
    public Animator Animator => animator;

    public EnemyAnimationEffect UniqueAnimation => uniqueAnimation;
    public EnemyPoseSwitcher PoseSwitcher => poseSwitcher;

    public StateMachine<IState> StateMachine => stateMachine;

    public ChaseState ChaseState => chaseState;
    public CombatState CombatState => combatState;

    public KnockbackState KnockbackState => knockbackState;

    internal int RunnerIndex { get; set; } = -1;
    internal int ThinPhase { get; set; }
    internal int LastTickStep { get; set; }
    internal int TickSteps { get; private set; } = 1;
    internal bool IsFar { get; private set; }

    private bool canThin;

    private EnemyGenerator spawner;

    private void Awake()
    {
        cachedTransform = transform;
        ResetEngageDistance();
        canThin = gameObject.layer == LayerMask.NameToLayer("Enemy");
        stateMachine = new StateMachine<IState>();

        chaseState = new ChaseState(this, stateMachine);
        combatState = new CombatState(this, stateMachine);
        knockbackState = new KnockbackState(this, stateMachine);
    }

  

    private void OnEnable()
    {
        EnemyBrainRunner.Register(this);
    }

    internal void Tick(int step)
    {
        TickSteps = Mathf.Clamp(step - LastTickStep, 1, EnemyBrainRunner.ThinInterval);
        LastTickStep = step;
        stateMachine.Update();
    }

    internal void UpdateFar(float distance)
    {
        if (!canThin)
            return;

        if (IsFar)
        {
            if (distance < EnemyBrainRunner.NearDistance)
                IsFar = false;
        }
        else if (distance > EnemyBrainRunner.FarDistance)
        {
            IsFar = true;
        }
    }

    internal void ClearFar()
    {
        IsFar = false;
    }

    public void SetSpawner(EnemyGenerator generator)
    {
        spawner = generator;
    }

    internal bool TryRelocate(float distance, Vector3 targetPosition)
    {
        if (spawner == null)
            return false;

        bool isTooFar = distance > spawner.RelocateDistance;
        bool hasFallen = cachedTransform.position.y < targetPosition.y - spawner.FallRelocateDepth;
        if (!isTooFar && !hasFallen)
            return false;

        if (!spawner.TryGetRelocatePosition(out Vector3 destination))
            return false;

        if (rb != null)
        {
            rb.position = destination;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        else
        {
            cachedTransform.position = destination;
        }

        return true;
    }

    private void OnDisable()
    {
        EnemyBrainRunner.Unregister(this);
        spawner = null;
        ClearFar();
        attack?.CancelAttack();
    }

    public void ResetBrain()
    {
        ResetEngageDistance();
        if (animator != null)
        {
            animator.enabled = false;
            animator.enabled = true;

            animator.SetBool("Iscombat", false);
        }
        if (poseSwitcher != null)
        {
            poseSwitcher.ResetPose();
        }
        if(uniqueAnimation != null)
        {
            uniqueAnimation.Stop();
        }

        stateMachine.ChangeState(chaseState);
    }

    public void ChangeState(IState newState)
    {
        stateMachine.ChangeState(newState);
    }

    /// <summary>共有Configを書き換えずに、この個体の戦闘開始距離を変更する</summary>
    public void SetEngageDistance(float distance)
    {
        EngageDistance = Mathf.Max(0f, distance);
    }

    /// <summary>生成時とプールからの再出現時に初期値へ戻す</summary>
    private void ResetEngageDistance()
    {
        SetEngageDistance(config != null ? config.EngageDistance : 0f);
    }
}
