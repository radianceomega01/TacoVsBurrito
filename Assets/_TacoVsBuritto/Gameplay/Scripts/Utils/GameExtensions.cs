using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TacoVsBurrito
{
    public static class GameExtensions
    {
        public static Dictionary<CardBase, int> RetrieveUniqueCards(this List<CardBase> cards)
        {
            Dictionary<CardBase, int> cardMap = new();
            foreach (var card in cards)
            {
                if (card is NoBuenoCard ||
                    card is OrderEnvyCard ||
                    card is CraftyCrowCard ||
                    card is FoodFightCard ||
                    card is HotSauceBossCard ||
                    card is HealthInspectorCard ||
                    card is TrashPandaCard)
                {
                    if (cardMap.ContainsKey(card))
                        cardMap[card]++;
                    else
                        cardMap.Add(card, 1);
                }
                else if (card is TummyAcheCard tummyAcheCard)
                {
                    var existingEntry = cardMap.FirstOrDefault(x => x.Key is TummyAcheCard tummyAcheCard);
                    if (!existingEntry.Equals(default(KeyValuePair<CardBase, int>)))
                    {
                        TummyAcheCard existingCard = (TummyAcheCard)existingEntry.Key;
                        // New card has higher value → replace old entry
                        if (tummyAcheCard.CardValue > existingCard.CardValue)
                        {
                            int count = existingEntry.Value;

                            cardMap.Remove(existingEntry.Key);
                            cardMap.Add(tummyAcheCard, 1);
                        }
                        // Same value → increase count
                        else if (tummyAcheCard.CardValue == existingCard.CardValue)
                        {
                            cardMap[existingEntry.Key]++;
                        }
                    }
                    else
                        cardMap.Add(card, 1);
                }
                else if (card is IngredientCardBase ingredientCard)
                {
                    var existingEntry = cardMap.FirstOrDefault(x => x.Key is IngredientCardBase ingredientCard);
                    if (!existingEntry.Equals(default(KeyValuePair<CardBase, int>)))
                    {
                        IngredientCardBase existingCard = (IngredientCardBase)existingEntry.Key;
                        // New card has higher value → replace old entry
                        if (ingredientCard.CardValue > existingCard.CardValue)
                        {
                            int count = existingEntry.Value;

                            cardMap.Remove(existingEntry.Key);
                            cardMap.Add(ingredientCard, 1);
                        }
                        // Same value → increase count
                        else if (ingredientCard.CardValue == existingCard.CardValue)
                        {
                            cardMap[existingEntry.Key]++;
                        }
                    }
                    else
                        cardMap.Add(card, 1);
                }
            }
            return cardMap;
        }
    }
}
