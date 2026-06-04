using TacoVsBurrito;
using UnityEngine;

namespace TacoVsBurrito
{
    public interface ICardPickupTarget
    {
        public void PickCardBeforeDrag(CardBase card);
        public void ReturnCardOnNoTarget(CardBase card);
        public void SetCardHolderToCard(CardBase card);
    }
}
