using TacoVsBurrito;
using UnityEngine;

namespace TacoVsBurrito
{
    public class FoodFightCard : ActionCardBase
    {
        public override void ExecuteAction()
        {
            GameEvents.OnFoodFightAction?.Invoke();
        }
        public override TurnState GetStateOnTrashed() => TurnState.NoBuenoWindowPhase;
    }
}
