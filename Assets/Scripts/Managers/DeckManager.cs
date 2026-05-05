// ============================================================
//  DeckManager.cs
//  Draw pile + Trash pile management.
//
//  REAL RULES implemented:
//  - Health Inspector cards are REMOVED from starting hands.
//    If a player is dealt one, it is shuffled back and they
//    draw a replacement (no-reshuffle of trash during game).
//  - Trash pile is NEVER reshuffled during play.
//  - When draw pile is empty: players keep playing from hand
//    but skip the draw step. Game ends when a player empties
//    their hand.
// ============================================================

using System.Collections.Generic;
using UnityEngine;

namespace TacoVsBurrito
{
    public class DeckManager
    {
        private List<CardBase> _drawPile = new();
        private List<CardBase> _trashPile = new();

        public int DrawCount  => _drawPile.Count;
        public int TrashCount => _trashPile.Count;
        public bool DrawPileEmpty => _drawPile.Count == 0;

        // ---- Build ----

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

        // ---- Draw ----

        /// Draw one card. Returns null if draw pile is empty (do NOT reshuffle trash).
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

        // ---- Deal starting hands --------------------------------------------------------
        // RULE: No player should start with a Health Inspector in hand.
        // If one is dealt, shuffle it back and deal a replacement card.
        // This loops until the hand is Health-Inspector-free.
        // ---------------------------------------------------------------------------------
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

        // ---- Trash pile ----

        public void Trash(CardBase card)
        {
            if (card != null) _trashPile.Add(card);
        }

        public void TrashAll(IEnumerable<CardBase> cards)
        {
            foreach (var c in cards) Trash(c);
        }

        /// View the full trash pile (for Trash Panda selection).
        public IReadOnlyList<CardBase> PeekTrash() => _trashPile;

        /// Retrieve a specific card from the trash pile (Trash Panda action).
        /// Returns true if found and removed.
        public bool RetrieveFromTrash(CardBase card)
        {
            return _trashPile.Remove(card);
        }

        // ---- Utilities ----

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