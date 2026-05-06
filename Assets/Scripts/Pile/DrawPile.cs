using System.Collections.Generic;
using UnityEngine;
namespace TacoVsBurrito
{
    public class DrawPile : MonoBehaviour
    {
        private List<CardBase> _drawPile = new();

        public int DrawCount  => _drawPile.Count;
        public bool IsDrawPileEmpty => _drawPile.Count == 0;

        public void InitDrawPile()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (!transform.GetChild(i).TryGetComponent<CardBase>(out var card))
                    continue;
                _drawPile.Add(card);
            }
        }
        
        public void BuildAndShuffle(CardBase db)
        {
            // _drawPile.Clear();
            // _trashPile.Clear();

            // List<CardData> defs = db != null && db.allCardAssets.Count > 0
            //     ? db.allCardAssets
            //     : CardDatabase.BuildDefaultDeck();

            // foreach (var def in defs)
            //     _drawPile.Add(new Card(def));

            // Shuffle(_drawPile);
            Debug.Log($"[DeckManager] Built deck: {_drawPile.Count} cards.");
        }

        public CardBase Draw()
        {
            if (_drawPile.Count == 0) return null;
            var c = _drawPile[0];
            _drawPile.RemoveAt(0);
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

        public List<CardBase> DealStartingHand(int handSize)
        {
            var hand = new List<CardBase>();

            while (hand.Count < handSize)
            {
                var card = Draw();
                if (card == null) break;                   // edge case: tiny deck

                if (card is HealthInspectorCard)
                {
                    // Shuffle back
                    int insertAt = Random.Range(0, _drawPile.Count + 1);
                    _drawPile.Insert(insertAt, card);
                    Debug.Log("[DeckManager] Health Inspector removed from starting hand and reshuffled.");
                    continue;                               // draw again
                }

                hand.Add(card);
            }

            return hand;
        }

        private void Shuffle(List<CardBase> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        /// Flip the top card from the draw pile (Food Fight).
        /// Returns null if empty. Caller is responsible for placing it back or keeping it.
        public CardBase FlipTop() => Draw();

        /// Shuffle a list of cards back into the draw pile (Food Fight leftovers).
        public void ShuffleBackIn(List<CardBase> cards)
        {
            _drawPile.AddRange(cards);
            Shuffle(_drawPile);
        }
    }
}
