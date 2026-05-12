using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using UnityEditor.Experimental.GraphView;
using System.Runtime.InteropServices;
using System.Net.Http.Headers;
using System;

namespace TacoVsBurrito
{
    // ----------------------------------------------------------
    //  Hand  (private cards held by a player)
    // ----------------------------------------------------------
    public class Hand: MonoBehaviour, ICardPickupTarget
    {
        private RectTransform _rectTransform;
        private readonly List<CardBase> _cards = new List<CardBase>();
        private PlayerBase parentPlayer;

        private const float CARD_SPACING = 4f;

        public IReadOnlyList<CardBase> Cards => _cards;
        public int Count => _cards.Count;
        public PlayerBase ParentPlayer => parentPlayer;
        public Transform ParentTransform => transform.parent;

        void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            GameEvents.OnTurnStateChanged += ManageTurnStateChanged;
        }

        void OnDestroy()
        {
            GameEvents.OnTurnStateChanged -= ManageTurnStateChanged;
        }

        void Start()
        {
            parentPlayer = GetComponentInParent<PlayerBase>();
        }

        public void RemoveCard(CardBase c)
        {
            _cards.Remove(c);
            ArrangeCardsAnimated();
        }
        public CardBase GetAt(int i) => (i >= 0 && i < _cards.Count) ? _cards[i] : null;

        /// Take all cards out of the hand (used by Order Envy).
        public List<CardBase> TakeAll()
        {
            var all = new List<CardBase>(_cards);
            _cards.Clear();
            return all;
        }

        public void AddCard(CardBase c)
        {
            _cards.Add(c);
            if(parentPlayer is not SelfPlayer )
                c.DisableInteraction();
            ArrangeCardsAnimated();
        }


        /// Replace entire hand with a new set of cards (used by Order Envy).
        public void ReplaceWith(List<CardBase> newCards)
        {
            _cards.Clear();
            _cards.AddRange(newCards);
            ArrangeCardsAnimated();
        }

        public List<CardBase> GetAll() => new List<CardBase>(_cards);

        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"  Hand ({Count} cards):");
            foreach (var c in _cards) sb.AppendLine($"    {c}");
            return sb.ToString();
        }

        private void ArrangeCardsAnimated()
        {
            int count = _cards.Count;
            if (count == 0) return;

            float totalWidth = (count - 1) * CARD_SPACING;
            float startOffset = -totalWidth / 2f;

            for (int i = 0; i < count; i++)
            {
                float offset = startOffset + i * CARD_SPACING;
                Vector3 targetPos = transform.position + transform.right * offset;

                _cards[i].ChangePosition(targetPos);
                _cards[i].ChangeParent(transform);
            }
            ModifyWidthOfRect();
        }

        void ModifyWidthOfRect()
        {
            int count = _cards.Count;
            if (count == 0) return;

            float totalSpacing = (count - 1) * CARD_SPACING;
            float newWidth = count * Cards[0].GetWidth() - totalSpacing;

            _rectTransform.sizeDelta = new Vector2(newWidth, _rectTransform.sizeDelta.y); // Base width plus spacing
        }

        void ManageTurnStateChanged(TurnState turnState, PlayerBase player)
        {
            if(parentPlayer is SelfPlayer)
            {
                if(turnState == TurnState.PlayPhase || turnState == TurnState.NoBuenoWindowPhase)
                {
                    _cards.ForEach(card => card.EnableInteraction());
                    return;
                }
            }
            _cards.ForEach(card => card.DisableInteraction());
        }

        public void PickCardBeforeDrag(CardBase card)
        { 
            RemoveCard(card);
        }
        public void ReturnCardOnNoTarget(CardBase card)
        {
            card.transform.SetSiblingIndex(transform.childCount -1);
            AddCard(card);
        }
    }
}

