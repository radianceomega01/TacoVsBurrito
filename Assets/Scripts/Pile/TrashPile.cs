using System.Collections.Generic;
using UnityEngine;

namespace TacoVsBurrito
{
    public class TrashPile : MonoBehaviour
    {
        public int TrashCount => _trashPile.Count;
        
        private List<CardBase> _trashPile = new();
        
        public void Trash(CardBase card)
        {
            if (card != null) _trashPile.Add(card);
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
    }
}
