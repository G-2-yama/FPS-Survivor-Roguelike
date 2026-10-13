using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>敵の行動と遠距離の更新間隔をまとめて管理する</summary>
[DisallowMultipleComponent, DefaultExecutionOrder(-10000)]
public class EnemyBrainRunner : MonoBehaviour
{
    internal const int ThinInterval = 3;
    internal const float FarDistance = 40f;
    internal const float NearDistance = 35f;

    /// <summary>Sceneに配置した更新担当を通常の個体より先に公開する</summary>
    private void Awake()
    {
        if (instance != null && instance != this)
            throw new InvalidOperationException("Sceneの更新担当が重複しています: EnemyBrainRunner");
        instance = this;
    }

    /// <summary>Scene終了時に登録番号と静的参照を解放する</summary>
    private void OnDestroy()
    {
        if (instance != this) return;
        // 破棄順にかかわらず、個体側の無効化処理が古い登録番号を使用しないようにする
        foreach (var entry in brains) entry.RunnerIndex = -1;
        brains.Clear();
        cachedTarget = null;
        instance = null;
    }

    private static EnemyBrainRunner instance;
    private static readonly List<EnemyBrain> brains = new List<EnemyBrain>();

    private static Transform cachedTarget;
    private static Vector3 cachedTargetPosition;

    private static int stepCount;
    private static int nextThinPhase;

    private EnemyBrain[] snapshot = Array.Empty<EnemyBrain>();

    /// <summary>プレイ開始時に前回の静的な登録情報を初期化する</summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        instance = null;
        brains.Clear();
        cachedTarget = null;
        stepCount = 0;
        nextThinPhase = 0;
    }

    /// <summary>同じ更新内で共有する対象位置を返す</summary>
    public static Vector3 GetTargetPosition(Transform target)
    {
        if (!ReferenceEquals(target, cachedTarget))
        {
            cachedTarget = target;
            cachedTargetPosition = target.position;
        }
        return cachedTargetPosition;
    }

    /// <summary>個体をSceneの更新担当へ登録する</summary>
    public static void Register(EnemyBrain brain)
    {
        if (instance == null)
            throw new InvalidOperationException("SceneにEnemyBrainRunnerを事前配置してください");

        brain.RunnerIndex = brains.Count;
        brain.ThinPhase = nextThinPhase;
        brain.LastTickStep = stepCount;
        nextThinPhase = (nextThinPhase + 1) % ThinInterval;
        brains.Add(brain);
    }

    /// <summary>個体を登録から外して末尾の個体の番号を更新する</summary>
    public static void Unregister(EnemyBrain brain)
    {
        int index = brain.RunnerIndex;
        if (index < 0)
            return;

        int last = brains.Count - 1;
        EnemyBrain moved = brains[last];
        brains[index] = moved;
        moved.RunnerIndex = index;
        brains.RemoveAt(last);
        brain.RunnerIndex = -1;
    }

    /// <summary>更新対象のスナップショットから敵の行動を更新する</summary>
    private void FixedUpdate()
    {
        int count = brains.Count;
        if (snapshot.Length < count)
        {
            snapshot = new EnemyBrain[Mathf.NextPowerOfTwo(count)];
        }
        brains.CopyTo(snapshot);
        cachedTarget = null;
        stepCount++;

        for (int i = 0; i < count; i++)
        {
            EnemyBrain brain = snapshot[i];
            if (brain.RunnerIndex < 0)
                continue;

            if (brain.IsFar && (stepCount + brain.ThinPhase) % ThinInterval != 0)
                continue;

            try
            {
                brain.Tick(stepCount);
            }
            catch (Exception e)
            {
                Debug.LogException(e, brain);
            }
        }

        Array.Clear(snapshot, 0, count);
        cachedTarget = null;
    }
}
