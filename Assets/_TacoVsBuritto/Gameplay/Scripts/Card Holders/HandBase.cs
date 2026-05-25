using System.Collections.Generic;
using UnityEngine;

namespace TacoVsBurrito
{
    public class HandBase : MonoBehaviour
    {
        protected readonly List<CardBase> _cards = new List<CardBase>();
        public IReadOnlyList<CardBase> Cards => _cards;
        public int Count => _cards.Count;
        public virtual void AddCard(CardBase c)
        {
            _cards.Add(c);
        }
        public virtual void RemoveCard(CardBase c)
        {
            _cards.Remove(c);
        }

        public List<CardBase> TakeAll()
        {
            var all = new List<CardBase>(_cards);
            _cards.Clear();
            return all;
        }
        public CardBase GetAt(int i) => (i >= 0 && i < _cards.Count) ? _cards[i] : null;
    }
}
