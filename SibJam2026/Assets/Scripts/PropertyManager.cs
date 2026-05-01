using UnityEngine;

public class PropertyManager : MonoBehaviour
{
    [Tooltip("Дочерние объекты свойств (порядок важен, если используешь SetByIndex)")]
    public GameObject[] propertyChildren;

    [Tooltip("Индекс дочернего объекта свойства, который включится при старте")]
    public int startIndex = 0;

    private int currentIndex;

    void Start()
    {
        SetActiveChild(startIndex);
    }

    public void SetActiveChild(int index)
    {
        for (int i = 0; i < propertyChildren.Length; i++)
        {
            if (propertyChildren[i] != null)
                propertyChildren[i].SetActive(i == index);
        }
        currentIndex = index;
    }

    public void SetPropertyByName(string childName)
    {
        for (int i = 0; i < propertyChildren.Length; i++)
        {
            if (propertyChildren[i] != null && propertyChildren[i].name == childName)
            {
                SetActiveChild(i);
                return;
            }
        }
        Debug.LogWarning($"Property child '{childName}' не найден!");
    }

    public void DisableAll()
    {
        foreach (var child in propertyChildren)
            if (child != null) child.SetActive(false);
        currentIndex = -1;
    }
}