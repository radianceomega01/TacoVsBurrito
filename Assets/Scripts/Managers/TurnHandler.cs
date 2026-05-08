using UnityEngine;
namespace TacoVsBurrito
{
    public class TurnHandler
    {
        TurnState turnState = TurnState.None;
        PlayerBase currentPlayer;

        public TurnHandler()
        {
            GameEvents.OnTurnStarted += ManageTurnStarted;
            GameEvents.OnTurnEnded += ManageTurnEnded;
        }
        ~TurnHandler()
        {
            GameEvents.OnTurnStarted -= ManageTurnStarted;
            GameEvents.OnTurnEnded -= ManageTurnEnded;
        }

        public void GoToNextState()
        {
            if(turnState == TurnState.None || turnState == TurnState.Proceed)
            {
                SwitchState(TurnState.Draw);
            }
            else if(turnState == TurnState.Draw)
            {
                SwitchState(TurnState.Proceed);
            }
        }
        public void SwitchState(TurnState turnState)
        {
            this.turnState = turnState;
            Debug.Log("State changed to"+ turnState.ToString());
            GameEvents.OnTurnStateChanged?.Invoke(turnState, currentPlayer);
        }

        void ManageTurnStarted(PlayerBase player)
        {
            currentPlayer = player;
            SwitchState(TurnState.Draw);
        }
        void ManageTurnEnded(PlayerBase player)
        {
            SwitchState(TurnState.Proceed);
        }
    }

    public enum TurnState
    {
        None,
        Draw,
        Proceed
    }
}
