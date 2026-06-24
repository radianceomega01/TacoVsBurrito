using UnityEngine;
namespace TacoVsBurrito
{
    public class GameManager : MonoBehaviour
    {
        const int FRAME_RATE = 90;
        private static GameManager instance;
        void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
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
