using UnityEngine;

namespace TacoVsBurrito
{
    public class PlayerMealVertical : PlayerMealBase
    {
        protected override float CARD_SPACING => 5f;
        protected override float CARD_SCALE => 1.2f;

        protected override void RearrangeCards()
        {
            int count = _cards.Count;
            if (count == 0) return;

            Vector3 basePos = cardsTransform.position;
            Vector3 up = cardsTransform.up;

            int stackCount = _cardStacks.Count;
            if (stackCount == 0) return;

            float totalHeight = (stackCount - 1) * CARD_SPACING;
            float startOffset = -totalHeight / 2f;

            for (int i = 0; i < stackCount; i++)
            {
                MealCardStack stack = _cardStacks[i];

                float offset = startOffset + i * CARD_SPACING;

                Vector3 stackPos = basePos - up * offset;

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
