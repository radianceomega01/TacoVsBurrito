using System;
using TacoVsBurrito;
using UnityEngine;

namespace TacoVsBurrito
{
    public class FoodFightCard : ActionCardBase
    {
        PlayerBase winner;

        protected override void Awake() {
            base.Awake();
            GameEvents.OnCardsPileCardTargeted += ManageCardTargeted;
            GameEvents.OnFoodFightFinished += ManageFoodFightFinished;
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            GameEvents.OnCardsPileCardTargeted -= ManageCardTargeted;
            GameEvents.OnFoodFightFinished -= ManageFoodFightFinished;
        }

        public override void ExecuteAction()
        {
            GameEvents.OnFoodFightAction?.Invoke();
            GameEvents.OnLogMessage?.Invoke($"🍽 FOOD FIGHT!");
        }

        void ManageCardTargeted(CardBase selectedCard)
        {
            resolver.ResolveFoodFight(winner, selectedCard);
        }
        void ManageFoodFightFinished(PlayerBase winner)
        {
            this.winner = winner;
        }

        public override TurnState GetStateOnTrashed() => TurnState.ActionTargetPhase;
    }
}
