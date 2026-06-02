using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        [SerializeField] PlayerBase selfPlayer;
        [SerializeField] GameObject opponentPlayerPrefab;
        [SerializeField] List<Transform> playerPositions;

        private const int SHUFFLE_DELAY_IN_MS = 1000;
        private const int DISTRIBUTION_DELAY_IN_MS = 1000;

        List<PlayerBase> activePlayers = new();

        void Start()
        {
            SetupPlayers();
            BeginGame();
        }

        async void BeginGame()
        {
            GameEvents.OnGameInit?.Invoke(activePlayers);
            await InitCardShuffle();
            await InitDistributeInitialHands();
        }

        void SetupPlayers()
        {
            selfPlayer.InitIndex(0);
            activePlayers.Add(selfPlayer);
            GameManager.Instance.AddPlayerBeforeGameStarts(selfPlayer);
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
                PlayerBase player = playerObj.GetComponent<PlayerBase>();
                player.InitIndex(i + 1);
                activePlayers.Add(player);
                GameManager.Instance.AddPlayerBeforeGameStarts(player);
            }
        }

        async Task InitCardShuffle()
        {
            await Task.Delay(SHUFFLE_DELAY_IN_MS);
            GameEvents.OnShuffleCards?.Invoke();
        }

        async Task InitDistributeInitialHands()
        {
            await Task.Delay(DISTRIBUTION_DELAY_IN_MS);
            GameEvents.OnDistributeCards?.Invoke(activePlayers);
        }
    }
    public enum GameMode
    {
        VsPlayer,
        VsAI
    }
}
