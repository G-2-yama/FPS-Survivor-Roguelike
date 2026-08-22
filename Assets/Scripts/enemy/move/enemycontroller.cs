using UnityEngine;

public class EnemyBrain : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private EnemyConfig config;
    [SerializeField] private EnemyTargetProvider targetProvider;
    [SerializeField] private EnemyAttackController attack;
    [SerializeField] private Animator animator;
    [SerializeField] private MonoBehaviour uniqueAnimation;
    private StateMachine<IState> stateMachine;

    private ChaseState chaseState;
    private CombatState combatState;
    private KnockbackState knockbackState;

    public Rigidbody Rb => rb;
    public EnemyConfig Config => config;
    public Transform Target => targetProvider?.CurrentTarget;
    public EnemyAttackController Attack => attack;
    public Animator Animator => animator;

    public MonoBehaviour UniqueAnimation => uniqueAnimation;

    public StateMachine<IState> StateMachine => stateMachine;

    public ChaseState ChaseState => chaseState;
    public CombatState CombatState => combatState;

    public KnockbackState KnockbackState => knockbackState;
   

    private void Awake()
    {
        
        stateMachine = new StateMachine<IState>();

        chaseState = new ChaseState(this, stateMachine);
        combatState = new CombatState(this, stateMachine);
        knockbackState = new KnockbackState(this, stateMachine);
    }

    private void OnEnable()
    {
        ResetBrain();
    }

    private void FixedUpdate()
    {
        stateMachine.Update();
    }

    private void OnDisable()
    {
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

        stateMachine.ChangeState(chaseState);
    }

    public void ChangeState(IState newState)
    {
        stateMachine.ChangeState(newState);
    }
}