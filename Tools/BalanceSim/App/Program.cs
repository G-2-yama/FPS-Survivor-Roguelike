using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using BalanceSim;

const string InputPrefix = "input_";
const string OutputPrefix = "output_";

if (args.Length != 2)
{
    Console.Error.WriteLine("usage: BalanceSim <input-dir> <output-dir>");
    return 2;
}

try
{
    var context = new SimJsonContext(new JsonSerializerOptions
    {
        IncludeFields = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    });

    string[] inputs = Directory.GetFiles(args[0], $"{InputPrefix}*.json");
    Array.Sort(inputs, StringComparer.Ordinal);
    if (inputs.Length == 0)
    {
        throw new InvalidDataException($"入力がありません: {args[0]}");
    }
    Directory.CreateDirectory(args[1]);

    Simulator.RunBatch(inputs.Length, i => Load(inputs[i], context), (i, result) =>
    {
        string path = Path.Combine(args[1], OutputPrefix + Path.GetFileName(inputs[i]).Substring(InputPrefix.Length));
        string temp = path + ".tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(result, context.SimResult));
        File.Move(temp, path, true);
    });
    return 0;
}
catch (Exception e)
{
    Console.Error.WriteLine(e);
    return 1;
}

static SimBatchCase Load(string path, SimJsonContext context)
{
    try
    {
        SimInput input = JsonSerializer.Deserialize(File.ReadAllText(path), context.SimInput)
            ?? throw new InvalidDataException("入力が空です");
        return new SimBatchCase { Input = input, PolicyFactory = PolicyFactory.Create(input.choicePolicy) };
    }
    catch (Exception e)
    {
        throw new InvalidDataException($"{path}: {e.Message}", e);
    }
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
