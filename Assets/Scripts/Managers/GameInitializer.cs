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
            GameEvents.OnGameStarted?.Invoke();
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
            await Task.Delay(1500);
            GameEvents.OnShuffleCards?.Invoke();
        }

        async Task InitDistributeInitialHands()
        {
            await Task.Delay(1500);
            GameEvents.OnDistributeCards?.Invoke(activePlayers);
        }
    }
    public enum GameMode
    {
        VsPlayer,
        VsAI
    }
}
