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
            if(! IsSufficientCardForCraftyCrow())
            {
                GameEvents.OnLogMessage("CraftyCrow cancelled due to insufficient cards!");
                GameEvents.OnTurnEnded?.Invoke(GameManager.Instance.CurrentPlayer);
                return;
            }
            
            GameEvents.OnLogMessage("Pick an Enemy Card from Meal!");
            GameEvents.OnCraftyCrowAction?.Invoke();
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

        bool IsSufficientCardForCraftyCrow()
        {
            bool value = false;
            foreach(PlayerBase player in GameManager.Instance.Players)
            {
                if(player is SelfPlayer)
                    continue;

                if(player.Meal.Cards.Count > 0)
                {
                    value = true;
                    break;
                }
            }
            return value;
        }
    }
}
