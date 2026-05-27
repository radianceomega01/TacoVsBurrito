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
        private List<NoBuenoCard> noBuenoCardsPlayed = new();
        private ActionCardBase currentTrashedCard;
        private TrashPile trashPile;

        const int NO_BUENO_WINDOW_DURATION_MS = 5000;

        public TurnHandler(TrashPile trashPile)
        {
            this.trashPile = trashPile;
            activePlayers = new();
            GameEvents.OnGameInit += DecidePlayers;
            GameEvents.OnGameStarted += StartGame;
            GameEvents.OnTurnEnded += ManageTurnEnded;
            GameEvents.OnTurnChangedInFoodFight += ManageTurnChangedInFoodFight;
            GameEvents.OnDrawPhaseSkipped += ManageDrawPhaseSkipped;
            GameEvents.OnActionCardPlayed += ManageActionCardPlayed;
            GameEvents.OnStartNoBuenoInterruptWindow += ManageStartNoBuenoInterruptWindow;
            GameEvents.OnNoBuenoPlayed += ManageNoBuenoPlayed;
            GameEvents.OnPlayerAndTurnStateChanged += ManagePlayerAndStateChanged;

            GameEvents.OnCraftyCrowActionTargeted += ManageTargetAction;
            GameEvents.OnOrderEnvyActionTargeted += ManageTargetAction;
            GameEvents.OnCardsPileCardTargeted += ManageTargetAction;
        }

        ~TurnHandler()
        {
            GameEvents.OnGameInit -= DecidePlayers;
            GameEvents.OnGameStarted -= StartGame;
            GameEvents.OnTurnEnded -= ManageTurnEnded;
            GameEvents.OnTurnChangedInFoodFight -= ManageTurnChangedInFoodFight;
            GameEvents.OnDrawPhaseSkipped -= ManageDrawPhaseSkipped;
            GameEvents.OnActionCardPlayed -= ManageActionCardPlayed;
            GameEvents.OnStartNoBuenoInterruptWindow -= ManageStartNoBuenoInterruptWindow;
            GameEvents.OnNoBuenoPlayed -= ManageNoBuenoPlayed;
            GameEvents.OnPlayerAndTurnStateChanged -= ManagePlayerAndStateChanged;

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

        void ManageActionCardPlayed(ActionCardBase card)
        {
            Debug.LogWarning("reached turn handler with card: "+ card.GetType());
            if(card is not NoBuenoCard) currentTrashedCard = card;
            
            //SwitchState(card.GetStateOnTrashed());
            //card.ExecuteAction();    
            if(card is IImmediateTypeAction)
            {
                card.ExecuteAction();
                return;
            }
            ManageStartNoBuenoInterruptWindow(card);
        }

        void ManageTurnEnded(PlayerBase oldPlayer)
        {
            currentPlayerIndex = (currentPlayerIndex + 1) % activePlayers.Count;
            GameEvents.OnTurnStarted?.Invoke(CurrentPlayer);
            SwitchState(TurnState.DrawPhase);
            currentTrashedCard = null;
        }
        void ManageTurnChangedInFoodFight(PlayerBase newPlayer)
        {
            currentPlayerIndex = newPlayer.Index;
            GameEvents.OnTurnStarted?.Invoke(CurrentPlayer);
            SwitchState(TurnState.DrawPhase);
        }
        void ManagePlayerAndStateChanged(TurnState state, PlayerBase player)
        {
            currentPlayerIndex = player.Index;
            SwitchState(state);
        }

        void ManageDrawPhaseSkipped()
        {
            GoToNextState();
        }
        void ManageStartNoBuenoInterruptWindow(ActionCardBase card)
        {
            SwitchState(TurnState.NoBuenoWindowPhase);
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
                if(noBuenoCardsPlayed.Count % 2 == 0)
                {
                    SwitchState(TurnState.ActionResolvePhase);
                    //CheckAndResolveAction();
                    CheckAndExecuteAction();
                }
                else
                {
                    trashPile.Trash(currentTrashedCard);
                    ManageTurnEnded(GameManager.Instance.CurrentPlayer);
                }
                noBuenoCardsPlayed.ForEach(card => trashPile.Trash(card));
                noBuenoCardsPlayed.Clear();    
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
                GameEvents.OnActionResolved?.Invoke(currentTrashedCard);
                targetTyeCard.ResolveAction();
            }
        }
        void CheckAndExecuteAction()
        {
            if (currentTrashedCard != null && currentTrashedCard is not NoBuenoCard) //No bueno is immediately executed
            {
                currentTrashedCard.ExecuteAction();
                if(currentTrashedCard is ITargetTypeAction)
                {
                    SwitchState(TurnState.ActionTargetPhase);
                }
            }
        }

        void ManageNoBuenoPlayed(NoBuenoCard noBuenoCard)
        {
            //PLayer played no bueno as a regular action card in play area
            if(currentTrashedCard == null)
            {
                GameEvents.OnTurnEnded?.Invoke(GameManager.Instance.CurrentPlayer);
                return;
            }

            noBuenoCardsPlayed.Add(noBuenoCard);
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
