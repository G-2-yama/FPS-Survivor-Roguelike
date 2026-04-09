using UnityEngine;

public class moveenemy : MonoBehaviour
{
    public Transform Target { get; private set; }

    [SerializeField] private enemyDatas enemyDatas;
    public enemyDatas EnemyDatas => enemyDatas;

    [SerializeField] float radius = 3f;
    public float Radius => radius;

    [SerializeField] float length = 5f;
    public float Length => length;

    private StateMachine<IState> stateMachine;

    void Start()
    {
        Target = GameObject.Find("Player").transform;

        stateMachine = new StateMachine<IState>();
        stateMachine.ChangeState(new ChaseState(this, stateMachine));
    }

    void Update()
    {
        stateMachine.Update();
    }
}