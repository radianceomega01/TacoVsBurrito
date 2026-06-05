using System;
using UnityEngine;

namespace TacoVsBurrito
{
    [CreateAssetMenu(fileName = "GameData", menuName = "Scriptable Objects/GameData")]
    public class GameDataSO : ScriptableObject
    {
        public int numOfPlayers;
    }
}
