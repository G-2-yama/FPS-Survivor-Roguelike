using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BalanceSim.Editor
{
    public sealed class ProjectileReader
    {
        private const string UpwardSuffix = "@上向き";

        private readonly List<string> _warnings;
        private readonly Dictionary<string, GameObject> _sources = new();

        public List<SimProjectileDef> Projectiles { get; } = new();

        public ProjectileReader(List<string> warnings)
        {
            _warnings = warnings;
        }

        public string Add(GameObject prefab, bool upward = false)
        {
            if (prefab == null)
            {
                return null;
            }

            string name = upward ? prefab.name + UpwardSuffix : prefab.name;
            if (_sources.TryGetValue(name, out GameObject known))
            {
                if (known != prefab)
                {
                    _warnings.Add($"同じ名前の弾プレハブが複数あります: {prefab.name}");
                }
                return name;
            }

            _sources[name] = prefab;
            SimProjectileDef def = Read(prefab, name, upward);
            Projectiles.Add(def);
            return name;
        }

        private SimProjectileDef Read(GameObject prefab, string name, bool upward)
        {
            var def = new SimProjectileDef { name = name };
            var damage = prefab.GetComponent<DamageBase>();
            if (damage == null)
            {
                _warnings.Add($"弾 {prefab.name} に DamageBase がありません。ダメージを与えないものとして扱います");
                def.kind = SimDamageKind.Area;
                return def;
            }

            def.targetTeam = SerializedReader.Int(damage, "targetTeam");
            def.damage = SerializedReader.Int(damage, "damage");
            def.knockback = SerializedReader.Float(damage, "knockbackForce");
            def.lifetime = SerializedReader.Float(damage, "lifetime");

            switch (damage)
            {
                case DamageArea area:
                    def.kind = SimDamageKind.Area;
                    def.areaMode = (SimAreaMode)SerializedReader.Int(area, "mode");
                    def.interval = SerializedReader.Float(area, "interval");
                    ReadScale(prefab, area, def);
                    break;
                case DamageGenerator generator:
                {
                    def.kind = SimDamageKind.Generator;
                    SerializedProperty explosions = SerializedReader.Prop(generator, "explosionPrefabs");
                    for (int i = 0; i < explosions.arraySize; i++)
                    {
                        if (explosions.GetArrayElementAtIndex(i).objectReferenceValue is GameObject explosion)
                        {
                            def.explosions.Add(Add(explosion));
                        }
                    }
                    break;
                }
                case DamageObject:
                    def.kind = SimDamageKind.Object;
                    break;
                default:
                    _warnings.Add($"弾 {prefab.name} の {damage.GetType().Name} は未対応です。最初に当たった相手で消える弾として扱います");
                    def.kind = SimDamageKind.Object;
                    break;
            }

            prefab.TryGetComponent(out Rigidbody body);
            def.collides = prefab.GetComponents<Collider>().Any(c => !c.isTrigger) || (damage is DamageObject && body != null);
            def.shape = ReadShape(prefab, upward);
            def.move = ReadMove(prefab, body);
            return def;
        }

        private void ReadScale(GameObject prefab, DamageArea area, SimProjectileDef def)
        {
            var controller = SerializedReader.Ref<DamageAreaScaleController>(area, "scaleController");
            if (controller == null)
            {
                return;
            }

            if (controller.gameObject != prefab)
            {
                _warnings.Add($"弾 {prefab.name} の DamageAreaScaleController が根元にありません。大きさの変化を無視します");
                return;
            }

            float baseScale = prefab.transform.localScale.x;
            if (Mathf.Approximately(baseScale, 0f))
            {
                return;
            }

            def.defaultScale = SerializedReader.Prop(controller, "defaultScale").vector3Value.x / baseScale;
            SerializedProperty steps = SerializedReader.Prop(controller, "scaleDatas");
            for (int i = 0; i < steps.arraySize; i++)
            {
                SerializedProperty step = steps.GetArrayElementAtIndex(i);
                def.scaleSteps.Add(new SimScaleStep
                {
                    damageThreshold = step.FindPropertyRelative("damageThreshold").intValue,
                    scale = step.FindPropertyRelative("scale").vector3Value.x / baseScale,
                });
            }
        }

        private SimShapeDef ReadShape(GameObject prefab, bool upward)
        {
            List<Collider> colliders = prefab.GetComponents<Collider>().ToList();
            if (colliders.Count == 0)
            {
                _warnings.Add($"弾 {prefab.name} の根元に Collider がありません。当たらないものとして扱います");
                return new SimShapeDef();
            }

            Bounds bounds = SerializedReader.LocalBounds(prefab, c => c.gameObject == prefab);
            if (upward)
            {
                bounds = Rotate(bounds, Quaternion.LookRotation(Vector3.up));
            }

            bool circle = !upward && colliders.All(c => c is SphereCollider);
            return new SimShapeDef
            {
                circle = circle,
                radius = (bounds.extents.x + bounds.extents.z) * 0.5f,
                centerX = bounds.center.x,
                centerZ = bounds.center.z,
                halfX = bounds.extents.x,
                halfZ = bounds.extents.z,
            };
        }

        private static Bounds Rotate(Bounds bounds, Quaternion rotation)
        {
            Vector3 min = bounds.min;
            Vector3 max = bounds.max;
            var rotated = new Bounds(rotation * min, Vector3.zero);
            for (int i = 0; i < 8; i++)
            {
                var corner = new Vector3((i & 1) == 0 ? min.x : max.x, (i & 2) == 0 ? min.y : max.y, (i & 4) == 0 ? min.z : max.z);
                rotated.Encapsulate(rotation * corner);
            }
            return rotated;
        }

        private SimProjectileMoveDef ReadMove(GameObject prefab, Rigidbody body)
        {
            var move = new SimProjectileMoveDef();
            switch (prefab.GetComponent<MovementBase>())
            {
                case null:
                    move.kind = body != null && body.useGravity && !body.isKinematic
                        ? SimProjectileMoveKind.Fall
                        : SimProjectileMoveKind.None;
                    break;
                case StraightMovement straight:
                    move.kind = SimProjectileMoveKind.Straight;
                    move.speed = SerializedReader.Float(straight, "speed");
                    break;
                case TrackingMovement tracking:
                    move.kind = SimProjectileMoveKind.Tracking;
                    move.trackingRange = SerializedReader.Float(tracking, "trackingRange");
                    move.rotateSpeed = SerializedReader.Float(tracking, "rotateSpeed");
                    move.speed = SerializedReader.Float(tracking, "moveSpeed");
                    break;
                case BoomerangMovement boomerang:
                    move.kind = SimProjectileMoveKind.Boomerang;
                    move.width = SerializedReader.Float(boomerang, "width");
                    move.length = SerializedReader.Float(boomerang, "length");
                    move.duration = SerializedReader.Float(boomerang, "duration");
                    break;
                case ParabolaMovement parabola:
                    move.kind = SimProjectileMoveKind.Parabola;
                    move.speed = SerializedReader.Float(parabola, "speed");
                    move.gravity = SerializedReader.Float(parabola, "gravity");
                    move.upwardSpeed = SerializedReader.Float(parabola, "upwardSpeed");
                    break;
                case OrbitAmmoMovement orbit:
                    move.kind = SimProjectileMoveKind.Orbit;
                    move.radius = SerializedReader.Float(orbit, "radius");
                    move.speed = SerializedReader.Float(orbit, "speed");
                    break;
                case RotateMovement rotate:
                    move.kind = SimProjectileMoveKind.Rotate;
                    move.speed = SerializedReader.Float(rotate, "speed");
                    break;
                case MovementBase other:
                    _warnings.Add($"弾 {prefab.name} の動き {other.GetType().Name} は未対応です。動かないものとして扱います");
                    move.kind = SimProjectileMoveKind.None;
                    break;
            }
            return move;
        }
    }
}
