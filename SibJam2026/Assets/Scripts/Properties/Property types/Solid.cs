using UnityEngine;

public class Solid : BaseProperty
{
    private Collider col;
    private Animator anim;
    void Awake()
    {
        col = GetComponent<Collider>();
        anim=GetComponentInChildren<Animator>();
    }

    void OnEnable()
    {
        if (col) col.isTrigger = false;
        if (anim) anim.enabled = false;
    }

    void OnDisable() { }
}