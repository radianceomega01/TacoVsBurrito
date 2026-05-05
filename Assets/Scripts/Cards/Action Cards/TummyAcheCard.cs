using TacoVsBurrito;
using UnityEngine;

public class TummyAcheCard : ActionCardBase
{
    [SerializeField] int cardValue = -1;
    public int CardValue {get{ return cardValue; } }
    public override void ExecuteAction() { }

    public override int GetModifiedMealScore(int currentScore) => currentScore + cardValue;
}
