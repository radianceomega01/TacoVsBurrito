
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
            GameEvents.OnOrderEnvyActionTargeted?.Invoke(GameManager.Instance.CurrentPlayer, parentPlayer);
            ToggleInteraction(false);
        }

        void ToggleInteraction(bool value)
        {
            iconImage.raycastTarget = value;
        }
        void ManageOrderEnvyAction(PlayerBase player)
        {
            if(parentPlayer == player)
                return;
            ToggleInteraction(true);
        }
        void ManageOrderEnvyActionTargeted(PlayerBase player, PlayerBase victim)
        {
            if(parentPlayer == player)
                return;
            ToggleInteraction(false);
        }
    }
}
