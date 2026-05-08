using TacoVsBurrito;
using UnityEngine;

namespace TacoVsBurrito
{
    public interface ICardPickupTarget
    {
        public abstract void PickCardBeforeDrag(CardBase card);
    }
}
