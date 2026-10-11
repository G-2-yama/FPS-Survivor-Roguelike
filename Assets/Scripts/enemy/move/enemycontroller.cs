using UnityEngine;

public class EnemyBrain : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private EnemyConfig config;
    [SerializeField] private EnemyTargetProvider targetProvider;
    [SerializeField] private EnemyAttackController attack;
    [SerializeField] private Animator animator;
    [SerializeField] private EnemyAnimationEffect uniqueAnimation;
    private StateMachine<IState> stateMachine;
    private Transform cachedTransform;

    private ChaseState chaseState;
    private CombatState combatState;
    private KnockbackState knockbackState;

    public Rigidbody Rb => rb;
    public Transform CachedTransform => cachedTransform;
    public EnemyConfig Config => config;
    public Transform Target => targetProvider?.CurrentTarget;
    public EnemyAttackController Attack => attack;
    public Animator Animator => animator;

    public EnemyAnimationEffect UniqueAnimation => uniqueAnimation;

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


    private void Awake()
    {
        cachedTransform = transform;
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

    private void OnDisable()
    {
        EnemyBrainRunner.Unregister(this);
        ClearFar();
        attack?.CancelAttack();
    }

    public void ResetBrain()
    {
        if (animator != null)
        {
            animator.enabled = false;
            animator.enabled = true;

            animator.SetBool("Iscombat", false);
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
}