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
        public static Action                            OnGameStarted;     
        public static Action<PlayerBase>                OnTurnStarted;
        public static Action<TurnState, PlayerBase>     OnTurnStateChanged;
        public static Action<PlayerBase>                OnTurnEnded;
        public static Action<PlayerBase>                OnGameOver;         // winner

        // ---- Draw pile ----
        public static Action<PlayerBase, CardBase>       OnCardDrawn;        // (drawer, card drawn)
        public static Action                             OnDrawPileEmpty;    // draw pile exhausted
        public static Action                             OnAdvanceToNextPlayer;

        // ---- Hand changes ----
        public static Action<PlayerBase, CardBase>      OnCardAddedToHand;
        public static Action<PlayerBase>                OnHandChanged;      // general refresh

        // ---- Meal changes ----
        public static Action<PlayerBase, PlayerBase, CardBase>  OnCardPlacedInMeal; // (placer, destPlayer, card)
        public static Action<PlayerBase>                        OnMealCleared;      // Health Inspector wipe
        public static Action<PlayerBase>                        OnCraftyCrowActionByPlayer;      
        public static Action<PlayerBase, PlayerBase, CardBase>  OnCraftyCrowActionTargeted;   // (caster, victim, card being targeted for steal)
        public static Action<PlayerBase>                        OnCraftyCrowActionResolved;      
        public static Action<PlayerBase, PlayerBase, CardBase>  OnCardStolenFromMeal; // (thief, victim, card)
        public static Action<PlayerBase>                        OnOrderEnvyAction;  // (player)
        public static Action<PlayerBase, PlayerBase>            OnOrderEnvyActionTargeted;    // (swapper, swapTarget)
        public static Action<PlayerBase>                        OnMealScoreChanged; // any meal update
        
        // ---- Card Drag ----
        public static Action           OnCardDragBegin;
        public static Action           OnCardDragEnd;
        public static Action<CardBase> OnCardClickedForCraftCrow;

        // ---- Action cards ----
        public static Action<CardBase>                      OnActionCardTrashed;   // (card trashed)
        public static Action<PlayerBase, CardBase>          OnNoBuenoPlayed;    // (blocker, blocked card)
        public static Action                                OnHealthInspector;  // (victim)

        // ---- No Bueno interrupt window ----
        // Fired when a card is ABOUT to be played; any player can respond with No Bueno
        public static Action<PlayerBase, CardBase, bool, Action<PlayerBase, CardBase>>  OnCardAboutToBePlayed;
        //   ^ (activePlayer, cardBeingPlayed, isLastCard, noBuenoCallback)

        // ---- Target selection (UI calls back into game logic) ----
        public static Action<string, List<PlayerBase>, Action<PlayerBase>>      OnNeedPlayerBaseTarget;
        //   ^ (prompt, validTargets, callback)
        public static Action<string, List<CardBase>, Action<CardBase>>          OnNeedCardFromMeal;
        //   ^ (prompt, cards in target's meal, callback)
        public static Action<string, List<CardBase>, Action<CardBase>>          OnNeedCardFromTrash;
        //   ^ (prompt, trash pile contents, callback)
        public static Action<string, List<PlayerBase>, Action<PlayerBase, CardBase>> OnNeedPlayerBaseAndMealCard;
        //   ^ (prompt, validTargets, callback(player, card))

        // ---- Scoring ----
        public static Action<PlayerBase, int>           OnScoreChanged;     // (player, newScore)

        // ---- Game log ----
        public static Action<string>                OnLogMessage;

        // ---- Turn indicator ----
        public static Action                                OnDrawPhaseSkipped; // draw pile empty

        // ---- Food Fight specific ----
        public static Action<PlayerBase, CardBase>          OnFoodFightFlip;    // (player, flipped card)

        // Clears all subscriptions (call on scene reload)
        public static void Clear()
        {
            OnGameStarted           = null;
            OnTurnStarted           = null;
            OnTurnEnded             = null;
            OnGameOver              = null;
            OnCardDrawn             = null;
            OnDrawPileEmpty         = null;
            OnCardAddedToHand       = null;
            OnHandChanged           = null;
            OnCardPlacedInMeal      = null;
            OnMealCleared           = null;
            OnCardStolenFromMeal    = null;
            OnOrderEnvyAction         = null;
            OnOrderEnvyActionTargeted = null;
            OnMealScoreChanged      = null;
            OnNoBuenoPlayed         = null;
            OnHealthInspector       = null;
            OnCardAboutToBePlayed   = null;
            OnNeedPlayerBaseTarget      = null;
            OnNeedCardFromMeal      = null;
            OnNeedCardFromTrash     = null;
            OnNeedPlayerBaseAndMealCard = null;
            OnScoreChanged          = null;
            OnLogMessage            = null;
            OnDrawPhaseSkipped      = null;
            OnFoodFightFlip         = null;
        }
    }
}