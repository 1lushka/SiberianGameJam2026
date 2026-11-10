using UnityEngine;
using UnityEngine.Localization;


[CreateAssetMenu(fileName = "LevelSelectButtonData", menuName = "Level Map/Level Select Button Data")]
public class LevelSelectButtonData : ScriptableObject
{
    public LocalizedString displayName;
    public int requiredCompletedLevels;
}
