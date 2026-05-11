using System.Collections.Generic;
using UnityEngine;

namespace TacoVsBurrito
{
    public class TrashPile : MonoBehaviour, ICardDropTarget
    {
        [SerializeField] Transform cardsTransform;
        public int TrashCount => _trashPile.Count;
        
        private List<CardBase> _trashPile = new();

        bool WasActionCardTrashed(CardBase card)
        {
            if(card is ActionCardBase) return true;
            return false;
        }
        
        public void Trash(CardBase card)
        {
            if (card != null) _trashPile.Add(card);
            card.ChangePosition(cardsTransform.position);
            card.ChangeParent(cardsTransform);
            card.DisableInteraction();

            if(card is ActionCardBase @base)
            {
                GameEvents.OnActionCardTrashed?.Invoke(card);
                @base.ExecuteAction();
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

        public bool CanDrop(CardBase card)
        {
            return true;
        }
        public void DropCardAfterDrag(CardBase card)
        {
            Trash(card);
            GameManager.Instance.OnCardPlacedAfterDrawn();
        }
    }
}
