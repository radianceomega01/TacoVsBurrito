using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace TacoVsBurrito
{
    public class UiFeedbackTxt : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI feedbackText;
        CancellationTokenSource cts;
        const int FEEDBACK_DISPLAY_DURATION_IN_MS = 3000;

        void Awake()
        {
            GameEvents.OnLogMessage += UpdateTxt;
            cts = new CancellationTokenSource();
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
            cts.Cancel();
            feedbackText.text = text;
            await Task.Delay(FEEDBACK_DISPLAY_DURATION_IN_MS, cts.Token);
            feedbackText.text = "";
        }
    }
}