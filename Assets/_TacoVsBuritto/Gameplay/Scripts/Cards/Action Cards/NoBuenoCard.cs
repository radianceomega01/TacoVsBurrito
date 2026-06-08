using TacoVsBurrito;
using UnityEngine;

namespace TacoVsBurrito
{
    public class NoBuenoCard : ActionCardBase, IImmediateTypeAction
    {
        public PlayerBase NoBuenoPlayer { get; set; }
        public override void ExecuteAction()
        {
            GameEvents.OnNoBuenoPlayed?.Invoke(this);
        }
        public override TurnState GetStateOnTrashed() => TurnState.NoBuenoWindowPhase;
    }
}
