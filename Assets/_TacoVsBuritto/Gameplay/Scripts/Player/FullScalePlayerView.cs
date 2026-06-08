using TMPro;
using UnityEngine;
namespace TacoVsBurrito
{
    public class FullScalePlayerView : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI nameTxt;
        public void SetName(string name) => nameTxt.SetText(name);
    }
}
