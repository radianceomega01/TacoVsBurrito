using System;
using TacoVsBurrito;
using UnityEngine;

namespace TacoVsBurrito
{
    public class FoodFightCard : ActionCardBase, ITargetTypeAction
    {
        public override void ExecuteAction()
        {
            GameEvents.OnStartNoBuenoInterruptWindow?.Invoke(this);
        }

        public override TurnState GetStateOnTrashed() => TurnState.NoBuenoWindowPhase;

        public void OnActionTargeted(TargetTypeContext targetTypeContext)
        {
            resolver.ResolveFoodFight(targetTypeContext.caster, targetTypeContext.cardTargeted);
        }

        public void ResolveAction()
        {
            GameEvents.OnFoodFightAction?.Invoke();
            GameEvents.OnLogMessage?.Invoke($"🍽 FOOD FIGHT!");
        }
    }
}
