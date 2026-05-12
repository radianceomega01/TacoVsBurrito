
namespace TacoVsBurrito
{
    public abstract class ActionCardBase : CardBase
    {
        protected ActionResolver resolver;
        protected int noBuenoCounter = 0; 

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
        
        public virtual void ExecuteAction()
        {
            
        }
    }
}