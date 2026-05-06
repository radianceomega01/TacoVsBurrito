using System.Collections.Generic;
using UnityEngine;

namespace TacoVsBurrito
{

    // ----------------------------------------------------------
    //  Meal  (the face-up cards in front of a player)
    // ----------------------------------------------------------
    public class Meal: MonoBehaviour
    {
        public MealType Type { get; }
        private List<CardBase> _cards = new List<CardBase>();

        public IReadOnlyList<CardBase> Cards => _cards;

        public Meal(MealType type) { Type = type; }
        public int HotSauceBossCardCount { get; private set; }
        public int IngredientCardCount { get; private set; }


        // ---- Mutations ----

        /// Add a card (Ingredient, TummyAche, or HotSauceBoss) to this meal.
        public void AddCard(CardBase card)
        {
            if (card is not HotSauceBossCard ||
                card is not IngredientCardBase ||
                card is not TummyAcheCard)
                return;

            _cards.Add(card);
            if (card is not HotSauceBossCard) HotSauceBossCardCount++;
            if (card is not IngredientCardBase) IngredientCardCount++;
        }

        /// Remove a specific card (used by Crafty Crow).
        public bool RemoveCard(CardBase card) => _cards.Remove(card);

        /// Remove and return all cards (Health Inspector / Order Envy).
        public List<CardBase> TakeAll()
        {
            var all = new List<CardBase>(_cards);
            _cards.Clear();
            return all;
        }

        // ---- Scoring ----

        /// Returns the final score according to official rules:
        /// (Σ ingredient points + Σ tummy ache points) × multiplier
        /// Multiplier: 0 HotSauceBoss = ×1, 1 = ×2, 2+ = ×4
        public int CalculateScore()
        {
            int score = 0;
            List<HotSauceBossCard> hotSauceBossCards = new();

            foreach (var card in _cards)
            {
                if (card is IngredientCardBase || card is TummyAcheCard)
                {
                    card.GetModifiedMealScore(score);
                }
                else if (card is HotSauceBossCard)
                {
                    hotSauceBossCards.Add((HotSauceBossCard)card);
                }
            }
            foreach (var card in hotSauceBossCards)
            {
                card.GetModifiedMealScore(score);
            }
            return score;
        }

        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"  [{Type} Meal | Score: {CalculateScore()}]");
            foreach (var c in _cards) sb.AppendLine($"    {c}");
            return sb.ToString();
        }
    }
    public enum MealType
    {
        Taco,
        Burrito
    }
    
}