using UnityEngine;

namespace TacoVsBurrito
{
    //Used to make sure only one card can be dragged from player hand
    public static class DragManager
    {
        public static CardBase ActiveCard { get; private set; }

        public static bool TryStartDrag(CardBase card)
        {
            if (ActiveCard != null)
                return false;

            ActiveCard = card;
            return true;
        }

        public static void EndDrag(CardBase card)
        {
            if (ActiveCard == card)
                ActiveCard = null;
        }
    }
}