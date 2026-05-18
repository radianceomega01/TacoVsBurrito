using System;
using TacoVsBurrito;
using UnityEngine;

namespace TacoVsBurrito
{
    public class CraftyCrowCard : ActionCardBase
    {
        PlayerBase caster;
        PlayerBase victim;
        CardBase victimCard;
        protected override void Awake() {
            base.Awake();
            GameEvents.OnCraftyCrowActionTargeted += ManageActionTargeted;
            GameEvents.OnTurnStateChanged += ManageTurnStateChanged;
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            GameEvents.OnCraftyCrowActionTargeted -= ManageActionTargeted;
            GameEvents.OnTurnStateChanged -= ManageTurnStateChanged;
        }
        public override void ExecuteAction()
        {
            isActiveOnTrashPile = true;
            GameEvents.OnCraftyCrowActionByPlayer?.Invoke(GameManager.Instance.CurrentPlayer);
        }
        void ManageActionTargeted(PlayerBase caster, PlayerBase victim, CardBase card)
        {
            if(!isActiveOnTrashPile)
                return;

            this.caster = caster;
            this.victim = victim;
            this.victimCard = card;
            GameEvents.OnStartNoBuenoInterruptWindow?.Invoke(this);
        }
        void ManageTurnStateChanged(TurnState state, PlayerBase @base)
        {
            if(state != TurnState.ActionResolvePhase || !isActiveOnTrashPile)
                return;
            resolver.ResolveCraftyCrow(caster, victim, victimCard);
            isActiveOnTrashPile = false;    
        }
        public override TurnState GetStateOnTrashed() => TurnState.ActionTargetPhase;
    }
}
