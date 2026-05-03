using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckpointTrigger : MonoBehaviour
{
    [SerializeField] private Transform respawnPoint;

    private static string savedSceneName;
    private static Vector3 savedPosition;
    private static Quaternion savedRotation = Quaternion.identity;
    private static bool hasSavedCheckpoint;
    private static bool respawnPending;
    private static bool isRestarting;
    private static CheckpointRuntime runtime;

    private void Awake()
    {
        EnsureRuntime();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayerCollider(other))
            return;

        Transform targetPoint = respawnPoint != null ? respawnPoint : transform;
        SaveCheckpoint(targetPoint.position, targetPoint.rotation);
    }

    public static void RestartLevel()
    {
        EnsureRuntime();

        if (isRestarting)
            return;

        Scene activeScene = SceneManager.GetActiveScene();
        respawnPending = hasSavedCheckpoint && savedSceneName == activeScene.name;
        isRestarting = true;
        SceneManager.LoadScene(activeScene.name);
    }

    public static void ClearSavedCheckpoint()
    {
        hasSavedCheckpoint = false;
        respawnPending = false;
        savedSceneName = string.Empty;
        savedPosition = Vector3.zero;
        savedRotation = Quaternion.identity;
        isRestarting = false;
    }

    public static bool IsPlayerCollider(Collider other)
    {
        if (other == null)
            return false;

        if (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag("Player"))
            return true;

        if (other.CompareTag("Player"))
            return true;

        Transform root = other.transform.root;
        return root != null && root.CompareTag("Player");
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        EnsureRuntime();
    }

    private static void SaveCheckpoint(Vector3 position, Quaternion rotation)
    {
        Scene activeScene = SceneManager.GetActiveScene();
        savedSceneName = activeScene.name;
        savedPosition = position;
        savedRotation = rotation;
        hasSavedCheckpoint = true;
    }

    private static void EnsureRuntime()
    {
        if (runtime != null)
            return;

        GameObject runtimeObject = new GameObject("[CheckpointRuntime]");
        runtimeObject.hideFlags = HideFlags.HideAndDontSave;
        DontDestroyOnLoad(runtimeObject);
        runtime = runtimeObject.AddComponent<CheckpointRuntime>();

        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isRestarting = false;

        if (!respawnPending || !hasSavedCheckpoint || scene.name != savedSceneName || runtime == null)
            return;

        runtime.ApplyCheckpointNextFrame();
    }

    private static void ApplyPendingCheckpoint()
    {
        if (!respawnPending || !hasSavedCheckpoint)
            return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            return;

        Rigidbody playerRigidbody = player.GetComponent<Rigidbody>();
        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
            playerRigidbody.position = savedPosition;
            playerRigidbody.rotation = savedRotation;
        }
        else
        {
            player.transform.SetPositionAndRotation(savedPosition, savedRotation);
        }

        respawnPending = false;
    }

    private sealed class CheckpointRuntime : MonoBehaviour
    {
        private Coroutine applyCoroutine;

        public void ApplyCheckpointNextFrame()
        {
            if (applyCoroutine != null)
                StopCoroutine(applyCoroutine);

            applyCoroutine = StartCoroutine(ApplyAtEndOfFrame());
        }

        private IEnumerator ApplyAtEndOfFrame()
        {
            yield return null;
            CheckpointTrigger.ApplyPendingCheckpoint();
            applyCoroutine = null;
        }
    }
}
