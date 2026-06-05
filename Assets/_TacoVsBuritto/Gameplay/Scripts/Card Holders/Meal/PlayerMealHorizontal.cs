using UnityEngine;

namespace TacoVsBurrito
{
    public class PlayerMealHorizontal : PlayerMealBase
    {
        protected override float CARD_SPACING => 6f;
        protected override float CARD_SCALE => 0.8f;

        protected override void RearrangeCards()
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
    }
}
