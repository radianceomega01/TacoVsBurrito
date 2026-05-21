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

        const int CARD_DRAW_DELAY_IN_MS = 1500;
        const int THINKING_DELAY_IN_MS = 3000;

        bool isSelfTurnRunning = false;
        ActionCardBase currentActionCardPlayed = null;
        int noBuenoCounter = 0;

        protected override void Awake()
        {
            base.Awake();
            aIBrain = transform.AddComponent<AIBrain>();

            GameEvents.OnTurnStarted += ManageOnTurnStarted;
            GameEvents.OnTurnChanged += ManageOnTurnChanged;
            GameEvents.OnTurnStateChanged += ManageOnTurnStateChanged;
            GameEvents.OnStartNoBuenoInterruptWindow += ManageNoBuenoInterruptWindow;
            GameEvents.OnNoBuenoPlayed += ManageNoBuenoPlayed;
        }

        void OnDestroy()
        {
            GameEvents.OnTurnStarted -= ManageOnTurnStarted;
            GameEvents.OnTurnChanged -= ManageOnTurnChanged;
            GameEvents.OnTurnStateChanged -= ManageOnTurnStateChanged;
            GameEvents.OnStartNoBuenoInterruptWindow -= ManageNoBuenoInterruptWindow;
            GameEvents.OnNoBuenoPlayed -= ManageNoBuenoPlayed;
        }

        void Start()
        {
            drawPile = GameManager.Instance.GetDrawPile();
            trashPile = GameManager.Instance.GetTrashPile();

            //aIBrain.SetDifficulty(difficulty);
            aIBrain.SetDifficulty(AIDifficulty.Hard);
        }

        public AIBrain GetBrain() => aIBrain;
        void ManageOnTurnStarted(PlayerBase player)
        {
            noBuenoCounter = 0;
            currentActionCardPlayed = null;
            isSelfTurnRunning = player == this;

            if (player is AIPlayer && !drawPile.IsDrawPileEmpty)
                DrawACard();
        }

        async void DrawACard()
        {
            await Task.Delay(CARD_DRAW_DELAY_IN_MS);
            drawPile.TriggerDrawBtnClick();
        }

        async void PlayACard()
        {
            await Task.Delay(THINKING_DELAY_IN_MS);
            AIDecision decision = aIBrain.Decide(this, GameManager.Instance.Players, trashPile);

            var card = Hand.GetAt(decision.cardIndex);
            bool isLastCard = Hand.Count == 1;

            if (card == null)
                return;
            Hand.RemoveCard(card);
            if (decision.destIndex == -1)
            {
                trashPile.DropCardAfterDrag(card);
            }
            else
            {
                _players[decision.destIndex].Meal.DropCardAfterDrag(card);
            }

            if (isLastCard)
            {
                //Gameover
            }
        }

        void ManageOnTurnStateChanged(TurnState state, PlayerBase player)
        {
            if (player is AIPlayer)
            {
                if (state == TurnState.PlayPhase)
                {
                    PlayACard();
                }
                else if (state == TurnState.ActionResolvePhase)
                {
                    ResolveAction();
                }
            }
        }
        void ManageOnTurnChanged(PlayerBase player)
        {
            isSelfTurnRunning = player == this;
            if (player is AIPlayer)
                DrawACard();
        }

        void ResolveAction()
        {
            if(currentActionCardPlayed != null && currentActionCardPlayed.IsTargetTypeAction)
            {
                switch (currentActionCardPlayed)
                {
                    case CraftyCrowCard:
                        aIBrain.ChooseCraftyCrowVictim(this, _players, out PlayerBase victim, out CardBase cardToSteal);
                        if (victim != null)
                            GameEvents.OnCraftyCrowActionTargeted?.Invoke(new TargetTypeContext(this, victim, cardToSteal));
                        break;
                    case OrderEnvyCard:
                        var envyVictim = aIBrain.ChooseOrderEnvyVictim(this, _players);
                        if (envyVictim != null)
                            GameEvents.OnOrderEnvyActionTargeted?.Invoke(new TargetTypeContext(this, envyVictim, null));
                        break;
                }
            }
        }

        void ManageNoBuenoInterruptWindow(ActionCardBase card)
        {
            if (!isSelfTurnRunning)
                AIConsiderNoBueno(card);
            else
            {
                if (noBuenoCounter%2 == 1 && card is NoBuenoCard) //AI tries to cancel others nobueno for his action card
                {
                    PlayNoBueno();
                }
            }

            if(card is not NoBuenoCard)
            {
                currentActionCardPlayed = card;
            }
        }
        void ManageNoBuenoPlayed()
        {
            noBuenoCounter++;
        }

        void AIConsiderNoBueno(CardBase cardBeingPlayed)
        {
            // if (aIBrain.ShouldPlayNoBueno(this, cardBeingPlayed))
            // {
            //     PlayNoBueno();
            // }
        }
        async void PlayNoBueno()
        {
            await Task.Delay(1500);
            int index = aIBrain.FindFirstInHand<NoBuenoCard>(this);
            if (index == -1)
                return;
            else
            {
                Hand.RemoveCard(Hand.GetAt(index));
                trashPile.Trash(Hand.GetAt(index));
            }
        }
    }
}
