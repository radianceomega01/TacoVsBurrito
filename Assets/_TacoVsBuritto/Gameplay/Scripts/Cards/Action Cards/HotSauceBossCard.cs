using TacoVsBurrito;
using TMPro;
using UnityEngine;

namespace TacoVsBurrito
{
    public class HotSauceBossCard : ActionCardBase, IMealTypeAction
    {
        [Header("Fields")]
        [SerializeField] TextMeshProUGUI ValueTxtField;

        private const int VALUE_MULTIPLIER = 2;
        
        protected override void Start()
        {
            base.Start();
            ValueTxtField.text = $"X{VALUE_MULTIPLIER}";
        }

        public override void ExecuteAction() { }

        public int GetModifiedMealScore(int currentScore) => currentScore * VALUE_MULTIPLIER;
        public override TurnState GetStateOnTrashed() => TurnState.SkipPhase;
    }
}
