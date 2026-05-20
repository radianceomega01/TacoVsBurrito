using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TacoVsBurrito
{
    public class FoodFightDrawPile : MonoBehaviour
    {
        DrawPile drawPile;

        const float CARD_OFFSET_FROM_PLAYER = 10f;
        List<CardBase> cardsDrawn = new();

        public Action<CardBase, PlayerBase> OnCardDrawn;

        void Awake()
        {
            drawPile = GetComponent<DrawPile>();
        }

        public void Init()
        {
            drawPile.RemoveBtnListener();
            drawPile.AddBtnListener(OnDrawBtnClicked);

        }

        void OnDrawBtnClicked()
        {
            PlayerBase currentPlayer = GameManager.Instance.CurrentPlayer;
            CardBase card = drawPile.Draw();
            card.ChangePosition(GetOffsetPositionForCard(currentPlayer.transform.position));
            card.ToggleBackFace(false);
            cardsDrawn.Add(card);

            OnCardDrawn?.Invoke(card, currentPlayer);
        }
        public Vector3 GetOffsetPositionForCard(Vector3 playerPosition)
        {
            Vector3 directionToDeck = Vector3.up * (transform.position - playerPosition).normalized.y;
            Vector3 finalPosition = playerPosition + directionToDeck * CARD_OFFSET_FROM_PLAYER;

            return finalPosition;
        }
        public void ResetCards()
        {
            cardsDrawn.ForEach(card =>
            {
                card.ChangePosition(drawPile.transform.position);
                card.ToggleBackFace(true);
            });
        }
    }
}
