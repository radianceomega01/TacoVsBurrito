using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
namespace TacoVsBurrito
{
    public class GameInitializer : MonoBehaviour
    {
        [Header("Game Settings")]
        [Range(2, 4)]
        [SerializeField] int numberOfPlayers = 2;
        [SerializeField] GameMode gameMode = GameMode.VsAI;

        [SerializeField] Transform playersParent;
        [SerializeField] PlayerBase selfPlayer;
        [SerializeField] GameObject opponentPlayerHorizontalPrefab;
        [SerializeField] GameObject opponentPlayerVerticalPrefab;
        [SerializeField] List<Transform> opponentPositions;

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
            InitializeSelfPlayer();
            switch(numberOfPlayers)
            {
                case 2:
                    GameObject playerObj0 = Instantiate(opponentPlayerHorizontalPrefab, opponentPositions[0].position, Quaternion.identity, playersParent);
                    InitializeOpponentPlayer(playerObj0, 1);
                    break;
                case 3:
                    GameObject playerObj1_1 = Instantiate(opponentPlayerVerticalPrefab, opponentPositions[1].position, Quaternion.identity, playersParent);
                    InitializeOpponentPlayer(playerObj1_1, 1);
                    GameObject playerObj1_2 = Instantiate(opponentPlayerVerticalPrefab, opponentPositions[2].position, Quaternion.identity, playersParent);
                    InitializeOpponentPlayer(playerObj1_2, 2);
                    break;
                case 4:
                    GameObject playerObj2_1 = Instantiate(opponentPlayerVerticalPrefab, opponentPositions[1].position, Quaternion.identity, playersParent);
                    InitializeOpponentPlayer(playerObj2_1, 1);
                    GameObject playerObj2_2 = Instantiate(opponentPlayerHorizontalPrefab, opponentPositions[0].position, Quaternion.identity, playersParent);
                    InitializeOpponentPlayer(playerObj2_2, 2);
                    GameObject playerObj2_3 = Instantiate(opponentPlayerVerticalPrefab, opponentPositions[2].position, Quaternion.identity, playersParent);
                    InitializeOpponentPlayer(playerObj2_3, 3);
                    break;
            }
        }

        void InitializeSelfPlayer()
        {
            selfPlayer.InitIndex(0);
            activePlayers.Add(selfPlayer);
            GameManager.Instance.AddPlayerBeforeGameStarts(selfPlayer);
        }
        void InitializeOpponentPlayer(GameObject opponentPrefab, int index)
        {
            if (gameMode == GameMode.VsAI)
            {
                opponentPrefab.AddComponent<AIPlayer>();
            }
            else
            {
                opponentPrefab.AddComponent<HumanPlayer>();
            }
            PlayerBase player = opponentPrefab.GetComponent<PlayerBase>();
            player.InitIndex(index);
            activePlayers.Add(player);
            GameManager.Instance.AddPlayerBeforeGameStarts(player);
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
