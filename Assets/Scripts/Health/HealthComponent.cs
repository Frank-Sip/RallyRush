using UnityEngine;
using System;

public class HealthComponent : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100;
    private float currentHealth;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;

    public event Action OnDamageTaken;
    public event Action OnDeath;

    public void CustomAwake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        OnDamageTaken?.Invoke();
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        OnDeath?.Invoke();
        Destroy(gameObject);
    }
}
