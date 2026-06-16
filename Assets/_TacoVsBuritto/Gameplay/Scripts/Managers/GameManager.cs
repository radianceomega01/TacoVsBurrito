using UnityEngine;
namespace TacoVsBurrito
{
    public class GameManager : MonoBehaviour
    {
        const int FRAME_RATE = 90;
        void Awake()
        {
            DontDestroyOnLoad(this);
        }
        void Start()
        {
            Application.runInBackground = true;
            Application.targetFrameRate = FRAME_RATE;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Input.multiTouchEnabled = false;
        }
    }
}
