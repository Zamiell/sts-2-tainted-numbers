using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

namespace TaintedNumbers;

[ModInitializer(nameof(Initialize))]
public static class ModEntry
{
    public static void Initialize()
    {
        new Harmony("james.sts2.tainted-numbers").PatchAll(typeof(ModEntry).Assembly);
    }
}
