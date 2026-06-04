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

        void OnEnable()
        {
            GameEvents.OnTurnStateChanged += ManageTurnStateChanged;
        }

        void OnDisable()
        {
            GameEvents.OnTurnStateChanged -= ManageTurnStateChanged;
        }

        public void Trash(CardBase card)
        {
            SetCardOnPile(card);
            GameEvents.OnCardMovedSFX?.Invoke();
        }
        void SetCardOnPile(CardBase card)
        {
            pileCards.Add(card);
            card.ChangeScale(CARD_SCALE);
            card.ChangePosition(cardsParent.position);
            card.ChangeParent(cardsParent);
            card.DisableInteraction();
            card.ToggleInteractionType(InteractionType.Click);
            card.ToggleBackFace(false);
        }

        public void TrashAll(IEnumerable<CardBase> cards)
        {
            foreach (var c in cards) SetCardOnPile(c);
            GameEvents.OnCardDrawSFX?.Invoke();
        }

        public Dictionary<CardBase, int> RetrieveFromTrash()
        {
            return pileCards.RetrieveUniqueCards();
        }

        public bool CanDrop(CardBase card)
        {
            if (currentTurnState == TurnState.NoBuenoWindowPhase)
                return false;

            return true;
        }
        public void DropCardAfterDrag(CardBase card)
        {
            Trash(card);
            GameEvents.OnTurnEnded?.Invoke(GameManager.Instance.CurrentPlayer);
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
