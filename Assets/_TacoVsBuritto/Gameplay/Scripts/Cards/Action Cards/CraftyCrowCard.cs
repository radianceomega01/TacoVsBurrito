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
            GameEvents.OnLogMessage("Pick an Enemy Card from Meal!");
            GameEvents.OnCraftyCrowAction?.Invoke();
        }


        public void OnActionTargeted(TargetTypeContext targetTypeContext)
        {
            this.targetTypeContext = targetTypeContext;
            //GameEvents.OnStartNoBuenoInterruptWindow?.Invoke(this);
            ResolveAction();
        }

        public void ResolveAction()
        {
            GameManager.Instance.GetTrashPile().Trash(this);
            resolver.ResolveCraftyCrow(targetTypeContext.caster, targetTypeContext.victim, targetTypeContext.cardTargeted);
            targetTypeContext = default;
        }

        public override bool CanExecuteAction()
        {
            if(! IsSufficientCardForCraftyCrow())
            {
                GameEvents.OnLogMessage("CraftyCrow cancelled due to insufficient cards!");
                return false;
            }
            return true;
        }
        public override TurnState GetStateOnTrashed() => TurnState.ActionTargetPhase;

        bool IsSufficientCardForCraftyCrow()
        {
            bool value = false;
            foreach(PlayerBase player in GameManager.Instance.Players)
            {
                if(player == GameManager.Instance.CurrentPlayer)
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
