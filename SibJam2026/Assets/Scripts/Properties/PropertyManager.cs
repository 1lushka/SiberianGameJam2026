using UnityEngine;

public class PropertyManager : MonoBehaviour
{
    [Tooltip("Property scripts by index (Collectible, Bouncy, Damage, Solid, Slippery, Disappearing)")]
    public MonoBehaviour[] propertyScripts;

    [Tooltip("Materials for each property, matched by the same index")]
    public Material[] propertyMaterials;

    private Renderer targetRenderer;

    void Awake()
    {
        CacheRenderer();
    }

    private void CacheRenderer()
    {
        if (targetRenderer != null)
            return;

        targetRenderer = GetComponent<Renderer>();
        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<Renderer>(true);
    }

    /// <summary>
    /// Enables the property script with the given index, disables the others, and swaps the material.
    /// </summary>
    public void SetPropertyIndex(int index)
    {
        CacheRenderer();

        // Disable all property scripts first.
        for (int i = 0; i < propertyScripts.Length; i++)
        {
            if (propertyScripts[i] != null)
                propertyScripts[i].enabled = false;
        }

        // Then enable the selected one.
        if (index >= 0 && index < propertyScripts.Length && propertyScripts[index] != null)
            propertyScripts[index].enabled = true;

        // Swap the material after the renderer is cached.
        if (targetRenderer != null && propertyMaterials != null && index >= 0 && index < propertyMaterials.Length)
            targetRenderer.material = propertyMaterials[index];
    }
}
