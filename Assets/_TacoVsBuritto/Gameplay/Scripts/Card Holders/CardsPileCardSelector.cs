using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TacoVsBurrito
{
    public class CardsPileCardSelector : MonoBehaviour
    {
        [SerializeField] Transform cardsParent;
        [SerializeField] Transform rootTransform;

        List<CardBase> pileCards;
        CardPileBase cardPile;
        void OnEnable()
        {
            GameEvents.OnTrashPandaAction += ManageTrashPandaAction;
            GameEvents.OnCardClickedForActionTarget += CardClickedForActionTarget;
            GameEvents.OnCardSelectionForFoodFightWinner += ManageCardSelectionForFoodFightWinner;
        }
        void OnDisable()
        {
            GameEvents.OnTrashPandaAction -= ManageTrashPandaAction;
            GameEvents.OnCardClickedForActionTarget -= CardClickedForActionTarget;
            GameEvents.OnCardSelectionForFoodFightWinner -= ManageCardSelectionForFoodFightWinner;
        }

        void ManageTrashPandaAction(Dictionary<CardBase, int> cardMap)
        {
            cardPile = GameManager.Instance.GetTrashPile();
            ArrageCardsToSelect(cardMap, GameManager.Instance.CurrentPlayer);
        }

        void ManageCardSelectionForFoodFightWinner(Dictionary<CardBase, int> cardMap, PlayerBase winner)
        {
            cardPile = GameManager.Instance.GetDrawPile();
            ArrageCardsToSelect(cardMap, winner);
        }


        void ArrageCardsToSelect(Dictionary<CardBase, int> cardMap, PlayerBase selectingPlayer)
        {
            rootTransform.gameObject.SetActive(true);

            List<Vector3> positions = GenerateCardPositions(cardsParent.position, cardMap.Count);

            pileCards = cardMap.Keys.ToList();
            for(int i=0; i<pileCards.Count; i++)
            {
                pileCards[i].ToggleBackFace(false);
                pileCards[i].ChangePosition(positions[i]);
                pileCards[i].transform.SetParent(cardsParent);

                if(selectingPlayer is  SelfPlayer)
                    pileCards[i].EnableInteraction();
                else
                    pileCards[i].DisableInteraction();
            }
        }

        void CardClickedForActionTarget(CardBase selectedCard)
        {
            if(pileCards == null || !rootTransform.gameObject.activeInHierarchy)
                return;

            cardPile.PutCardsBack(pileCards);
            cardPile.RemoveCard(selectedCard);
            GameEvents.OnCardsPileCardTargeted?.Invoke(new TargetTypeContext(GameManager.Instance.CurrentPlayer, null, selectedCard));

            ResetParams();  
        }
        
        void ResetParams()
        {
            pileCards.Clear();
            rootTransform.gameObject.SetActive(false);
        }

        static List<Vector3> GenerateCardPositions(Vector3 startPosition, int cardCount, float horizontalSpacing = 10f, float verticalSpacing = 14f)
        {
            List<Vector3> positions = new();

            if (cardCount <= 0 || cardCount > 9)
                return positions;

            int remainingCards = cardCount;
            int currentRow = 0;

            while (remainingCards > 0)
            {
                // Maximum 3 cards per row
                int cardsInRow = Mathf.Min(3, remainingCards);

                // Center the row horizontally
                float rowWidth = (cardsInRow - 1) * horizontalSpacing;
                float startX = startPosition.x - (rowWidth / 2f);

                for (int col = 0; col < cardsInRow; col++)
                {
                    Vector3 pos = new Vector3(
                        startX + (col * horizontalSpacing),
                        startPosition.y - (currentRow * verticalSpacing),
                        startPosition.z);

                    positions.Add(pos);
                }

                remainingCards -= cardsInRow;
                currentRow++;
            }

            return positions;
        }
    }
}

