
using System;
using NUnit.Framework;
using UnityEngine;

namespace TacoVsBurrito
{
    public abstract class ActionCardBase : CardBase
    {
        [SerializeField] protected bool requiresInputToResolve = false;

        public bool RequiresInputToResolve => requiresInputToResolve;
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