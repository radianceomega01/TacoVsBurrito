using System;
using System.Collections.Generic;
using UnityEngine;

namespace TacoVsBurrito
{
    public abstract class CardPileBase : MonoBehaviour
    {
        [SerializeField] protected Transform cardsParent;

        protected const float CARD_SCALE = 1f;
        protected List<CardBase> pileCards = new();
        public IReadOnlyList<CardBase> PileCards => pileCards;

        protected virtual void Awake()
        {
            GameEvents.OnFoodFightAction += ManagePileOnFoodFight;
            GameEvents.OnFoodFightOver += ManagePileOnFoodFightOver;
        }

        protected virtual void OnDestroy()
        {
            GameEvents.OnFoodFightAction -= ManagePileOnFoodFight;
            GameEvents.OnFoodFightOver -= ManagePileOnFoodFightOver;
        }

        public virtual void PutCardsBack(List<CardBase> cards)
        {
            foreach (var card in cards)
            {
                card.ChangePosition(cardsParent.position);
                card.ChangeParent(cardsParent);
                card.DisableInteraction();
            }
        }
        public virtual void AddCardsBack(List<CardBase> cards)
        {
            foreach (var card in cards)
            {
                pileCards.Add(card);
                card.ChangeScale(CARD_SCALE);
                card.ChangeSiblingIndex(transform.parent.childCount - 1);
            }
        }
        public void RemoveCard(CardBase card)
        {
            pileCards.Remove(card);
        }
        protected virtual void ManagePileOnFoodFight(FoodFightCard card)
        {
            gameObject.SetActive(false);
        }
        protected virtual void ManagePileOnFoodFightOver()
        {
            gameObject.SetActive(true);
        }
    }
}
