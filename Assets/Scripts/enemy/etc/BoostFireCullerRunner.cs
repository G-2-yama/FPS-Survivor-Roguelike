using System;
using System.Collections.Generic;
using UnityEngine;
    /// <summary>受け取ったカメラを使って敵の演出表示を分散更新する</summary>

[DisallowMultipleComponent, DefaultExecutionOrder(-10000)]
public class BoostFireCullerRunner : MonoBehaviour
{
    private const float CheckInterval = 0.25f;

    /// <summary>Sceneに配置した更新担当を通常の個体より先に公開する</summary>
    private void Awake()
    {
        if (instance != null && instance != this)
            throw new InvalidOperationException("Sceneの更新担当が重複しています: BoostFireCullerRunner");
        instance = this;
    }

    /// <summary>Scene終了時に登録番号と静的参照を解放する</summary>
    private void OnDestroy()
    {
        if (instance != this) return;
        // 破棄順にかかわらず、個体側の無効化処理が古い登録番号を使用しないようにする
        foreach (var entry in cullers) entry.RunnerIndex = -1;
        cullers.Clear();
        viewCamera = null;
        instance = null;
    }

    private static BoostFireCullerRunner instance;
    private static readonly List<BoostFireCuller> cullers = new List<BoostFireCuller>();

    private static Camera viewCamera;

    /// <summary>プレイヤー初期化時に演出用カメラを受け取る</summary>
    public static void SetCamera(Camera camera) => viewCamera = camera;

    /// <summary>破棄されるプレイヤーのカメラ参照だけを解除する</summary>
    public static void ClearCamera(Camera camera)
    {
        if (viewCamera == camera) viewCamera = null;
    }

    private int cursor;
    private float pending;
    /// <summary>プレイ開始時に前回の静的な登録情報を初期化する</summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        instance = null;
        viewCamera = null;
        cullers.Clear();
    }
    /// <summary>個体をSceneの更新担当へ登録する</summary>
    public static void Register(BoostFireCuller culler)
    {
        if (instance == null)
            throw new InvalidOperationException("SceneにBoostFireCullerRunnerを事前配置してください");

        culler.RunnerIndex = cullers.Count;
        cullers.Add(culler);
    }
    /// <summary>個体を登録から外して末尾の個体の番号を更新する</summary>
    public static void Unregister(BoostFireCuller culler)
    {
        int index = culler.RunnerIndex;
        if (index < 0)
            return;

        int last = cullers.Count - 1;
        BoostFireCuller moved = cullers[last];
        cullers[index] = moved;
        moved.RunnerIndex = index;
        cullers.RemoveAt(last);
        culler.RunnerIndex = -1;
    }
    /// <summary>有効な登録対象をまとめて更新する</summary>
    private void Update()
    {
        int count = cullers.Count;
        if (count == 0)
            return;

        Camera camera = viewCamera;
        if (camera == null)
            return;

        Vector3 cameraPosition = camera.transform.position;

        pending = Mathf.Min(pending + count * Time.deltaTime / CheckInterval, count);
        int budget = Mathf.Min((int)pending, count);
        pending -= budget;

        for (int i = 0; i < budget; i++)
        {
            if (cursor >= cullers.Count)
                cursor = 0;

            BoostFireCuller culler = cullers[cursor];
            cursor++;

            try
            {
                culler.Refresh(cameraPosition);
            }
            catch (Exception e)
            {
                Debug.LogException(e, culler);
            }
        }
    }
}
