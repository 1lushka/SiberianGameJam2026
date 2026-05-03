using System;
using System.Collections.Generic;
using UnityEngine;

public static class CompletedLevelsRegistry
{
    private const string ProgressKey = "MapProgress";

    private static readonly HashSet<string> completedLevelIds = new HashSet<string>();
    private static bool isLoaded;

    public static int CompletedCount
    {
        get
        {
            EnsureLoaded();
            return completedLevelIds.Count;
        }
    }

    public static IReadOnlyCollection<string> GetCompletedLevelIds()
    {
        EnsureLoaded();
        return completedLevelIds;
    }

    public static bool IsCompleted(string levelId)
    {
        if (string.IsNullOrEmpty(levelId))
            return false;

        EnsureLoaded();
        return completedLevelIds.Contains(levelId);
    }

    public static void RefreshFromSave()
    {
        isLoaded = false;
        EnsureLoaded();
    }

    public static void ClearCache()
    {
        completedLevelIds.Clear();
        isLoaded = false;
    }

    private static void EnsureLoaded()
    {
        if (isLoaded)
            return;

        completedLevelIds.Clear();

        if (!PlayerPrefs.HasKey(ProgressKey))
        {
            isLoaded = true;
            return;
        }

        string json = PlayerPrefs.GetString(ProgressKey);
        if (string.IsNullOrWhiteSpace(json))
        {
            isLoaded = true;
            return;
        }

        ProgressSnapshot snapshot = JsonUtility.FromJson<ProgressSnapshot>(json);
        if (snapshot == null || snapshot.levelIds == null || snapshot.states == null)
        {
            isLoaded = true;
            return;
        }

        int count = Math.Min(snapshot.levelIds.Count, snapshot.states.Count);
        for (int i = 0; i < count; i++)
        {
            if (snapshot.states[i] != (int)LevelNode.NodeState.Completed)
                continue;

            string levelId = snapshot.levelIds[i];
            if (!string.IsNullOrEmpty(levelId))
                completedLevelIds.Add(levelId);
        }

        isLoaded = true;
    }

    [Serializable]
    private class ProgressSnapshot
    {
        public List<string> levelIds;
        public List<int> states;
    }
}
