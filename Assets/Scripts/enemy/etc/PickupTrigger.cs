using UnityEngine;

public class ExpPickupTrigger : MonoBehaviour
{
    private ExpObject expObject;
    private bool pickedUp=false;

    private void Awake()
    {
        expObject = GetComponentInParent<ExpObject>();
    }
    private void OnEnable()
    {
        pickedUp = false;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (pickedUp)
            return;
        
        Player player = other.GetComponent<Player>();

        if (player == null)
            return;
        pickedUp = true;
        player.AddExp(expObject.ExpAmount);
        expObject.Release();
    }
}