using System;
using System.Collections.Generic;
using System.Linq;
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
        private PlayArea playArea;
        private IReadOnlyList<PlayerBase> _players => GameManager.Instance.Players;

        const int CARD_DRAW_DELAY_IN_MS = 750;
        const int THINKING_DELAY_IN_MS = 1500;
        const int PLAY_NO_BUENO_DELAY_IN_MS = 1500;

        bool isSelfTurnRunning = false;
        int noBuenoCounter = 0;

        protected override void Awake()
        {
            base.Awake();
            aIBrain = transform.AddComponent<AIBrain>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            GameEvents.OnTurnStarted += ManageTurnStarted;
            GameEvents.OnTurnStateChanged += ManageTurnStateChanged;
            GameEvents.OnStartNoBuenoInterruptWindow += ManageNoBuenoInterruptWindow;
            GameEvents.OnNoBuenoPlayed += ManageNoBuenoPlayed;

            GameEvents.OnCraftyCrowAction += ManageCraftyCraftyCrowAction;
            GameEvents.OnOrderEnvyAction += ManageOrderEnvyAction;
            GameEvents.OnTrashPandaAction += ManageCardSelectionAction;
            GameEvents.OnCardSelectionForFoodFightWinner += ManageCardSelectionAction;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            GameEvents.OnTurnStarted -= ManageTurnStarted;
            GameEvents.OnTurnStateChanged -= ManageTurnStateChanged;
            GameEvents.OnStartNoBuenoInterruptWindow -= ManageNoBuenoInterruptWindow;
            GameEvents.OnNoBuenoPlayed -= ManageNoBuenoPlayed;

            GameEvents.OnCraftyCrowAction -= ManageCraftyCraftyCrowAction;
            GameEvents.OnOrderEnvyAction -= ManageOrderEnvyAction;
            GameEvents.OnTrashPandaAction -= ManageCardSelectionAction;
            GameEvents.OnCardSelectionForFoodFightWinner -= ManageCardSelectionAction;

        }

        void Start()
        {
            drawPile = GameManager.Instance.GetDrawPile();
            playArea = GameManager.Instance.GetPlayArea();

            //aIBrain.SetDifficulty(difficulty);
            aIBrain.SetDifficulty(AIDifficulty.Hard);
        }

        public AIBrain GetBrain() => aIBrain;
        void ManageTurnStarted(PlayerBase player)
        {
            
            noBuenoCounter = 0;
            isSelfTurnRunning = player is AIPlayer;

            if (isSelfTurnRunning && !drawPile.IsDrawPileEmpty)
            {
                DrawACard();
            }
        }

        async void DrawACard()
        {
            await Task.Delay(CARD_DRAW_DELAY_IN_MS);
            drawPile.TriggerDrawBtnClick();
        }

        async void PlayACard()
        {
            await Task.Delay(THINKING_DELAY_IN_MS);
            AIDecision decision = aIBrain.Decide(this, GameManager.Instance.Players);

            var card = Hand.GetAt(decision.cardIndex);

            if (card == null)
                return;
            Hand.RemoveCard(card);
            if (decision.destIndex == -1)
            {
                playArea.DropCardAfterDrag(card);
            }
            else
            {
                _players[decision.destIndex].Meal.DropCardAfterDrag(card);
            }
        }

        void ManageTurnStateChanged(TurnState state, PlayerBase player)
        {
            if (player is AIPlayer)
            {
                if (state == TurnState.PlayPhase)
                {
                    PlayACard();
                }
            }
        }

        void ManageOrderEnvyAction(PlayerBase @base)
        {
            if(!isSelfTurnRunning)
                return;
                
            var envyVictim = aIBrain.ChooseOrderEnvyVictim(this, _players);
            if (envyVictim != null)
                GameEvents.OnOrderEnvyActionTargeted?.Invoke(new TargetTypeContext(this, envyVictim, null));
        }

        void ManageCraftyCraftyCrowAction()
        {
            if(!isSelfTurnRunning)
                return;

            aIBrain.ChooseCraftyCrowVictim(this, _players, out PlayerBase victim, out CardBase cardToSteal);
            if (victim != null)
                GameEvents.OnCardClickedForActionTarget?.Invoke(cardToSteal);
        }

        async void ManageCardSelectionAction(Dictionary<CardBase, int> dictionary)
        {
            if(!isSelfTurnRunning)
                return;

            await Task.Delay(THINKING_DELAY_IN_MS);
            CardBase cardPicked = aIBrain.PickBestCardFromCardPile(dictionary.Keys.ToList());
            GameEvents.OnCardClickedForActionTarget?.Invoke(cardPicked);
        }
        async void ManageCardSelectionAction(Dictionary<CardBase, int> dictionary, PlayerBase winner)
        {
            if(winner is not AIPlayer)
                return;

            await Task.Delay(THINKING_DELAY_IN_MS);
            CardBase cardPicked = aIBrain.PickBestCardFromCardPile(dictionary.Keys.ToList());
            GameEvents.OnCardClickedForActionTarget?.Invoke(cardPicked);
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
        }
        void ManageNoBuenoPlayed(NoBuenoCard card)
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
            await Task.Delay(PLAY_NO_BUENO_DELAY_IN_MS);
            int index = aIBrain.FindFirstInHand<NoBuenoCard>(this);
            if (index == -1)
                return;
            else
            {
                Hand.RemoveCard(Hand.GetAt(index));
                playArea.PlayAction(Hand.GetAt(index));
            }
        }
    }
}
