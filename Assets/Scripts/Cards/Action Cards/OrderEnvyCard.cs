using TacoVsBurrito;
using UnityEngine;

namespace TacoVsBurrito
{
    public class OrderEnvyCard : ActionCardBase
    {
        protected override void Awake() {
            base.Awake();
            GameEvents.OnOrderEnvyActionTargeted += manageActionTargeted;
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            GameEvents.OnOrderEnvyActionTargeted -= manageActionTargeted;
        }
        public override void ExecuteAction()
        {
            GameEvents.OnOrderEnvyAction?.Invoke(GameManager.Instance.CurrentPlayer);
        }
        void manageActionTargeted(PlayerBase caster, PlayerBase victim)
        {
            resolver.ResolveOrderEnvy(caster, victim);
        }
    }
}
