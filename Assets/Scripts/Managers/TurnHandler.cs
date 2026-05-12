using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System;
namespace TacoVsBurrito
{
    public class TurnHandler
    {
        TurnState turnState = TurnState.None;
        List<PlayerBase> activePlayers;
        public PlayerBase CurrentPlayer => activePlayers[currentPlayerIndex];

        int currentPlayerIndex = 0;
        CancellationTokenSource noBuenoTimerCts;

        public TurnHandler()
        {
            activePlayers = new();
            GameEvents.OnGameInit += DecidePlayers;
            GameEvents.OnGameStarted += StartGame;
            GameEvents.OnTurnEnded += ManageTurnEnded;
            GameEvents.OnDrawPhaseSkipped += ManageDrawPhaseSkipped;
            GameEvents.OnActionCardTrashed += ManageActionCardTrashed;
            GameEvents.OnNoBuenoPlayed += ManageNoBuenoPlayed;
        }

        ~TurnHandler()
        {
            GameEvents.OnGameInit -= DecidePlayers;
            GameEvents.OnGameStarted -= StartGame;
            GameEvents.OnTurnEnded -= ManageTurnEnded;
            GameEvents.OnDrawPhaseSkipped -= ManageDrawPhaseSkipped;
            GameEvents.OnActionCardTrashed -= ManageActionCardTrashed;
            GameEvents.OnNoBuenoPlayed -= ManageNoBuenoPlayed;
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
            if (turnState == TurnState.NoBuenoWindowPhase)
            {
                StartNoBuenoTimer();
            }
        }

        void ManageActionCardTrashed(CardBase card)
        {
            if(card is HealthInspectorCard)
                SwitchState(TurnState.ActionResolvePhase);
            else
                SwitchState(TurnState.NoBuenoWindowPhase);    
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
        async void StartNoBuenoTimer()
        {
            noBuenoTimerCts?.Cancel();

            noBuenoTimerCts = new CancellationTokenSource();

            try
            {
                await Task.Delay(5000, noBuenoTimerCts.Token);

                Debug.Log("No Bueno window expired");
                SwitchState(TurnState.ActionResolvePhase);
            }
            catch (TaskCanceledException)
            {
                Debug.Log("No Bueno timer cancelled");
            }
        }
        void ManageNoBuenoPlayed()
        {
            SwitchState(TurnState.NoBuenoWindowPhase);
        }
    }

    public enum TurnState
    {
        None,
        DrawPhase,
        PlayPhase,
        NoBuenoWindowPhase,
        ActionResolvePhase
    }
}
