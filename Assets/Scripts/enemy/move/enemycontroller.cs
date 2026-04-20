using UnityEngine;

public class EnemyBrain : MonoBehaviour
{
    [SerializeField] private EnemyConfig config;
    [SerializeField] private EnemyTargetProvider targetProvider;
    [SerializeField] private EnemyMovementController movement;
    [SerializeField] private EnemyAttackController attack;

    private StateMachine<IState> stateMachine;

    public EnemyConfig Config => config;
    public Transform Target => targetProvider != null ? targetProvider.CurrentTarget : null;
    public EnemyMovementController Movement => movement;
    public EnemyAttackController Attack => attack;

    private void Awake()
    {
        stateMachine = new StateMachine<IState>();
    }

    private void Start()
    {
        stateMachine.ChangeState(new ChaseState(this, stateMachine));
    }

    private void Update()
    {
        stateMachine.Update();
    }

    private void OnDisable()
    {
        attack?.CancelAttack();
    }

    public void ChangeState(IState newState)
    {
        stateMachine.ChangeState(newState);
    }
}
