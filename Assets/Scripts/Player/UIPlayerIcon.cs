
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
        }

        void OnDestroy()
        {
            GameEvents.OnOrderEnvyAction -= ManageOrderEnvyAction;
        }

        void Start()
        {
            parentPlayer = GetComponentInParent<PlayerBase>();
        }

        public void OnIconClick()
        {
            GameEvents.OnOrderEnvyActionTargeted?.Invoke(GameManager.Instance.CurrentPlayer, parentPlayer);
        }

        void EnableInteraction()
        {
            iconImage.raycastTarget = true;
        }
        void ManageOrderEnvyAction(PlayerBase player)
        {
            if(parentPlayer == player)
                return;
            EnableInteraction();
        }
    }
}
