using System.Threading.Tasks;
using TacoVsBurrito;
using UnityEngine;

namespace TacoVsBurrito
{
    public class HealthInspectorCard : ActionCardBase, IImmediateTypeAction
    {
        public override async void ExecuteAction()
        {
            await Task.Delay(EXECUTION_DEALY_IN_MS);
            GameManager.Instance.GetTrashPile().Trash(this);

            await Task.Delay(EXECUTION_DEALY_IN_MS);
            resolver.ResolveHealthInspector(GameManager.Instance.CurrentPlayer);
        }
        
        public override TurnState GetStateOnTrashed() => TurnState.ActionResolvePhase;
    }
}
