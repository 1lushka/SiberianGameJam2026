using UnityEngine;

public class LevelInitializer : MonoBehaviour
{
    public static LevelInitializer Instance { get; private set; }
    public LevelConfiguration CurrentConfig { get; private set; }

    void Awake()
    {
        Instance = this;
        if (ProgressManager.Instance != null)
        {
            CurrentConfig = ProgressManager.Instance.CurrentLevelConfig;
            ProgressManager.Instance.CurrentLevelConfig = null;
        }
    }

    void Start()
    {
        if (CurrentConfig != null)
        {
            foreach (var binding in CurrentConfig.tagBindings)
            {
                GameObject[] objects = GameObject.FindGameObjectsWithTag(binding.tag);
                foreach (GameObject obj in objects)
                {
                    PropertyManager pm = obj.GetComponent<PropertyManager>();
                    if (pm != null) pm.SetPropertyIndex(binding.propertyIndex);
                }
            }
        }
    }
}