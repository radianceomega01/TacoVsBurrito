using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
namespace TacoVsBurrito
{
    public class AIPlayer : PlayerBase
    {
        [SerializeField] AIDifficulty difficulty = AIDifficulty.Hard;
        private AIBrain aIBrain;
        private DrawPile drawPile;
        private TrashPile trashPile;
        private IReadOnlyList<PlayerBase> _players => GameManager.Instance.Players;

        const int CARD_DRAW_DELAY_IN_MS = 1000;
        const int THINKING_DELAY_IN_MS = 2000;

        bool isSelfTurnRunning = false;

        protected override void Awake()
        {
            base.Awake();
            aIBrain = transform.AddComponent<AIBrain>();

            GameEvents.OnTurnStarted += ManageOnTurnStarted;
            GameEvents.OnTurnStateChanged += ManageOnTurnStateChanged;
            GameEvents.OnActionCardTrashed += ManageActionCardTrashed;
        }

        void OnDestroy()
        {
            GameEvents.OnTurnStarted -= ManageOnTurnStarted;
            GameEvents.OnTurnStateChanged -= ManageOnTurnStateChanged;
            GameEvents.OnActionCardTrashed -= ManageActionCardTrashed;
        }
        
        void Start()
        {
            drawPile = GameManager.Instance.GetDrawPile();
            trashPile = GameManager.Instance.GetTrashPile();
        }

        public AIBrain GetBrain() => aIBrain;
        async void ManageOnTurnStarted(PlayerBase player)
        {
            if(player is not AIPlayer)
                return;
            DrawACard();
        }

        async void DrawACard()
        {
            await Task.Delay(CARD_DRAW_DELAY_IN_MS);
            drawPile.OnDrawBtnClicked();
        }

        async void PlayACard()
        {
            await Task.Delay(THINKING_DELAY_IN_MS);
            AIDecision decision = aIBrain.Decide(this, GameManager.Instance.Players, trashPile);

            var card = Hand.GetAt(decision.cardIndex);
            bool isLastCard = Hand.Count == 1;

            if(card == null)
                return;
            Hand.RemoveCard(card);
            if(decision.destIndex == -1)
            {
                trashPile.DropCardAfterDrag(card);
            }
            else
            {
                _players[decision.destIndex].Meal.DropCardAfterDrag(card);
            }

            if(isLastCard)
            {
                //Gameover
            }
        }

        void ManageOnTurnStateChanged(TurnState state, PlayerBase player)
        {
            isSelfTurnRunning = player == this;

            if(player is AIPlayer && state == TurnState.PlayPhase )
                PlayACard();
        }

        private void ManageActionCardTrashed(CardBase card)
        {
            if(!isSelfTurnRunning)
                AIConsiderNoBueno(card);
        }

        private void AIConsiderNoBueno(CardBase cardBeingPlayed)
        {
            if (aIBrain.ShouldPlayNoBueno(this, cardBeingPlayed))
            {
                int index = aIBrain.FindFirstInHand<NoBuenoCard>(this);
                if(index == -1)
                    return;
                else
                {
                    Hand.RemoveCard(Hand.GetAt(index));
                    trashPile.DropCardAfterDrag(Hand.GetAt(index));
                }
            }
        }
    }
}
