using System.Collections.Generic;
using UnityEngine;

namespace TacoVsBurrito
{
    public class HandBase : MonoBehaviour
    {
        protected readonly List<CardBase> _cards = new List<CardBase>();
        public IReadOnlyList<CardBase> Cards => _cards;
        public int Count => _cards.Count;
        protected virtual void Awake()
        {
            GameEvents.OnTurnStateChanged += ManageTurnStateChanged;
        }
        protected virtual void OnDestroy()
        {
            GameEvents.OnTurnStateChanged -= ManageTurnStateChanged;
        }

        public virtual void AddCard(CardBase c)
        {
            _cards.Add(c);
            c.ToggleBackFace(true);
            c.DisableInteraction();
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
        protected virtual void ManageTurnStateChanged(TurnState turnState, PlayerBase player)
        {
            _cards.ForEach(card => card.DisableInteraction());
        }
        public CardBase GetAt(int i) => (i >= 0 && i < _cards.Count) ? _cards[i] : null;
    }
}
