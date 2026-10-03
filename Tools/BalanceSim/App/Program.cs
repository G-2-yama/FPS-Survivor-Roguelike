using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
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
    SimResult result = Simulator.Run(input);
    File.WriteAllText(args[1], JsonSerializer.Serialize(result, context.SimResult));
    return 0;
}
catch (Exception e)
{
    Console.Error.WriteLine(e);
    return 1;
}

[JsonSourceGenerationOptions(IncludeFields = true)]
[JsonSerializable(typeof(SimInput))]
[JsonSerializable(typeof(SimResult))]
internal partial class SimJsonContext : JsonSerializerContext
{
}
