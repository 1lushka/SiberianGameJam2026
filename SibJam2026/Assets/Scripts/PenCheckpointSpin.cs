using UnityEngine;

// Animated speed preserves the idle spin phase when the checkpoint activates.
public class PenCheckpointSpin : MonoBehaviour
{
    public float degreesPerSecond = 120f;
    void LateUpdate() => Tick(Time.deltaTime);
    public void Tick(float deltaTime)
    {
        if (degreesPerSecond != 0f)
            transform.localRotation *= Quaternion.AngleAxis(degreesPerSecond * deltaTime, Vector3.up);
    }
}
