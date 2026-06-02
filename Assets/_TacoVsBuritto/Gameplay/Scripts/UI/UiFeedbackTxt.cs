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
            cts = new CancellationTokenSource();
        }
        void OnEnable()
        {
            GameEvents.OnLogMessage += UpdateTxt;
        }

        void OnDisable()
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
            try
            {
                cts.Cancel();
                feedbackText.text = text;
                await Task.Delay(FEEDBACK_DISPLAY_DURATION_IN_MS, cts.Token);
                feedbackText.text = "";
            }
            catch (OperationCanceledException) { }
        }
    }
}