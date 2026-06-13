// ============================================================
//  GameEvents.cs  –  Static event bus
//  UI, audio, and other systems subscribe here.
//  Game-logic code fires events without knowing about MonoBehaviours.
// ============================================================

using System;
using System.Collections.Generic;

namespace TacoVsBurrito
{
    public static class GameEvents
    {
        // ---- Lifecycle ----
        public static Action<List<PlayerBase>>          OnGameInit;    
        public static Action<ActionResolver>            OnActionResolverSet;     
        public static Action                            OnShuffleCards;     
        public static Action<List<PlayerBase>>          OnDistributeCards;     
        public static Action                            OnCardsDistributed;
        public static Action<PlayerBase>                OnTurnStarted;
        public static Action<TurnState, PlayerBase>     OnTurnStateChanged;
        public static Action<PlayerBase>                OnTurnEnded;
        public static Action<PlayerBase>                OnTurnChangedInFoodFight;
        public static Action<TurnState, PlayerBase>     OnPlayerAndTurnStateChanged;
        public static Action                            OnGameFinished;
        public static Action<Tuple<PlayerBase, int>>    OnGameWinner;         // Winning player and score

        // ---- Draw pile ----
        public static Action                             OnDrawPileEmpty;    // draw pile exhausted

        // ---- Meal changes ----
        public static Action                                    OnCraftyCrowAction;      
        public static Action<TargetTypeContext>                 OnCraftyCrowActionTargeted;   // (caster, victim, card being targeted for steal)
        public static Action<PlayerBase>                        OnOrderEnvyAction;  // (player)
        public static Action<TargetTypeContext>                 OnOrderEnvyActionTargeted;    // (swapper, swapTarget)
        public static Action<Dictionary<CardBase, int>>         OnTrashPandaAction; 
        public static Action<TargetTypeContext>                 OnCardsPileCardTargeted; 
        public static Action<FoodFightCard>                     OnFoodFightAction;
        public static Action                                    OnFoodFightOver;
        public static Action<ActionCardBase>                    OnActionResolved;
        public static Action<Dictionary<CardBase, int>, PlayerBase>         OnCardSelectionForFoodFightWinner; // All cards drawn during food fight and winner

        
        // ---- Card Drag ----
        public static Action           OnCardDragBegin;
        public static Action           OnCardDragEnd;
        public static Action<CardBase> OnCardClickedForActionTarget;

        // ---- Action cards ----
        public static Action<ActionCardBase>                      OnActionCardPlayed;   // (card trashed)
        public static Action<ActionCardBase>                      OnStartNoBuenoInterruptWindow;
        public static Action<NoBuenoCard>                         OnNoBuenoPlayed;  

        // ---- Timer ----
        public static Action<int>                   OnTimerEvent;

        // ---- Game log ----
        public static Action<string>                OnLogMessage;

        //Audio Specific Events
        public static Action                        OnHealthInspectorSFX;  // (victim)
        public static Action                        OnGUIClickSFX;
        public static Action                        OnCardMovedSFX;
        public static Action                        OnCardFlippedSFX;
        public static Action                        OnCardDrawSFX;
        public static Action                        OnLaugh1Sfx;
        public static Action                        OnLaugh2Sfx;
        public static Action                        OnCrySfx;
    }
}