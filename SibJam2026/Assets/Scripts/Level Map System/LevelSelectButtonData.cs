using UnityEngine;

[CreateAssetMenu(fileName = "LevelSelectButtonData", menuName = "Level Map/Level Select Button Data")]
public class LevelSelectButtonData : ScriptableObject
{
    public string displayName;
    public int requiredCompletedLevels;
}
