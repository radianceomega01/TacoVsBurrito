using UnityEngine;
using System.Collections.Generic;
namespace TacoVsBurrito
{
    public class TurnHandler
    {
        TurnState turnState = TurnState.None;
        List<PlayerBase> activePlayers;
        public PlayerBase CurrentPlayer => activePlayers[currentPlayerIndex];

        int currentPlayerIndex = 0;

        public TurnHandler()
        {
            activePlayers = new();
            GameEvents.OnGameInit += DecidePlayers;
            GameEvents.OnGameStarted += StartGame;
            GameEvents.OnTurnEnded += ManageTurnEnded;
            GameEvents.OnDrawPhaseSkipped += ManageDrawPhaseSkipped;
            GameEvents.OnActionCardTrashed += ManageActionCardTrashed;
        }
        ~TurnHandler()
        {
            GameEvents.OnGameInit -= DecidePlayers;
            GameEvents.OnGameStarted -= StartGame;
            GameEvents.OnTurnEnded -= ManageTurnEnded;
            GameEvents.OnDrawPhaseSkipped -= ManageDrawPhaseSkipped;
            GameEvents.OnActionCardTrashed -= ManageActionCardTrashed;
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
                    SwitchState(TurnState.DrawPhase);
                    break;
                case TurnState.DrawPhase:
                    SwitchState(TurnState.PlayPhase);
                    break;
            }
        }
        void SwitchState(TurnState turnState)
        {
            this.turnState = turnState;
            Debug.Log($"State changed for player {CurrentPlayer.GetType()} to {turnState.ToString()}");
            GameEvents.OnTurnStateChanged?.Invoke(turnState, CurrentPlayer);
        }

        void ManageActionCardTrashed(CardBase card)
        {
            SwitchState(TurnState.ActionResolvePhase);
        }

        void ManageTurnEnded(PlayerBase oldPlayer)
        {
            //currentPlayerIndex = (currentPlayerIndex + 1) % activePlayers.Count;
            GameEvents.OnTurnStarted?.Invoke(CurrentPlayer);
            SwitchState(TurnState.DrawPhase);
        }
        void ManageDrawPhaseSkipped()
        {
            GoToNextState();
        }
    }

    public enum TurnState
    {
        None,
        DrawPhase,
        PlayPhase,
        ActionResolvePhase
    }
}
