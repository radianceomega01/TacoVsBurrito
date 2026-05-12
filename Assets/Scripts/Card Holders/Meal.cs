using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace TacoVsBurrito
{

    // ----------------------------------------------------------
    //  Meal  (the face-up cards in front of a player)
    // ----------------------------------------------------------
    public class Meal: MonoBehaviour, ICardDropTarget
    {
        [SerializeField] Transform cardsTransform;

        private const float CARD_SPACING = 4f;
        private PlayerBase parentPlayer;

        public MealType Type { get; }
        private List<CardBase> _cards;

        public IReadOnlyList<CardBase> Cards => _cards;
        public PlayerBase ParentPlayer => parentPlayer;

        public Meal(MealType type) { Type = type; }
        public int HotSauceBossCardCount { get; private set; }
        public int IngredientCardCount { get; private set; }
        public Transform ParentTransform => transform.parent;

        void Awake()
        {
            _cards = new();
            GameEvents.OnTurnStateChanged += ManageTurnStateChanged;
            GameEvents.OnCraftyCrowActionByPlayer += ManageCraftyCrowAction;
            GameEvents.OnCraftyCrowActionResolved += ManageCraftyCrowActionResolved;
            GameEvents.OnCardClickedForCraftCrow += ManageCardClickedForCraftCrow;
        }
        void OnDestroy()
        {
            GameEvents.OnTurnStateChanged -= ManageTurnStateChanged;
            GameEvents.OnCraftyCrowActionByPlayer -= ManageCraftyCrowAction;
            GameEvents.OnCraftyCrowActionResolved -= ManageCraftyCrowActionResolved;
            GameEvents.OnCardClickedForCraftCrow -= ManageCardClickedForCraftCrow;
        }

        void Start()
        {
            parentPlayer = GetComponentInParent<PlayerBase>();
        }
        // ---- Mutations ----

        /// Add a card (Ingredient, TummyAche, or HotSauceBoss) to this meal.
        public void AddCard(CardBase card)
        {
            if (card is not HotSauceBossCard &&
                card is not IngredientCardBase &&
                card is not TummyAcheCard)
                return;
            _cards.Add(card);
            card.DisableInteraction();
            ArrangeCardsAnimated();

            if (card is HotSauceBossCard) HotSauceBossCardCount++;
            if (card is IngredientCardBase) IngredientCardCount++;
        }

        /// Remove a specific card (used by Crafty Crow).
        public void RemoveCard(CardBase card)
        {
            _cards.Remove(card);
            ArrangeCardsAnimated();
        }

        /// Remove and return all cards (Health Inspector / Order Envy).
        public List<CardBase> TakeAll()
        {
            var all = new List<CardBase>(_cards);
            _cards.Clear();
            return all;
        }

        public void ChangeParentAndPosition(Transform parentTransform, Vector3 position)
        {
            transform.DOMove(position, 0.5f);
            transform.SetParent(parentTransform);
            transform.DORotate(Vector3.zero, 0.5f);
        }

        // ---- Scoring ----

        /// Returns the final score according to official rules:
        /// (Σ ingredient points + Σ tummy ache points) × multiplier
        /// Multiplier: 0 HotSauceBoss = ×1, 1 = ×2, 2+ = ×4
        public int CalculateScore()
        {
            int score = 0;
            List<HotSauceBossCard> hotSauceBossCards = new();

            foreach (var card in _cards)
            {
                if (card is IngredientCardBase || card is TummyAcheCard)
                {
                    card.GetModifiedMealScore(score);
                }
                else if (card is HotSauceBossCard)
                {
                    hotSauceBossCards.Add((HotSauceBossCard)card);
                }
            }
            foreach (var card in hotSauceBossCards)
            {
                card.GetModifiedMealScore(score);
            }
            return score;
        }

        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"  [{Type} Meal | Score: {CalculateScore()}]");
            foreach (var c in _cards) sb.AppendLine($"    {c}");
            return sb.ToString();
        }

        public bool CanDrop(CardBase card)
        {
            if (card is ActionCardBase)
            {
                if (card is HotSauceBossCard || card is TummyAcheCard)
                    return true;
                else
                    return false;
            }
            return true;
        }
        public void DropCardAfterDrag(CardBase card)
        {
            AddCard(card);
            GameManager.Instance.OnCardPlacedAfterDrawn();
        }

        void ArrangeCardsAnimated()
        {
            int count = _cards.Count;
            if (count == 0) return;

            float totalWidth = (count - 1) * CARD_SPACING;
            float startOffset = -totalWidth / 2f;
            for (int i = 0; i < count; i++)
            {
                float offset = startOffset + i * CARD_SPACING;
                Vector3 targetPos = cardsTransform.position + cardsTransform.right * offset;

                _cards[i].ChangePosition(targetPos);
                _cards[i].ChangeParent(cardsTransform);

            }
        }

        void ManageTurnStateChanged(TurnState turnState, PlayerBase player){}
        void ManageCraftyCrowAction(PlayerBase player)
        {
            if(player != parentPlayer)
            {
                _cards.ForEach(card =>
                {
                    card.EnableInteraction();
                    card.ToggleInteractionType();
                });
            }
        }
        private void ManageCraftyCrowActionResolved(PlayerBase player)
        {
            if(player != parentPlayer)
            {
                _cards.ForEach(card =>
                {
                    card.DisableInteraction();
                    card.ToggleInteractionType();
                });
            }
        }

        private void ManageCardClickedForCraftCrow(CardBase card)
        {
            if(_cards.Contains(card))
            {
                RemoveCard(card);
                GameEvents.OnCraftyCrowActionTargeted?.Invoke(GameManager.Instance.CurrentPlayer, parentPlayer, card);
            }
        }
        
    }
    public enum MealType
    {
        Taco,
        Burrito
    }
    
}