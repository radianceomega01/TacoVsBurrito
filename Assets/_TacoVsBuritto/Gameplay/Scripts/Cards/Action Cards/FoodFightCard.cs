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

        public void OnActionTargeted(TargetTypeContext targetTypeContext)
        {
            resolver.ResolveFoodFight(targetTypeContext.caster, targetTypeContext.cardTargeted);
        }

        public void ResolveAction()
        {
            GameEvents.OnFoodFightAction?.Invoke();
        }
        
        public override TurnState GetStateOnTrashed() => TurnState.NoBuenoWindowPhase;
    }
}
