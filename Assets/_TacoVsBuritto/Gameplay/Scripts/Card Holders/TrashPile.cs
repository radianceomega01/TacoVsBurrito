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
            _cards.Add(card);
            card.ChangePosition(cardsTransform.position);
            card.ChangeParent(cardsTransform);
            card.DisableInteraction();
            card.ToggleBackFace(false);
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
            foreach (var c in cards) SetCardOnPile(c);
        }

        /// View the full trash pile (for Trash Panda selection).
        public IReadOnlyList<CardBase> PeekTrash() => _cards;

        public Dictionary<CardBase, int> RetrieveFromTrash()
        {
            return _cards.RetrieveUniqueCards();
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


        void ManageTurnStateChanged(TurnState state, PlayerBase player)
        {
            currentTurnState = state;
        }
    }
}
