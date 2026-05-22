using TacoVsBurrito;
using UnityEngine;

namespace TacoVsBurrito
{
    public class OrderEnvyCard : ActionCardBase, ITargetTypeAction
    {
        TargetTypeContext targetTypeContext;
        public override void ExecuteAction()
        {
            GameEvents.OnLogMessage("Pick an Enemy to swap Cards!");
            GameEvents.OnOrderEnvyAction?.Invoke(GameManager.Instance.CurrentPlayer);
        }

        public void OnActionTargeted(TargetTypeContext targetTypeContext)
        {
            this.targetTypeContext = targetTypeContext;
            GameEvents.OnStartNoBuenoInterruptWindow?.Invoke(this);
        }

        public void ResolveAction()
        {
            resolver.ResolveOrderEnvy(targetTypeContext.caster, targetTypeContext.victim);
            targetTypeContext = default;
        }
        public override TurnState GetStateOnTrashed() => TurnState.ActionTargetPhase;
    }
}
