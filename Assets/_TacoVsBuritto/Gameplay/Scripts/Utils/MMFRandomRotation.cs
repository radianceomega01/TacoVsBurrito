using MoreMountains.Feedbacks;
using UnityEngine;

public class MMFRandomRotation : MonoBehaviour
{
    [SerializeField] private MMF_Player mmfPlayer;
    [SerializeField] private int rotationFeedbackIndex = 0;
    [SerializeField] private float minAngle = -10f;
    [SerializeField] private float maxAngle = 10f;

    public void ModifyValue()
    {
        MMF_Rotation _throwRotation = mmfPlayer.FeedbacksList[rotationFeedbackIndex] as MMF_Rotation;

        if (_throwRotation != null)
            _throwRotation.DestinationAngles = new Vector3(0f, 0f, Random.Range(minAngle, maxAngle));
    }
}
