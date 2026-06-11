// ============================================================
//  AIBrain.cs  –  AI decision logic
//
//  Strategy tiers:
//  Easy  – mostly random; rarely plays No Bueno
//  Normal – greedy (maximise own score, sabotage leader)
//  Hard  – full look-ahead: protects Hot Sauce Boss,
//          targets leader with Crafty Crow / Order Envy,
//          saves No Bueno for when opponent has HotSauceBoss
// ============================================================

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TacoVsBurrito
{
    public class AIBrain : MonoBehaviour
    {
        AIDifficulty difficulty = AIDifficulty.Easy;

        public void SetDifficulty(AIDifficulty difficulty)
        {
            this.difficulty = difficulty;
        }
        public AIDecision Decide(AIPlayer ai, IReadOnlyList<PlayerBase> allPlayers)
        {
            return difficulty switch
            {
                AIDifficulty.Easy => DecideEasy(ai, allPlayers),
                AIDifficulty.Hard => DecideHard(ai, allPlayers),
                _ => DecideNormal(ai, allPlayers)
            };
        }

        // ===========================================================
        //  EASY  –  random legal move
        // ===========================================================
        private AIDecision DecideEasy(AIPlayer ai, IReadOnlyList<PlayerBase> allPlayers)
        {
            int idx = Random.Range(0, ai.Hand.Count);
            var card = ai.Hand.GetAt(idx);

            if (card is NoBuenoCard)
                DecideEasy(ai, allPlayers); // don't play No Bueno

            int dest = -1; // default trash pile
            if (card is IMealTypeAction)
            {
                if (card is TummyAcheCard)
                    dest = PickRandomOpponent(ai, allPlayers);
                else
                    dest = ai.Index;
            }

            return new AIDecision { cardIndex = idx, destIndex = dest };
        }

        // ===========================================================
        //  NORMAL  –  greedy heuristic
        // ===========================================================
        private AIDecision DecideNormal(AIPlayer ai, IReadOnlyList<PlayerBase> allPlayers)
        {
            var hand = ai.Hand;

            // Priority 1: Play Hot Sauce Boss into own meal (huge value)
            int hsb = FindFirstInHand<HotSauceBossCard>(ai);
            if (hsb >= 0) return new AIDecision { cardIndex = hsb, destIndex = ai.Index };

            // Priority 2: Play highest-value ingredient into own meal
            int bestIng = FindBestIngredient(ai);
            if (bestIng >= 0) return new AIDecision { cardIndex = bestIng, destIndex = ai.Index };

            // Priority 3: Dump Tummy Ache on the leader
            int ta = FindFirstInHand<TummyAcheCard>(ai);
            if (ta >= 0)
            {
                int leader = GetLeaderIndex(ai, allPlayers);
                return new AIDecision { cardIndex = ta, destIndex = leader };
            }

            // Priority 4: Play any action card
            int act = FindFirstAction(ai, typeof(CraftyCrowCard),
                                        typeof(TrashPandaCard),
                                        typeof(FoodFightCard));
            if (act >= 0)
                return new AIDecision { cardIndex = act, destIndex = -1 };

            // // Fallback: discard first card
            return new AIDecision { cardIndex = 0, destIndex = -1 };
        }

        // ===========================================================
        //  HARD  –  look-ahead strategic
        // ===========================================================
        private AIDecision DecideHard(AIPlayer ai, IReadOnlyList<PlayerBase> allPlayers)
        {
            //1. If we have Hot Sauce Boss AND at least 3 ingredients in meal → play it
            int hsb = FindFirstInHand<HotSauceBossCard>(ai);
            if (hsb >= 0 && ai.Meal.IngredientCardCount >= 3)
                return new AIDecision { cardIndex = hsb, destIndex = ai.Index };

            // 2. Use Crafty Crow to steal leader's Hot Sauce Boss
            int cc = FindFirstAction(ai, typeof(CraftyCrowCard));
            if (cc >= 0)
            {
                var leader = GetLeader(ai, allPlayers);
                if (leader != null && leader.Meal.HotSauceBossCardCount > 0)
                    return new AIDecision { cardIndex = cc, destIndex = -1 };
            }

            // 3. Order Envy if leader has way more points
            // int oe = FindFirstAction(ai, typeof(OrderEnvyCard));
            // if (oe >= 0)
            // {
            //     var leader = GetLeader(ai, allPlayers);
            //     if (leader != null && leader.Score > ai.Score + 5)
            //         return new AIDecision { cardIndex = oe, destIndex = leader.Index };
            // }

            // 4. Play HSB now if available
            if (hsb >= 0)
                return new AIDecision { cardIndex = hsb, destIndex = ai.Index };

            // 5. Best ingredient into own meal
            int bestIng = FindBestIngredient(ai);
            if (bestIng >= 0) return new AIDecision { cardIndex = bestIng, destIndex = ai.Index };

            // 6. Tummy ache to leader
            int ta = FindFirstInHand<TummyAcheCard>(ai);
            if (ta >= 0)
                return new AIDecision
                {
                    cardIndex = ta,
                    destIndex = GetLeaderIndex(ai, allPlayers)
                };

            // 7. Trash Panda
            int tp = FindFirstAction(ai, typeof(TrashPandaCard));
            if (tp >= 0 && GameplayManager.Instance.GetTrashPile().TrashCount > 0)
                return new AIDecision { cardIndex = tp, destIndex = -1 };

            // 8. Food Fight
            int ff = FindFirstAction(ai, typeof(FoodFightCard));
            if (ff >= 0) return new AIDecision { cardIndex = ff, destIndex = -1 };

            // 9. Crafty Crow against anyone with a meal card
            if (cc >= 0)
            {
                var victim = allPlayers.Where(p => p != ai && p.Meal.Cards.Count > 0)
                                       .OrderByDescending(p => p.Score).FirstOrDefault();
                if (victim != null)
                    return new AIDecision { cardIndex = cc, destIndex = -1 };
            }

            return new AIDecision { cardIndex = 0, destIndex = -1 };
        }

        // ===========================================================
        //  No Bueno reaction
        // ===========================================================
        /// Returns true if the AI should play No Bueno against this card.
        public bool ShouldPlayNoBueno(AIPlayer ai, IReadOnlyList<PlayerBase> allPlayers, CardBase cardBeingPlayed)
        {
            // Always block Order Envy targeting us
            if (cardBeingPlayed is OrderEnvyCard && IsCurrentlyWinning(ai, allPlayers)) return true;

            // Block Crafty Crow if we have a Hot Sauce Boss in our meal
            if (cardBeingPlayed is CraftyCrowCard &&
                ai.Meal.HotSauceBossCardCount > 0 && 
                IsCurrentlyWinning(ai, allPlayers)) return true;

            // Block a Tummy Ache being placed in our meal if we have Hot Sauce Boss
            // if (cardBeingPlayed is TummyAcheCard &&
            //     ai.Meal.HotSauceBossCardCount > 0) return true;

            return false;
        }

        // ===========================================================
        //  Helpers
        // ===========================================================
        public int FindFirstInHand<T>(PlayerBase p) where T : CardBase
        {
            for (int i = 0; i < p.Hand.Count; i++)
            {
                if (p.Hand.GetAt(i) is T)
                    return i;
            }
            return -1;
        }

        public PlayerBase ChooseOrderEnvyVictim(AIPlayer ai, IReadOnlyList<PlayerBase> allPlayers)
        {
            var victim = GetLeader(ai, allPlayers);
            return victim;
        }
        public void ChooseCraftyCrowVictim(AIPlayer ai, IReadOnlyList<PlayerBase> allPlayers, out PlayerBase victim, out CardBase cardToSteal)
        {
            victim = GetPlayerWithHotSauceBoss(ai, allPlayers);
            if (victim == null) victim = GetLeader(ai, allPlayers);
            cardToSteal = PickBestCardFromOpponentMeal(victim);
        }

        private int FindBestIngredient(PlayerBase p)
        {
            int best = -1, bestVal = -1;
            for (int i = 0; i < p.Hand.Count; i++)
            {
                var c = p.Hand.GetAt(i);
                if (c is IngredientCardBase @base && @base.CardValue > bestVal)
                { bestVal = @base.CardValue; best = i; }
            }
            return best;
        }

        private int FindFirstAction(PlayerBase p, params System.Type[] actionTypes)
        {
            for (int i = 0; i < p.Hand.Count; i++)
            {
                var card = p.Hand.GetAt(i);

                if (card is not ActionCardBase)
                    continue;

                foreach (var type in actionTypes)
                {
                    if (type.IsAssignableFrom(card.GetType()))
                        return i;
                }
            }
            return -1;
        }

        private int GetLeaderIndex(AIPlayer ai, IReadOnlyList<PlayerBase> all)
        {
            PlayerBase leader = GetLeader(ai, all);
            return leader?.Index ?? ((ai.Index + 1) % all.Count);
        }

        private PlayerBase GetLeader(AIPlayer ai, IReadOnlyList<PlayerBase> all)
        {
            return all.Where(p => p != ai).OrderByDescending(p => p.Score).FirstOrDefault();
        }

        private int PickRandomOpponent(AIPlayer ai, IReadOnlyList<PlayerBase> all)
        {
            var others = all.Where(p => p != ai).ToList();
            return others.Count > 0 ? others[Random.Range(0, others.Count)].Index : -1;
        }
        private CardBase PickBestCardFromOpponentMeal(PlayerBase opponent)
        {
            return opponent.Meal.Cards.OrderByDescending(c => c is HotSauceBossCard ? 100 : (c is IngredientCardBase ib ? ib.CardValue : 0)).FirstOrDefault();
        }
        private bool IsCurrentlyWinning(AIPlayer ai, IReadOnlyList<PlayerBase> allPlayers)
        {
            return allPlayers.OrderByDescending(p => p.Score).FirstOrDefault() == ai;
        }
        private PlayerBase GetPlayerWithHotSauceBoss(AIPlayer ai, IReadOnlyList<PlayerBase> players)
        {
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i] == ai)
                    continue;
                if (players[i].Meal.HotSauceBossCardCount > 0)
                    return players[i];
            }
            return null;
        }
        public CardBase PickBestCardFromCardPile(List<CardBase> cards)
        {
            return cards.OrderByDescending(c =>
            {
                if (c is HotSauceBossCard) return 100;
                else if (c is OrderEnvyCard) return 90;
                else if (c is CraftyCrowCard) return 80;
                else if (c is NoBuenoCard) return 70;
                else if (c is TrashPandaCard) return 60;
                else if (c is FoodFightCard) return 50;
                else if (c is IValueTypeCard valueCard) return valueCard.GetValue();
                else return 0;
            }).FirstOrDefault();
        }

    }
    public enum AIDifficulty { Easy, Normal, Hard }

    public struct AIDecision
    {
        public int cardIndex;         // index in AI's hand
        public int destIndex;   // -1 for playArea, otherwise player index to target
    }
}