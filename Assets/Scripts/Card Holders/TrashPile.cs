using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace TacoVsBurrito
{
    public class TrashPile : MonoBehaviour, ICardDropTarget
    {
        [SerializeField] Transform cardsTransform;
        public int TrashCount => _trashPile.Count;
        
        private List<CardBase> _trashPile = new();
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
            _trashPile.Add(card);
            card.ChangePosition(cardsTransform.position);
            card.ChangeParent(cardsTransform);
            card.DisableInteraction();
            card.ToggleBackFace(false);
            currentTrashedCard = card;
            
            if(card is ActionCardBase @actionCard)
            {
                GameEvents.OnActionCardTrashed?.Invoke(@actionCard);
            }
            else
            {
               GameEvents.OnTurnEnded?.Invoke(GameManager.Instance.CurrentPlayer);
            }
        }

        public void TrashAll(IEnumerable<CardBase> cards)
        {
            foreach (var c in cards) Trash(c);
        }

        /// View the full trash pile (for Trash Panda selection).
        public IReadOnlyList<CardBase> PeekTrash() => _trashPile;

        /// Retrieve a specific card from the trash pile (Trash Panda action).
        /// Returns true if found and removed.
        public bool RetrieveFromTrash(CardBase card)
        {
            return _trashPile.Remove(card);
        }
        public  CardBase RetrieveFromTrash()
        {            
            if (_trashPile.Count <= 1) return null;
            CardBase card = _trashPile[0];
            _trashPile.Remove(card);
            return card;
        }

        public bool CanDrop(CardBase card)
        {
            if(currentTurnState == TurnState.NoBuenoWindowPhase && card is not NoBuenoCard)
                return false;

            return true;
        }
        public void DropCardAfterDrag(CardBase card)
        {
            Trash(card);
        }

        void CheckAndExecuteAction()
        {
            if(currentTrashedCard is ActionCardBase @card && currentTrashedCard is not NoBuenoCard) //No bueno is immediately executed
            {
                @card.ExecuteAction();
                currentTrashedCard = null;
            }
        }

        void ManageTurnStateChanged(TurnState state, PlayerBase player)
        {
            currentTurnState = state;
            if(state == TurnState.ActionTargetedPhase)
            {
                CheckAndExecuteAction();
            }
            else if(state == TurnState.ActionResolvePhase)
            {
                currentTrashedCard = null;
            }
        }
    }
}
