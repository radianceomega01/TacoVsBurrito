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
            //GameEvents.OnStartNoBuenoInterruptWindow?.Invoke(this);
            GameEvents.OnOrderEnvyAction?.Invoke(GameplayManager.Instance.CurrentPlayer);
        }

        public void OnActionTargeted(TargetTypeContext targetTypeContext)
        {
            this.targetTypeContext = targetTypeContext;
            ResolveAction();   
        }

        public void ResolveAction()
        {
            GameplayManager.Instance.GetTrashPile().Trash(this);
            resolver.ResolveOrderEnvy(targetTypeContext.caster, targetTypeContext.victim);
            targetTypeContext = default;
        }
        public override TurnState GetStateOnPlayed() => TurnState.ActionTargetPhase;
    }
}
