// ============================================================
//  ScoreboardUI.cs  –  End-game results screen
//
//  Displays:
//  - Winner announcement
//  - Ranked score table with breakdown:
//      Ingredient sum + Tummy Ache sum → raw score → × multiplier
//  - Tie: indicates Food Fight tiebreaker result
//  - Play Again / Quit buttons
// ============================================================

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

namespace TacoVsBurrito
{
    public class ScoreboardUI : MonoBehaviour
    {
        [Header("Panel")]
        public GameObject           panel;

        [Header("Winner Banner")]
        public TextMeshProUGUI      winnerNameLabel;
        public TextMeshProUGUI      winnerScoreLabel;
        public TextMeshProUGUI      winnerMealLabel;    // "🌮 Taco" or "🌯 Burrito"
        public Image                winnerBanner;

        [Header("Score Table")]
        public Transform            rowsParent;
        public GameObject           scoreRowPrefab;     // 4 TMP labels: rank | name | breakdown | total

        [Header("Buttons")]
        public Button               playAgainButton;
        public Button               quitButton;

        // -------------------------------------------------------
        private void Awake()
        {
            panel.SetActive(false);
            GameEvents.OnGameOver += Show;
            playAgainButton.onClick.AddListener(() =>
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex));
            quitButton.onClick.AddListener(() => Application.Quit());
        }

        private void OnDestroy() => GameEvents.OnGameOver -= Show;

        // -------------------------------------------------------
        private void Show(Player winner)
        {
            panel.SetActive(true);

            winnerNameLabel.text  = winner.Name;
            winnerScoreLabel.text = $"{winner.Score} pts";
            winnerMealLabel.text  = winner.MealChoice == MealType.Taco ? "🌮 Team Taco" : "🌯 Team Burrito";

            if (winnerBanner)
                winnerBanner.color = winner.MealChoice == MealType.Taco
                    ? new Color(1f, 0.9f, 0.2f, 0.4f)
                    : new Color(0.9f, 0.9f, 0.9f, 0.4f);

            // Build ranked rows
            var ranked = GameManager.Instance.Players
                .OrderByDescending(p => p.Score)
                .ToList();

            foreach (Transform t in rowsParent) Destroy(t.gameObject);

            for (int i = 0; i < ranked.Count; i++)
            {
                var p  = ranked[i];
                var go = Instantiate(scoreRowPrefab, rowsParent);

                var labels = go.GetComponentsInChildren<TextMeshProUGUI>();
                // Expected prefab label order: rank, name, breakdown, total
                if (labels.Length >= 4)
                {
                    labels[0].text = $"#{i + 1}";
                    labels[1].text = $"{p.Name}\n({p.MealChoice})";
                    labels[2].text = BuildBreakdown(p);
                    labels[3].text = $"{p.Score} pts";
                }

                // Highlight winner row
                if (i == 0)
                {
                    var rowImg = go.GetComponent<Image>();
                    if (rowImg) rowImg.color = new Color(1f, 0.9f, 0.2f, 0.25f);
                }
            }
        }

        // -------------------------------------------------------
        //  Score breakdown string  e.g. "(+3+2-1) × 2 = 8"
        // -------------------------------------------------------
        private string BuildBreakdown(Player p)
        {
            var meal = p.Meal;
            if (meal.CardCount == 0) return "Empty meal = 0";

            int ingSum = 0, taSum = 0, hsb = 0;
            var parts = new List<string>();

            foreach (var c in meal.Cards)
            {
                if (c is IngredientCardBase ingredientCard)
                { ingSum += ingredientCard.CardValue; parts.Add($"+{ingredientCard.CardValue}"); }
                else if (c is TummyAcheCard tummyAcheCard)
                { taSum += tummyAcheCard.CardValue; parts.Add($"{tummyAcheCard.CardValue}"); }
                else if (c is HotSauceBossCard)
                { hsb++; }
            }

            int raw = ingSum + taSum;
            int mult = hsb == 0 ? 1 : hsb == 1 ? 2 : 4;
            string breakdown = $"({string.Join(" ", parts)}) = {raw}";
            if (mult > 1) breakdown += $" × {mult} = {raw * mult}";
            if (hsb > 0) breakdown += $" 🌶×{hsb}";
            return breakdown;
        }
    }
}