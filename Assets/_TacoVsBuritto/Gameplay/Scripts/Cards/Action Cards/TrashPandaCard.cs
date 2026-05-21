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
            await Task.Delay(700);
            GameEvents.OnTrashPandaAction?.Invoke(GameManager.Instance.GetTrashPile().RetrieveFromTrash());
        }

        public void OnActionTargeted(TargetTypeContext targetTypeContext)
        {
            this.targetTypeContext = targetTypeContext;
            GameEvents.OnStartNoBuenoInterruptWindow?.Invoke(this);
        }

        public void ResolveAction()
        {
            resolver.ResolveTrashPanda(targetTypeContext.caster, targetTypeContext.cardTargeted);
            targetTypeContext = default;
        }
        public override TurnState GetStateOnTrashed() => TurnState.ActionTargetPhase;
    }
}
