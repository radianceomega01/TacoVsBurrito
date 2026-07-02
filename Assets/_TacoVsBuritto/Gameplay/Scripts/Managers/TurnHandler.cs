using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System;
namespace TacoVsBurrito
{
    public class TurnHandler: IDisposable
    {
        TurnState currentTurnState = TurnState.None;
        List<PlayerBase> activePlayers;
        public PlayerBase CurrentPlayer => activePlayers[currentPlayerIndex];

        int currentPlayerIndex = 0;
        CancellationTokenSource noBuenoTimerCts;
        private List<NoBuenoCard> noBuenoCardsPlayed = new();
        private ActionCardBase currentPlayedActionCard; // All action cards except noBueno
        private ActionCardBase currentActiveActionCard; // All action cards including no bueno
        private TrashPile trashPile;
        private bool isDrawPileEmpty = false;

        const int NO_BUENO_WINDOW_DURATION_MS = 5000;
        const int CARD_TRASH_DELAY_IN_MS = 1500;
        const int STATE_TRANSITION_DELAY_IN_MS = 100; //To avoid race condition like nobueno played and turnstate changed

        public TurnHandler(TrashPile trashPile)
        {
            this.trashPile = trashPile;
            activePlayers = new();
            GameEvents.OnGameInit += DecidePlayers;
            GameEvents.OnCardsDistributed += StartGame;
            GameEvents.OnTurnEnded += ManageTurnEnded;
            GameEvents.OnTurnChangedAfterFoodFight += ManageTurnChangedInFoodFight;
            GameEvents.OnDrawPileEmpty += ManageDrawPileEmpty;
            GameEvents.OnActionCardPlayed += ManageActionCardPlayed;
            GameEvents.OnNoBuenoPlayed += ManageNoBuenoPlayed;
            GameEvents.OnPlayerAndTurnStateChanged += ManagePlayerAndStateChanged;

            GameEvents.OnCraftyCrowActionTargeted += ManageTargetAction;
            GameEvents.OnOrderEnvyActionTargeted += ManageTargetAction;
            GameEvents.OnCardsPileCardTargeted += ManageTargetAction;
        }

        void UnsubscribeEvents()
        {
            GameEvents.OnGameInit -= DecidePlayers;
            GameEvents.OnCardsDistributed -= StartGame;
            GameEvents.OnTurnEnded -= ManageTurnEnded;
            GameEvents.OnTurnChangedAfterFoodFight -= ManageTurnChangedInFoodFight;
            GameEvents.OnDrawPileEmpty -= ManageDrawPileEmpty;
            GameEvents.OnActionCardPlayed -= ManageActionCardPlayed;
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
            Debug.Log($"State changed for player {CurrentPlayer.Name} to {turnState.ToString()}");
            GameEvents.OnTurnStateChanged?.Invoke(turnState, CurrentPlayer);
        }

        async void ManageActionCardPlayed(ActionCardBase card)
        {
            currentActiveActionCard = card;
            if(card is not NoBuenoCard) currentPlayedActionCard = card; //No bueno is immediately executed
            
            //SwitchState(card.GetStateOnTrashed());
            //card.ExecuteAction();    
            if(card is IImmediateTypeAction)
            {
                card.ExecuteAction();
                return;
            }

            if(card.CanExecuteAction())
            {
                if(CurrentPlayer.Hand.Count == 0) // Last Hand card played Nobueno cannot be done
                {
                    //CheckAndResolveAction();
                    CheckAndExecuteAction();
                }
                else
                {
                    await Task.Delay(card.FEEL_ANIM_DELAY_IN_MS);
                    ManageStartNoBuenoInterruptWindow();
                }
            }
            else
            {
                await Task.Delay(CARD_TRASH_DELAY_IN_MS);
                trashPile.Trash(card);
                ManageTurnEnded(GameplayManager.Instance.CurrentPlayer);
            }
        }

        async void ManageTurnEnded(PlayerBase oldPlayer)
        {
            currentActiveActionCard = null;
            currentPlayedActionCard = null;
            if(CheckAndFinishGame())
            {
                return;
            }
            
            currentPlayerIndex = (currentPlayerIndex + 1) % activePlayers.Count;
            await Task.Delay(STATE_TRANSITION_DELAY_IN_MS);
            GameEvents.OnTurnStarted?.Invoke(CurrentPlayer);

            if(isDrawPileEmpty)
            {
                GameEvents.OnLogMessage?.Invoke($"  (Draw pile empty – skip draw step)");
                SwitchState(TurnState.PlayPhase);
            }
            else
            {
                SwitchState(TurnState.DrawPhase);
            }
        }

        bool CheckAndFinishGame()
        {
            foreach(var player in activePlayers)
            {
                if(player.Hand.Count == 0)
                {
                    GameEvents.OnGameFinished?.Invoke();
                    return true;
                }
            }
            return false;
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

        void ManageDrawPileEmpty()
        {
            isDrawPileEmpty = true;
        }
        async Task ManageStartNoBuenoInterruptWindow()
        {
            await Task.Delay(STATE_TRANSITION_DELAY_IN_MS);
            SwitchState(TurnState.NoBuenoWindowPhase);
            GameEvents.OnStartNoBuenoInterruptWindow?.Invoke(currentActiveActionCard);
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
                GameEvents.OnResetCurrentDraggingCard?.Invoke();
                if(noBuenoCardsPlayed.Count % 2 == 0)
                {
                    //CheckAndResolveAction();
                    CheckAndExecuteAction();
                }
                else
                {
                    trashPile.Trash(currentPlayedActionCard);
                    ManageTurnEnded(GameplayManager.Instance.CurrentPlayer);
                }
                noBuenoCardsPlayed.ForEach(card =>
                {
                    trashPile.Trash(card);
                });
                noBuenoCardsPlayed.Clear();    
            }
            catch (TaskCanceledException)
            {
                Debug.Log("No Bueno timer cancelled");
            }
        }

        void CheckAndResolveAction()
        {
            if (currentPlayedActionCard != null && currentPlayedActionCard is ITargetTypeAction targetTyeCard)
            {
                GameEvents.OnActionResolved?.Invoke(currentPlayedActionCard);
                targetTyeCard.ResolveAction();
            }
        }
        void CheckAndExecuteAction()
        {
            if (currentPlayedActionCard != null) 
            {
                if(currentPlayedActionCard is ITargetTypeAction)
                {
                    SwitchState(TurnState.ActionTargetPhase);
                }
                else
                    SwitchState(TurnState.ActionResolvePhase);
                currentPlayedActionCard.ExecuteAction();
            }
        }

        async void ManageNoBuenoPlayed(NoBuenoCard noBuenoCard)
        {
            //Player played no bueno as a regular action card in play area or no bueno phase expired
            if(currentTurnState != TurnState.NoBuenoWindowPhase)
            {
                if(currentTurnState == TurnState.PlayPhase) // means played on play area in play phase
                {
                    await Task.Delay(CARD_TRASH_DELAY_IN_MS);
                    trashPile.Trash(noBuenoCard);
                    GameEvents.OnTurnEnded?.Invoke(GameplayManager.Instance.CurrentPlayer);
                }
                else //means no bueno time expired 
                {
                    noBuenoCard.NoBuenoPlayer.Hand.AddCard(noBuenoCard);
                }
                return;
            } 

            //Valid no bueno thown during no bueno phase
            noBuenoCardsPlayed.Add(noBuenoCard);
            ManageStartNoBuenoInterruptWindow();
            // if(noBuenoCard.NoBuenoPlayer.Hand.Count == 0) //No bueno was last card of the player
            // {
            //     trashPile.Trash(currentPlayedActionCard);
            //     noBuenoCardsPlayed.ForEach(card =>
            //     {
            //         trashPile.Trash(card);
            //     });
            //     noBuenoCardsPlayed.Clear();
            //     GameEvents.OnGameFinished?.Invoke();
            // }
            // else
            // {
            //     ManageStartNoBuenoInterruptWindow();
            // }
                
        }
        
        void ManageTargetAction(TargetTypeContext context)
        {
            if(currentPlayedActionCard is ITargetTypeAction targetTypeCard)
            {
                SwitchState(TurnState.ActionResolvePhase);
                targetTypeCard.OnActionTargeted(context);
            }
        }

        public void Dispose()
        {
            UnsubscribeEvents();
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
