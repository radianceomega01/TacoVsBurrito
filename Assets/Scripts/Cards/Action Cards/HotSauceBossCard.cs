using TacoVsBurrito;
using UnityEngine;

public class HotSauceBossCard : ActionCardBase
{
    private const int VALUE_MULTIPLIER = 2;
    public override void ExecuteAction() { }

    public override int GetModifiedMealScore(int currentScore) => currentScore * VALUE_MULTIPLIER;
}
