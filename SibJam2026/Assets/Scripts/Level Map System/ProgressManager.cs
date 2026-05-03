using System.Collections.Generic;
using UnityEngine;

public class ProgressManager : MonoBehaviour
{
    public static ProgressManager Instance { get; private set; }

    const string PROGRESS_KEY = "MapProgress";
    Dictionary<string, LevelNode.NodeState> nodeStates;
    Dictionary<string, int> coinCounts;

    public System.Action OnProgressChanged;
    public LevelConfiguration CurrentLevelConfig { get; set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
        }
        else Destroy(gameObject);
    }

    public void LevelCompleted(string levelId, int coinsCollected)
    {
        nodeStates[levelId] = LevelNode.NodeState.Completed;
        if (coinCounts.ContainsKey(levelId))
            coinCounts[levelId] = Mathf.Max(coinCounts[levelId], coinsCollected);
        else coinCounts[levelId] = coinsCollected;

        Save();
        OnProgressChanged?.Invoke();
    }

    public LevelNode.NodeState GetState(string levelId)
    {
        if (nodeStates.ContainsKey(levelId)) return nodeStates[levelId];
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
            Debug.Log($"[ProgressManager] Новый узел '{levelId}' добавлен как Unlocked.");
            return;
        }

        if (nodeStates[levelId] == LevelNode.NodeState.Locked)
        {
            nodeStates[levelId] = LevelNode.NodeState.Unlocked;
            Save();
            OnProgressChanged?.Invoke();
            Debug.Log($"[ProgressManager] Узел '{levelId}' разблокирован.");
        }
    }

    void Save()
    {
        ProgressData data = new ProgressData(nodeStates, coinCounts);
        PlayerPrefs.SetString(PROGRESS_KEY, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    void Load()
    {
        if (PlayerPrefs.HasKey(PROGRESS_KEY))
        {
            ProgressData data = JsonUtility.FromJson<ProgressData>(PlayerPrefs.GetString(PROGRESS_KEY));
            nodeStates = data.ToStateDictionary();
            coinCounts = data.ToCoinDictionary();
        }
        else
        {
            nodeStates = new Dictionary<string, LevelNode.NodeState>();
            coinCounts = new Dictionary<string, int>();
        }
    }

    [System.Serializable]
    class ProgressData
    {
        public List<string> levelIds;
        public List<int> states;
        public List<string> coinIds;
        public List<int> coins;

        public ProgressData(Dictionary<string, LevelNode.NodeState> stateDict, Dictionary<string, int> coinDict)
        {
            levelIds = new List<string>(stateDict.Keys);
            states = new List<int>();
            foreach (var id in levelIds) states.Add((int)stateDict[id]);

            coinIds = new List<string>(coinDict.Keys);
            coins = new List<int>();
            foreach (var id in coinIds) coins.Add(coinDict[id]);
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