using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TacoVsBurrito
{
    public static class GameExtensions
    {
        public static Dictionary<CardBase, int> RetrieveUniqueCards(this List<CardBase> cards)
        {
            Dictionary<CardBase, int> uniqueCards = new();

            // Cards without values
            Dictionary<Type, (CardBase card, int count)> normalCards = new();

            // Cards with values
            Dictionary<Type, (CardBase card, int count, int highestValue)> valueCards
                = new();

            foreach (var card in cards)
            {
                switch (card)
                {
                    // -----------------------------------------
                    // Ingredient Cards (+1, +2, +3)
                    // -----------------------------------------
                    case IngredientCardBase ingredient:
                        {
                            Type type = ingredient.GetType();

                            if (!valueCards.ContainsKey(type))
                            {
                                valueCards[type] =
                                    (ingredient, 1, ingredient.CardValue);
                            }
                            else
                            {
                                var existing = valueCards[type];

                                // Higher value found → replace
                                if (ingredient.CardValue > existing.highestValue)
                                {
                                    valueCards[type] =
                                        (ingredient, 1, ingredient.CardValue);
                                }
                                // Same highest value → increment count
                                else if (ingredient.CardValue ==
                                         existing.highestValue)
                                {
                                    valueCards[type] =
                                    (
                                        existing.card,
                                        existing.count + 1,
                                        existing.highestValue
                                    );
                                }
                            }

                            break;
                        }

                    // -----------------------------------------
                    // Tummy Ache Cards (-1, -2, -3)
                    // Higher means larger absolute value
                    // Example: -3 > -2
                    // -----------------------------------------
                    case TummyAcheCard tummyAche:
                        {
                            Type type = tummyAche.GetType();

                            int absValue = Mathf.Abs(tummyAche.CardValue);

                            if (!valueCards.ContainsKey(type))
                            {
                                valueCards[type] =
                                    (tummyAche, 1, absValue);
                            }
                            else
                            {
                                var existing = valueCards[type];

                                // Higher absolute penalty found
                                if (absValue > existing.highestValue)
                                {
                                    valueCards[type] =
                                        (tummyAche, 1, absValue);
                                }
                                // Same value
                                else if (absValue ==
                                         existing.highestValue)
                                {
                                    valueCards[type] =
                                    (
                                        existing.card,
                                        existing.count + 1,
                                        existing.highestValue
                                    );
                                }
                            }

                            break;
                        }

                    // -----------------------------------------
                    // All non-value cards
                    // -----------------------------------------
                    default:
                        {
                            Type type = card.GetType();

                            if (normalCards.ContainsKey(type))
                            {
                                var existing = normalCards[type];

                                normalCards[type] =
                                (
                                    existing.card,
                                    existing.count + 1
                                );
                            }
                            else
                            {
                                normalCards[type] = (card, 1);
                            }

                            break;
                        }
                }
            }

            // Merge into final dictionary

            foreach (var pair in normalCards.Values)
            {
                uniqueCards.Add(pair.card, pair.count);
            }

            foreach (var pair in valueCards.Values)
            {
                uniqueCards.Add(pair.card, pair.count);
            }

            return uniqueCards;
        }
    }
}
