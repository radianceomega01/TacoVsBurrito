using System.Collections;
using TMPro;
using UnityEngine;

namespace TacoVsBurrito
{
    public class UiFeedbackTxt : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI feedbackText;

        const int FEEDBACK_DISPLAY_DURATION_IN_SECS = 3;

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
            StartCoroutine(DisplayFeedback(text));
        }
        IEnumerator DisplayFeedback(string text)
        {
            feedbackText.text = text;
            yield return new WaitForSeconds(FEEDBACK_DISPLAY_DURATION_IN_SECS);
            feedbackText.text = "";
        }
    }
}