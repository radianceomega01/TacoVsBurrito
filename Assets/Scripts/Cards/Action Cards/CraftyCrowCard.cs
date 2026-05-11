using TacoVsBurrito;
using UnityEngine;

namespace TacoVsBurrito
{
    public class CraftyCrowCard : ActionCardBase
    {
        public override void ExecuteAction()
        {
            GameEvents.OnCraftyCrowActionByPlayer?.Invoke(GameManager.Instance.CurrentPlayer);

            //resolver.ResolveCraftyCrow(GameManager.Instance.CurrentPlayer);
        }
    }
}
