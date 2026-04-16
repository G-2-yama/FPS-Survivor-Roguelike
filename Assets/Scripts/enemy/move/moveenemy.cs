using UnityEngine;

public class moveenemy : MonoBehaviour
{
    public Transform Target { get; private set; }
    [SerializeField] private enemyDatas enemyDatas;
    public enemyDatas EnemyDatas => enemyDatas;

    [SerializeField] private Transform shotPoint;
    public Transform ShotPoint => shotPoint;

    public float Radius = 3f;
    public float Length = 5f;

    private StateMachine<IState> stateMachine;

    void Start()
    {
        Target = GameObject.Find("Player")?.transform;
        stateMachine = new StateMachine<IState>();
        stateMachine.ChangeState(new ChaseState(this, stateMachine));
    }

    void Update() => stateMachine.Update();

    // 外部（StateやCondition）から状態を変えるためのメソッド
    public void ChangeState(IState newState) => stateMachine.ChangeState(newState);
}