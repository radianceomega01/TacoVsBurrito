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
        CardBase choosenTrashCard; 

        protected override void Awake() {
            base.Awake();
            GameEvents.OnCardsPileCardTargeted += ManageTrashPandaTargeted;
            GameEvents.OnTurnStateChanged += ManageTurnStateChanged;
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            GameEvents.OnCardsPileCardTargeted -= ManageTrashPandaTargeted;
            GameEvents.OnTurnStateChanged -= ManageTurnStateChanged;
        }

        public override async void ExecuteAction()
        {
            isActiveOnTrashPile = true;

            await Task.Delay(700);
            trashPile = GameManager.Instance.GetTrashPile();
            GameEvents.OnTrashPandaAction?.Invoke(trashPile.RetrieveFromTrash());
        }

        private void ManageTrashPandaTargeted(CardBase card)
        {
            if(!isActiveOnTrashPile)
                return;

            choosenTrashCard = card;
            GameEvents.OnStartNoBuenoInterruptWindow?.Invoke(this);
        }
        void ManageTurnStateChanged(TurnState state, PlayerBase player)
        {
            if(state != TurnState.ActionResolvePhase || !isActiveOnTrashPile)
                return;
            resolver.ResolveTrashPanda(GameManager.Instance.CurrentPlayer, choosenTrashCard);
            ResetParams();   
        }
        void ResetParams()
        {
            isActiveOnTrashPile = false;
            choosenTrashCard = null;
        }
        public override TurnState GetStateOnTrashed() => TurnState.ActionTargetPhase;
    }
}
