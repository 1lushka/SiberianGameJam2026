using UnityEngine;

public class Solid : BaseProperty
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