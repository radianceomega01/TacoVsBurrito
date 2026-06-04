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
            _cards.Add(c);
            c.ToggleBackFace(true);
            c.DisableInteraction();
            c.ToggleInteractionType(InteractionType.Click);
            ArrangeCard(c);
            UpdateCountTxt();
        }

        public override void AddCardWithoutArranging(CardBase c)
        {
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
