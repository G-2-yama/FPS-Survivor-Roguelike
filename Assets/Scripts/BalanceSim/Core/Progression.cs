using System;
using System.Collections.Generic;

namespace BalanceSim
{
    public sealed class Progression
    {
        public const int WeaponSlotCount = 6;
        public const int ItemSlotCount = 6;
        private const int ChoiceCount = 3;

        private readonly SimInput _input;
        private readonly Dictionary<string, SimWeaponDef> _weapons = new();
        private readonly Dictionary<string, SimItemDef> _items = new();
        private readonly IUpgradeChoicePolicy _policy;
        private readonly Random _random;
        private readonly PlayerState _player;

        public string[] WeaponSlots { get; } = new string[WeaponSlotCount];
        public SimItemDef[] Items { get; } = new SimItemDef[ItemSlotCount];
        public float Exp { get; private set; }
        public float RequiredExp { get; private set; }
        public int Level { get; private set; } = 1;
        public int PendingLevelUps { get; private set; }
        public float DamageMultiplier { get; private set; } = 1f;
        public float KnockbackMultiplier { get; private set; } = 1f;
        public bool WeaponSync { get; private set; }
        public Dictionary<string, float> RangeBonus { get; } = new();
        public List<SimLevelUpRecord> LevelUps { get; } = new();
        public List<SimEvolutionRecord> Evolutions { get; } = new();

        public Progression(SimInput input, PlayerState player, IUpgradeChoicePolicy policy, Random random)
        {
            _input = input;
            _player = player;
            _policy = policy;
            _random = random;
            RequiredExp = input.player.levelUpRequiredExp;

            foreach (SimWeaponDef weapon in input.weapons)
            {
                _weapons[weapon.name] = weapon;
            }
            foreach (SimItemDef item in input.items)
            {
                _items[item.name] = item;
            }

            for (int i = 0; i < WeaponSlotCount; i++)
            {
                string initial = i < input.player.initialWeapons.Count ? input.player.initialWeapons[i] : null;
                WeaponSlots[i] = string.IsNullOrEmpty(initial) ? null : initial;
            }
        }

        public static List<string> EvolvedWeapons(SimInput input)
        {
            var nextNames = new HashSet<string>();
            foreach (SimWeaponDef weapon in input.weapons)
            {
                if (!string.IsNullOrEmpty(weapon.next))
                {
                    nextNames.Add(weapon.next);
                }
            }

            var names = new List<string>();
            foreach (SimWeaponDef weapon in input.weapons)
            {
                if (string.IsNullOrEmpty(weapon.next) && nextNames.Contains(weapon.name) && !names.Contains(weapon.name))
                {
                    names.Add(weapon.name);
                }
            }
            return names;
        }

        public bool HasEvolved(string weapon)
        {
            foreach (SimEvolutionRecord evolution in Evolutions)
            {
                if (evolution.weapon == weapon)
                {
                    return true;
                }
            }
            return false;
        }

        public float RangeBonusOf(string key)
        {
            return key != null && RangeBonus.TryGetValue(key, out float bonus) ? bonus : 0f;
        }

        public void AddExp(float amount, float time)
        {
            Exp += amount;
            if (Exp >= RequiredExp)
            {
                Exp -= RequiredExp;
                Level++;
                PendingLevelUps++;
                RequiredExp *= _input.player.levelUpRate;
            }

            while (PendingLevelUps > 0)
            {
                ResolveLevelUp(time);
                PendingLevelUps--;
            }
        }

        private void ResolveLevelUp(float time)
        {
            List<SimUpgradeDef> choices = GenerateChoices();
            var record = new SimLevelUpRecord { time = time, level = Level };
            foreach (SimUpgradeDef choice in choices)
            {
                record.options.Add(choice.name);
            }

            if (choices.Count > 0)
            {
                int index = _policy.Choose(new UpgradeChoiceContext
                {
                    Options = choices,
                    Time = time,
                    Level = Level,
                    Hp = _player.Hp,
                    MaxHp = _player.MaxHp,
                    Weapons = WeaponSlots,
                    Items = ItemNames(),
                    Random = _random,
                });
                SimUpgradeDef selected = choices[Math.Clamp(index, 0, choices.Count - 1)];
                Apply(selected, time);
                record.chosen = selected.name;
            }

            LevelUps.Add(record);
        }

        private List<string> ItemNames()
        {
            var names = new List<string>(ItemSlotCount);
            foreach (SimItemDef item in Items)
            {
                names.Add(item?.name);
            }
            return names;
        }

        public List<SimUpgradeDef> GenerateChoices()
        {
            var pool = new List<SimUpgradeDef>();
            foreach (SimUpgradeDef upgrade in _input.upgrades)
            {
                if (IsAvailable(upgrade))
                {
                    pool.Add(upgrade);
                }
            }

            var choices = new List<SimUpgradeDef>(ChoiceCount);
            for (int i = 0; i < ChoiceCount; i++)
            {
                if (pool.Count == 0)
                {
                    break;
                }

                SimUpgradeDef selected = WeightedRandom(pool);
                choices.Add(selected);
                pool.Remove(selected);
            }
            return choices;
        }

        private SimUpgradeDef WeightedRandom(List<SimUpgradeDef> pool)
        {
            int total = 0;
            foreach (SimUpgradeDef upgrade in pool)
            {
                total += upgrade.weight;
            }

            int random = total > 0 ? _random.Next(total) : 0;
            foreach (SimUpgradeDef upgrade in pool)
            {
                random -= upgrade.weight;
                if (random < 0)
                {
                    return upgrade;
                }
            }
            return pool[0];
        }

        public bool IsAvailable(SimUpgradeDef upgrade)
        {
            switch (upgrade.kind)
            {
                case SimUpgradeKind.WeaponLevelUp:
                    return HasWeapon(upgrade.slot) && !string.IsNullOrEmpty(WeaponDef(WeaponSlots[upgrade.slot])?.next);
                case SimUpgradeKind.WeaponUnlock:
                {
                    SimWeaponDef target = WeaponDef(upgrade.weapon);
                    if (target == null)
                    {
                        return false;
                    }

                    int left = FirstSlotOf(target.type);
                    return !HasWeapon(left + 1) || !HasWeapon(left);
                }
                case SimUpgradeKind.GetItem:
                    return Array.IndexOf(Items, null) >= 0;
                case SimUpgradeKind.ItemLevelUp:
                    return upgrade.itemIndex >= 0 && upgrade.itemIndex < ItemSlotCount
                        && !string.IsNullOrEmpty(Items[upgrade.itemIndex]?.next);
                default:
                    return false;
            }
        }

        public void Apply(SimUpgradeDef upgrade, float time = 0f)
        {
            switch (upgrade.kind)
            {
                case SimUpgradeKind.WeaponLevelUp:
                {
                    string next = WeaponDef(WeaponSlots[upgrade.slot]).next;
                    WeaponSlots[upgrade.slot] = next;
                    SimWeaponDef nextDef = WeaponDef(next);
                    if (nextDef != null && string.IsNullOrEmpty(nextDef.next))
                    {
                        Evolutions.Add(new SimEvolutionRecord { time = time, weapon = next });
                    }
                    break;
                }
                case SimUpgradeKind.WeaponUnlock:
                {
                    int left = FirstSlotOf(WeaponDef(upgrade.weapon).type);
                    if (!HasWeapon(left))
                    {
                        WeaponSlots[left] = upgrade.weapon;
                    }
                    else if (!HasWeapon(left + 1))
                    {
                        WeaponSlots[left + 1] = upgrade.weapon;
                    }
                    break;
                }
                case SimUpgradeKind.GetItem:
                    EquipItem(ItemDef(upgrade.item));
                    break;
                case SimUpgradeKind.ItemLevelUp:
                {
                    SimItemDef target = Items[upgrade.itemIndex];
                    DiscardItem(upgrade.itemIndex);
                    EquipItem(ItemDef(target.next));
                    break;
                }
            }
        }

        private bool HasWeapon(int slot) => slot >= 0 && slot < WeaponSlotCount && WeaponSlots[slot] != null;

        private static int FirstSlotOf(SimWeaponType type) => (int)type * 2;

        private SimWeaponDef WeaponDef(string name) => name != null && _weapons.TryGetValue(name, out SimWeaponDef def) ? def : null;

        private SimItemDef ItemDef(string name) => name != null && _items.TryGetValue(name, out SimItemDef def) ? def : null;

        private void EquipItem(SimItemDef item)
        {
            if (item == null)
            {
                return;
            }

            int index = Array.IndexOf(Items, null);
            if (index < 0)
            {
                return;
            }

            Items[index] = item;
            ApplyItem(item, 1f);
        }

        private void DiscardItem(int index)
        {
            if (Items[index] == null)
            {
                return;
            }

            ApplyItem(Items[index], -1f);
            for (int i = index; i < ItemSlotCount - 1; i++)
            {
                Items[i] = Items[i + 1];
            }
            Items[ItemSlotCount - 1] = null;
        }

        private void ApplyItem(SimItemDef item, float sign)
        {
            switch (item.kind)
            {
                case SimItemKind.HealthUp:
                    _player.IncreaseHp((int)(sign * item.amount));
                    break;
                case SimItemKind.ExpCatcher:
                    RangeBonus[item.rangeKey ?? string.Empty] = RangeBonusOf(item.rangeKey) + sign * item.amount;
                    break;
                case SimItemKind.DamageUp:
                    DamageMultiplier += sign * item.amount;
                    break;
                case SimItemKind.KnockbackUp:
                    KnockbackMultiplier += sign * item.amount;
                    break;
                case SimItemKind.SyncWeapon:
                    WeaponSync = sign > 0f;
                    break;
            }
        }
    }

    public sealed class PlayerState
    {
        public Vec2 Position { get; set; }
        public Vec2 MoveDirection { get; set; } = Vec2.Forward;
        public Vec2 Facing { get; set; } = Vec2.Forward;
        public float Radius { get; set; }
        public float RunSpeed { get; set; }
        public int Hp { get; private set; }
        public int MaxHp { get; private set; }
        public bool IsDead => Hp <= 0;
        public bool DeathTriggered { get; private set; }

        public PlayerState(int maxHp)
        {
            Hp = maxHp;
            MaxHp = maxHp;
        }

        public void TakeDamage(int damage)
        {
            if (IsDead || damage <= 0)
            {
                return;
            }

            Hp = Math.Max(Hp - damage, 0);
            if (Hp == 0)
            {
                DeathTriggered = true;
            }
        }

        public void Heal(int amount)
        {
            if (IsDead || amount <= 0)
            {
                return;
            }

            Hp = Math.Min(Hp + amount, MaxHp);
        }

        public void IncreaseHp(int amount)
        {
            MaxHp += amount;
            Hp += amount;
        }
    }
}
