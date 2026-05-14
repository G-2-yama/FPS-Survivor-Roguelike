using UnityEngine;

public class ExpPickupTrigger : MonoBehaviour
{
    private ExpObject expObject;

    private void Awake()
    {
        expObject = GetComponentInParent<ExpObject>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.name);

        Player player = other.GetComponent<Player>();

        if (player == null)
            return;

        Debug.Log("pickup");

        player.AddExp(expObject.ExpAmount);

        expObject.Release();
    }
}