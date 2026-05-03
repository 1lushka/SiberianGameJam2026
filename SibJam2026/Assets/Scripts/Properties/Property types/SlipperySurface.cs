using UnityEngine;

public class SlipperySurface : BaseProperty
{
	[Tooltip("Optional movement preset override for this surface. If empty, the controller will use its default ice preset.")]
	public CharacterMovementPreset movementPresetOverride;
    private void OnDisable()
    {
        //Destroy(this);
    }
    private void OnEnable()
    {
        SetTopParentLayerAndChildren(gameObject, "Unclimable");
    }
    public void SetTopParentLayerAndChildren(GameObject anyChildObject, string layerName)
    {
        if (anyChildObject == null)
            return;

        int layer = LayerMask.NameToLayer(layerName);
        if (layer == -1)
        {
            Debug.LogError($"Layer '{layerName}' not found.");
            return;
        }

        Transform topParent = anyChildObject.transform.root;
        SetLayerRecursively(topParent, layer);
    }

    private void SetLayerRecursively(Transform target, int layer)
    {
        target.gameObject.layer = layer;

        foreach (Transform child in target)
            SetLayerRecursively(child, layer);
    }
}
