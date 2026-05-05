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
        public static Action<List<Player>>          OnGameStarted;      // (all players)
        public static Action<Player>                OnTurnStarted;
        public static Action<Player>                OnTurnEnded;
        public static Action<Player>                OnGameOver;         // winner

        // ---- Draw pile ----
        public static Action<Player, CardBase>          OnCardDrawn;        // (drawer, card drawn)
        public static Action                        OnDrawPileEmpty;    // draw pile exhausted

        // ---- Hand changes ----
        public static Action<Player, CardBase>          OnCardAddedToHand;
        public static Action<Player>                OnHandChanged;      // general refresh

        // ---- Meal changes ----
        public static Action<Player, Player, CardBase>  OnCardPlacedInMeal; // (placer, destPlayer, card)
        public static Action<Player>                OnMealCleared;      // Health Inspector wipe
        public static Action<Player, Player, CardBase>  OnCardStolenFromMeal; // (thief, victim, card)
        public static Action<Player, Player>        OnOrderEnvySwap;    // (swapper, swapTarget)
        public static Action<Player>                OnMealScoreChanged; // any meal update

        // ---- Action cards ----
        public static Action<Player, CardBase>          OnNoBuenoPlayed;    // (blocker, blocked card)
        public static Action<Player>                OnHealthInspector;  // (victim)

        // ---- No Bueno interrupt window ----
        // Fired when a card is ABOUT to be played; any player can respond with No Bueno
        public static Action<Player, CardBase, bool, Action<Player, CardBase>>  OnCardAboutToBePlayed;
        //   ^ (activePlayer, cardBeingPlayed, isLastCard, noBuenoCallback)

        // ---- Target selection (UI calls back into game logic) ----
        public static Action<string, List<Player>, Action<Player>>      OnNeedPlayerTarget;
        //   ^ (prompt, validTargets, callback)
        public static Action<string, List<CardBase>, Action<CardBase>>          OnNeedCardFromMeal;
        //   ^ (prompt, cards in target's meal, callback)
        public static Action<string, List<CardBase>, Action<CardBase>>          OnNeedCardFromTrash;
        //   ^ (prompt, trash pile contents, callback)
        public static Action<string, List<Player>, Action<Player, CardBase>> OnNeedPlayerAndMealCard;
        //   ^ (prompt, validTargets, callback(player, card))

        // ---- Scoring ----
        public static Action<Player, int>           OnScoreChanged;     // (player, newScore)

        // ---- Game log ----
        public static Action<string>                OnLogMessage;

        // ---- Turn indicator ----
        public static Action<bool>                  OnDrawPhaseSkipped; // draw pile empty

        // ---- Food Fight specific ----
        public static Action<Player, CardBase>          OnFoodFightFlip;    // (player, flipped card)

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
            OnOrderEnvySwap         = null;
            OnMealScoreChanged      = null;
            OnNoBuenoPlayed         = null;
            OnHealthInspector       = null;
            OnCardAboutToBePlayed   = null;
            OnNeedPlayerTarget      = null;
            OnNeedCardFromMeal      = null;
            OnNeedCardFromTrash     = null;
            OnNeedPlayerAndMealCard = null;
            OnScoreChanged          = null;
            OnLogMessage            = null;
            OnDrawPhaseSkipped      = null;
            OnFoodFightFlip         = null;
        }
    }
}