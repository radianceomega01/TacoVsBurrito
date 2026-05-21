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
        private ActionCardBase currentTrashedCard;

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

            GameEvents.OnCraftyCrowActionTargeted += ManageTargetAction;
            GameEvents.OnOrderEnvyActionTargeted += ManageTargetAction;
            GameEvents.OnCardsPileCardTargeted += ManageTargetAction;
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

            GameEvents.OnCraftyCrowActionTargeted -= ManageTargetAction;
            GameEvents.OnOrderEnvyActionTargeted -= ManageTargetAction;
            GameEvents.OnCardsPileCardTargeted -= ManageTargetAction;
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
            if(card is not NoBuenoCard) currentTrashedCard = card;

            SwitchState(card.GetStateOnTrashed());
            card.ExecuteAction();    
        }

        void ManageTurnEnded(PlayerBase oldPlayer)
        {
            currentPlayerIndex = (currentPlayerIndex + 1) % activePlayers.Count;
            GameEvents.OnTurnStarted?.Invoke(CurrentPlayer);
            SwitchState(TurnState.DrawPhase);
            currentTrashedCard = null;
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
                {
                    SwitchState(TurnState.ActionResolvePhase);
                    CheckAndResolveAction();
                }
                else
                    ManageTurnEnded(GameManager.Instance.CurrentPlayer);
                noBuenoCounter = 0;    
            }
            catch (TaskCanceledException)
            {
                Debug.Log("No Bueno timer cancelled");
            }
        }

        void CheckAndResolveAction()
        {
            if (currentTrashedCard != null && currentTrashedCard is ITargetTypeAction targetTyeCard) //No bueno is immediately executed
            {
                targetTyeCard.ResolveAction();
            }
        }

        void ManageNoBuenoPlayed()
        {
            if(currentTrashedCard == null)
            {
                GameEvents.OnTurnEnded?.Invoke(GameManager.Instance.CurrentPlayer);
                return;
            }

            noBuenoCounter++;
            SwitchState(TurnState.NoBuenoWindowPhase);
            StartNoBuenoTimer();
        }
        void ManageTargetAction(TargetTypeContext context)
        {
            if(currentTrashedCard is ITargetTypeAction targetTypeCard)
            {
                targetTypeCard.OnActionTargeted(context);
            }
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
