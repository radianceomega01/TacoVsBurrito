using UnityEngine;
using System.Collections.Generic;

namespace TacoVsBurrito
{
    // ----------------------------------------------------------
    //  Hand  (private cards held by a player)
    // ----------------------------------------------------------
    public class Hand: MonoBehaviour
    {
        private readonly List<CardBase> _cards = new List<CardBase>();

        public IReadOnlyList<CardBase> Cards => _cards;
        public int Count => _cards.Count;

        public void AddCard(CardBase c)    => _cards.Add(c);
        public bool RemoveCard(CardBase c) => _cards.Remove(c);
        public CardBase GetAt(int i)       => (i >= 0 && i < _cards.Count) ? _cards[i] : null;

        /// Take all cards out of the hand (used by Order Envy).
        public List<CardBase> TakeAll()
        {
            var all = new List<CardBase>(_cards);
            _cards.Clear();
            return all;
        }

        /// Replace entire hand with a new set of cards (used by Order Envy).
        public void ReplaceWith(List<CardBase> newCards)
        {
        }

        public List<CardBase> GetAll() => new List<CardBase>(_cards);

        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"  Hand ({Count} cards):");
            foreach (var c in _cards) sb.AppendLine($"    {c}");
            return sb.ToString();
        }
    }
}

