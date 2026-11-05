using System.Reflection;
using System.Runtime.Loader;

if (args.Length != 2)
{
    throw new ArgumentException("Usage: Compatibility <game-data-directory> <TaintedNumbers.dll>");
}

var gameData = Path.GetFullPath(args[0]);
AssemblyLoadContext.Default.Resolving += (_, name) =>
{
    var path = Path.Combine(gameData, name.Name + ".dll");
    return File.Exists(path) ? AssemblyLoadContext.Default.LoadFromAssemblyPath(path) : null;
};

var mod = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.GetFullPath(args[1]));
mod.GetType("TaintedNumbers.ModEntry", throwOnError: true)!
    .GetMethod("Initialize")!.Invoke(null, null);

var harmonyAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(gameData, "0Harmony.dll"));
var harmonyType = harmonyAssembly.GetType("HarmonyLib.Harmony", throwOnError: true)!;
var game = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(gameData, "sts2.dll"));
var card = game.GetType("MegaCrit.Sts2.Core.Models.CardModel", throwOnError: true)!;
foreach (var name in new[] { "GetDescriptionForPile", "GetDescriptionForUpgradePreview" })
{
    var target = card.GetMethods(BindingFlags.Public | BindingFlags.Instance)
        .Single(method => method.Name == name);
    if (harmonyType.GetMethod("GetPatchInfo")!.Invoke(null, [target]) == null)
    {
        throw new InvalidOperationException($"Harmony did not patch {name}.");
    }
}

Console.WriteLine("PASS: Both description patches install against the installed game.");
