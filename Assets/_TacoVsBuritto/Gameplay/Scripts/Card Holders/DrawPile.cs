
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
namespace TacoVsBurrito
{
    public class DrawPile : CardPileBase
    {
        [SerializeField] Button drawBtn;
        private const int STARTING_HAND_SIZE = 5;
        public bool IsDrawPileEmpty => pileCards.Count == 0;


        void Awake()
        {
            GameEvents.OnShuffleCards += ManageCardShuffle;
            GameEvents.OnDistributeCards += DealStartingHand;
            GameEvents.OnTurnStateChanged += ManageTurnStateChanged;

            drawBtn.onClick.AddListener(OnDrawBtnClicked);
        }

        void OnDestroy()
        {
            GameEvents.OnShuffleCards -= ManageCardShuffle;
            GameEvents.OnDistributeCards -= DealStartingHand;
            GameEvents.OnTurnStateChanged -= ManageTurnStateChanged;
        }

        void Start()
        {
            InitDrawPile();
        }

        void InitDrawPile()
        {
            for (int i = 0; i < cardsParent.childCount; i++)
            {
                if (!cardsParent.GetChild(i).TryGetComponent<CardBase>(out var card))
                    continue;
                pileCards.Add(card);
            }
        }

        void ManageCardShuffle()
        {
            Shuffle(pileCards);
        }

        CardBase Draw()
        {
            if (pileCards.Count == 0)
                return null;
            var c = pileCards[^1];
            pileCards.RemoveAt(pileCards.Count - 1);
            return c;
        }
        List<CardBase> DrawMany(int count)
        {
            var result = new List<CardBase>();
            for (int i = 0; i < count; i++)
            {
                var c = Draw();
                if (c != null) result.Add(c); else break;
            }
            return result;
        }

        void DealStartingHand(List<PlayerBase> players)
        {
            int cardsDistributed = 0;
            int playerIndex = 0;
            while (cardsDistributed < players.Count * STARTING_HAND_SIZE)
            {
                var card = Draw();

                if (card is HealthInspectorCard)
                {
                    // Move to bottom of deck
                    pileCards.Insert(0, card);
                    card.ChangeSiblingIndex(0);
                    continue;                               // draw again
                }
                players[playerIndex % players.Count].Hand.AddCard(card);
                cardsDistributed++;
                playerIndex++;
            }
        }

        void Shuffle(List<CardBase> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }

            // Sync hierarchy order with list order
            for (int i = 0; i < list.Count; i++)
            {
                list[i].ChangeSiblingIndex(i);
            }
        }

        public void OnDrawBtnClicked()
        {   
            CardBase drawnCard = Draw();

            if (IsDrawPileEmpty)
            {
                // Draw pile just became empty
                GameEvents.OnDrawPileEmpty?.Invoke();
                GameEvents.OnLogMessage?.Invoke(
                    "📭 Draw pile is empty! PlayerBases now skip the draw step.");
            }
            GameManager.Instance.CardDrawnFromPile(drawnCard);
        }
        public override void PutCardsBack(List<CardBase> cards)
        {
            base.PutCardsBack(cards);
            foreach (var card in cards)
            {
                card.ToggleBackFace(true);
            }
        }

        public void TriggerDrawBtnClick() => drawBtn.onClick.Invoke();

        public void TogglePileInteraction(bool value) => drawBtn.interactable = value;

        void ManageTurnStateChanged(TurnState turnState, PlayerBase player)
        {
            if (turnState == TurnState.DrawPhase && IsDrawPileEmpty)
            {
                GameEvents.OnDrawPhaseSkipped?.Invoke();
                GameEvents.OnLogMessage?.Invoke($"  (Draw pile empty – skip draw step)");
                return;
            }
            
            Debug.LogWarning(turnState+ ", "+ player.GetType()+ ", "+!IsDrawPileEmpty);
            if (turnState == TurnState.DrawPhase && player is SelfPlayer && !IsDrawPileEmpty)
            {
                TogglePileInteraction(true);
                GameEvents.OnLogMessage?.Invoke("Draw a card!");
            }
            else
                TogglePileInteraction(false);
        }

        /// Flip the top card from the draw pile (Food Fight).
        /// Returns null if empty. Caller is responsible for placing it back or keeping it.
        public CardBase FlipTop() => Draw();

        /// Shuffle a list of cards back into the draw pile (Food Fight leftovers).
        public void ShuffleBackIn(List<CardBase> cards)
        {
            pileCards.AddRange(cards);
            Shuffle(pileCards);
        }

        public void ChangeBtnListener(UnityAction drawClicked)
        {
            drawBtn.onClick.RemoveAllListeners();
            drawBtn.onClick.AddListener(drawClicked);
        }
        public void ResetBtnListener()
        {
            drawBtn.onClick.RemoveAllListeners();
            drawBtn.onClick.AddListener(OnDrawBtnClicked);
        }

    }
}
