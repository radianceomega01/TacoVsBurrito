using UnityEngine;

namespace TacoVsBurrito
{
    public interface ITargetTypeAction
    {
        void OnActionTargeted(TargetTypeContext targetTypeContext);
        void ResolveAction();
    }

    public struct TargetTypeContext
    {
        public PlayerBase caster;
        public PlayerBase victim;
        public CardBase cardTargeted;

        public TargetTypeContext(PlayerBase caster,PlayerBase victim,CardBase cardTargeted)
        {
            this.caster = caster;
            this.victim = victim;
            this.cardTargeted = cardTargeted;
        }
    }
}
