using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace TacoVsBurrito
{
    public class TrashPile : MonoBehaviour, ICardDropTarget
    {
        [SerializeField] Transform cardsTransform;
        public int TrashCount => _cards.Count;

        private List<CardBase> _cards = new();
        private TurnState currentTurnState;

        private CardBase currentTrashedCard;

        void Awake()
        {
            GameEvents.OnTurnStateChanged += ManageTurnStateChanged;
        }

        void OnDestroy()
        {
            GameEvents.OnTurnStateChanged -= ManageTurnStateChanged;
        }

        public void Trash(CardBase card)
        {
            _cards.Add(card);
            card.ChangePosition(cardsTransform.position);
            card.ChangeParent(cardsTransform);
            card.DisableInteraction();
            card.ToggleBackFace(false);
            currentTrashedCard = card;

            if (card is ActionCardBase @actionCard)
            {
                GameEvents.OnActionCardTrashed?.Invoke(@actionCard);
            }
            else
            {
                GameEvents.OnTurnEnded?.Invoke(GameManager.Instance.CurrentPlayer);
            }
        }

        public void RemoveCard(CardBase card)
        {
            _cards.Remove(card);
        }
        public void PutCardsBack(List<CardBase> cards)
        {
            foreach (var card in cards)
            {
                card.ChangePosition(cardsTransform.position);
                card.ChangeParent(cardsTransform);
                card.DisableInteraction();
            }
        }

        public void TrashAll(IEnumerable<CardBase> cards)
        {
            foreach (var c in cards) Trash(c);
        }

        /// View the full trash pile (for Trash Panda selection).
        public IReadOnlyList<CardBase> PeekTrash() => _cards;

        public Dictionary<CardBase, int> RetrieveFromTrash()
        {
            Dictionary<CardBase, int> cardMap = new();
            foreach (var card in _cards)
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

        public bool CanDrop(CardBase card)
        {
            if (currentTurnState == TurnState.NoBuenoWindowPhase && card is not NoBuenoCard)
                return false;

            return true;
        }
        public void DropCardAfterDrag(CardBase card)
        {
            Trash(card);
        }

        void CheckAndExecuteAction()
        {
            if (currentTrashedCard is ActionCardBase @card && currentTrashedCard is not NoBuenoCard) //No bueno is immediately executed
            {
                @card.ExecuteAction();
                currentTrashedCard = null;
            }
        }

        void ManageTurnStateChanged(TurnState state, PlayerBase player)
        {
            currentTurnState = state;
            if (state == TurnState.ActionTargetPhase)
            {
                CheckAndExecuteAction();
            }
            else if (state == TurnState.ActionResolvePhase || state == TurnState.SkipPhase)
            {
                currentTrashedCard = null;
            }
        }
    }
}
