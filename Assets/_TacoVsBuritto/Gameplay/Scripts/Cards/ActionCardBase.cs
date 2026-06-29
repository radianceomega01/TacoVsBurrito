
using System;
using MoreMountains.Feedbacks;
using NUnit.Framework;
using UnityEngine;

namespace TacoVsBurrito
{
    public abstract class ActionCardBase : CardBase
    {
        [Header("Feel - MMF Players")]
        public MMF_Player feedbackCardPlayed;

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

        public void PlayCardPlayedEffect() => feedbackCardPlayed?.PlayFeedbacks();
        
        public abstract void ExecuteAction();
        public virtual bool CanExecuteAction() => true;
        public virtual TurnState GetStateOnPlayed() => TurnState.ActionResolvePhase; 
    }
}