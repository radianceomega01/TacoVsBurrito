using TacoVsBurrito;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace TacoVsBurrito
{
    public class TrashPandaCard : ActionCardBase, ITargetTypeAction
    {
        TargetTypeContext targetTypeContext;

        public override async void ExecuteAction()
        {
            base.ExecuteAction();
            await Task.Delay(EXECUTION_DEALY_IN_MS);
            GameEvents.OnTrashPandaAction?.Invoke(GameplayManager.Instance.GetTrashPile().RetrieveFromTrash());
        }

        public void OnActionTargeted(TargetTypeContext targetTypeContext)
        {
            this.targetTypeContext = targetTypeContext;
            //GameEvents.OnStartNoBuenoInterruptWindow?.Invoke(this);
            ResolveAction();
        }

        public void ResolveAction()
        {
            GameplayManager.Instance.GetTrashPile().Trash(this);
            resolver.ResolveTrashPanda(targetTypeContext.caster, targetTypeContext.cardTargeted);
            targetTypeContext = default;
        }
        public override bool CanExecuteAction()
        {
            Dictionary<CardBase, int> pileCards = GameplayManager.Instance.GetTrashPile().RetrieveFromTrash();
            if(pileCards.Count == 0)
            {
                GameEvents.OnLogMessage("TrashPanda cancelled due to insufficient cards!");
                return false;
            }
            return true;
        }
        public override TurnState GetStateOnPlayed() => TurnState.ActionTargetPhase;
    }
}
