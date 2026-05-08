using CMF;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance { get; private set; }

    [Header("Scene References")]
    public Transform player;
    public Transform startPoint;

    // Static checkpoint data survives a scene reload.
    private static string savedSceneName;
    private static Vector3 savedPosition;
    private static Quaternion savedRotation = Quaternion.identity;
    private static bool hasSavedCheckpoint;

    // One-shot restore flag for reloading the current level.
    private static bool restoreAfterSceneReload;
    private static string restoreSceneName;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (restoreAfterSceneReload && scene.name == restoreSceneName)
        {
            restoreAfterSceneReload = false;
            RestorePlayerAfterReload();
            return;
        }

        ClearCheckpoint();
    }

    public void Respawn()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        if (string.IsNullOrWhiteSpace(currentScene))
        {
            Debug.LogError("RespawnManager: Failed to resolve current scene for reload.");
            return;
        }

        PreserveCurrentLevelConfig();

        restoreAfterSceneReload = true;
        restoreSceneName = currentScene;

        Debug.Log($"RespawnManager: Reloading scene '{currentScene}' from checkpoint.");
        SceneManager.LoadScene(currentScene);
    }

    private void PreserveCurrentLevelConfig()
    {
        if (ProgressManager.Instance == null || LevelInitializer.Instance == null)
            return;

        LevelConfiguration currentConfig = LevelInitializer.Instance.CurrentConfig;
        if (currentConfig != null)
            ProgressManager.Instance.CurrentLevelConfig = currentConfig;
    }

    private void RestorePlayerAfterReload()
    {
        ResolveSceneReferences();

        if (player == null)
        {
            Debug.LogError("RespawnManager: Player not found after scene reload.");
            return;
        }

        string currentScene = SceneManager.GetActiveScene().name;
        bool useCheckpoint = hasSavedCheckpoint && savedSceneName == currentScene;

        Vector3 targetPos;
        Quaternion targetRot;

        if (useCheckpoint)
        {
            targetPos = savedPosition;
            targetRot = savedRotation;
            Debug.Log("Respawning to checkpoint after reload: " + targetPos);
        }
        else
        {
            if (startPoint == null)
            {
                Debug.LogError("RespawnManager: Start Point is not assigned.");
                return;
            }

            targetPos = startPoint.position;
            targetRot = startPoint.rotation;
            Debug.Log("Respawning to start after reload: " + targetPos);
        }

        ApplyPosition(targetPos, targetRot);
    }

    private void ResolveSceneReferences()
    {
        if (player == null)
        {
            AdvancedWalkerController controller = FindObjectOfType<AdvancedWalkerController>();
            if (controller != null)
                player = controller.transform;
        }
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

    public void SaveCheckpoint(Vector3 pos, Quaternion rot)
    {
        savedSceneName = SceneManager.GetActiveScene().name;
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
        savedSceneName = "";
        restoreAfterSceneReload = false;
        restoreSceneName = "";
    }
}
