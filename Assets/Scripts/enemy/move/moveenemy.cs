using UnityEngine;

public class moveenemy : MonoBehaviour
{
    public Transform target;
    [SerializeField] float speed = 50f;
    [SerializeField] float movespeed = 3f;
    [SerializeField] float radius = 3f;
    [SerializeField] float length = 5f;
    private void Start()
    {
        target=GameObject.Find("Player").GetComponent<Transform>();
    }
    void Update()
    {
        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0;

        float distance = toTarget.magnitude;

        // ■ まだ遠い → 近づく
        if (distance > length)
        {
            transform.position += toTarget.normalized * movespeed * Time.deltaTime;

            // ついでにターゲット向く
            transform.rotation = Quaternion.LookRotation(toTarget*-1);
        }
        else
        {
            // ■ 円運動モード
            Vector3 offset = transform.position - target.position;
            offset.y = 0;
            offset = offset.normalized * radius;
            transform.rotation = Quaternion.LookRotation(offset);
            offset = Quaternion.AngleAxis(speed * Time.deltaTime, Vector3.up) * offset;

            transform.position = target.position + offset;

        }
    }
}