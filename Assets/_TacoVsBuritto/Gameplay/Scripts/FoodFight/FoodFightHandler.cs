using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
namespace TacoVsBurrito
{
    public class FoodFightHandler : MonoBehaviour
    {
        DrawPile drawPile;
        FoodFightDrawPile foodFightDrawPile;

        List<PlayerBase> allPlayers;
        List<PlayerBase> activePlayersInRound;
        List<CardBase> cardsInDrawPile;
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
        void BeginFoodFight()
        {
            drawPile = GameManager.Instance.GetDrawPile();
            allPlayers = new(GameManager.Instance.Players);
            activePlayersInRound = new(GameManager.Instance.Players);
            cardsInDrawPile = new(drawPile.PileCards);

            if (CancelFoodFightOnInsufficientCards())
            {
                return;
            }

            EnableFoodFightDrawPile();
            BeginRound(GameManager.Instance.CurrentPlayer);
        }

        void OnCardDrawn(CardBase card, PlayerBase player)
        {
            playerCardInRound.Add(player, card);
            cardsInDrawPile.Remove(card);
            cardsDrawn.Add(card);
            if (playerCardInRound.Count == activePlayersInRound.Count)
            {
                RoundEnd();
                return;
            }
            PlayerBase nextPlayer = GetNextPlayer();
            GameEvents.OnTurnChanged?.Invoke(nextPlayer);
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
        void EnableFoodFightDrawPile()
        {
            drawPile.enabled = false;
            foodFightDrawPile = drawPile.AddComponent<FoodFightDrawPile>();
            SubscribeLocalEvents();
            foodFightDrawPile.Init();
        }
        void DisableFoodFightDrawPile()
        {
            drawPile.enabled = true;
            UnSubscribeLocalEvents();
            Destroy(foodFightDrawPile);
        }

        void BeginRound(PlayerBase player)
        {
            currentPlayerIndex = player.Index;
            GameEvents.OnTurnChanged?.Invoke(player);
        }
        void RoundEnd()
        {
            activePlayersInRound = GetPromotedPlayers();
            if (activePlayersInRound.Count == 1)
            {
                FinishFoodFight(activePlayersInRound[0]);
            }
            else
            {
                if (CancelFoodFightOnInsufficientCards())
                    return;
                else
                    BeginRound(GetNextPlayer());
            }

        }

        void FinishFoodFight(PlayerBase winner)
        {
            DisableFoodFightDrawPile();
            foodFightDrawPile.Reset();
            GameEvents.OnFoodFightFinished?.Invoke(winner);
            GameEvents.OnCardSelectionForFoodFightWinner?.Invoke(cardsDrawn.RetrieveUniqueCards());

            ResetParams();
        }

        bool CancelFoodFightOnInsufficientCards()
        {
            if (cardsInDrawPile.Count < GameManager.Instance.Players.Count)
            {
                GameEvents.OnLogMessage?.Invoke("Food Fight cancelled due to insifficient cards in Draw Pile!");
                GameEvents.OnTurnEnded?.Invoke(GameManager.Instance.CurrentPlayer);
                foodFightDrawPile.Reset();
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

            playerCardInRound.Clear();

            //Fallback if all players got action card in the round
            if (promotedPlayers.Count == 0)
                return activePlayersInRound;

            return promotedPlayers;
        }

        void ResetParams()
        {
            allPlayers.Clear();
            activePlayersInRound.Clear();
            cardsInDrawPile.Clear();
            cardsDrawn.Clear();
            playerCardInRound.Clear();
            currentPlayerIndex = 0;
        }

    }
}
