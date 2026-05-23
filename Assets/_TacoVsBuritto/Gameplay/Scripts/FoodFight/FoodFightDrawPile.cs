using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TacoVsBurrito
{
    public class FoodFightDrawPile : MonoBehaviour
    {
        DrawPile drawPile;

        const float CARD_OFFSET_FROM_PILE = 12f;
        List<CardBase> cardsDrawn = new();

        public Action<CardBase, PlayerBase> OnCardDrawn;
        public List<CardBase> CardsDrawn => cardsDrawn;

        void Awake()
        {
            drawPile = GetComponent<DrawPile>();
        }

        public void Init()
        {
            drawPile.ChangeBtnListener(OnDrawBtnClicked);
        }

        void OnDrawBtnClicked()
        {
            PlayerBase currentPlayer = GameManager.Instance.CurrentPlayer;
            CardBase card = drawPile.FlipTop();
            card.ChangePosition(GetOffsetPositionForCard(currentPlayer.transform.position));
            card.SetAsLastSibbling();
            card.ToggleBackFace(false);
            cardsDrawn.Add(card);

            OnCardDrawn?.Invoke(card, currentPlayer);
        }

        public Vector3 GetOffsetPositionForCard(Vector3 playerPosition)
        {
            Vector3 modifiedPilePosition = new Vector3(playerPosition.x, transform.position.y, transform.position.z);
            Vector3 directionToPlayer = (playerPosition - modifiedPilePosition).normalized;
            Vector3 finalPosition = modifiedPilePosition + directionToPlayer * CARD_OFFSET_FROM_PILE;

            return finalPosition;
        }
        public void Reset()
        {
            cardsDrawn.ForEach(card =>
            {
                card.ChangePosition(drawPile.transform.position);
                card.ToggleBackFace(true);
            });
            //drawPile.AddCardsBack(cardsDrawn);
            drawPile.ResetBtnListener();
        }
    }
}
