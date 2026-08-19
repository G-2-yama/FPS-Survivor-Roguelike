using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Enemy/AttackPattern/BurstShot")]
public class BurstShotPattern : AttackPattern
{
    [SerializeField, Min(1)] private int shotCount = 3;
    [SerializeField, Min(0f)] private float interval = 0.2f;
    [SerializeField, Min(0f)] private float firstshotintervalrate = 3f;
    public float Interval => interval;

    public override IEnumerator Execute(AttackContext context)
    {
        
        for (int i = 0; i < shotCount; i++)
        {

            if (i == 0)
            {
            yield return new WaitForSeconds(interval*firstshotintervalrate);
            }
            yield return new WaitForSeconds(interval);
            if (context.Target == null)
                yield break;

            Vector3 direction = (context.Target.position - context.ShotPoint.position).normalized;
            context.Launcher.Shoot(context.ShotPoint, direction, context.AttackPower);

            
        }
    }
}
