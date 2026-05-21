using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace TacoVsBurrito
{
    public class TrashPile : CardPileBase, ICardDropTarget
    {
        public int TrashCount => pileCards.Count;
        private TurnState currentTurnState;

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
            SetCardOnPile(card);
            if (card is ActionCardBase @actionCard)
            {
                GameEvents.OnActionCardTrashed?.Invoke(@actionCard);
            }
            else
            {
                GameEvents.OnTurnEnded?.Invoke(GameManager.Instance.CurrentPlayer);
            }
        }
        void SetCardOnPile(CardBase card)
        {
            pileCards.Add(card);
            card.ChangePosition(cardsParent.position);
            card.ChangeParent(cardsParent);
            card.DisableInteraction();
            card.ToggleBackFace(false);
        }

        public void RemoveCard(CardBase card)
        {
            pileCards.Remove(card);
        }

        public void TrashAll(IEnumerable<CardBase> cards)
        {
            foreach (var c in cards) SetCardOnPile(c);
        }

        public Dictionary<CardBase, int> RetrieveFromTrash()
        {
            return pileCards.RetrieveUniqueCards();
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

        public override void PutCardsBack(List<CardBase> cards)
        {
            base.PutCardsBack(cards);
            foreach (var card in cards)
            {
                card.ToggleBackFace(false);
            }
        }

        void ManageTurnStateChanged(TurnState state, PlayerBase player)
        {
            currentTurnState = state;
        }
    }
}
