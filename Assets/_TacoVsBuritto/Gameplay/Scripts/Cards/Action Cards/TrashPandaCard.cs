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
            await Task.Delay(EXECUTION_DEALY_IN_MS);
            Dictionary<CardBase, int> pileCards = GameManager.Instance.GetTrashPile().RetrieveFromTrash();

            if(pileCards.Count == 0)
            {
                GameEvents.OnLogMessage("TrashPanda cancelled due to insufficient cards!");
                GameManager.Instance.GetTrashPile().Trash(this);
                GameEvents.OnTurnEnded?.Invoke(GameManager.Instance.CurrentPlayer);
                return;
            }
            GameEvents.OnTrashPandaAction?.Invoke(GameManager.Instance.GetTrashPile().RetrieveFromTrash());
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
            resolver.ResolveTrashPanda(targetTypeContext.caster, targetTypeContext.cardTargeted);
            targetTypeContext = default;
        }
        public override TurnState GetStateOnTrashed() => TurnState.ActionTargetPhase;
    }
}
