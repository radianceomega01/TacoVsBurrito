using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
namespace TacoVsBurrito
{
    public class FoodFightHandler : MonoBehaviour
    {
        const int DELAY_BETWEEN_TURNS_IN_MS = 300;
        const int DELAY_BETWEEN_ROUNDS_IN_MS = 500;
        const int DELAY_AFTER_FOOD_FIGHT_IN_MS = 1000;
        DrawPile drawPile;
        TrashPile trashPile;
        FoodFightDrawPile foodFightDrawPile;
        FoodFightCard foodFightCard;

        List<PlayerBase> allPlayers;
        List<PlayerBase> activePlayersInRound;
        List<CardBase> cardsDrawn = new();
        Dictionary<PlayerBase, CardBase> playerCardInRound = new();
        int currentPlayerIndex;

        private void OnEnable()
        {
            GameEvents.OnFoodFightAction += BeginFoodFight;
        }

        private void OnDisable()
        {
            GameEvents.OnFoodFightAction -= BeginFoodFight;
        }

        void SubscribeLocalEvents()
        {
            foodFightDrawPile.OnCardDrawn += OnCardDrawn;
        }
        void UnSubscribeLocalEvents()
        {
            foodFightDrawPile.OnCardDrawn -= OnCardDrawn;
        }
        void BeginFoodFight(FoodFightCard foodFightCard)
        {
            this.foodFightCard = foodFightCard;
            drawPile = GameplayManager.Instance.GetDrawPile();
            trashPile = GameplayManager.Instance.GetTrashPile();
            allPlayers = new(GameplayManager.Instance.Players);
            activePlayersInRound = new(GameplayManager.Instance.Players);

            GameEvents.OnLogMessage?.Invoke($"🍽 FOOD FIGHT!");
            ActivateFoodFightDrawPile();
            BeginRound(GameplayManager.Instance.CurrentPlayer);
        }

        async void OnCardDrawn(CardBase card, PlayerBase player)
        {
            playerCardInRound.Add(player, card);
            cardsDrawn.Add(card);

            // if (CancelFoodFightOnInsufficientCards())
            //     return;
            if (playerCardInRound.Count == activePlayersInRound.Count)
            {
                RoundEnd();
                return;
            }
            PlayerBase nextPlayer = GetNextPlayer();

            //Automatic FoodFight
            await Task.Delay(DELAY_BETWEEN_TURNS_IN_MS);
            foodFightDrawPile.UpdateCurrentPlayer(nextPlayer);
            drawPile.TriggerDrawBtnClick();
            //GameEvents.OnTurnChangedInFoodFight?.Invoke(nextPlayer);
        }

        PlayerBase GetNextPlayer()
        {
            for (int i = 0; i < allPlayers.Count; i++)
            {
                currentPlayerIndex = (currentPlayerIndex + 1) % allPlayers.Count;

                PlayerBase nextPlayer =
                    activePlayersInRound.Find(x => x.Index == currentPlayerIndex);

                if (nextPlayer != null)
                    return nextPlayer;
            }
            throw new InvalidOperationException("No active player found.");
        }

        void ActivateFoodFightDrawPile()
        {
            //drawPile.enabled = false;
            foodFightDrawPile = drawPile.AddComponent<FoodFightDrawPile>();
            SubscribeLocalEvents();
            foodFightDrawPile.Init(drawPile);
        }
        void DeactivateFoodFightDrawPile()
        {
            //drawPile.enabled = true;
            foodFightDrawPile.Reset();
            UnSubscribeLocalEvents();
            Destroy(foodFightDrawPile);
            foodFightDrawPile = null;
        }

        async void BeginRound(PlayerBase player)
        {
            currentPlayerIndex = player.Index;
            playerCardInRound.Clear();
            await Task.Delay(DELAY_BETWEEN_ROUNDS_IN_MS);

            //Automatic FoodFight
            foodFightDrawPile.UpdateCurrentPlayer(player);
            drawPile.TriggerDrawBtnClick();
            //GameEvents.OnTurnChangedInFoodFight?.Invoke(player);
        }

        async void RoundEnd()
        {
            activePlayersInRound = GetPromotedPlayers();
            if (activePlayersInRound.Count == 1)
            {
                GameEvents.OnLogMessage?.Invoke(activePlayersInRound[0].Name + " won the FoodFight!");
                FinishFoodFight(activePlayersInRound[0]);
            }
            else
            {
                if (CancelFoodFightOnInsufficientCards())
                    return;
                else
                {
                    await Task.Delay(DELAY_BETWEEN_ROUNDS_IN_MS);
                    BeginRound(GetNextPlayer());
                }
            }
        }

        async void FinishFoodFight(PlayerBase winner)
        {
            await Task.Delay(DELAY_AFTER_FOOD_FIGHT_IN_MS);
            DeactivateFoodFightDrawPile();
            drawPile.AddCardsBack(cardsDrawn);

            Dictionary<CardBase, int> uniqueCards = cardsDrawn.RetrieveUniqueCards();
            GameEvents.OnCardSelectionForFoodFightWinner?.Invoke(uniqueCards, winner);
            GameEvents.OnPlayerAndTurnStateChanged?.Invoke(TurnState.ActionTargetPhase, winner);

            ResetParams();
        }

        bool CancelFoodFightOnInsufficientCards()
        {
            if (drawPile.IsDrawPileEmpty || drawPile.PileCards.Count < GameplayManager.Instance.Players.Count)
            {
                GameEvents.OnLogMessage?.Invoke("Food Fight cancelled due to insifficient cards in Draw Pile!");
                if (foodFightDrawPile != null)
                {
                    DeactivateFoodFightDrawPile();
                }
                drawPile.AddCardsBack(cardsDrawn);
                trashPile.Trash(foodFightCard);
                GameEvents.OnFoodFightOver?.Invoke();
                GameEvents.OnTurnEnded?.Invoke(GameplayManager.Instance.CurrentPlayer);
                return true;
            }
            return false;
        }
        List<PlayerBase> GetPromotedPlayers()
        {
            int maxCardValue = 0;
            List<PlayerBase> promotedPlayers = new();

            for (int i = 0; i < activePlayersInRound.Count; i++)
            {
                if (playerCardInRound[activePlayersInRound[i]] is IngredientCardBase ingredientCard)
                {
                    if (ingredientCard.CardValue > maxCardValue)
                    {
                        maxCardValue = ingredientCard.CardValue;
                        promotedPlayers.Clear();
                        promotedPlayers.Add(activePlayersInRound[i]);
                    }
                    else if (ingredientCard.CardValue == maxCardValue)
                    {
                        promotedPlayers.Add(activePlayersInRound[i]);
                    }
                }
            }

            //Fallback if all players got action card in the round
            if (promotedPlayers.Count == 0)
                return activePlayersInRound;

            return promotedPlayers;
        }

        void ResetParams()
        {
            allPlayers.Clear();
            activePlayersInRound.Clear();
            cardsDrawn.Clear();
            playerCardInRound.Clear();
            currentPlayerIndex = 0;
        }

    }
}
