using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using System;

namespace TacoVsBurrito
{
    // ----------------------------------------------------------
    //  Hand  (private cards held by a player)
    // ----------------------------------------------------------
    public class SelfHand : HandBase, ICardPickupTarget
    {
        private RectTransform _rectTransform;

        private const float CARD_SPACING = 7f;
        private const float CARD_SCALE = 1.4f;
        private const float ARC_RADIUS = 50f;
        private const float ARC_ANGLE = 30f;

        protected override void Awake()
        {
            base.Awake();
            _rectTransform = GetComponent<RectTransform>();
        }

        public override void RemoveCard(CardBase c)
        {
            base.RemoveCard(c);
            ArrangeCardsAnimated();
        }

        public override void AddCard(CardBase c)
        {
            _cards.Add(c);
            c.ScaleTo(CARD_SCALE);
            c.ToggleBackFace(false);
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

            // If only one card, keep centered
            if (count == 1)
            {
                _cards[0].ChangePosition(transform.position);
                _cards[0].ChangeRotation(Quaternion.identity);
                _cards[0].ChangeParent(transform);
                return;
            }

            float angleStep = ARC_ANGLE / (count - 1);
            float startAngle = -ARC_ANGLE / 2f;

            for (int i = 0; i < count; i++)
            {
                float angle = startAngle + angleStep * i;

                // Convert angle to radians
                float rad = angle * Mathf.Deg2Rad;

                // Arc position
                Vector3 offset = new Vector3(
                    Mathf.Sin(rad) * ARC_RADIUS,
                    Mathf.Cos(rad) * ARC_RADIUS - ARC_RADIUS,
                    0f
                );

                Vector3 targetPos = transform.position + offset;

                // Card rotation
                Quaternion targetRot = Quaternion.Euler(0f, 0f, -angle);

                _cards[i].ChangePosition(targetPos);
                _cards[i].ChangeRotation(targetRot);
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

        protected override void ManageTurnStateChanged(TurnState turnState, PlayerBase player)
        {
            if (turnState == TurnState.PlayPhase || turnState == TurnState.NoBuenoWindowPhase)
            {
                _cards.ForEach(card => card.EnableInteraction());
                return;
            }
        }

        public void PickCardBeforeDrag(CardBase card)
        {
            RemoveCard(card);
        }
        public void ReturnCardOnNoTarget(CardBase card)
        {
            card.transform.SetSiblingIndex(transform.childCount - 1);
            AddCard(card);
        }
    }
}

