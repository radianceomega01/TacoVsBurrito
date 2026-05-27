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
        const int DELAY_BETWEEN_TURNS_IN_MS = 1000;
        const int DELAY_AFTER_FOOD_FIGHT_IN_MS = 1500;
        DrawPile drawPile;
        TrashPile trashPile;
        FoodFightDrawPile foodFightDrawPile;
        FoodFightCard foodFightCard;

        List<PlayerBase> allPlayers;
        List<PlayerBase> activePlayersInRound;
        List<CardBase> cardsDrawn = new();
        Dictionary<PlayerBase, CardBase> playerCardInRound = new();
        int currentPlayerIndex;

        private void Awake()
        {
            GameEvents.OnFoodFightAction += BeginFoodFight;
        }

        private void OnDestroy()
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
            drawPile = GameManager.Instance.GetDrawPile();
            trashPile = GameManager.Instance.GetTrashPile();
            allPlayers = new(GameManager.Instance.Players);
            activePlayersInRound = new(GameManager.Instance.Players);

            if (CancelFoodFightOnInsufficientCards())
            {
                return;
            }

            GameEvents.OnLogMessage?.Invoke($"🍽 FOOD FIGHT!");
            ActivateFoodFightDrawPile();
            BeginRound(GameManager.Instance.CurrentPlayer);
        }

        void OnCardDrawn(CardBase card, PlayerBase player)
        {
            playerCardInRound.Add(player, card);
            cardsDrawn.Add(card);
            if (playerCardInRound.Count == activePlayersInRound.Count)
            {
                RoundEnd();
                return;
            }
            PlayerBase nextPlayer = GetNextPlayer();
            GameEvents.OnTurnChangedInFoodFight?.Invoke(nextPlayer);
        }

        PlayerBase GetNextPlayer()
        {
            currentPlayerIndex = (currentPlayerIndex + 1) % allPlayers.Count;
            PlayerBase nextPlayer = activePlayersInRound.Find(x => x.Index == currentPlayerIndex);
            if (nextPlayer == null)
            {
                GetNextPlayer();
            }
            return nextPlayer;

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
            await Task.Delay(DELAY_BETWEEN_TURNS_IN_MS);
            GameEvents.OnTurnChangedInFoodFight?.Invoke(player);
        }

        void RoundEnd()
        {
            activePlayersInRound = GetPromotedPlayers();
            if (activePlayersInRound.Count == 1)
            {
                GameEvents.OnLogMessage?.Invoke(activePlayersInRound[0].GetType() + " won the FoodFight!");
                FinishFoodFight(activePlayersInRound[0]);
            }
            else
            {
                if (CancelFoodFightOnInsufficientCards())
                    return;
                else
                {
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
            if (drawPile.PileCards.Count < GameManager.Instance.Players.Count)
            {
                GameEvents.OnLogMessage?.Invoke("Food Fight cancelled due to insifficient cards in Draw Pile!");
                GameEvents.OnTurnEnded?.Invoke(GameManager.Instance.CurrentPlayer);
                if(foodFightDrawPile != null)
                {
                    DeactivateFoodFightDrawPile();
                    drawPile.AddCardsBack(cardsDrawn);
                }
                trashPile.Trash(foodFightCard);
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
