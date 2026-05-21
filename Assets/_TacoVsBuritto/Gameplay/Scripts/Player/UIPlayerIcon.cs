
using System;
using UnityEngine;
using UnityEngine.UI;

namespace TacoVsBurrito
{
    public class UIPlayerIcon : MonoBehaviour
    {
        [SerializeField] Image iconImage;
        private PlayerBase parentPlayer;

        void Awake()
        {
            GameEvents.OnOrderEnvyAction += ManageOrderEnvyAction;
            GameEvents.OnOrderEnvyActionTargeted += ManageOrderEnvyActionTargeted;
        }

        void OnDestroy()
        {
            GameEvents.OnOrderEnvyAction -= ManageOrderEnvyAction;
            GameEvents.OnOrderEnvyActionTargeted -= ManageOrderEnvyActionTargeted;
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
        }
    }
}
