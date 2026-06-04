using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace TacoVsBurrito
{
    public abstract class PlayerHandBase : MonoBehaviour
    {
        [SerializeField] protected TextMeshProUGUI cardCountTxt;

        protected readonly List<CardBase> _cards = new List<CardBase>();

        public IReadOnlyList<CardBase> Cards => _cards;
        public int Count => _cards.Count;
        protected virtual void OnEnable()
        {
            GameEvents.OnTurnStateChanged += ManageTurnStateChanged;
        }
        protected virtual void OnDisable()
        {
            GameEvents.OnTurnStateChanged -= ManageTurnStateChanged;
        }

        public abstract void AddCard(CardBase c);
        public abstract void AddCardWithoutArranging(CardBase c);

        public virtual void RemoveCard(CardBase c)
        {
            _cards.Remove(c);
            UpdateCountTxt();
        }

        public List<CardBase> TakeAll()
        {
            var all = new List<CardBase>(_cards);
            _cards.Clear();
            return all;
        }
        
        protected virtual void ManageTurnStateChanged(TurnState turnState, PlayerBase player){}
        protected void UpdateCountTxt() => cardCountTxt.SetText(Count.ToString());
        public CardBase GetAt(int i) => (i >= 0 && i < _cards.Count) ? _cards[i] : null;
    }
}
