using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace TacoVsBurrito
{

    // ----------------------------------------------------------
    //  Meal  (the face-up cards in front of a player)
    // ----------------------------------------------------------
    public class Meal : MonoBehaviour, ICardDropTarget
    {
        [SerializeField] Transform cardsTransform;
        [SerializeField] TextMeshProUGUI scoreTxt;

        private const float CARD_SPACING = 6.2f;
        private const float CARD_SCALE = 0.8f;
        private PlayerBase parentPlayer;

        public MealType Type { get; }
        private List<CardBase> _cards; // All meal cards
        private List<MealCardStack> _cardStacks; // For stacking cards of same meal value or type
        private CardBase currentGlowingCard;

        public IReadOnlyList<CardBase> Cards => _cards;
        public PlayerBase ParentPlayer => parentPlayer;

        public int HotSauceBossCardCount { get; private set; }
        public int IngredientCardCount { get; private set; }

        void Awake()
        {
            _cards = new();
            _cardStacks = new();
        }
        void OnEnable()
        {
            GameEvents.OnTurnStateChanged += ManageTurnStateChanged;
            GameEvents.OnCraftyCrowAction += ManageCraftyCrowAction;
            GameEvents.OnCardClickedForActionTarget += ManageCardClickedForCraftyCrow;
            GameEvents.OnActionResolved += ManageActionResolved;
        }
        void OnDisable()
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

            card.ChangeParent(cardsTransform);
            _cards.Add(card);
            AddCardInStack(card);

            card.ChangeScale(CARD_SCALE);
            card.transform.SetAsLastSibling();
            card.DisableInteraction();
            card.ToggleBackFace(false);
            card.ChangeRotation(Quaternion.identity);
            card.ToggleInteractionType(InteractionType.Click);

            if (card is HotSauceBossCard) HotSauceBossCardCount++;
            if (card is IngredientCardBase) IngredientCardCount++;

            UpdateScore();
        }

        void AddCardInStack(CardBase card)
        {
            MealCardStack stack = FindMatchingStack(card);

            if (stack == null)
            {
                stack = new MealCardStack();
                _cardStacks.Add(stack);
            }
            stack.Cards.Add(card);
            RearrangeCards();
        }

        /// Remove a specific card (used by Crafty Crow).
        public void RemoveCard(CardBase card)
        {
            // Remove from flat gameplay list
            _cards.Remove(card);
            RemoveCardFromStack(card);
            // Update counters
            if (card is HotSauceBossCard)
                HotSauceBossCardCount--;

            if (card is IngredientCardBase)
                IngredientCardCount--;

            UpdateScore();
        }

        void RemoveCardFromStack(CardBase card)
        {
            MealCardStack stack =
            _cardStacks.Find(s => s.Cards.Contains(card));

            if (stack != null)
            {
                stack.Cards.Remove(card);

                if (stack.Cards.Count == 0)
                {
                    _cardStacks.Remove(stack);
                }
            }
            RearrangeCards();   
        }

        /// Remove and return all cards (Health Inspector / Order Envy).
        public List<CardBase> TakeAll()
        {
            var all = new List<CardBase>(_cards);
            _cards.Clear();
            DisableCountForSimilarCards();
            _cardStacks.Clear();
            UpdateScore();
            return all;
        }

        void DisableCountForSimilarCards()
        {
            foreach (var stack in _cardStacks)
            {
                stack.TopCard.DisableCountOnSimilarCards();
            }
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
                else if (card is TummyAcheCard @tummyAcheCard)
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

        void RearrangeCards()
        {
            int count = _cards.Count;
            if (count == 0) return;

            Vector3 basePos = cardsTransform.position;
            Vector3 right = cardsTransform.right;

            // STACK-BASED LAYOUT (optimized system)

            int stackCount = _cardStacks.Count;
            if (stackCount == 0) return;

            float totalWidth = (stackCount - 1) * CARD_SPACING;
            float startOffset = -totalWidth / 2f;

            for (int i = 0; i < stackCount; i++)
            {
                MealCardStack stack = _cardStacks[i];

                float offset = startOffset + i * CARD_SPACING;

                Vector3 stackPos = basePos + right * offset;

                int cardCount = stack.Cards.Count;

                for (int j = 0; j < cardCount; j++)
                {
                    CardBase card = stack.Cards[j];

                    card.ChangePosition(stackPos);
                }

                UpdateCountOnSimilarCardStack(stack);
            }
        }

        void UpdateCountOnSimilarCardStack(MealCardStack stack)
        {
            stack.TopCard.ShowCountOnSimilarCards(stack.Cards.Count);
            if (stack.Cards.Count > 1)
            {
                stack.Cards[^2].DisableCountOnSimilarCards();
            }
        }

        // void RearrangeCards()
        // {
        //     int count = _cards.Count;
        //     if (count == 0) return;

        //     float totalWidth = (count - 1) * CARD_SPACING;
        //     float startOffset = -totalWidth / 2f;
        //     for (int i = 0; i < count; i++)
        //     {
        //         float offset = startOffset + i * CARD_SPACING;
        //         Vector3 targetPos = cardsTransform.position + cardsTransform.right * offset;

        //         _cards[i].ChangePosition(targetPos);
        //         _cards[i].ChangeParent(cardsTransform);

        //     }
        // }

        MealCardStack FindMatchingStack(CardBase newCard)
        {
            foreach (var stack in _cardStacks)
            {
                CardBase existing = stack.TopCard;

                // Hot Sauce Boss stack
                if (existing is HotSauceBossCard && newCard is HotSauceBossCard)
                    return stack;

                // Tummy Ache stack
                if (existing is TummyAcheCard x &&
                    newCard is TummyAcheCard y &&
                    x.CardValue == y.CardValue)
                {
                    return stack;
                }

                // Ingredient value stack
                if (existing is IngredientCardBase a &&
                    newCard is IngredientCardBase b &&
                    a.CardValue == b.CardValue)
                {
                    return stack;
                }
            }

            return null;
        }

        void ManageTurnStateChanged(TurnState turnState, PlayerBase player)
        {
            //Deactivate glow if action on card was canceled due to no bueno
            if (turnState == TurnState.DrawPhase && currentGlowingCard != null)
            {
                currentGlowingCard?.DeactivateGlow();
                currentGlowingCard = null;
            }
        }
        void ManageCraftyCrowAction()
        {
            if (GameManager.Instance.CurrentPlayer is not SelfPlayer)
                return;
                
            if (GameManager.Instance.CurrentPlayer != parentPlayer)
            {
                _cards.ForEach(card =>
                {
                    card.EnableInteraction();
                    card.ActivateGlow();
                });
            }
        }
        private void DisableInteraction(PlayerBase player)
        {
            if (player != parentPlayer)
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
                if (c != card) c.DeactivateGlow();
            });
            DisableInteraction(GameManager.Instance.CurrentPlayer);

            if (_cards.Contains(card))
                GameEvents.OnCraftyCrowActionTargeted?.Invoke(new TargetTypeContext(GameManager.Instance.CurrentPlayer, parentPlayer, card));
        }

        void ManageActionResolved(ActionCardBase actionCard)
        {
            if (actionCard is not CraftyCrowCard)
                return;
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

    [Serializable]
    public class MealCardStack
    {
        public List<CardBase> Cards = new();
        public CardBase TopCard => Cards[^1];
    }

}