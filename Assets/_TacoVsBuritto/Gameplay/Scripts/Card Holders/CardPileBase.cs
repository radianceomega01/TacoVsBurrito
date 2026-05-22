using System.Collections.Generic;
using UnityEngine;

namespace TacoVsBurrito
{
    public abstract class CardPileBase : MonoBehaviour
    {
        [SerializeField] protected Transform cardsParent;

        protected List<CardBase> pileCards = new();
        public IReadOnlyList<CardBase> PileCards => pileCards;

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
                card.ChangeSiblingIndex(transform.parent.childCount - 1);
            }
        }
        public void RemoveCard(CardBase card)
        {
            pileCards.Remove(card);
        }
    }
}
