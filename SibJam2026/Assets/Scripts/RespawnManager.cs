using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance { get; private set; }

    [Header("Drag & Drop in Inspector")]
    public Transform player;
    public Transform startPoint;

    // Статические данные чекпоинта (сохраняются глобально)
    private static string savedSceneName;
    private static Vector3 savedPosition;
    private static Quaternion savedRotation = Quaternion.identity;
    private static bool hasSavedCheckpoint;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Вызвать из DeathZone или где угодно.
    /// Перемещает игрока на чекпоинт или на startPoint.
    /// </summary>
    public void Respawn()
    {
        if (player == null)
        {
            Debug.LogError("RespawnManager: Player Transform not assigned!");
            return;
        }

        string currentScene = gameObject.scene.name;
        bool useCheckpoint = hasSavedCheckpoint && savedSceneName == currentScene;

        Vector3 targetPos;
        Quaternion targetRot;

        if (useCheckpoint)
        {
            targetPos = savedPosition;
            targetRot = savedRotation;
            Debug.Log("Respawning to checkpoint: " + targetPos);
        }
        else
        {
            if (startPoint == null)
            {
                Debug.LogError("RespawnManager: Start Point not assigned!");
                return;
            }
            targetPos = startPoint.position;
            targetRot = startPoint.rotation;
            Debug.Log("Respawning to start: " + targetPos);
        }

        ApplyPosition(targetPos, targetRot);
    }

    private void ApplyPosition(Vector3 pos, Quaternion rot)
    {
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            player.SetPositionAndRotation(pos, rot);
            cc.enabled = true;
            return;
        }

        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null && !rb.isKinematic)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = pos;
            rb.rotation = rot;
            return;
        }

        player.SetPositionAndRotation(pos, rot);
    }

    // Вызывается из CheckpointTrigger
    public void SaveCheckpoint(Vector3 pos, Quaternion rot)
    {
        savedSceneName = gameObject.scene.name;
        savedPosition = pos;
        savedRotation = rot;
        hasSavedCheckpoint = true;
        Debug.Log("Checkpoint saved: " + pos);
    }

    public void ClearCheckpoint()
    {
        hasSavedCheckpoint = false;
        savedPosition = Vector3.zero;
        savedRotation = Quaternion.identity;
    }
}