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

        int numOfPlayers;

        public void InitWithRoomName(string name)
        {
            if(roomNameText.text.IsNullOrEmpty())
                roomNameText.SetText(name);
        }
        public void AddPlayer(string playerName)
        {
            if(numOfPlayers == gameDataSO.numOfPlayers)
                return;

            GameObject obj = Instantiate(playerTemplate);
            obj.GetComponentInChildren<TextMeshProUGUI>().SetText(playerName);
            numOfPlayers++;
        }

    }
}
