
using System;
using NUnit.Framework;
using UnityEngine;

namespace TacoVsBurrito
{
    public abstract class ActionCardBase : CardBase
    {
        public bool IsTargetTypeAction => GetStateOnTrashed() == TurnState.ActionTargetPhase;
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
        public virtual TurnState GetStateOnTrashed() => TurnState.ActionResolvePhase; 
    }
}