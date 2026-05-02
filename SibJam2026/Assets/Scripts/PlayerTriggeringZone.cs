using UnityEngine;

public class PlayerTriggeringZone : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerEnter(Collider other)
    {
        var disaperObj = other.gameObject.GetComponent<Disappearing>();
        if (disaperObj != null)
        {
            disaperObj.TriggerDisappearance();
        }
    }
}
