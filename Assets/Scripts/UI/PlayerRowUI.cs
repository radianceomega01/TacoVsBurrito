// ============================================================
//  PlayerRowUI – attach to each player row prefab
// ============================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace TacoVsBurrito
{
    public class PlayerRowUI : MonoBehaviour
    {
        [Header("Row Elements")]
        public TextMeshProUGUI indexLabel;
        public TMP_InputField nameInput;

        public Button tacoButton;
        public Button burritoButton;

        public Button humanButton;
        public Button aiButton;

        // Optional difficulty selector (AI only)
        public TMP_Dropdown aiDifficultyDropdown;

        public string PlayerName => nameInput.text;
        public MealType SelectedMeal { get; private set; }
        public PlayerType SelectedType { get; private set; }
        public AIDifficulty AIDifficulty { get; private set; } = AIDifficulty.Normal;

        public void Initialise(int index, string defaultName, MealType meal, PlayerType type)
        {
            SetRowIndex(index);
            nameInput.text = defaultName;
            SelectedMeal = meal;
            SelectedType = type;

            tacoButton.onClick.AddListener(() => SetMeal(MealType.Taco));
            burritoButton.onClick.AddListener(() => SetMeal(MealType.Burrito));
            humanButton.onClick.AddListener(() => SetType(PlayerType.Human));
            aiButton.onClick.AddListener(() => SetType(PlayerType.AI));

            if (aiDifficultyDropdown)
            {
                aiDifficultyDropdown.ClearOptions();
                aiDifficultyDropdown.AddOptions(new List<string> { "Easy", "Normal", "Hard" });
                aiDifficultyDropdown.value = 1; // Normal default
                aiDifficultyDropdown.onValueChanged.AddListener(v =>
                    AIDifficulty = (AIDifficulty)v);
                aiDifficultyDropdown.gameObject.SetActive(type == PlayerType.AI);
            }

            RefreshButtons();
        }

        public void SetRowIndex(int index)
        {
            if (indexLabel) indexLabel.text = $"P{index + 1}";
        }

        private void SetMeal(MealType m) { SelectedMeal = m; RefreshButtons(); }
        private void SetType(PlayerType t)
        {
            SelectedType = t;
            if (aiDifficultyDropdown)
                aiDifficultyDropdown.gameObject.SetActive(t == PlayerType.AI);
            RefreshButtons();
        }

        private void RefreshButtons()
        {
            tacoButton.image.color = SelectedMeal == MealType.Taco ? Color.yellow : Color.grey;
            burritoButton.image.color = SelectedMeal == MealType.Burrito ? Color.white : Color.grey;
            humanButton.image.color = SelectedType == PlayerType.Human ? Color.cyan : Color.grey;
            aiButton.image.color = SelectedType == PlayerType.AI ? Color.cyan : Color.grey;
        }
    }
}