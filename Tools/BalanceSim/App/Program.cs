using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using BalanceSim;

if (args.Length != 2)
{
    Console.Error.WriteLine("usage: BalanceSim <input.json> <output.json>");
    return 2;
}

try
{
    var context = new SimJsonContext(new JsonSerializerOptions
    {
        IncludeFields = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    });

    SimInput input = JsonSerializer.Deserialize(File.ReadAllText(args[0]), context.SimInput)
        ?? throw new InvalidDataException($"入力が空です: {args[0]}");
    Func<IUpgradeChoicePolicy> policyFactory = PolicyFactory.Create(input.choicePolicy);
    SimResult result = Simulator.Run(input, policyFactory);
    File.WriteAllText(args[1], JsonSerializer.Serialize(result, context.SimResult));
    return 0;
}
catch (Exception e)
{
    Console.Error.WriteLine(e);
    return 1;
}

internal static class PolicyFactory
{
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "BalanceSim.Core は TrimmerRootAssembly で丸ごと残している")]
    public static Func<IUpgradeChoicePolicy> Create(SimPolicyRef policy)
    {
        if (string.IsNullOrEmpty(policy?.typeName))
        {
            return () => new RandomChoicePolicy();
        }

        Type type = typeof(IUpgradeChoicePolicy).Assembly.GetType(policy.typeName)
            ?? throw new InvalidDataException($"選び方の型が見つかりません: {policy.typeName}");
        if (!typeof(IUpgradeChoicePolicy).IsAssignableFrom(type))
        {
            throw new InvalidDataException($"{policy.typeName} は IUpgradeChoicePolicy を実装していません");
        }

        var options = new JsonSerializerOptions
        {
            IncludeFields = true,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        };
        string json = string.IsNullOrEmpty(policy.json) ? "{}" : policy.json;
        return () => (IUpgradeChoicePolicy)JsonSerializer.Deserialize(json, type, options);
    }
}

[JsonSourceGenerationOptions(IncludeFields = true)]
[JsonSerializable(typeof(SimInput))]
[JsonSerializable(typeof(SimResult))]
internal partial class SimJsonContext : JsonSerializerContext
{
}
