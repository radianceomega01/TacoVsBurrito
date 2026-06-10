
using System;
using NUnit.Framework;
using UnityEngine;

namespace TacoVsBurrito
{
    public abstract class ActionCardBase : CardBase
    {
        protected const int EXECUTION_DEALY_IN_MS = 500;
        protected ActionResolver resolver;

        protected override void OnEnable() {
            base.OnEnable();
            GameEvents.OnActionResolverSet += ManageActionResolverSet;
        }

        protected override void OnDisable() {
            base.OnDisable();
            GameEvents.OnActionResolverSet -= ManageActionResolverSet;
        }
        protected void ManageActionResolverSet(ActionResolver resolver)
        {
            this.resolver = resolver;
        }
        
        public abstract void ExecuteAction();
        public virtual bool CanExecuteAction() => true;
        public virtual TurnState GetStateOnPlayed() => TurnState.ActionResolvePhase; 
    }
}