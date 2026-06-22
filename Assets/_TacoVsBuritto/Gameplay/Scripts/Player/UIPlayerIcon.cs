
using System;
using UnityEngine;
using UnityEngine.UI;

namespace TacoVsBurrito
{
    public class UIPlayerIcon : MonoBehaviour, IGlowEntity
    {
        [SerializeField] Button iconButton;
        [SerializeField] GlowBGUI glowBG;
        private PlayerBase parentPlayer;

        void OnEnable()
        {
            GameEvents.OnOrderEnvyAction += ManageOrderEnvyAction;
            GameEvents.OnOrderEnvyActionTargeted += ManageOrderEnvyActionTargeted;
            GameEvents.OnActionResolved += ManageActionResolved;
            GameEvents.OnTurnStarted += ManageTurnStarted;
            GameEvents.OnTurnEnded += ManageTurnEnded;
            GameEvents.OnTurnChangedAfterFoodFight += ManageTurnChangedAfterFoodFight;
        }

        void OnDisable()
        {
            GameEvents.OnOrderEnvyAction -= ManageOrderEnvyAction;
            GameEvents.OnOrderEnvyActionTargeted -= ManageOrderEnvyActionTargeted;
            GameEvents.OnActionResolved -= ManageActionResolved;
            GameEvents.OnTurnStarted -= ManageTurnStarted;
            GameEvents.OnTurnEnded -= ManageTurnEnded;
            GameEvents.OnTurnChangedAfterFoodFight -= ManageTurnChangedAfterFoodFight;
        }

        void Start()
        {
            parentPlayer = GetComponentInParent<PlayerBase>();
            ToggleInteraction(false);
        }

        public void OnIconClick()
        {
            GameEvents.OnOrderEnvyActionTargeted?.Invoke(new TargetTypeContext(GameplayManager.Instance.CurrentPlayer, parentPlayer, null));
            GameEvents.OnGUIClickSFX?.Invoke();
        }

        void ManageOrderEnvyAction(PlayerBase player)
        {
            if (parentPlayer == player)
                return;
            ToggleInteraction(true);
            ActivateGlow();
        }

        //For self player who had interaction enabled
        void ToggleInteraction(bool value)
        {
            iconButton.interactable = value;
        }

        //For other players who had interaction enabled
        void ManageOrderEnvyActionTargeted(TargetTypeContext targetTypeContext)
        {
            if (parentPlayer == targetTypeContext.caster)
                return;
            ToggleInteraction(false);

            if (parentPlayer != targetTypeContext.victim)
                DeactivateGlow();
        }
        private void ManageTurnStarted(PlayerBase player)
        {
            if (parentPlayer.Index == player.Index)
            {
                ActivateGlow();
            }
        }

        void ManageTurnEnded(PlayerBase player)
        {
            if (glowBG.IsGlowActive)
            {
                DeactivateGlow();
            }
        }

        private void ManageTurnChangedAfterFoodFight(PlayerBase player)
        {
            if (parentPlayer.Index == player.Index)
            {
                if (!glowBG.IsGlowActive)
                {
                    ActivateGlow();
                }
            }
            else
            {
                if (glowBG.IsGlowActive)
                {
                    DeactivateGlow();
                }
            }
        }

        void ManageActionResolved(ActionCardBase actionCard)
        {
            if (actionCard is not OrderEnvyCard)
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
