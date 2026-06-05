using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace TacoVsBurrito
{
    public class FoodFightDrawPile : MonoBehaviour
    {
        DrawPile drawPile;
        RectTransform drawPileRect;
        const float CARD_DRAWN_OFFSET_FROM_PILE = 14f;
        List<CardBase> cardsDrawn = new();
        PlayerBase currentPlayer;

        private const float PILE_POS_ON_FOOD_FIGHT = 0f;
        private float originalPilePosX;
        private const float MOVE_TIME_IN_IN_SECS = 0.2f;
        private const int REPOSITIONING_DELAY_IN_MS = 500;

        public Action<CardBase, PlayerBase> OnCardDrawn;
        public List<CardBase> CardsDrawn => cardsDrawn;

        public void Init(DrawPile drawPile)
        {
            this.drawPile = drawPile;
            drawPileRect = drawPile.GetRectTransform();
            drawPile.ChangeBtnListener(OnDrawBtnClicked);
            ManageDrawPileOnFoodFight();
            currentPlayer = GameManager.Instance.CurrentPlayer;
        }

        public void OnDrawBtnClicked()
        {
            CardBase card = drawPile.FlipTop();
            card.ChangePosition(GetOffsetPositionForCard(currentPlayer.transform.position));
            card.SetAsLastSibbling();
            card.ToggleBackFace(false);
            cardsDrawn.Add(card);

            OnCardDrawn?.Invoke(card, currentPlayer);
            GameEvents.OnCardMovedSFX?.Invoke();
        }

        public Vector3 GetOffsetPositionForCard(Vector3 playerPosition)
        {
            Vector3 modifiedPilePosition = new Vector3(playerPosition.x, transform.position.y, transform.position.z);
            Vector3 directionToPlayer = (playerPosition - modifiedPilePosition).normalized;
            Vector3 finalPosition = modifiedPilePosition + directionToPlayer * CARD_DRAWN_OFFSET_FROM_PILE;

            return finalPosition;
        }

        public void UpdateCurrentPlayer(PlayerBase player) => currentPlayer = player;

        async void ManageDrawPileOnFoodFight()
        {
            originalPilePosX = drawPileRect.localPosition.x;
            drawPileRect.DOAnchorPosX(PILE_POS_ON_FOOD_FIGHT, MOVE_TIME_IN_IN_SECS);
        }
        async void ManageDrawPileOnFoodFightOver()
        {
            await Task.Delay(REPOSITIONING_DELAY_IN_MS);
            drawPileRect.DOAnchorPosX(originalPilePosX, MOVE_TIME_IN_IN_SECS);
        }
        
        public void Reset()
        {
            ManageDrawPileOnFoodFightOver();
            cardsDrawn.ForEach(card =>
            {
                card.ChangePosition(drawPile.transform.position);
                card.ToggleBackFace(true);
            });
            drawPile.ResetBtnListener();
        }
    }
}
