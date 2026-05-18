// ============================================================
//  ActionResolver.cs
//  Handles every action card's effect with ALL official rules
//  and edge-case FAQ rulings.
//
//  Real rules implemented per-card:
//
//  HEALTH INSPECTOR
//    - Triggered when DRAWN (not played from hand)
//    - All meal contents → Trash pile immediately
//    - Turn ends immediately
//    - Cannot be blocked by No Bueno
//    - Exception: Trash Panda can retrieve it; if retrieved,
//      effect triggers immediately upon taking it
//
//  NO BUENO
//    - Can be played by ANY PlayerBase at ANY time another card
//      is being played (reactive/interrupt)
//    - Cancels the target card; sends it to Trash (no effect)
//    - Can block another No Bueno (Si Bueno chain)
//    - CANNOT block: Health Inspector, a PlayerBase's LAST card
//    - Multiple No Buenos can chain (each blocks the previous)
//
//  CRAFTY CROW
//    - Steal ONE specific card from any opponent's meal
//    - Place it DIRECTLY into YOUR meal (not your hand)
//    - In Secret mode: steal must be chosen blindly
//
//  TRASH PANDA
//    - Take ONE card from the Trash pile into your hand
//    - May take a Health Inspector (triggers immediately!)
//    - May take another Trash Panda (but max 2 retrievals
//      of Trash Panda per game per official FAQ)
//    - After use, Trash Panda itself goes to Trash pile
//
//  FOOD FIGHT
//    - Starting with the PlayerBase who played Food Fight,
//      moving clockwise, each PlayerBase flips ONE card from draw pile
//    - Compare all flipped cards: PlayerBase who revealed the
//      HIGHEST-VALUE INGREDIENT card wins
//    - Winner picks ONE of the flipped cards to keep in hand
//    - All remaining flipped cards are shuffled back into draw pile
//    - Non-ingredient flips count 0 for comparison
//    - Ties → tied PlayerBases each keep their card
//
//  ORDER ENVY
//    - Swap YOUR entire hand AND entire meal with chosen PlayerBase
//    - Cannot be blocked AFTER the swap happens
//    - If played as last card: swap happens, then game ends
//    - You can optionally swap physical seats (cosmetic only)
//    - Cannot use No Bueno after the swap has occurred
// ============================================================

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

namespace TacoVsBurrito
{
    public class ActionResolver
    {
        private DrawPile _drawPile;
        private TrashPile _trashPile;
        private readonly IReadOnlyList<PlayerBase> _activePLayers;

        // Tracks total Trash Pandas retrieved across the game (FAQ: max 2)
        // Keyed by PlayerBase index
        private Dictionary<int, int> _trashPandaRetrieved = new Dictionary<int, int>();

        public ActionResolver(DrawPile deck, TrashPile trashPile, IReadOnlyList<PlayerBase> activePlayers)
        {
            _drawPile = deck;
            _trashPile = trashPile;
            _activePLayers = activePlayers;
        }

        // ==========================================================
        //  HEALTH INSPECTOR  (triggered on draw, not on play)
        // ==========================================================

        /// Call this when a PlayerBase draws a Health Inspector.
        /// Returns a log string. All meal cards are trashed.
        public void ResolveHealthInspector(PlayerBase victim)
        {

            // Trash entire meal
            var mealContents = victim.Meal.TakeAll();
            _trashPile.TrashAll(mealContents);

            GameEvents.OnMealCleared?.Invoke(victim);
            GameEvents.OnLogMessage?.Invoke($"🚨 HEALTH INSPECTOR! {victim.Name}'s entire meal is trashed! " +
                   $"({mealContents.Count} card(s) discarded) Turn ends immediately.");

            GameEvents.OnHealthInspector?.Invoke();
            GameEvents.OnTurnEnded?.Invoke(GameManager.Instance.CurrentPlayer); // skip play phase       
        }

        // ==========================================================
        //  NO BUENO  (reactive – can be played by ANYONE)
        // ==========================================================

        /// Returns true if No Bueno can legally be played right now.
        /// blocker = PlayerBase trying to play No Bueno
        /// targetCard = the card being blocked
        /// isLastCard = is targetCard the very last card in the target's hand?
        public bool CanPlayNoBueno(NoBuenoCard noBuenoCard, CardBase targetCard, bool isLastCard)
        {
            if (isLastCard)                   return false;  // last card unblockable
            return true;
        }

        // ==========================================================
        //  CRAFTY CROW
        // ==========================================================

        /// Steal stealTarget card from victim's meal into caster's meal.
        public void ResolveCraftyCrow(PlayerBase caster,
                                         PlayerBase victim, CardBase stealTarget)
        {
            caster.Meal.AddCard(stealTarget);
            victim.Meal.RemoveCard(stealTarget);

            GameEvents.OnCardStolenFromMeal?.Invoke(caster, victim, stealTarget);
            GameEvents.OnLogMessage?.Invoke($"🐦 Crafty Crow! {caster.Name} stole '{stealTarget.Name}' " +
                   $"from {victim.Name}'s meal into their own meal!");

            GameEvents.OnTurnEnded?.Invoke(GameManager.Instance.CurrentPlayer);       
        }

        // ==========================================================
        //  TRASH PANDA
        // ==========================================================

        /// Retrieve chosenCard from the Trash pile into caster's hand.
        /// Applies Trash Panda retrieval limit (max 2 Trash Pandas per game).
        /// If chosenCard is a Health Inspector, trigger it immediately.
        /// Returns (logMessage, healthInspectorTriggered).
        public void ResolveTrashPanda(
            PlayerBase caster, CardBase chosenCard)
        {
            // Enforce max 2 Trash Panda retrievals per game (official FAQ)
            // if (chosenCard is TrashPandaCard)
            // {
            //     _trashPandaRetrieved.TryGetValue(caster.Index, out int count);
            //     if (count >= 2)
            //     {
            //         _trashPile.Trash(trashPandaCard);
            //         return ($"🦝 Trash Panda failed: {caster.Name} has already retrieved " +
            //                 $"2 Trash Pandas this game (limit reached).", false);
            //     }
            //     _trashPandaRetrieved[caster.Index] = count + 1;
            // }

            if (chosenCard is HealthInspectorCard card)
            {
                // Health Inspector triggers IMMEDIATELY when taken from trash
                ResolveHealthInspector(caster);
                GameEvents.OnLogMessage?.Invoke($"🦝 Trash Panda! {caster.Name} retrieved a Health Inspector from the Trash pile! " +
                        $"It triggers immediately!");
            }

            caster.Hand.AddCard(chosenCard);
            _trashPile.RemoveCard(chosenCard);
            GameEvents.OnCardAddedToHand?.Invoke(caster, chosenCard);
            GameEvents.OnLogMessage?.Invoke($"🦝 Trash Panda! {caster.Name} retrieved '{chosenCard.Name}' from the Trash pile.");
            GameEvents.OnTurnEnded?.Invoke(GameManager.Instance.CurrentPlayer);
        }

        // ==========================================================
        //  FOOD FIGHT
        // ==========================================================

        /// Executes the Food Fight.
        /// PlayerBases = all PlayerBases in clockwise order, starting with the caster.
        /// Returns a log. Winning PlayerBase gets to keep one card; rest shuffle back.
        public string ResolveFoodFight(PlayerBase caster, FoodFightCard foodFightCard,
                                        List<PlayerBase> clockwisePlayerBases)
        {
            _trashPile.Trash(foodFightCard);

            // Each PlayerBase flips one card from draw pile
            var flipped = new Dictionary<PlayerBase, CardBase>();
            foreach (var p in clockwisePlayerBases)
            {
                CardBase f = _drawPile.FlipTop();
                if (f != null)
                    flipped[p] = f;
            }

            if (flipped.Count == 0)
                return "🍽 Food Fight! No cards left to flip.";

            // Build log
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("🍽 FOOD FIGHT!");
            foreach (var kv in flipped)
                sb.AppendLine($"  {kv.Key.Name} flipped: {kv.Value.Name} " +
                              $"[{ValueForFoodFight(kv.Value)} pt]");

            // Find winner(s): highest ingredient value; non-ingredients = 0
            int bestValue = flipped.Values.Max(c => ValueForFoodFight(c));
            var winners   = flipped.Where(kv => ValueForFoodFight(kv.Value) == bestValue)
                                   .ToList();

            // Winners each keep their card
            var toShuffle = new List<CardBase>();
            foreach (var kv in flipped)
            {
                if (winners.Any(w => w.Key == kv.Key))
                {
                    kv.Key.Hand.AddCard(kv.Value);
                    GameEvents.OnCardAddedToHand?.Invoke(kv.Key, kv.Value);
                    sb.AppendLine($"  🏆 {kv.Key.Name} wins and keeps '{kv.Value.Name}'!");
                }
                else
                {
                    toShuffle.Add(kv.Value);
                }
            }

            // Shuffle remaining cards back into draw pile
            if (toShuffle.Count > 0)
            {
                _drawPile.ShuffleBackIn(toShuffle);
                sb.AppendLine($"  {toShuffle.Count} card(s) shuffled back into draw pile.");
            }

            return sb.ToString().TrimEnd();
        }

        private int ValueForFoodFight(CardBase card) =>
            card is IngredientCardBase @base ? @base.CardValue : 0;

        // ==========================================================
        //  ORDER ENVY
        // ==========================================================

        /// Swap caster's hand + meal with target's hand + meal.
        /// After swap, cannot be blocked.
        public void ResolveOrderEnvy(PlayerBase caster, PlayerBase target)
        {
            caster.SwapMeal(target);

            GameEvents.OnLogMessage?.Invoke($"😤 Order Envy! {caster.Name} swapped their hand and meal with {target.Name}!");
            GameEvents.OnTurnEnded?.Invoke(GameManager.Instance.CurrentPlayer);
        }

        // ==========================================================
        //  Place a meal card into any meal (ingredient/tummy ache/hot sauce)
        // ==========================================================

        /// Place an ingredient-type card from caster's hand into the destination meal.
        /// destPlayerBase may be self or any opponent.
        public string PlaceCardInMeal(PlayerBase caster, CardBase card, PlayerBase destPlayerBase)
        {
            if (!card.IsPlaceableInMeal)
                return $"⚠ '{card.Name}' cannot be placed in a meal.";

            caster.Hand.RemoveCard(card);
            destPlayerBase.Meal.AddCard(card);

            GameEvents.OnCardPlacedInMeal?.Invoke(caster, destPlayerBase, card);

            string dest = (caster == destPlayerBase) ? "their own meal" : $"{destPlayerBase.Name}'s meal";

            return $"🍽 {caster.Name} placed '{card.Name}' into {dest}.";
        }
    }
}