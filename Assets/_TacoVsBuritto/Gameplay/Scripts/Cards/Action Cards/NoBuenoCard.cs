using TacoVsBurrito;
using UnityEngine;

namespace TacoVsBurrito
{
    public class NoBuenoCard : ActionCardBase
    {
        
        public override void ExecuteAction()
        {
            GameEvents.OnNoBuenoPlayed?.Invoke();
        }
        public override TurnState GetStateOnTrashed() => TurnState.NoBuenoWindowPhase;
    }
}
