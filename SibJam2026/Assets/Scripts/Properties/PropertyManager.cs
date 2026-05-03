using UnityEngine;

public class PropertyManager : MonoBehaviour
{
    [Tooltip("Скрипты свойств (Collectible, Bouncy, Damage, Solid, Slippery, Disappearing)")]
    public MonoBehaviour[] propertyScripts;

    [Tooltip("Материалы для каждого свойства (по тому же индексу)")]
    public Material[] propertyMaterials;

    //public int startIndex = 0;

    private MeshRenderer meshRenderer;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        //SetPropertyIndex(startIndex);
    }

    /// <summary>
    /// Включает скрипт с указанным индексом, остальные отключает. Меняет материал.
    /// </summary>
    public void SetPropertyIndex(int index)
    {
        // Выключаем все скрипты
        for (int i = 0; i < propertyScripts.Length; i++)
        {
            if (propertyScripts[i] != null)
                propertyScripts[i].enabled = false;
        }

        // Включаем нужный
        if (index >= 0 && index < propertyScripts.Length && propertyScripts[index] != null)
            propertyScripts[index].enabled = true;

        // Меняем материал
        if (meshRenderer != null && propertyMaterials != null && index >= 0 && index < propertyMaterials.Length)
            meshRenderer.material = propertyMaterials[index];
    }
}