using System.Threading.Tasks;
using UnityEngine;

namespace TacoVsBurrito
{
    public class OpponentHand : PlayerHandBase
    {
        private const float CARD_SCALE = 0.4f;
        private const int CARD_SQUEEZED_DELAY_IN_MS = 200;

        public override void AddCard(CardBase c)
        {
            base.AddCard(c);
            c.ToggleBackFace(true);
            c.DisableInteraction();
            c.ToggleInteractionType(InteractionType.Click);
            ArrangeCard(c);
        }

        public override void AddCardWithoutArranging(CardBase c)
        {
            base.AddCardWithoutArranging(c);
            AddCard(c);
        }

        async void ArrangeCard(CardBase card)
        {
            card.ChangePosition(transform.position);
            card.ChangeRotation(Quaternion.identity);
            card.ChangeParent(transform);
            card.ChangeScale(CARD_SCALE);

            await Task.Delay(CARD_SQUEEZED_DELAY_IN_MS);
            card.ChangeScale(0);
        }
    }
}
