using TacoVsBurrito;
using UnityEngine;

namespace TacoVsBurrito
{
    public class OrderEnvyCard : ActionCardBase
    {
        PlayerBase caster;
        PlayerBase victim;

        protected override void Awake() {
            base.Awake();
            GameEvents.OnOrderEnvyActionTargeted += ManageActionTargeted;
            GameEvents.OnTurnStateChanged += ManageTurnStateChanged;
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            GameEvents.OnOrderEnvyActionTargeted -= ManageActionTargeted;
            GameEvents.OnTurnStateChanged -= ManageTurnStateChanged;
        }
        public override void ExecuteAction()
        {
            isActiveOnTrashPile = true;
            GameEvents.OnOrderEnvyAction?.Invoke(GameManager.Instance.CurrentPlayer);
        }
        void ManageActionTargeted(PlayerBase caster, PlayerBase victim)
        {
            if(!isActiveOnTrashPile)
                return;

            this.caster = caster;
            this.victim = victim;
            
            GameEvents.OnStartNoBuenoInterruptWindow?.Invoke(this);
        }
        void ManageTurnStateChanged(TurnState state, PlayerBase @base)
        {
            if(state != TurnState.ActionResolvePhase || !isActiveOnTrashPile)
                return;
            resolver.ResolveOrderEnvy(caster, victim);
            isActiveOnTrashPile = false;    
        }
        public override TurnState GetStateOnTrashed() => TurnState.ActionTargetPhase;
    }
}
