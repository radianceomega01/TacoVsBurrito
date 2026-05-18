using TacoVsBurrito;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace TacoVsBurrito
{
    public class TrashPandaCard : ActionCardBase
    {
        TrashPile trashPile;
        protected override void Awake() {
            base.Awake();
            GameEvents.OnTrashPandaActionTargeted += ManageTrashPandaTargeted;
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            GameEvents.OnTrashPandaActionTargeted -= ManageTrashPandaTargeted;
        }

        public override async void ExecuteAction()
        {
            await Task.Delay(700);
            trashPile = GameManager.Instance.GetTrashPile();
            GameEvents.OnTrashPandaAction?.Invoke(trashPile.RetrieveFromTrash());
            // if (card != null) // more than just the Trash Panda itself
            // {
            //     resolver.ResolveTrashPanda(GameManager.Instance.CurrentPlayer, card);
            // }
            // else
            // {
            //     GameEvents.OnTurnEnded?.Invoke(GameManager.Instance.CurrentPlayer); // skip play phase 
            // }
        }
        private void ManageTrashPandaTargeted(CardBase card)
        {
            resolver.ResolveTrashPanda(GameManager.Instance.CurrentPlayer, card);
        }
        public override TurnState GetStateOnTrashed() => TurnState.ActionTargetPhase;
    }
}
