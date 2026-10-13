using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>ダメージ表示の移動と寿命をまとめて更新する</summary>
[DisallowMultipleComponent, DefaultExecutionOrder(-10000)]
public class DamagePopupRunner : MonoBehaviour
{
    /// <summary>Sceneに配置した更新担当を通常の個体より先に公開する</summary>
    private void Awake()
    {
        if (instance != null && instance != this)
            throw new InvalidOperationException("Sceneの更新担当が重複しています: DamagePopupRunner");
        instance = this;
    }

    /// <summary>Scene終了時に登録番号と静的参照を解放する</summary>
    private void OnDestroy()
    {
        if (instance != this) return;
        // 破棄順にかかわらず、個体側の無効化処理が古い登録番号を使用しないようにする
        foreach (var entry in popups) entry.RunnerIndex = -1;
        popups.Clear();

        instance = null;
    }

    private static DamagePopupRunner instance;
    private static readonly List<DamagePopup> popups = new List<DamagePopup>();

    private DamagePopup[] snapshot = Array.Empty<DamagePopup>();

    /// <summary>プレイ開始時に前回の静的な登録情報を初期化する</summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        instance = null;
        popups.Clear();
    }

    /// <summary>個体をSceneの更新担当へ登録する</summary>
    public static void Register(DamagePopup popup)
    {
        if (popup.RunnerIndex >= 0)
            return;

        if (instance == null)
            throw new InvalidOperationException("SceneにDamagePopupRunnerを事前配置してください");

        popup.RunnerIndex = popups.Count;
        popups.Add(popup);
    }

    /// <summary>個体を登録から外して末尾の個体の番号を更新する</summary>
    public static void Unregister(DamagePopup popup)
    {
        int index = popup.RunnerIndex;
        if (index < 0)
            return;

        int last = popups.Count - 1;
        DamagePopup moved = popups[last];
        popups[index] = moved;
        moved.RunnerIndex = index;
        popups.RemoveAt(last);
        popup.RunnerIndex = -1;
    }

    /// <summary>更新対象のスナップショットからダメージ表示を更新する</summary>
    private void LateUpdate()
    {
        int count = popups.Count;
        if (snapshot.Length < count)
        {
            snapshot = new DamagePopup[Mathf.NextPowerOfTwo(count)];
        }
        popups.CopyTo(snapshot);

        for (int i = 0; i < count; i++)
        {
            DamagePopup popup = snapshot[i];
            if (popup.RunnerIndex < 0)
                continue;

            try
            {
                popup.Tick();
            }
            catch (Exception e)
            {
                Debug.LogException(e, popup);
            }
        }

        Array.Clear(snapshot, 0, count);
    }
}
