using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TacoVsBurrito
{
    public class TrashPileCardSelector : MonoBehaviour
    {
        [SerializeField] Transform cardsParent;
        [SerializeField] Transform rootTransform;

        List<CardBase> trashPileCards;
        void Awake()
        {
            GameEvents.OnTrashPandaAction += ManageTrashPandaAction;
            GameEvents.OnCardClickedForActionTarget += CardClickedForActionTarget;
            GameEvents.OnStartNoBuenoInterruptWindow += ManageNoBuenoWindowStarted;
        }
        void OnDestroy()
        {
            GameEvents.OnTrashPandaAction -= ManageTrashPandaAction;
            GameEvents.OnCardClickedForActionTarget -= CardClickedForActionTarget;
            GameEvents.OnStartNoBuenoInterruptWindow -= ManageNoBuenoWindowStarted;
        }

        void ManageTrashPandaAction(Dictionary<CardBase, int> cardMap)
        {
            rootTransform.gameObject.SetActive(true);

            List<Vector3> positions =
            GenerateCardPositions(
                startPosition: cardsParent.position,
                cardCount: cardMap.Count,
                horizontalSpacing: 10f,
                verticalSpacing: 14f);

            trashPileCards = cardMap.Keys.ToList();
            for(int i=0; i<trashPileCards.Count; i++)
            {
                trashPileCards[i].ToggleInteractionType();
                trashPileCards[i].EnableInteraction();
                trashPileCards[i].ChangePosition(positions[i]);
                trashPileCards[i].transform.SetParent(cardsParent);
            }
        }
        void CardClickedForActionTarget(CardBase card)
        {
            if(trashPileCards == null)
                return;

            trashPileCards.ForEach(card => card.ToggleInteractionType());
             
            GameManager.Instance.GetTrashPile().PutCardsBack(trashPileCards);    
            GameEvents.OnTrashPileCardTargeted?.Invoke(card);

            ResetParams();  
        }

        void ManageNoBuenoWindowStarted(ActionCardBase actionCard)
        {
            //ResetParams();
        }
        
        void ResetParams()
        {
            trashPileCards.Clear();
            rootTransform.gameObject.SetActive(false);
        }

        static List<Vector3> GenerateCardPositions(Vector3 startPosition, int cardCount, float horizontalSpacing = 2f, float verticalSpacing = 2.5f)
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

