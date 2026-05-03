using UnityEngine;

public class Solid : MonoBehaviour
{
    private Collider col;

    void Awake()
    {
        col = GetComponent<Collider>();
    }

    void OnEnable()
    {
        if (col) col.isTrigger = false;
    }

    void OnDisable() { }
}