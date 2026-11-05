using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Entities.Cards
{
    public enum PileType { None, Hand }
    public enum CardPreviewMode { None }
}

namespace MegaCrit.Sts2.Core.Entities.Creatures
{
    public sealed class Creature
    {
        public bool IsAlive { get; set; } = true;
        public MonsterModel? Monster { get; set; }
        public decimal Multiplier { get; set; } = 1m;
        public int Tainted { get; set; }
        public decimal DamageCap { get; set; } = decimal.MaxValue;
    }
}

namespace MegaCrit.Sts2.Core.Models
{
    public sealed class CombatState
    {
        public object RunState { get; } = new();
        public List<Creature> Enemies { get; } = [];
    }

    public sealed class Player
    {
        public Creature Creature { get; } = new();
    }

    public sealed class MonsterModel
    {
        public MoveState NextMove { get; } = new();
    }

    public sealed class MoveState
    {
        public List<object> Intents { get; } = [];
    }

    public sealed class CardModel
    {
        public AfflictionModel? Affliction { get; set; }
        public CombatState? CombatState { get; set; } = new();
        public Player Owner { get; } = new();
        public string Text { get; set; } = "Gain 5 Block.\n[purple]Gain 3 Tainted.[/purple]\nRetain.";

        [MethodImpl(MethodImplOptions.NoInlining)]
        public string GetDescriptionForPile(PileType pileType, Creature? target = null) => Text;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public string GetDescriptionForUpgradePreview() => Text;
    }

    public class AfflictionModel
    {
        public int Amount { get; set; } = 3;
        public LocString? DynamicExtraCardText { get; set; } = new("Gain 3 Tainted.");
    }

    public sealed class LocString(string text)
    {
        public string GetFormattedText() => text;
    }
}

namespace MegaCrit.Sts2.Core.Models.Afflictions
{
    public sealed class Tainted : AfflictionModel;
}

namespace MegaCrit.Sts2.Core.MonsterMoves.Intents
{
    public sealed class AttackIntent(decimal damage, int repeats = 1)
    {
        public Func<decimal>? DamageCalc { get; } = () => damage;
        public int Repeats { get; set; } = repeats;
    }
}

namespace MegaCrit.Sts2.Core.ValueProps
{
    public enum ValueProp { Move }
}

namespace MegaCrit.Sts2.Core.Hooks
{
    public enum ModifyDamageHookType { All }

    public static class Hook
    {
        public static decimal ModifyDamage(object runState, CombatState combatState,
            Creature target, Creature dealer, decimal damage, ValueProp props,
            CardModel? cardSource, object? cardPlay, ModifyDamageHookType hookType,
            CardPreviewMode previewMode, out IEnumerable<object> modifiers)
        {
            if (runState != combatState.RunState || !combatState.Enemies.Contains(dealer)
                || target.Monster != null || cardSource != null || cardPlay != null)
            {
                throw new Exception("Wrong damage preview context.");
            }
            modifiers = [];
            return Math.Max(0, Math.Min(target.DamageCap,
                (damage + target.Tainted) * dealer.Multiplier * target.Multiplier));
        }
    }
}

namespace MegaCrit.Sts2.Core.Logging
{
    public static class Log
    {
        public static List<string> Warnings { get; } = [];
        public static void Warn(string message) => Warnings.Add(message);
    }
}
