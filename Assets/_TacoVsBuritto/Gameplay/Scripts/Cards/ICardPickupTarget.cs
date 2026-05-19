using TacoVsBurrito;
using UnityEngine;

namespace TacoVsBurrito
{
    public interface ICardPickupTarget
    {
        public abstract void PickCardBeforeDrag(CardBase card);
        public abstract void ReturnCardOnNoTarget(CardBase card);
    }
}
