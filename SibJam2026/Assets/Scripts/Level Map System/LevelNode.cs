using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class LevelNode : MonoBehaviour
{
    public string levelId;
    public bool isStartNode;
    public LevelConfiguration levelConfig;

    [Header("Сцены")]
    public string gameSceneName = "GameScene";

    [Header("Какие ноды открыть после прохождения")]
    public LevelNode[] levelsToUnlock;

    [Header("UI")]
    public Image background;
    public Button button;
    public TextMeshProUGUI coinsText;
    public GameObject lockIcon;

    public enum NodeState { Locked, Unlocked, Completed }

    void Start()
    {
        Debug.Log($"[LevelNode] Start: '{levelId}' isStartNode={isStartNode}");
        if (ProgressManager.Instance == null)
        {
            Debug.LogError("[LevelNode] ProgressManager.Instance is NULL!");
            return;
        }
        UpdateVisuals();
        if (button != null) button.onClick.AddListener(OnClick);
        else Debug.LogError("[LevelNode] Button не назначен!");
    }

    public void UpdateVisuals()
    {
        if (ProgressManager.Instance == null) return;
        var state = ProgressManager.Instance.GetState(levelId);
        Debug.Log($"[LevelNode] UpdateVisuals '{levelId}': state={state}");

        if (isStartNode && state == NodeState.Locked)
        {
            Debug.Log($"[LevelNode] Стартовая нода '{levelId}' Locked -> вызываю UnlockNode");
            ProgressManager.Instance.UnlockNode(levelId);
            state = ProgressManager.Instance.GetState(levelId);
            Debug.Log($"[LevelNode] После UnlockNode state={state}");
        }

        if (background != null)
        {
            switch (state)
            {
                case NodeState.Locked: background.color = new Color(0.3f, 0.3f, 0.3f); break;
                case NodeState.Unlocked: background.color = Color.white; break;
                case NodeState.Completed: background.color = Color.green; break;
            }
        }
        if (button != null)
        {
            button.interactable = state != NodeState.Locked;
            Debug.Log($"[LevelNode] button.interactable = {button.interactable}");
        }
        if (lockIcon != null) lockIcon.SetActive(state == NodeState.Locked);
        if (coinsText != null) coinsText.text = ProgressManager.Instance.GetCoins(levelId).ToString();
    }

    void OnClick()
    {
        Debug.Log($"[LevelNode] OnClick '{levelId}'");
        if (ProgressManager.Instance == null) { Debug.LogError("PM is null"); return; }
        if (ProgressManager.Instance.GetState(levelId) == NodeState.Locked)
        {
            Debug.LogWarning($"[LevelNode] '{levelId}' всё ещё Locked!");
            return;
        }
        if (levelConfig == null) { Debug.LogError("LevelConfig is null!"); return; }

        // Собираем ID следующих уровней
        List<string> ids = new List<string>();
        if (levelsToUnlock != null)
        {
            foreach (var node in levelsToUnlock)
                if (node != null) ids.Add(node.levelId);
        }
        Debug.Log($"[LevelNode] Передаю уровни для открытия: {string.Join(", ", ids)}");
        FinishTrigger.SetLevelsToUnlock(ids.ToArray());

        ProgressManager.Instance.CurrentLevelConfig = levelConfig;
        Debug.Log($"[LevelNode] Загружаю сцену '{gameSceneName}'");
        UnityEngine.SceneManagement.SceneManager.LoadScene(gameSceneName);
    }
}