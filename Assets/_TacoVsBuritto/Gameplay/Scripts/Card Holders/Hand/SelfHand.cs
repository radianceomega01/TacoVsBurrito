using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using System;
using System.Threading.Tasks;

namespace TacoVsBurrito
{
    // ----------------------------------------------------------
    //  Hand  (private cards held by a player)
    // ----------------------------------------------------------
    public class SelfHand : PlayerHandBase, ICardPickupTarget
    {
        private const float CARD_SCALE = 1.4f;
        private const float MIN_ARC_RADIUS = 5f;
        private const float MAX_ARC_RADIUS = 50f;
        private const float ARC_RADIUS_GROWTH = 10f;
        private const float ARC_ANGLE = 30f;
        private const int CARD_ARRANGE_DEALY_IN_MS = 500;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            GameEvents.OnCardsDistributed += ArrangeCardsAfterDistribution;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            GameEvents.OnCardsDistributed -= ArrangeCardsAfterDistribution;
        }

        async void ArrangeCardsAfterDistribution()
        {
            await Task.Delay(CARD_ARRANGE_DEALY_IN_MS);
            _cards.ForEach(card => card.ToggleBackFace(false));
            GameEvents.OnCardFlippedSFX?.Invoke();
            ArrangeCardsAnimated();
        }

        public override void RemoveCard(CardBase c)
        {
            base.RemoveCard(c);
            ArrangeCardsAnimated();
        }

        public override void AddCard(CardBase c)
        {
            _cards.Add(c);
            SetCurrentPickupTargetToCard(c);
            c.ChangeScale(CARD_SCALE);
            c.ToggleBackFace(false);
            c.ToggleInteractionType(InteractionType.Drag);
            ArrangeCardsAnimated();
            UpdateCountTxt();
        }
        public override void AddCardWithoutArranging(CardBase c)
        {
            _cards.Add(c);
            c.ChangeScale(CARD_SCALE);
            c.ToggleBackFace(true);
            c.ToggleInteractionType(InteractionType.Drag);
            SetCardInDefaultPlace(c);
            UpdateCountTxt();
            c.DisableInteraction();
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

            if (count == 1)
            {
                SetCardInDefaultPlace(_cards[0]);
                return;
            }

            float angleStep = ARC_ANGLE / (count - 1);
            float startAngle = -ARC_ANGLE / 2f;

            float dynamicRadius =
                MIN_ARC_RADIUS + (count - 1) * ARC_RADIUS_GROWTH;    

            dynamicRadius = Mathf.Min(dynamicRadius, MAX_ARC_RADIUS);

            for (int i = 0; i < count; i++)
            {
                float angle = startAngle + angleStep * i;

                float rad = angle * Mathf.Deg2Rad;

                Vector3 offset = new Vector3(
                    Mathf.Sin(rad) * dynamicRadius,
                    Mathf.Cos(rad) * dynamicRadius - dynamicRadius,
                    0f
                );

                Vector3 targetPos = transform.position + offset;

                Quaternion targetRot = Quaternion.Euler(0f, 0f, -angle);

                _cards[i].ChangePosition(targetPos);
                _cards[i].ChangeRotation(targetRot);
                _cards[i].ChangeParent(transform);
            }
        }

        void SetCardInDefaultPlace(CardBase card)
        {
            card.ChangePosition(transform.position);
            card.ChangeRotation(Quaternion.identity);
            card.ChangeParent(transform);
        }

        protected override void ManageTurnStateChanged(TurnState turnState, PlayerBase player)
        {
            if (GameManager.Instance.CurrentPlayer is SelfPlayer && turnState == TurnState.PlayPhase)
            {
                _cards.ForEach(card => card.EnableInteraction());
            }
            else if (GameManager.Instance.CurrentPlayer is not SelfPlayer && turnState == TurnState.NoBuenoWindowPhase)
            {
                _cards.ForEach(card =>
                {
                    if (card is NoBuenoCard) card.EnableInteraction();
                });
            }
            else
            {
                _cards.ForEach(card => card.DisableInteraction());
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

        public void SetCurrentPickupTargetToCard(CardBase card) => card.SetCurrentPickupTarget(this);
    }
}

