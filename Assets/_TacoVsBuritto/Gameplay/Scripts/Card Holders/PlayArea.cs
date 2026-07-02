using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace TacoVsBurrito
{
    public class PlayArea : CardPileBase, ICardDropTarget
    {
        private TurnState currentTurnState;
        private TrashPile trashPile;

        private const int TRASH_DEALY_IN_MS = 500;

        void OnEnable()
        {
            GameEvents.OnTurnStateChanged += ManageTurnStateChanged;
        }

        void OnDisable()
        {
            GameEvents.OnTurnStateChanged -= ManageTurnStateChanged;
        }

        void Start()
        {
            trashPile = GameplayManager.Instance.GetTrashPile();
        }

        public async void PlayAction(CardBase card)
        {
            SetCardOnPile(card);
            await Task.Delay(card.GetTimeToMove());
            if (card is ActionCardBase @actionCard)
            {
                GameEvents.OnActionCardPlayed?.Invoke(@actionCard);
            }
            else
            {
                await Task.Delay(TRASH_DEALY_IN_MS);
                trashPile.Trash(card);
                GameEvents.OnTurnEnded?.Invoke(GameplayManager.Instance.CurrentPlayer);
            }
        }
        async void SetCardOnPile(CardBase card)
        {
            //These are handled 
            if(card is not ActionCardBase) card.ChangeScale(CARD_SCALE);
            if(card is not ActionCardBase) card.ChangeRotation(Quaternion.identity);

            card.ChangePosition(cardsParent.position);
            card.DisableInteraction();
            card.ToggleInteractionType(InteractionType.Click);
            card.ToggleBackFace(false);

            if(card is ActionCardBase @actionCard) await Task.Delay(@actionCard.FEEL_ANIM_DELAY_IN_MS);
            card.ChangeParent(cardsParent);
        }

        public bool CanDrop(CardBase card)
        {
            if (currentTurnState == TurnState.NoBuenoWindowPhase && card is not NoBuenoCard)
                return false;
            if(DragManager.ActiveCard != card)
                return false;
                
            return true;
        }
        public void DropCardAfterDrag(CardBase card)
        {
            PlayAction(card);
            if(card is ActionCardBase @actionCard)
            {
                @actionCard.PlayCardPlayedEffect();
            }
            else
            {
                card.PlayDropEffect();
            }
            GameEvents.OnCardMovedSFX?.Invoke();
        }

        void ManageTurnStateChanged(TurnState state, PlayerBase player)
        {
            currentTurnState = state;
        }
    }
}
