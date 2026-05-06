// ============================================================
//  PlayerSetupUI.cs
//  Pre-game setup screen.
//
//  Handles:
//  - Adding 2–4 players (2–8 with Foodie Expansion toggle)
//  - Each player picks a name, Taco/Burrito, Human/AI
//  - Youngest-player-first prompt (informational)
//  - Validates and starts game
// ============================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TacoVsBurrito
{
    public class PlayerSetupUI : MonoBehaviour
    {
        [Header("Panel")]
        public GameObject setupPanel;

        [Header("Player Rows")]
        public Transform    rowsParent;
        public GameObject   playerRowPrefab;     // Prefab: PlayerRowUI component

        [Header("Controls")]
        public Button           addPlayerButton;
        public Button           removePlayerButton;
        public Button           startButton;
        public TextMeshProUGUI  playerCountLabel;
        public Toggle           foodieExpansionToggle;   // unlocks 5–8 players
        public TextMeshProUGUI  validationLabel;

        [Header("Settings")]
        public int baseMaxPlayers     = 4;
        public int minPlayers         = 2;

        private List<PlayerRowUI> _rows = new List<PlayerRowUI>();

        // -------------------------------------------------------
        private void Awake()
        {
            addPlayerButton.onClick.AddListener(AddRow);
            removePlayerButton.onClick.AddListener(RemoveLastRow);
            startButton.onClick.AddListener(TryStartGame);
            if (foodieExpansionToggle) foodieExpansionToggle.onValueChanged.AddListener(_ => Refresh());
        }

        private void Start()
        {
            // Default: 2 human players
            AddRow();
            AddRow();
            Refresh();
        }

        // -------------------------------------------------------
        private void AddRow()
        {
            if (_rows.Count >= baseMaxPlayers) return;

            var go  = Instantiate(playerRowPrefab, rowsParent);
            var row = go.GetComponent<PlayerRowUI>();
            int idx = _rows.Count;

            row.Initialise(idx, $"Player {idx + 1}",
                           idx % 2 == 0 ? MealType.Taco : MealType.Burrito,
                           PlayerType.Human);
            _rows.Add(row);
            Refresh();
        }

        private void RemoveLastRow()
        {
            if (_rows.Count <= minPlayers) return;
            var last = _rows[_rows.Count - 1];
            _rows.RemoveAt(_rows.Count - 1);
            Destroy(last.gameObject);
            Refresh();
        }

        private void Refresh()
        {
            playerCountLabel.text = $"{_rows.Count} Players";
            addPlayerButton.interactable    = _rows.Count < baseMaxPlayers;
            removePlayerButton.interactable = _rows.Count > minPlayers;

            for (int i = 0; i < _rows.Count; i++)
                _rows[i].SetRowIndex(i);

            Validate();
        }

        private void Validate()
        {
            // Check for duplicate names
            var names = new HashSet<string>();
            foreach (var row in _rows)
            {
                string n = row.PlayerName.Trim();
                if (string.IsNullOrEmpty(n))
                {
                    validationLabel.text = "⚠ All players must have a name.";
                    startButton.interactable = false;
                    return;
                }
                if (!names.Add(n.ToLower()))
                {
                    validationLabel.text = $"⚠ Duplicate name: '{n}'";
                    startButton.interactable = false;
                    return;
                }
            }
            validationLabel.text = "";
            startButton.interactable = true;
        }

        private void TryStartGame()
        {
            Validate();
            if (!startButton.interactable) return;

            // var setupData = new List<PlayerSetupData>();
            // foreach (var row in _rows)
            // {
            //     setupData.Add(new PlayerSetupData
            //     {
            //         playerName = row.PlayerName.Trim(),
            //         mealType   = row.SelectedMeal,
            //         playerType = row.SelectedType
            //     });
            // }

            // setupPanel.SetActive(false);
            // GameManager.Instance.StartGame(setupData);
        }
    }
}