using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace TacoVsBurrito
{
    public class PlayerHand : MonoBehaviour
    {
        [SerializeField] protected TextMeshProUGUI cardCountTxt;

        protected readonly List<CardBase> _cards = new List<CardBase>();
        private const float CARD_SCALE = 0.4f;
        private const int CARD_SQUEEZED_DELAY_IN_MS = 200;

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
            ArrangeCard(c);
            UpdateCountTxt();
        }

        public virtual void AddCardWithoutArranging(CardBase c)
        {
            AddCard(c);
        }

        async Task ArrangeCard(CardBase card)
        {
            card.ChangePosition(transform.position);
            card.ChangeRotation(Quaternion.identity);
            card.ChangeParent(transform);
            card.ChangeScale(CARD_SCALE);

            await Task.Delay(CARD_SQUEEZED_DELAY_IN_MS);
            card.ChangeScale(0);
        }
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
        // {
        //     _cards.ForEach(card => card.DisableInteraction());
        // }
        protected void UpdateCountTxt() => cardCountTxt.SetText(Count.ToString());
        public CardBase GetAt(int i) => (i >= 0 && i < _cards.Count) ? _cards[i] : null;
    }
}
