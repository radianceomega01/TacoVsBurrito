using System;
using TacoVsBurrito;
using UnityEngine;

namespace TacoVsBurrito
{
    public class CraftyCrowCard : ActionCardBase, ITargetTypeAction
    {
        TargetTypeContext targetTypeContext;
        
        public override void ExecuteAction()
        {
            GameEvents.OnCraftyCrowActionByPlayer?.Invoke(GameManager.Instance.CurrentPlayer);
        }


        public void OnActionTargeted(TargetTypeContext targetTypeContext)
        {
            this.targetTypeContext = targetTypeContext;
            GameEvents.OnStartNoBuenoInterruptWindow?.Invoke(this);
        }

        public void ResolveAction()
        {
            resolver.ResolveCraftyCrow(targetTypeContext.caster, targetTypeContext.victim, targetTypeContext.cardTargeted);
            targetTypeContext = default;
        }
        public override TurnState GetStateOnTrashed() => TurnState.ActionTargetPhase;
    }
}
