using UnityEngine;

public class EnemyPoseSwitcher : MonoBehaviour
{
    [System.Serializable]
    public struct PosePart
    {
        [Tooltip("角度を切り替える部品")]
        public Transform target;
        [Tooltip("追跡中（待機）の localEulerAngles")]
        public Vector3 idleEuler;
        [Tooltip("戦闘中の localEulerAngles")]
        public Vector3 combatEuler;
    }

    [Header("部品")]
    [SerializeField] private PosePart[] parts;

    [Header("時間（秒）")]
    [Tooltip("切り替え全体にかける時間")]
    [SerializeField] private float blendDuration = 0.25f;
    [Tooltip("戦闘の姿勢へ向かう動きの時間")]
    [SerializeField] private float toCombatDuration = 0.083f;
    [Tooltip("待機の姿勢へ戻る動きの時間")]
    [SerializeField] private float toIdleDuration = 0.083f;

    private Quaternion[] idleRotations;
    private Quaternion[] combatRotations;
    private Quaternion[] startRotations;
    private bool isCombat;
    private float elapsed;

    private void Awake()
    {
        idleRotations = new Quaternion[parts.Length];
        combatRotations = new Quaternion[parts.Length];
        startRotations = new Quaternion[parts.Length];

        for (int i = 0; i < parts.Length; i++)
        {
            idleRotations[i] = Quaternion.Euler(parts[i].idleEuler);
            combatRotations[i] = Quaternion.Euler(parts[i].combatEuler);
        }

        ResetPose();
    }

    public void ResetPose()
    {
        isCombat = false;
        enabled = false;

        for (int i = 0; i < parts.Length; i++)
        {
            parts[i].target.localRotation = idleRotations[i];
        }
    }

    public void SetCombat(bool combat)
    {
        if (combat == isCombat)
            return;

        isCombat = combat;
        elapsed = 0f;

        for (int i = 0; i < parts.Length; i++)
        {
            startRotations[i] = parts[i].target.localRotation;
        }

        enabled = true;
    }

    private void Update()
    {
        elapsed += Time.deltaTime;

        float moveDuration = isCombat ? toCombatDuration : toIdleDuration;
        float t = Progress(blendDuration) * Mathf.SmoothStep(0f, 1f, Progress(moveDuration));
        Quaternion[] targets = isCombat ? combatRotations : idleRotations;

        for (int i = 0; i < parts.Length; i++)
        {
            parts[i].target.localRotation = Quaternion.Slerp(startRotations[i], targets[i], t);
        }

        if (t >= 1f)
            enabled = false;
    }

    private float Progress(float duration)
    {
        return duration > 0f ? Mathf.Clamp01(elapsed / duration) : 1f;
    }
}
