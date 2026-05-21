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
    }
}
