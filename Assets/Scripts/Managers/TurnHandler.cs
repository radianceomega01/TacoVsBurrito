
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

        public void SwitchState()
        {
            if(turnState == TurnState.None || turnState == TurnState.Proceed)
            {
                turnState = TurnState.Draw;
            }
            else if(turnState == TurnState.Draw)
            {
                turnState = TurnState.Proceed;
            }
            GameEvents.OnTurnStateChanged?.Invoke(turnState, currentPlayer);
        }
        public void SwitchState(TurnState turnState)
        {
            this.turnState = turnState;
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
