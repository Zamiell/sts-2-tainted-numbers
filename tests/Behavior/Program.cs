using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using TaintedNumbers;

new Harmony("tainted-numbers.behavior").PatchAll(typeof(TaintedDescriptionPatch).Assembly);

var card = new CardModel { Affliction = new Tainted() };
var combat = card.CombatState!;
var enemy = new Creature { Monster = new MonsterModel() };
var attack = new AttackIntent(10);
enemy.Monster.NextMove.Intents.Add(attack);
combat.Enemies.Add(enemy);
Check(3);
Check(3); // Repeated renders must not accumulate suffixes.

attack.Repeats = 3;
Check(9);
enemy.Multiplier = 0.75m;
Check(6);

enemy.Monster.NextMove.Intents.Clear();
enemy.Monster.NextMove.Intents.Add(new AttackIntent(9, 3));
Check(9); // floor(12 * .75) - floor(9 * .75) = 3, not floor(3 * .75).

card.Owner.Creature.Tainted = 2;
Check(6);
card.Owner.Creature.Multiplier = 1.5m;
Check(9);
card.Owner.Creature.DamageCap = 1;
Check(0);
card.Owner.Creature.DamageCap = decimal.MaxValue;
card.Owner.Creature.Multiplier = 1;
card.Owner.Creature.Tainted = 0;

var secondEnemy = new Creature { Monster = new MonsterModel() };
secondEnemy.Monster.NextMove.Intents.Add(new AttackIntent(20, 2));
combat.Enemies.Add(secondEnemy);
Check(15);
secondEnemy.IsAlive = false;
Check(9);
enemy.Monster.NextMove.Intents.Clear();
enemy.Monster.NextMove.Intents.Add(new object());
Check(0);
enemy.Monster.NextMove.Intents.Add(new AttackIntent(-5));
Check(0);

card.Affliction.Amount = 5;
card.Affliction.DynamicExtraCardText = new LocString("Gain [red]5[/red] Tainted.");
card.Text = "Block.\n[purple]Gain [red]5[/red] Tainted.[/purple]\nRetain.";
enemy.Monster.NextMove.Intents.Clear();
enemy.Multiplier = 1;
enemy.Monster.NextMove.Intents.Add(new AttackIntent(10));
Check(5);

card.CombatState = null;
Unchanged();
card.CombatState = combat;
card.Owner.Creature.IsAlive = false;
Unchanged();
card.Owner.Creature.IsAlive = true;
card.Affliction = new AfflictionModel();
Unchanged();
card.Affliction = null;
Unchanged();
card.Affliction = new Tainted();
card.Text = "A different mod replaced the description.";
Unchanged();
if (Log.Warnings.Count != 2)
{
    throw new Exception("Missing Tainted text should report a warning for each render.");
}

Console.WriteLine("PASS: Single/multi-hit, Weak rounding, existing stacks, modifiers, caps, multiple enemies, live updates, text preservation, and unrelated cards.");

void Check(int expected)
{
    var stacks = card.Owner.Creature.Tainted;
    var expectedText = card.Text.Replace("[/purple]", $" (+{expected} damage)[/purple]", StringComparison.Ordinal);
    Equal(expectedText, card.GetDescriptionForPile(PileType.Hand));
    Equal(expectedText, card.GetDescriptionForUpgradePreview());
    Equal(stacks, card.Owner.Creature.Tainted);
}

void Unchanged()
{
    Equal(card.Text, card.GetDescriptionForPile(PileType.Hand));
    Equal(card.Text, card.GetDescriptionForUpgradePreview());
}

static void Equal<T>(T expected, T actual)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
    {
        throw new Exception($"Expected {expected}, got {actual}.");
    }
}
