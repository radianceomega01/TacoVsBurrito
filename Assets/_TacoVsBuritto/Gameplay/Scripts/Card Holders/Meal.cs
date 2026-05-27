using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace TacoVsBurrito
{

    // ----------------------------------------------------------
    //  Meal  (the face-up cards in front of a player)
    // ----------------------------------------------------------
    public class Meal: MonoBehaviour, ICardDropTarget
    {
        [SerializeField] Transform cardsTransform;
        [SerializeField] TextMeshProUGUI scoreTxt;

        private const float CARD_SPACING = 5f;
        private const float CARD_SCALE = 0.8f;
        private PlayerBase parentPlayer;

        public MealType Type { get; }
        private List<CardBase> _cards;
        private CardBase currentGlowingCard;

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
            GameEvents.OnCraftyCrowAction += ManageCraftyCrowAction;
            GameEvents.OnCardClickedForActionTarget += ManageCardClickedForCraftyCrow;
            GameEvents.OnActionResolved += ManageActionResolved;
        }
        void OnDestroy()
        {
            GameEvents.OnTurnStateChanged -= ManageTurnStateChanged;
            GameEvents.OnCraftyCrowAction -= ManageCraftyCrowAction;
            GameEvents.OnCardClickedForActionTarget -= ManageCardClickedForCraftyCrow;
            GameEvents.OnActionResolved -= ManageActionResolved;
        }

        void Start()
        {
            parentPlayer = GetComponentInParent<PlayerBase>();
            UpdateScore();
        }
        // ---- Mutations ----

        /// Add a card (Ingredient, TummyAche, or HotSauceBoss) to this meal.
        public void AddCard(CardBase card)
        {
            if (card is not IMealTypeAction)
                return;
            _cards.Add(card);
            card.ChangeScale(CARD_SCALE);
            card.DisableInteraction();
            card.ToggleBackFace(false);
            ArrangeCardsAnimated();

            if (card is HotSauceBossCard) HotSauceBossCardCount++;
            if (card is IngredientCardBase) IngredientCardCount++;

            UpdateScore();
        }

        /// Remove a specific card (used by Crafty Crow).
        public void RemoveCard(CardBase card)
        {
            _cards.Remove(card);
            UpdateScore();
            ArrangeCardsAnimated();
        }

        /// Remove and return all cards (Health Inspector / Order Envy).
        public List<CardBase> TakeAll()
        {
            var all = new List<CardBase>(_cards);
            _cards.Clear();
            UpdateScore();
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
                if (card is IngredientCardBase @ingredientCard)
                {
                    score = @ingredientCard.GetModifiedMealScore(score);
                }
                else if(card is TummyAcheCard @tummyAcheCard)
                {
                    score = @tummyAcheCard.GetModifiedMealScore(score);
                }
                else if (card is HotSauceBossCard @hotSauceBossCard)
                {
                    hotSauceBossCards.Add(@hotSauceBossCard);
                }
            }
            foreach (var card in hotSauceBossCards)
            {
                score = card.GetModifiedMealScore(score);
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
            return card is IMealTypeAction;
        }
        public void DropCardAfterDrag(CardBase card)
        {
            AddCard(card);
            GameEvents.OnTurnEnded?.Invoke(GameManager.Instance.CurrentPlayer);
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

        void ManageTurnStateChanged(TurnState turnState, PlayerBase player)
        {
            //Deactivate glow if action on card was canceled due to no bueno
            if(turnState == TurnState.DrawPhase && currentGlowingCard != null)
            {
                currentGlowingCard?.DeactivateGlow();
                currentGlowingCard = null;
            }
        }
        void ManageCraftyCrowAction()
        {
            if(GameManager.Instance.CurrentPlayer != parentPlayer)
            {
                _cards.ForEach(card =>
                {
                    card.EnableInteraction();
                    card.ToggleInteractionType();
                    card.ActivateGlow();
                });
            }
        }
        private void ToggleInteractionType(PlayerBase player)
        {
            if(player != parentPlayer)
            {
                _cards.ForEach(card =>
                {
                    card.ToggleInteractionType();
                });
            }
        }
        private void DisableInteraction(PlayerBase player)
        {
            if(player != parentPlayer)
            {
                _cards.ForEach(card =>
                {
                    card.DisableInteraction();
                });
            }
        }

        private void ManageCardClickedForCraftyCrow(CardBase card)
        {
            currentGlowingCard = card;
            _cards.ForEach(c => 
            {
                if(c != card) c.DeactivateGlow();
            });
            DisableInteraction(GameManager.Instance.CurrentPlayer); 

            if(_cards.Contains(card))
                GameEvents.OnCraftyCrowActionTargeted?.Invoke(new TargetTypeContext(GameManager.Instance.CurrentPlayer, parentPlayer, card));
        }

        void ManageActionResolved(ActionCardBase actionCard)
        {
            if(actionCard is not CraftyCrowCard)
                return;
            ToggleInteractionType(GameManager.Instance.CurrentPlayer);
            currentGlowingCard?.DeactivateGlow();
            currentGlowingCard = null;    
        }
        
        void UpdateScore() => scoreTxt.SetText(CalculateScore().ToString());
    }
    public enum MealType
    {
        Taco,
        Burrito
    }
    
}