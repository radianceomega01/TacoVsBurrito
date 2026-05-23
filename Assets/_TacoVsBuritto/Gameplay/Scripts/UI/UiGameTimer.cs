using System;
using System.Collections;
using TMPro;
using UnityEngine;
namespace TacoVsBurrito
{
    public class UiGameTimer : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI timerText;

        Coroutine currentCoroutine;
        const int TIMER_INTERVAL_IN_SECS = 1;
        void Awake()
        {
            GameEvents.OnTimerEvent += ManageTimerEvent;
        }

        void OnDestroy()
        {
            GameEvents.OnTimerEvent -= ManageTimerEvent;
        }
        void Start()
        {
            timerText.text = "";
        }
        private void ManageTimerEvent(int value)
        {
            if(currentCoroutine != null) StopCoroutine(currentCoroutine);
            currentCoroutine = StartCoroutine(StartTimer(value));
        }

        IEnumerator StartTimer(int value)
        {
            for (int i = value; i > 0; i--)
            {
                timerText.text = i.ToString();
                yield return new WaitForSeconds(TIMER_INTERVAL_IN_SECS);
            }
            timerText.text = "";
        }
    }
}
