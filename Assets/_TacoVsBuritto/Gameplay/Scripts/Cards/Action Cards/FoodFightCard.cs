using System;
using TacoVsBurrito;
using UnityEngine;

namespace TacoVsBurrito
{
    public class FoodFightCard : ActionCardBase, ITargetTypeAction
    {
        public override void ExecuteAction()
        {
            //GameEvents.OnStartNoBuenoInterruptWindow?.Invoke(this);
            GameEvents.OnFoodFightAction?.Invoke();
        }

        public void OnActionTargeted(TargetTypeContext targetTypeContext)
        {
            GameManager.Instance.GetTrashPile().Trash(this);
            resolver.ResolveFoodFight(targetTypeContext.caster, targetTypeContext.cardTargeted);
        }

        public void ResolveAction()
        {
            GameManager.Instance.GetTrashPile().Trash(this);
            GameEvents.OnFoodFightAction?.Invoke();
        }
        
        public override TurnState GetStateOnTrashed() => TurnState.NoBuenoWindowPhase;
    }
}
