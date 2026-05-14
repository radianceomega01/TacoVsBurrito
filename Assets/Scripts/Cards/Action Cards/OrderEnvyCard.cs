using TacoVsBurrito;
using UnityEngine;

namespace TacoVsBurrito
{
    public class OrderEnvyCard : ActionCardBase
    {
        protected override void Awake() {
            base.Awake();
            GameEvents.OnOrderEnvyActionTargeted += ManageActionTargeted;
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            GameEvents.OnOrderEnvyActionTargeted -= ManageActionTargeted;
        }
        public override void ExecuteAction()
        {
            GameEvents.OnOrderEnvyAction?.Invoke(GameManager.Instance.CurrentPlayer);
        }
        void ManageActionTargeted(PlayerBase caster, PlayerBase victim)
        {
            resolver.ResolveOrderEnvy(caster, victim);
        }
    }
}
