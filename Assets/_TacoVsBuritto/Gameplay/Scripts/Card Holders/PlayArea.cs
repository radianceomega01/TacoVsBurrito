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
        private CardBase currentPlayedCard;
        private TrashPile trashPile;

        private const int TRASH_DEALY_IN_MS = 500;

        protected override void Awake()
        {
            base.Awake();
            GameEvents.OnTurnStateChanged += ManageTurnStateChanged;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            GameEvents.OnTurnStateChanged -= ManageTurnStateChanged;
        }

        void Start()
        {
            trashPile = GameManager.Instance.GetTrashPile();
        }

        public async void PlayAction(CardBase card)
        {
            SetCardOnPile(card);
            if (card is ActionCardBase @actionCard)
            {
                GameEvents.OnActionCardPlayed?.Invoke(@actionCard);
            }
            else
            {
                await Task.Delay(TRASH_DEALY_IN_MS);
                trashPile.Trash(card);
                GameEvents.OnTurnEnded?.Invoke(GameManager.Instance.CurrentPlayer);
            }
        }
        void SetCardOnPile(CardBase card)
        {
            currentPlayedCard = card;
            card.ChangeScale(CARD_SCALE);
            card.ChangePosition(cardsParent.position);
            card.ChangeParent(cardsParent);
            card.DisableInteraction();
            card.ToggleInteractionType(InteractionType.Click);
            card.ToggleBackFace(false);
        }

        public bool CanDrop(CardBase card)
        {
            if (currentTurnState == TurnState.NoBuenoWindowPhase && card is not NoBuenoCard)
                return false;

            return true;
        }
        public void DropCardAfterDrag(CardBase card)
        {
            PlayAction(card);
        }

        void ManageTurnStateChanged(TurnState state, PlayerBase player)
        {
            currentTurnState = state;
            if(state == TurnState.ActionResolvePhase)
            {
                currentPlayedCard = null;
            }
        }
    }
}
