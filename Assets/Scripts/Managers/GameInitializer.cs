using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
namespace TacoVsBurrito {
    public class GameInitializer : MonoBehaviour
    {
        [Header("Game Settings")]
        [Range(2, 4)] 
        [SerializeField] int numberOfPlayers = 2;
        [SerializeField] GameMode gameMode = GameMode.VsAI;
        
        [SerializeField] Transform playersParent;
        [SerializeField] GameObject opponentPlayerPrefab;
        [SerializeField] List<Transform> playerPositions;

        GameState gameState = GameState.None;

        void Start()
        {
            SetupPlayers();
            GameManager.Instance.StartGame();
        }

        void SetupPlayers()
        {
            for (int i = 0; i < numberOfPlayers - 1; i++)
            {
                GameObject playerObj = Instantiate(opponentPlayerPrefab, playerPositions[0].position, Quaternion.identity, playersParent);
                if (gameMode == GameMode.VsAI)
                {
                    playerObj.AddComponent<AIPlayer>();
                }
                else
                {
                    playerObj.AddComponent<HumanPlayer>();
                }
                PlayerBase player= playerObj.GetComponent<PlayerBase>();
                GameManager.Instance.AddPlayerBeforeGameStarts(player);
            }
        }
    }

    public enum GameState
    {
        None,
        Init,
        CardDistribution,
        Running,
        Completed

    }
    public enum GameMode
    {
        VsPlayer,
        VsAI
    }
}
