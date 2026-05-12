using TacoVsBurrito;
using UnityEngine;

namespace TacoVsBurrito
{
    public class CraftyCrowCard : ActionCardBase
    {
        protected override void Awake() {
            base.Awake();
            GameEvents.OnCraftyCrowActionTargeted += manageActionTargeted;
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            GameEvents.OnCraftyCrowActionTargeted -= manageActionTargeted;
        }
        public override void ExecuteAction()
        {
            GameEvents.OnCraftyCrowActionByPlayer?.Invoke(GameManager.Instance.CurrentPlayer);
        }
        void manageActionTargeted(PlayerBase caster, PlayerBase victim, CardBase card)
        {
            resolver.ResolveCraftyCrow(caster, victim, card);
        }
    }
}
