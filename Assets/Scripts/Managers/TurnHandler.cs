using UnityEngine;
using System.Collections.Generic;
namespace TacoVsBurrito
{
    public class TurnHandler
    {
        TurnState turnState = TurnState.None;
        PlayerBase CurrentPlayer => activePlayers[currentPlayerIndex];
        List<PlayerBase> activePlayers;

        int currentPlayerIndex = 0;

        public TurnHandler()
        {
            activePlayers = new();
            GameEvents.OnGameInit += DecidePlayers;
            GameEvents.OnGameStarted += StartGame;
        }
        ~TurnHandler()
        {
            GameEvents.OnGameInit -= DecidePlayers;
            GameEvents.OnGameStarted -= StartGame;
        }

        void DecidePlayers(List<PlayerBase> players)
        {
            activePlayers = players;
            currentPlayerIndex = 0; // youngest player starts (index 0)
        }

        void StartGame()
        {
            GameEvents.OnLogMessage?.Invoke(
                $"🎮 Game started! {activePlayers.Count} players. Youngest goes first. Play clockwise.");
            GameEvents.OnTurnStarted?.Invoke(CurrentPlayer);
            GoToNextState();
            GameEvents.OnLogMessage?.Invoke($"\n--- {CurrentPlayer.Name}'s turn ---");
        }

        public void GoToNextState()
        {
            switch(turnState)
            {
                case TurnState.None:
                    SwitchState(TurnState.Draw);
                    break;
                case TurnState.Draw:
                    SwitchState(TurnState.Proceed);
                    break;    
                case TurnState.Proceed:
                    SwitchState(TurnState.None);
                    ManageTurnEnded();
                    break;
            }
        }
        void SwitchState(TurnState turnState)
        {
            this.turnState = turnState;
            Debug.Log("State changed to"+ turnState.ToString());
            GameEvents.OnTurnStateChanged?.Invoke(turnState, CurrentPlayer);
        }

        void ManageTurnEnded()
        {
            GameEvents.OnTurnEnded?.Invoke(CurrentPlayer);
            currentPlayerIndex = (currentPlayerIndex + 1) % activePlayers.Count;
            GameEvents.OnTurnStarted?.Invoke(CurrentPlayer);
            GoToNextState();
        }
    }

    public enum TurnState
    {
        None,
        Draw,
        Proceed
    }
}
