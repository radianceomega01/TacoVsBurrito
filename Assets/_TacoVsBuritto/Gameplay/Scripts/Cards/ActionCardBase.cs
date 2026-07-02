
using System;
using MoreMountains.Feedbacks;
using NUnit.Framework;
using UnityEngine;

namespace TacoVsBurrito
{
    public abstract class ActionCardBase : CardBase
    {
        [Header("Feel - MMF Players")]
        [SerializeField] private MMF_Player feedbackCardPlayed;
        [SerializeField] private MMFRandomRotation mmfRandomRotation;

        protected const int EXECUTION_DEALY_IN_MS = 500;
        public virtual int FEEL_ANIM_DELAY_IN_MS => 1500;
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

        public virtual void PlayCardPlayedEffect() => feedbackCardPlayed?.PlayFeedbacks();
        
        public virtual void ExecuteAction()
        {
            mmfRandomRotation?.ModifyValue();
        }
        public virtual bool CanExecuteAction() => true;
        public virtual TurnState GetStateOnPlayed() => TurnState.ActionResolvePhase; 
    }
}