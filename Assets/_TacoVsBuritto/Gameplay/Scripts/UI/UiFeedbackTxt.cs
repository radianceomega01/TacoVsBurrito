using System;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace TacoVsBurrito
{
    public class UiFeedbackTxt : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI feedbackText;

        const int FEEDBACK_DISPLAY_DURATION_IN_MS = 3000;

        void Awake()
        {
            GameEvents.OnLogMessage += UpdateTxt;
        }

        void OnDestroy()
        {
            GameEvents.OnLogMessage -= UpdateTxt;
        }
        void Start()
        {
            feedbackText.text = "";
        }
        void UpdateTxt(string text)
        {
            DisplayFeedback(text);
        }
        async void DisplayFeedback(string text)
        {
            feedbackText.text = text;
            await Task.Delay(FEEDBACK_DISPLAY_DURATION_IN_MS);
            feedbackText.text = "";
        }
    }
}