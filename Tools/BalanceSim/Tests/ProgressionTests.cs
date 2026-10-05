using BalanceSim;

namespace BalanceSim.Tests;

public class ProgressionTests
{
    private sealed class FirstChoicePolicy : IUpgradeChoicePolicy
    {
        public int Choose(UpgradeChoiceContext context) => 0;
    }

    private static (Progression progression, PlayerState player) Create(SimInput input)
    {
        var player = new PlayerState(input.player.maxHp);
        var progression = new Progression(input, player, new FirstChoicePolicy(), new Random(0));
        return (progression, player);
    }

    [Fact]
    public void AddExp_LevelsUpAtMostOncePerPickupAndCarriesOver()
    {
        SimInput input = TestInputs.Minimal();
        (Progression progression, _) = Create(input);

        progression.AddExp(120f, 0f);

        Assert.Equal(2, progression.Level);
        Assert.Equal(70f, progression.Exp, 3);
        Assert.Equal(54f, progression.RequiredExp, 3);

        progression.AddExp(0f, 0f);

        Assert.Equal(3, progression.Level);
        Assert.Equal(16f, progression.Exp, 3);
    }

    [Fact]
    public void Unlock_FillsLeftSlotFirstThenRight()
    {
        SimInput input = TestInputs.Minimal();
        input.player.initialWeapons = new List<string> { "", "", "", "", "", "" };
        input.weapons.Add(new SimWeaponDef { name = "AR_00", type = SimWeaponType.Main, next = "AR_01" });
        var unlock = new SimUpgradeDef { name = "ARUnlock", kind = SimUpgradeKind.WeaponUnlock, weight = 10, weapon = "AR_00" };
        (Progression progression, _) = Create(input);

        progression.Apply(unlock);
        Assert.Equal("AR_00", progression.WeaponSlots[0]);
        Assert.True(progression.IsAvailable(unlock));

        progression.Apply(unlock);
        Assert.Equal("AR_00", progression.WeaponSlots[1]);
        Assert.False(progression.IsAvailable(unlock));
    }

    [Fact]
    public void ItemLevelUp_RevertsShiftsAndReequipsAtFirstEmptySlot()
    {
        SimInput input = TestInputs.Minimal();
        input.items.Add(new SimItemDef { name = "HP_0", kind = SimItemKind.HealthUp, amount = 200f, next = "HP_1" });
        input.items.Add(new SimItemDef { name = "HP_1", kind = SimItemKind.HealthUp, amount = 300f, next = "" });
        input.items.Add(new SimItemDef { name = "Exp_0", kind = SimItemKind.ExpCatcher, amount = 5f, rangeKey = "dropitem", next = "" });
        (Progression progression, PlayerState player) = Create(input);

        progression.Apply(new SimUpgradeDef { kind = SimUpgradeKind.GetItem, item = "HP_0" });
        progression.Apply(new SimUpgradeDef { kind = SimUpgradeKind.GetItem, item = "Exp_0" });
        Assert.Equal(1200, player.MaxHp);
        Assert.Equal(5f, progression.RangeBonusOf("dropitem"));

        var levelUpSlot0 = new SimUpgradeDef { kind = SimUpgradeKind.ItemLevelUp, itemIndex = 0 };
        Assert.True(progression.IsAvailable(levelUpSlot0));
        progression.Apply(levelUpSlot0);

        Assert.Equal("Exp_0", progression.Items[0].name);
        Assert.Equal("HP_1", progression.Items[1].name);
        Assert.Equal(1300, player.MaxHp);
        Assert.Equal(1300, player.Hp);
    }

    [Fact]
    public void ItemLevelUp_ForMissingIndexIsNeverAvailable()
    {
        SimInput input = TestInputs.Minimal();
        input.items.Add(new SimItemDef { name = "A", kind = SimItemKind.DamageUp, amount = 0.1f, next = "A2" });
        input.items.Add(new SimItemDef { name = "A2", kind = SimItemKind.DamageUp, amount = 0.2f, next = "" });
        (Progression progression, _) = Create(input);
        for (int i = 0; i < 3; i++)
        {
            progression.Apply(new SimUpgradeDef { kind = SimUpgradeKind.GetItem, item = "A" });
        }

        Assert.True(progression.IsAvailable(new SimUpgradeDef { kind = SimUpgradeKind.ItemLevelUp, itemIndex = 2 }));
        Assert.False(progression.IsAvailable(new SimUpgradeDef { kind = SimUpgradeKind.ItemLevelUp, itemIndex = 3 }));
    }

    [Fact]
    public void GenerateChoices_PicksThreeDistinctAvailableUpgrades()
    {
        SimInput input = TestInputs.Minimal();
        input.weapons.Add(new SimWeaponDef { name = "HG_00", type = SimWeaponType.Main, next = "HG_01" });
        input.weapons.Add(new SimWeaponDef { name = "HG_01", type = SimWeaponType.Main, next = "" });
        input.upgrades.Add(new SimUpgradeDef { name = "LeftMainLevelUp", kind = SimUpgradeKind.WeaponLevelUp, weight = 120, slot = 0 });
        input.upgrades.Add(new SimUpgradeDef { name = "RightMainLevelUp", kind = SimUpgradeKind.WeaponLevelUp, weight = 120, slot = 1 });
        input.upgrades.Add(new SimUpgradeDef { name = "HandGunUnlock", kind = SimUpgradeKind.WeaponUnlock, weight = 10, weapon = "HG_00" });
        input.upgrades.Add(new SimUpgradeDef { name = "GetA", kind = SimUpgradeKind.GetItem, weight = 10, item = "A" });
        input.upgrades.Add(new SimUpgradeDef { name = "GetB", kind = SimUpgradeKind.GetItem, weight = 10, item = "B" });
        (Progression progression, _) = Create(input);

        for (int i = 0; i < 50; i++)
        {
            List<SimUpgradeDef> choices = progression.GenerateChoices();
            Assert.Equal(3, choices.Count);
            Assert.Equal(3, choices.Select(c => c.name).Distinct().Count());
            Assert.DoesNotContain(choices, c => c.name == "RightMainLevelUp");
        }
    }

    [Fact]
    public void EvolvedWeapons_AreChainEndsReachedByLevelUp()
    {
        SimInput input = TestInputs.Minimal();
        input.weapons.Add(new SimWeaponDef { name = "HG_00", next = "HG_01" });
        input.weapons.Add(new SimWeaponDef { name = "HG_01", next = "HG_Evo" });
        input.weapons.Add(new SimWeaponDef { name = "HG_Evo", next = "" });
        input.weapons.Add(new SimWeaponDef { name = "Single", next = "" });

        Assert.Equal(new[] { "HG_Evo" }, Progression.EvolvedWeapons(input));
    }

    [Fact]
    public void WeaponLevelUp_RecordsEvolutionOnlyAtChainEnd()
    {
        SimInput input = TestInputs.Minimal();
        input.weapons.Add(new SimWeaponDef { name = "HG_00", type = SimWeaponType.Main, next = "HG_01" });
        input.weapons.Add(new SimWeaponDef { name = "HG_01", type = SimWeaponType.Main, next = "HG_Evo" });
        input.weapons.Add(new SimWeaponDef { name = "HG_Evo", type = SimWeaponType.Main, next = "" });
        var levelUp = new SimUpgradeDef { kind = SimUpgradeKind.WeaponLevelUp, slot = 0 };
        (Progression progression, _) = Create(input);

        progression.Apply(levelUp, 10f);
        Assert.Empty(progression.Evolutions);
        Assert.False(progression.HasEvolved("HG_Evo"));

        progression.Apply(levelUp, 25f);
        SimEvolutionRecord evolution = Assert.Single(progression.Evolutions);
        Assert.Equal("HG_Evo", evolution.weapon);
        Assert.Equal(25f, evolution.time);
        Assert.True(progression.HasEvolved("HG_Evo"));
        Assert.False(progression.IsAvailable(levelUp));
    }

    [Fact]
    public void PriorityPolicy_ChoosesHighestRankedOption()
    {
        var policy = new PriorityChoicePolicy { priority = new List<string> { "B", "A" } };
        var context = new UpgradeChoiceContext
        {
            Options = new List<SimUpgradeDef> { new() { name = "A" }, new() { name = "C" }, new() { name = "B" } },
            Random = new Random(0),
        };

        Assert.Equal(2, policy.Choose(context));
    }
}
