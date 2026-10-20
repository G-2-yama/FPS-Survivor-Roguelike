using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialTaskRunner : MonoBehaviour
{
    private const string JumpActionName = "Jump";
    private const string LeftReloadActionName = "LeftReload";
    private const string RightReloadActionName = "RightReload";

    [Tooltip("上から順に出す課題の一覧")]
    [SerializeField] private TutorialTaskList taskList;

    [Tooltip("課題の文を出すテキスト")]
    [SerializeField] private TutorialBindingText instruction;

    [Tooltip("何番目の課題かを出すテキスト")]
    [SerializeField] private TMP_Text counter;

    [Tooltip("進み具合のバー 横方向のアンカーで長さを変える")]
    [SerializeField] private RectTransform progressFill;

    [Tooltip("進み具合のバー全体 すべての課題を終えたら隠す")]
    [SerializeField] private GameObject progressBar;

    [Tooltip("課題を達成したときに出す表示")]
    [SerializeField] private GameObject clearLabel;

    [Tooltip("達成の表示を出してから次の課題へ進むまでの秒数")]
    [Min(0f)]
    [SerializeField] private float clearDisplaySeconds = 1f;

    private PlayerController playerController;
    private CharacterController characterController;
    private PlayerInventory inventory;
    private InputAction jumpAction;
    private InputAction leftReloadAction;
    private InputAction rightReloadAction;
    private Transform lookTransform;

    private int taskIndex = -1;
    private float achieved;
    private float clearTimer;
    private Vector3 lastPosition;
    private Quaternion lastLookRotation;
    private PlayerActionState lastActionState;
    private int lastAmmo;
    private bool targetFired;

    private TutorialTask CurrentTask =>
        taskList != null && taskIndex >= 0 && taskIndex < taskList.Tasks.Count ? taskList.Tasks[taskIndex] : null;

    private void Start()
    {
        clearLabel.SetActive(false);
        if (taskList == null)
        {
            Debug.LogError("課題の一覧が設定されていないため、チュートリアルの課題を出せません", this);
            enabled = false;
            return;
        }

        TrainingDummy.OnAnyDamaged += OnDummyDamaged;
    }

    private void OnDestroy()
    {
        TrainingDummy.OnAnyDamaged -= OnDummyDamaged;
        if (jumpAction != null)
            jumpAction.performed -= OnJumpPerformed;
        if (leftReloadAction != null)
            leftReloadAction.performed -= OnReloadPerformed;
        if (rightReloadAction != null)
            rightReloadAction.performed -= OnReloadPerformed;
    }

    private void Update()
    {
        if (playerController == null && !TryBindPlayer())
            return;

        if (taskIndex < 0)
            StartTask(0);

        TutorialTask task = CurrentTask;
        if (task == null)
            return;

        if (clearTimer > 0f)
        {
            clearTimer -= Time.deltaTime;
            if (clearTimer <= 0f)
                StartTask(taskIndex + 1);
            return;
        }

        Measure(task);
        UpdateProgress(task);
        if (achieved >= task.Amount)
            Clear();
    }

    private bool TryBindPlayer()
    {
        playerController = FindFirstObjectByType<PlayerController>();
        if (playerController == null)
            return false;

        characterController = playerController.GetComponent<CharacterController>();
        Player player = FindFirstObjectByType<Player>();
        inventory = player != null ? player.Inventory : null;
        if (inventory == null)
            Debug.LogWarning("プレイヤーの武器が見つからないため、攻撃の課題で武器を持たせられず達成もできません", this);
        Camera mainCamera = Camera.main;
        lookTransform = mainCamera != null ? mainCamera.transform : playerController.transform;

        PlayerInput playerInput = FindFirstObjectByType<PlayerInput>();
        jumpAction = playerInput != null ? playerInput.actions.FindAction(JumpActionName) : null;
        if (jumpAction != null)
            jumpAction.performed += OnJumpPerformed;
        else
            Debug.LogWarning($"アクション {JumpActionName} が見つからないため、2段ジャンプの課題を達成できません", this);

        leftReloadAction = BindReloadAction(playerInput, LeftReloadActionName);
        rightReloadAction = BindReloadAction(playerInput, RightReloadActionName);

        return true;
    }

    private InputAction BindReloadAction(PlayerInput playerInput, string actionName)
    {
        InputAction action = playerInput != null ? playerInput.actions.FindAction(actionName) : null;
        if (action != null)
            action.performed += OnReloadPerformed;
        else
            Debug.LogWarning($"アクション {actionName} が見つからないため、リロードの課題を達成できません", this);
        return action;
    }

    private void StartTask(int index)
    {
        taskIndex = index;
        achieved = 0f;
        clearTimer = 0f;
        clearLabel.SetActive(false);
        Equip(CurrentTask);
        ResetMeasurement();

        TutorialTask task = CurrentTask;
        progressBar.SetActive(task != null);
        if (task == null)
        {
            instruction.Template = taskList.FinishedText;
            counter.text = string.Empty;
            return;
        }

        instruction.Template = task.Template;
        counter.text = $"{taskIndex + 1} / {taskList.Tasks.Count}";
        SetFill(0f);
    }

    private void Equip(TutorialTask task)
    {
        if (task == null || inventory == null)
            return;

        foreach (TutorialEquip equip in task.Equips)
            inventory.EquipWeapon(equip.Slot, equip.Weapon);
    }

    private void ResetMeasurement()
    {
        lastPosition = playerController.transform.position;
        lastLookRotation = lookTransform.rotation;
        lastActionState = playerController.CurrentActionState;
        lastAmmo = GetAmmo(CurrentTask);
        targetFired = false;
    }

    private int GetAmmo(TutorialTask task)
    {
        if (task == null || inventory == null || !inventory.WeaponSlots.TryGetValue(task.Slot, out Weapon weapon))
            return 0;

        return weapon.CurrentAmmo;
    }

    private void Measure(TutorialTask task)
    {
        Vector3 position = playerController.transform.position;
        Quaternion lookRotation = lookTransform.rotation;
        PlayerActionState actionState = playerController.CurrentActionState;
        int ammo = GetAmmo(task);

        switch (task.Goal)
        {
            case TutorialGoal.MoveDistance:
                Vector3 moved = position - lastPosition;
                moved.y = 0f;
                achieved += moved.magnitude;
                break;
            case TutorialGoal.LookAngle:
                achieved += Quaternion.Angle(lastLookRotation, lookRotation);
                break;
            case TutorialGoal.Dash:
                CountEntered<PlayerDashActionState>(actionState);
                break;
            case TutorialGoal.Slide:
                CountEntered<PlayerSlideActionState>(actionState);
                break;
            case TutorialGoal.FastFall:
                CountEntered<PlayerFastFallActionState>(actionState);
                break;
            case TutorialGoal.WeaponHit:
                if (ammo < lastAmmo)
                    targetFired = true;
                break;
        }

        lastPosition = position;
        lastLookRotation = lookRotation;
        lastActionState = actionState;
        lastAmmo = ammo;
    }

    private void CountEntered<T>(PlayerActionState actionState) where T : PlayerActionState
    {
        if (actionState is T && !(lastActionState is T))
            achieved += 1f;
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        TutorialTask task = CurrentTask;
        if (task == null || task.Goal != TutorialGoal.AirJump || clearTimer > 0f)
            return;

        if (characterController != null && !characterController.isGrounded)
            achieved += 1f;
    }

    private void OnReloadPerformed(InputAction.CallbackContext context)
    {
        TutorialTask task = CurrentTask;
        if (task == null || task.Goal != TutorialGoal.Reload || clearTimer > 0f)
            return;

        InputAction expected = task.Slot switch
        {
            SlotType.LeftMain => leftReloadAction,
            SlotType.RightMain => rightReloadAction,
            _ => null,
        };
        if (context.action == expected)
            achieved += 1f;
    }

    private void OnDummyDamaged()
    {
        TutorialTask task = CurrentTask;
        if (task == null || task.Goal != TutorialGoal.WeaponHit || clearTimer > 0f)
            return;

        if (targetFired)
            achieved += 1f;
    }

    private void UpdateProgress(TutorialTask task)
    {
        SetFill(Mathf.Clamp01(achieved / task.Amount));
    }

    private void Clear()
    {
        SetFill(1f);
        clearLabel.SetActive(true);
        clearTimer = Mathf.Max(clearDisplaySeconds, Mathf.Epsilon);
    }

    private void SetFill(float ratio)
    {
        progressFill.anchorMax = new Vector2(ratio, progressFill.anchorMax.y);
    }
}
