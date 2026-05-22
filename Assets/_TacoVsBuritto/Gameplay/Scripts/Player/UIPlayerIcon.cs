
using System;
using UnityEngine;
using UnityEngine.UI;

namespace TacoVsBurrito
{
    public class UIPlayerIcon : MonoBehaviour, IGlowEntity
    {
        [SerializeField] Image iconImage;
        [SerializeField] GlowBGUI glowBG;
        private PlayerBase parentPlayer;

        void Awake()
        {
            GameEvents.OnOrderEnvyAction += ManageOrderEnvyAction;
            GameEvents.OnOrderEnvyActionTargeted += ManageOrderEnvyActionTargeted;
            GameEvents.OnActionResolved += ManageActionResolved;
        }

        void OnDestroy()
        {
            GameEvents.OnOrderEnvyAction -= ManageOrderEnvyAction;
            GameEvents.OnOrderEnvyActionTargeted -= ManageOrderEnvyActionTargeted;
            GameEvents.OnActionResolved -= ManageActionResolved;
        }

        void Start()
        {
            parentPlayer = GetComponentInParent<PlayerBase>();
        }

        public void OnIconClick()
        {
            GameEvents.OnOrderEnvyActionTargeted?.Invoke(new TargetTypeContext(GameManager.Instance.CurrentPlayer, parentPlayer, null));
        }
        
        void ManageOrderEnvyAction(PlayerBase player)
        {
            if(parentPlayer == player)
                return;
            ToggleInteraction(true);
            ActivateGlow();
        }

        //For self player who had interaction enabled
        void ToggleInteraction(bool value)
        {
            iconImage.raycastTarget = value;
        }

        //For other players who had interaction enabled
        void ManageOrderEnvyActionTargeted(TargetTypeContext targetTypeContext)
        {
            if(parentPlayer == targetTypeContext.caster)
                return;
            ToggleInteraction(false);

            if(parentPlayer != targetTypeContext.victim)
                DeactivateGlow();
        }
        void ManageActionResolved(ActionCardBase actionCard)
        {
            if(actionCard is not OrderEnvyCard)
                return;
            DeactivateGlow();
        }

        public void ActivateGlow()
        {
            glowBG.ShowEffect();
        }

        public void DeactivateGlow()
        {
            glowBG.Reset();
        }
    }
}
