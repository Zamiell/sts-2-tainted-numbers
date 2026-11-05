using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;

namespace TaintedNumbers;

[HarmonyPatch]
internal static class TaintedDescriptionPatch
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(CardModel), nameof(CardModel.GetDescriptionForPile),
            [typeof(PileType), typeof(Creature)]);
        yield return AccessTools.Method(typeof(CardModel), nameof(CardModel.GetDescriptionForUpgradePreview));
    }

    private static void Postfix(CardModel __instance, ref string __result)
    {
        if (__instance.Affliction is not Tainted tainted
            || __instance.CombatState is not { } combat
            || __instance.Owner?.Creature is not { IsAlive: true } target)
        {
            return;
        }

        var extraText = tainted.DynamicExtraCardText!.GetFormattedText();
        var line = $"[purple]{extraText}[/purple]";
        var index = __result.LastIndexOf(line, StringComparison.Ordinal);
        if (index < 0)
        {
            Log.Warn("Tainted Numbers could not find the Tainted line in the card description.");
            return;
        }

        var extraDamage = 0;
        foreach (var enemy in combat.Enemies)
        {
            if (!enemy.IsAlive || enemy.Monster is not { } monster)
            {
                continue;
            }

            foreach (var attack in monster.NextMove.Intents.OfType<AttackIntent>())
            {
                var damage = attack.DamageCalc!();
                var before = PreviewDamage(damage);
                // Tainted adds damage before Weak and other multipliers. Never change live powers for a preview.
                var after = PreviewDamage(damage + tainted.Amount);
                extraDamage += (after - before) * attack.Repeats;

                int PreviewDamage(decimal baseDamage) => (int)Hook.ModifyDamage(
                    combat.RunState, combat, target, enemy, baseDamage, ValueProp.Move,
                    null, null, ModifyDamageHookType.All, CardPreviewMode.None, out _);
            }
        }

        var insertion = index + "[purple]".Length + extraText.Length;
        __result = __result.Insert(insertion, $" ({extraDamage:+0;-0;+0} damage)");
    }
}
