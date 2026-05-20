using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System;
namespace TacoVsBurrito
{
    public class TurnHandler
    {
        TurnState currentTurnState = TurnState.None;
        List<PlayerBase> activePlayers;
        public PlayerBase CurrentPlayer => activePlayers[currentPlayerIndex];

        int currentPlayerIndex = 0;
        CancellationTokenSource noBuenoTimerCts;
        private int noBuenoCounter = 0;
        private CardBase currentTrashedCard;

        const int NO_BUENO_WINDOW_DURATION_MS = 5000;

        public TurnHandler()
        {
            activePlayers = new();
            GameEvents.OnGameInit += DecidePlayers;
            GameEvents.OnGameStarted += StartGame;
            GameEvents.OnTurnEnded += ManageTurnEnded;
            GameEvents.OnTurnChanged += ManageTurnChanged;
            GameEvents.OnDrawPhaseSkipped += ManageDrawPhaseSkipped;
            GameEvents.OnActionCardTrashed += ManageActionCardTrashed;
            GameEvents.OnStartNoBuenoInterruptWindow += ManageStartNoBuenoInterruptWindow;
            GameEvents.OnNoBuenoPlayed += ManageNoBuenoPlayed;
        }

        ~TurnHandler()
        {
            GameEvents.OnGameInit -= DecidePlayers;
            GameEvents.OnGameStarted -= StartGame;
            GameEvents.OnTurnEnded -= ManageTurnEnded;
            GameEvents.OnTurnChanged -= ManageTurnChanged;
            GameEvents.OnDrawPhaseSkipped -= ManageDrawPhaseSkipped;
            GameEvents.OnActionCardTrashed -= ManageActionCardTrashed;
            GameEvents.OnStartNoBuenoInterruptWindow -= ManageStartNoBuenoInterruptWindow;
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
            //GameEvents.OnLogMessage?.Invoke($"\n--- {CurrentPlayer.Name}'s turn ---");
        }

        public void GoToNextState()
        {
            switch(currentTurnState)
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
            currentTurnState = turnState;
            Debug.Log($"State changed for player {CurrentPlayer.GetType()} to {turnState.ToString()}");
            GameEvents.OnTurnStateChanged?.Invoke(turnState, CurrentPlayer);
            
        }

        void ManageActionCardTrashed(ActionCardBase card)
        {
            currentTrashedCard = card;

            SwitchState(card.GetStateOnTrashed());
            switch (currentTurnState)
            {
                case TurnState.ActionTargetPhase:
                    CheckAndExecuteAction();
                    currentTrashedCard = null;
                    break;
                case TurnState.NoBuenoWindowPhase:
                    StartNoBuenoTimer();
                    break;
                case TurnState.ActionResolvePhase:
                    card.ExecuteAction();
                    currentTrashedCard = null;
                    break;
                case TurnState.SkipPhase:
                    ManageTurnEnded(GameManager.Instance.CurrentPlayer);
                    currentTrashedCard = null;
                    break;
            }    
        }
        void CheckAndExecuteAction()
        {
            if (currentTrashedCard is ActionCardBase @card && currentTrashedCard is not NoBuenoCard) //No bueno is immediately executed
            {
                @card.ExecuteAction();
            }
        }

        void ManageTurnEnded(PlayerBase oldPlayer)
        {
            currentPlayerIndex = (currentPlayerIndex + 1) % activePlayers.Count;
            GameEvents.OnTurnStarted?.Invoke(CurrentPlayer);
            SwitchState(TurnState.DrawPhase);
        }
        void ManageTurnChanged(PlayerBase newPlayer)
        {
            currentPlayerIndex = newPlayer.Index;
            GameEvents.OnTurnStarted?.Invoke(CurrentPlayer);
            SwitchState(TurnState.DrawPhase);
        }

        void ManageDrawPhaseSkipped()
        {
            GoToNextState();
        }
        void ManageStartNoBuenoInterruptWindow(ActionCardBase card)
        {
            StartNoBuenoTimer();
        }
        async void StartNoBuenoTimer()
        {
            noBuenoTimerCts?.Cancel();

            noBuenoTimerCts = new CancellationTokenSource();
            try
            {
                GameEvents.OnTimerEvent(NO_BUENO_WINDOW_DURATION_MS/1000);
                GameEvents.OnLogMessage?.Invoke("⏰ No Bueno window!");
                await Task.Delay(NO_BUENO_WINDOW_DURATION_MS, noBuenoTimerCts.Token);

                Debug.Log("No Bueno window expired");
                GameEvents.OnLogMessage?.Invoke("⏰ No Bueno window expired!");
                if(noBuenoCounter%2 == 0)
                    SwitchState(TurnState.ActionResolvePhase);
                else
                    ManageTurnEnded(GameManager.Instance.CurrentPlayer);
                noBuenoCounter = 0;    
            }
            catch (TaskCanceledException)
            {
                Debug.Log("No Bueno timer cancelled");
            }
        }

        void ManageNoBuenoPlayed()
        {
            noBuenoCounter++;
            SwitchState(TurnState.NoBuenoWindowPhase);
            StartNoBuenoTimer();
        }
    }

    public enum TurnState
    {
        None,
        DrawPhase,
        PlayPhase,
        ActionTargetPhase,
        NoBuenoWindowPhase,
        ActionResolvePhase,
        SkipPhase
    }
}
