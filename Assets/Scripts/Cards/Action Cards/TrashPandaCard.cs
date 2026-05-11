using TacoVsBurrito;
using UnityEngine;
using System.Collections.Generic;

namespace TacoVsBurrito
{
    public class TrashPandaCard : ActionCardBase
    {
        TrashPile trashPile;

        public override void ExecuteAction()
        {
            trashPile = GameManager.Instance.GetTrashPile();
            CardBase card = trashPile.RetrieveFromTrash();
            if (card != null) // more than just the Trash Panda itself
            {
                resolver.ResolveTrashPanda(GameManager.Instance.CurrentPlayer, card);
            }
            else
            {
                GameEvents.OnTurnEnded?.Invoke(GameManager.Instance.CurrentPlayer); // skip play phase 
            }
        }
    }
}
