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
        Dictionary<PlayerRef, TextMeshProUGUI> playersAndNameMap;
        bool isRoomNameSet;

        public void InitWithRoomName(string name)
        {
            if(!isRoomNameSet)
            {
                roomNameText.SetText(name);
                isRoomNameSet = true;
            }
        }
        public void AddPlayer(PlayerData playerData)
        {
            Debug.LogWarning("numOfPlayers: "+ NumOfPlayers+ ", gameDataSO.numOfPlayers: "+gameDataSO.numOfPlayers);
            if(NumOfPlayers == gameDataSO.numOfPlayers)
                return;

            GameObject obj = Instantiate(playerTemplate, playerTemplate.transform.parent);
            TextMeshProUGUI textComponent = obj.GetComponentInChildren<TextMeshProUGUI>();
            playersAndNameMap.Add(playerData.Player, textComponent);
            obj.SetActive(true);
            Debug.LogWarning("Instantiated player");
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
            for(int i=0; i<players.Count; i++)
            {
                if(playersAndNameMap.ContainsKey(players[i].Player))
                    playersAndNameMap[players[i].Player].SetText(players[i].Name.ToString());
            }
        }

    }
}
