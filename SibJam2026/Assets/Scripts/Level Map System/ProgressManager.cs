using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class ProgressManager : MonoBehaviour
{
    public static ProgressManager Instance { get; private set; }

    private const string PROGRESS_KEY = "MapProgress";

    private Dictionary<string, LevelNode.NodeState> nodeStates;
    private Dictionary<string, int> coinCounts;

    public System.Action OnProgressChanged;
    public LevelConfiguration CurrentLevelConfig { get; set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LevelCompleted(string levelId, int coinsCollected)
    {
        nodeStates[levelId] = LevelNode.NodeState.Completed;
        if (coinCounts.ContainsKey(levelId))
            coinCounts[levelId] = Mathf.Max(coinCounts[levelId], coinsCollected);
        else
            coinCounts[levelId] = coinsCollected;

        Save();
        Debug.Log($"[ProgressManager] Level '{levelId}' marked Completed. Coins={GetCoins(levelId)}. CompletedCount={GetCompletedLevelCount()}.");
        Debug.Log(GetDebugSummary());
        OnProgressChanged?.Invoke();
    }

    public LevelNode.NodeState GetState(string levelId)
    {
        if (nodeStates.ContainsKey(levelId))
            return nodeStates[levelId];

        return LevelNode.NodeState.Locked;
    }

    public int GetCoins(string levelId)
    {
        return coinCounts.ContainsKey(levelId) ? coinCounts[levelId] : 0;
    }

    public void UnlockNode(string levelId)
    {
        if (!nodeStates.ContainsKey(levelId))
        {
            nodeStates[levelId] = LevelNode.NodeState.Unlocked;
            Save();
            OnProgressChanged?.Invoke();
            Debug.Log($"[ProgressManager] New node '{levelId}' added as Unlocked.");
            return;
        }

        if (nodeStates[levelId] == LevelNode.NodeState.Locked)
        {
            nodeStates[levelId] = LevelNode.NodeState.Unlocked;
            Save();
            OnProgressChanged?.Invoke();
            Debug.Log($"[ProgressManager] Node '{levelId}' unlocked.");
        }
    }

    public int GetCompletedLevelCount()
    {
        int count = 0;
        foreach (var pair in nodeStates)
        {
            if (pair.Value == LevelNode.NodeState.Completed)
                count++;
        }

        return count;
    }

    public List<string> GetCompletedLevelIds()
    {
        List<string> completedIds = new List<string>();
        foreach (var pair in nodeStates)
        {
            if (pair.Value == LevelNode.NodeState.Completed)
                completedIds.Add(pair.Key);
        }

        completedIds.Sort();
        return completedIds;
    }

    public string GetDebugSummary()
    {
        StringBuilder builder = new StringBuilder();
        List<string> completedIds = GetCompletedLevelIds();

        builder.AppendLine("[ProgressManager] Progress debug summary");
        builder.AppendLine($"Completed count: {completedIds.Count}");
        builder.AppendLine(completedIds.Count > 0
            ? $"Completed IDs: {string.Join(", ", completedIds)}"
            : "Completed IDs: <empty>");

        if (nodeStates.Count > 0)
        {
            builder.AppendLine("All node states:");
            foreach (var pair in nodeStates)
                builder.AppendLine($"- {pair.Key}: {pair.Value}");
        }
        else
        {
            builder.AppendLine("All node states: <empty>");
        }

        return builder.ToString();
    }

    public void LogDebugSummary()
    {
        Debug.Log(GetDebugSummary());
    }

    public void ClearAllProgress()
    {
        nodeStates.Clear();
        coinCounts.Clear();

        PlayerPrefs.DeleteKey(PROGRESS_KEY);
        PlayerPrefs.Save();

        CompletedLevelsRegistry.ClearCache();
        Debug.Log("[ProgressManager] Progress cleared. MapProgress save deleted.");
        OnProgressChanged?.Invoke();
    }

    private void Save()
    {
        ProgressData data = new ProgressData(nodeStates, coinCounts);
        PlayerPrefs.SetString(PROGRESS_KEY, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    private void Load()
    {
        if (PlayerPrefs.HasKey(PROGRESS_KEY))
        {
            ProgressData data = JsonUtility.FromJson<ProgressData>(PlayerPrefs.GetString(PROGRESS_KEY));
            nodeStates = data.ToStateDictionary();
            coinCounts = data.ToCoinDictionary();
            Debug.Log($"[ProgressManager] Progress loaded. CompletedCount={GetCompletedLevelCount()}.");
        }
        else
        {
            nodeStates = new Dictionary<string, LevelNode.NodeState>();
            coinCounts = new Dictionary<string, int>();
            Debug.Log("[ProgressManager] No saved progress found.");
        }
    }

    [System.Serializable]
    private class ProgressData
    {
        public List<string> levelIds;
        public List<int> states;
        public List<string> coinIds;
        public List<int> coins;

        public ProgressData(Dictionary<string, LevelNode.NodeState> stateDict, Dictionary<string, int> coinDict)
        {
            levelIds = new List<string>(stateDict.Keys);
            states = new List<int>();
            foreach (var id in levelIds)
                states.Add((int)stateDict[id]);

            coinIds = new List<string>(coinDict.Keys);
            coins = new List<int>();
            foreach (var id in coinIds)
                coins.Add(coinDict[id]);
        }

        public Dictionary<string, LevelNode.NodeState> ToStateDictionary()
        {
            var dict = new Dictionary<string, LevelNode.NodeState>();
            for (int i = 0; i < levelIds.Count; i++)
                dict[levelIds[i]] = (LevelNode.NodeState)states[i];
            return dict;
        }

        public Dictionary<string, int> ToCoinDictionary()
        {
            var dict = new Dictionary<string, int>();
            for (int i = 0; i < coinIds.Count; i++)
                dict[coinIds[i]] = coins[i];
            return dict;
        }
    }
}
