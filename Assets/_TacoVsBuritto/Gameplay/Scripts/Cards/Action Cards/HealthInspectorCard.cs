using System.Threading.Tasks;
using TacoVsBurrito;
using UnityEngine;

namespace TacoVsBurrito
{
    public class HealthInspectorCard : ActionCardBase, IImmediateTypeAction
    {
        public override int FEEL_ANIM_DELAY_IN_MS => 0;
        
        public override async void ExecuteAction()
        {
            base.ExecuteAction();
            await Task.Delay(EXECUTION_DEALY_IN_MS);
            GameplayManager.Instance.GetTrashPile().Trash(this);

            await Task.Delay(EXECUTION_DEALY_IN_MS);
            resolver.ResolveHealthInspector(GameplayManager.Instance.CurrentPlayer);
        }
        
        public override TurnState GetStateOnPlayed() => TurnState.ActionResolvePhase;
        public override void PlayCardPlayedEffect(){}
    }
}
