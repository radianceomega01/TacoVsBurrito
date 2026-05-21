
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
namespace TacoVsBurrito
{
    public class DrawPile : MonoBehaviour
    {
        [SerializeField] Transform cardsParent;
        [SerializeField] Button drawBtn;
        private List<CardBase> _pileCards = new();
        private const int STARTING_HAND_SIZE = 5;

        public int CardCount => _pileCards.Count;
        public bool IsDrawPileEmpty => _pileCards.Count == 0;
        public IReadOnlyList<CardBase> PileCards => _pileCards;


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
                _pileCards.Add(card);
            }
        }

        void ManageCardShuffle()
        {
            Shuffle(_pileCards);
        }

        public CardBase Draw()
        {
            if (_pileCards.Count == 0)
                return null;
            var c = _pileCards[^1];
            _pileCards.RemoveAt(_pileCards.Count - 1);
            return c;
        }
        public List<CardBase> DrawMany(int count)
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
                    _pileCards.Insert(0, card);
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
            _pileCards.AddRange(cards);
            Shuffle(_pileCards);
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
