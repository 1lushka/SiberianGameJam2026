using UnityEngine;
using System.Collections.Generic;

public class FinishTrigger : MonoBehaviour
{
    public string mapSceneName = "Map";
    public string victorySceneName = "VictoryScene";

    private static List<string> levelsToUnlockOnFinish = new List<string>();

    public static void SetLevelsToUnlock(string[] ids)
    {
        levelsToUnlockOnFinish.Clear();
        if (ids != null)
            levelsToUnlockOnFinish.AddRange(ids);

        Debug.Log($"[FinishTrigger] Saved IDs to unlock: {string.Join(", ", levelsToUnlockOnFinish)}");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        string levelId = ResolveCompletedLevelId();
        int coins = CoinCounter.Instance != null ? CoinCounter.Instance.Coins : 0;
        ProgressManager.Instance.LevelCompleted(levelId, coins);
        Debug.Log($"[FinishTrigger] Level '{levelId}' completed, coins: {coins}");

        Debug.Log($"[FinishTrigger] Unlocking levels: {string.Join(", ", levelsToUnlockOnFinish)}");
        foreach (string id in levelsToUnlockOnFinish)
        {
            ProgressManager.Instance.UnlockNode(id);
            var newState = ProgressManager.Instance.GetState(id);
            Debug.Log($"[FinishTrigger] After UnlockNode('{id}') state = {newState}");
        }

        bool isFinal = LevelInitializer.Instance != null &&
                       LevelInitializer.Instance.CurrentConfig != null
                       && LevelInitializer.Instance.CurrentConfig.isFinalLevel;
        string nextScene = isFinal ? victorySceneName : mapSceneName;
        Debug.Log($"[FinishTrigger] Loading scene '{nextScene}'");
        UnityEngine.SceneManagement.SceneManager.LoadScene(nextScene);
    }

    private string ResolveCompletedLevelId()
    {
        if (LevelInitializer.Instance != null &&
            LevelInitializer.Instance.CurrentConfig != null &&
            !string.IsNullOrWhiteSpace(LevelInitializer.Instance.CurrentConfig.levelID))
        {
            return LevelInitializer.Instance.CurrentConfig.levelID;
        }

        return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
    }
}
