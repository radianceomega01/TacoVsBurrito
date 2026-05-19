using TMPro;
using UnityEngine;

namespace TacoVsBurrito
{
    public class UiTurnIndicator : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI turnTxt;

        void Awake()
        {
            GameEvents.OnTurnStarted += UpdateTxt;
        }
        void OnDestroy()
        {
            GameEvents.OnTurnStarted -= UpdateTxt;
        }
        void Start()
        {
            turnTxt.text = "";
        }
        void UpdateTxt(PlayerBase player)
        {
            turnTxt.text = player is SelfPlayer ? "Your Turn!" : "Opponent's Turn!";
        }
    }
}
