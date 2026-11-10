using UnityEngine;

// Editor guide only: the checkpoint root's local XZ plane is the planting surface.
public class PenPlacementGuide : MonoBehaviour
{
    public Vector2 size = new Vector2(12, 12);
    void OnDrawGizmos()
    {
        var previous = Gizmos.matrix;
        var previousColor = Gizmos.color;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = new Color(.15f, .85f, 1f, .8f);
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(size.x, 0, size.y));
        Gizmos.DrawLine(new Vector3(-size.x/2, 0, 0), new Vector3(size.x/2, 0, 0));
        Gizmos.DrawLine(new Vector3(0, 0, -size.y/2), new Vector3(0, 0, size.y/2));
        Gizmos.matrix = previous;
        Gizmos.color = previousColor;
    }
}
