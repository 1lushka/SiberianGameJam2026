using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class LevelSelectButtonLockController : MonoBehaviour
{
    [SerializeField] private LevelSelectButtonData buttonData;
    [SerializeField] private LevelNode levelNode;
    [SerializeField] private Button targetButton;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private GameObject lockVisual;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private bool destroyLockVisualWhenUnlocked = true;

    private Coroutine delayedRefreshRoutine;

    private void Reset()
    {
        AutoAssignReferences();
    }

    private void Awake()
    {
        AutoAssignReferences();
    }

    private void OnEnable()
    {
        SubscribeToProgress();
        RefreshDeferred();
    }

    private void Start()
    {
        Refresh();
    }

    private void OnDisable()
    {
        UnsubscribeFromProgress();
    }

    private void OnValidate()
    {
        AutoAssignReferences();
        ApplyStaticText();
    }

    private void OnProgressChanged()
    {
        Refresh();
    }

    public void Refresh()
    {
        ApplyStaticText();

        if (buttonData == null)
            return;

        CompletedLevelsRegistry.RefreshFromSave();

        int completedCount = CompletedLevelsRegistry.CompletedCount;
        int requiredCount = Mathf.Max(0, buttonData.requiredCompletedLevels);
        bool isUnlocked = completedCount >= requiredCount;

        if (isUnlocked)
            UnlockLevelNodeIfNeeded();

        if (targetButton != null)
            targetButton.interactable = isUnlocked;

        if (progressText != null)
            progressText.text = isUnlocked ? string.Empty : $"{completedCount}/{requiredCount}";

        if (lockVisual != null)
        {
            if (isUnlocked)
            {
                if (destroyLockVisualWhenUnlocked)
                    Destroy(lockVisual);
                else
                    lockVisual.SetActive(false);
            }
            else
            {
                lockVisual.SetActive(true);
            }
        }
    }

    private void RefreshDeferred()
    {
        if (delayedRefreshRoutine != null)
            StopCoroutine(delayedRefreshRoutine);

        delayedRefreshRoutine = StartCoroutine(RefreshNextFrame());
    }

    private IEnumerator RefreshNextFrame()
    {
        yield return null;
        delayedRefreshRoutine = null;
        Refresh();
    }

    private void UnlockLevelNodeIfNeeded()
    {
        if (levelNode == null || ProgressManager.Instance == null || string.IsNullOrEmpty(levelNode.levelId))
            return;

        if (ProgressManager.Instance.GetState(levelNode.levelId) == LevelNode.NodeState.Locked)
            ProgressManager.Instance.UnlockNode(levelNode.levelId);
    }

    private void ApplyStaticText()
    {
        if (titleText != null && buttonData != null && !string.IsNullOrWhiteSpace(buttonData.displayName))
            titleText.text = buttonData.displayName;
    }

    private void SubscribeToProgress()
    {
        if (ProgressManager.Instance != null)
            ProgressManager.Instance.OnProgressChanged += OnProgressChanged;
    }

    private void UnsubscribeFromProgress()
    {
        if (ProgressManager.Instance != null)
            ProgressManager.Instance.OnProgressChanged -= OnProgressChanged;
    }

    private void AutoAssignReferences()
    {
        if (levelNode == null)
            levelNode = GetComponent<LevelNode>();

        if (targetButton == null)
            targetButton = GetComponent<Button>();

        if (titleText == null)
        {
            Transform titleTransform = transform.Find("LevelTitle");
            if (titleTransform != null)
                titleText = titleTransform.GetComponent<TMP_Text>();
        }

        if (lockVisual == null)
        {
            Transform lockTransform = transform.Find("Lock (1)");
            if (lockTransform != null)
                lockVisual = lockTransform.gameObject;
        }

        if (progressText == null && lockVisual != null)
            progressText = lockVisual.GetComponentInChildren<TMP_Text>(true);
    }
}
