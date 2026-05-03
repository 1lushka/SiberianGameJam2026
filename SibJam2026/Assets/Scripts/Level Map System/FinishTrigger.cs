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
        if (ids != null) levelsToUnlockOnFinish.AddRange(ids);
        Debug.Log($"[FinishTrigger] Сохранены ID для открытия: {string.Join(", ", levelsToUnlockOnFinish)}");
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        string levelId = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        int coins = CoinCounter.Instance != null ? CoinCounter.Instance.Coins : 0;
        ProgressManager.Instance.LevelCompleted(levelId, coins);
        Debug.Log($"[FinishTrigger] Уровень '{levelId}' пройден, монет: {coins}");

        Debug.Log($"[FinishTrigger] Открываю уровни: {string.Join(", ", levelsToUnlockOnFinish)}");
        foreach (string id in levelsToUnlockOnFinish)
        {
            ProgressManager.Instance.UnlockNode(id);
            var newState = ProgressManager.Instance.GetState(id);
            Debug.Log($"[FinishTrigger] После UnlockNode('{id}') состояние = {newState}");
        }

        bool isFinal = LevelInitializer.Instance.CurrentConfig != null
                       && LevelInitializer.Instance.CurrentConfig.isFinalLevel;
        string nextScene = isFinal ? victorySceneName : mapSceneName;
        Debug.Log($"[FinishTrigger] Загружаю сцену '{nextScene}'");
        UnityEngine.SceneManagement.SceneManager.LoadScene(nextScene);
    }
}