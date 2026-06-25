using System.Collections.Generic;
using System.Linq;
using Fusion;
using TMPro;
using UnityEngine;
using WebSocketSharp;

namespace TacoVsBurrito
{
    public class RoomPlayersPanel : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI roomNameText;
        [SerializeField] GameObject playerTemplate;
        [SerializeField] GameDataSO gameDataSO;

        public int NumOfPlayers => playersAndNameMap.Count;
        Dictionary<PlayerRef, TextMeshProUGUI> playersAndNameMap = new();
        bool isRoomNameSet;

        public void InitWithRoomName(string name)
        {
            if(!isRoomNameSet)
            {
                roomNameText.SetText(name);
                isRoomNameSet = true;
            }
        }
        public void AddPlayer(List<PlayerData> players)
        {
            if(NumOfPlayers == gameDataSO.numOfPlayers)
                return;

            for(int i=0; i<players.Count; i++)
            {
                if(playersAndNameMap.ContainsKey(players[i].Player))
                    continue;

                GameObject obj = Instantiate(playerTemplate, playerTemplate.transform.parent);
                TextMeshProUGUI textComponent = obj.GetComponentInChildren<TextMeshProUGUI>();
                playersAndNameMap.Add(players[i].Player, textComponent);
                textComponent.SetText(players[i].Name.ToString());
                obj.SetActive(true);    
            }
        }

        public void UpdatePlayerNames(List<PlayerData> players)
        {
            for(int i=0; i<players.Count; i++)
            {
                if(playersAndNameMap.ContainsKey(players[i].Player))
                    playersAndNameMap[players[i].Player].SetText(players[i].Name.ToString());
            }
        }

        public void RemovePlayer(List<PlayerData> players)
        {
            // 1. Create a list of PlayerRefs that are currently in the UI but missing from the network data
            List<PlayerRef> playersToRemove = new List<PlayerRef>();

            foreach (var playerRef in playersAndNameMap.Keys)
            {
                if (!players.Any(p => p.Player == playerRef))
                {
                    playersToRemove.Add(playerRef);
                }
            }

            // 2. Iterate through the marked players, destroy their UI elements, and clear them from the map
            foreach (var playerRef in playersToRemove)
            {
                if (playersAndNameMap.TryGetValue(playerRef, out TextMeshProUGUI textComponent))
                {
                    Destroy(textComponent.transform.parent.gameObject);
                    playersAndNameMap.Remove(playerRef);
                }
            }
        }

    }
}
