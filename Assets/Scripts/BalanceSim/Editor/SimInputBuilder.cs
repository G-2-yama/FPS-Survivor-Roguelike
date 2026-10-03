using UnityEditor;

namespace BalanceSim.Editor
{
    public static class SimInputBuilder
    {
        public static SimInput Build(BalanceSimSettings settings)
        {
            var input = new SimInput
            {
                runCount = settings.RunCount,
                seed = settings.Seed,
            };

            foreach (string guid in AssetDatabase.FindAssets("t:PhaseData"))
            {
                var phase = AssetDatabase.LoadAssetAtPath<PhaseData>(AssetDatabase.GUIDToAssetPath(guid));
                input.phases.Add(ToSimPhase(phase));
            }
            input.phases.Sort((a, b) => a.startTime.CompareTo(b.startTime));

            return input;
        }

        private static SimPhase ToSimPhase(PhaseData phase)
        {
            var simPhase = new SimPhase
            {
                name = phase.name,
                startTime = phase.StartTime,
                minSpawnCount = phase.MinSpawnCount,
                spawnInterval = phase.SpawnInterval,
            };

            if (phase.Enemies == null)
            {
                return simPhase;
            }

            foreach (EnemySpawnData entry in phase.Enemies)
            {
                simPhase.enemies.Add(new SimSpawnEntry
                {
                    prefabName = entry.Prefab != null ? entry.Prefab.name : null,
                    spawnWeight = entry.SpawnWeight,
                });
            }

            return simPhase;
        }
    }
}
