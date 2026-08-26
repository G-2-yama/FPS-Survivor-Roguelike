using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class missileshotposchange : EnemyAnimationEffect
{
    public List<GameObject> missiles = new List<GameObject>();
    [SerializeField] private BurstShotPattern Bp;

    private GameObject previousTarget = null;

    
    public override void Play()
    {
        StartCoroutine(MoveRoutine());
    }
    public override void Stop()
    {
        StopAllCoroutines();
        if (previousTarget != null)
        {
            previousTarget.SetActive(true);
            previousTarget = null;
        }
    }

        IEnumerator MoveRoutine()
    {
        int i = 0;

        while (true)
        {
            if (missiles.Count == 0)
            {
                yield return null;
                continue;
            }

            if (i >= missiles.Count)
            {
                i = 0;
            }
            if (i == 0)
            {
                yield return new WaitForSeconds(Bp.Interval*Bp.FirstshotInterval);
            }
            yield return new WaitForSeconds(Bp.Interval);

            GameObject target = missiles[i];

            // 前のやつを再表示
            if (previousTarget != null)
            {
                previousTarget.SetActive(true);
            }

            if (target != null)
            {
                transform.position = target.transform.position;

                // 今のターゲットを非表示
                target.SetActive(false);

                // 記録
                previousTarget = target;
            }

           

            i++;
        }
    }
}