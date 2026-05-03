using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "Level Configuration")]
public class LevelConfiguration : ScriptableObject
{
    public string levelID;
    public bool isFinalLevel;

    [System.Serializable]
    public struct TagPropertyBinding
    {
        public string tag;
        public int propertyIndex;
    }

    public TagPropertyBinding[] tagBindings;
}