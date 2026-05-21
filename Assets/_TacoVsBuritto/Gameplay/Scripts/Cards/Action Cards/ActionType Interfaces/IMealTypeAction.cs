using UnityEngine;

namespace TacoVsBurrito
{
    public interface IMealTypeAction
    {
        int GetModifiedMealScore(int currentScore);
    }
}
