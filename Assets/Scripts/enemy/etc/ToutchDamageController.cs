using UnityEngine;

public class ToutchDamageController : MonoBehaviour
{
    [SerializeField] private int damageAmount = 100;
    
    private void OnCollisionEnter(Collision other)
    {
        Player player = other.gameObject.GetComponent<Player>();
       
        if (player == null)
        {
            return;
        }
        
           player.TakeDamage(damageAmount,0);
        
    }
}

