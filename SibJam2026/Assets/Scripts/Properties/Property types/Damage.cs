using UnityEngine;

public class Damage : BaseProperty
{
    public int damageAmount = 1;

    void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        //PlayerHealth health = collision.gameObject.GetComponent<PlayerHealth>();
        //if (health)
        //    health.TakeDamage(damageAmount);
    }
}