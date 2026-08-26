using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace InfiniteTileWorld
{
    [CustomEditor(typeof(StageManager))]
    public class StagePanelEditor : Editor
    {
        private GameObject _stagePanelPrefab;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Editor Tools", EditorStyles.boldLabel);

            _stagePanelPrefab = (GameObject)EditorGUILayout.ObjectField(
                "StagePanel Prefab", _stagePanelPrefab, typeof(GameObject), false);

            EditorGUILayout.HelpBox(
                "Prefab 未設定時は空の GameObject + StagePanel を生成します。",
                MessageType.Info);

            EditorGUILayout.Space(4);

            if (GUILayout.Button("Setup Panels", GUILayout.Height(32)))
                SetupPanels();
        }

        private void SetupPanels()
        {
            var manager = (StageManager)target;
            float size = manager.TileSize;

            Undo.RecordObject(manager, "Setup Stage Panels");
            manager.Panels.Clear();

            int gridSize = manager.GridSize;
            int center = gridSize / 2;

            for (int gz = 0; gz < manager.GridSize; gz++)
            {
                for (int gx = 0; gx < manager.GridSize; gx++)
                {
                    Vector3 pos = new Vector3((gx - center) * size, 0f, (gz - center) * size);
                    GameObject go = CreatePanelObject(pos, manager.transform, gx, gz);
                    Undo.RegisterCreatedObjectUndo(go, "Create StagePanel");

                    var panel = go.GetComponent<StagePanel>() ?? Undo.AddComponent<StagePanel>(go);

                    if (go.GetComponent<Collider>() == null)
                    {
                        var col = Undo.AddComponent<BoxCollider>(go);
                        col.size = new Vector3(size, 10f, size);
                        col.isTrigger = true;
                    }

                    manager.Panels.Add(panel);
                }
            }

            EditorUtility.SetDirty(manager);
            Debug.Log($"[InfiniteTileWorld] {manager.Panels.Count} panels configured.");
        }

        private GameObject CreatePanelObject(Vector3 position, Transform parent, int gx, int gz)
        {
            string name = $"StagePanel_{gx}_{gz}";
            GameObject go;

            if (_stagePanelPrefab != null)
            {
                go = (GameObject)PrefabUtility.InstantiatePrefab(_stagePanelPrefab, parent);
                go.name = name;
                go.transform.position = position;
            }
            else
            {
                go = new GameObject(name);
                go.transform.SetParent(parent);
                go.transform.position = position;
            }

            return go;
        }
    }
}
#endif