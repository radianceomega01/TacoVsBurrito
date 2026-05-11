
namespace TacoVsBurrito
{
    public abstract class ActionCardBase : CardBase
    {
        protected ActionResolver resolver;

        protected override void Awake() {
            base.Awake();
            GameEvents.OnActionResolverSet += ManageActionResolverSet;
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            GameEvents.OnActionResolverSet -= ManageActionResolverSet;
        }
        protected void ManageActionResolverSet(ActionResolver resolver)
        {
            this.resolver = resolver;
        }
        
        public abstract void ExecuteAction();
    }
}