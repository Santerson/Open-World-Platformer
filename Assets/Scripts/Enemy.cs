using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] float MaxHealth = 3;
    [SerializeField] float Damage = 1;
    [SerializeField] float InvincibilityTime = 1f;

    float CurrentHealth;
    float InvincibilityTimeLeft = 0f;

    // Start is called before the first frame update
    void Start()
    {
        CurrentHealth = MaxHealth;
    }

    private void Update()
    {
        if (InvincibilityTimeLeft > 0)
        {
            InvincibilityTimeLeft -= Time.deltaTime;
        }
    }

    public void TakenDamage(float damage)
    {
        if (InvincibilityTimeLeft <= 0)
        {
            Debug.Log("Hit enemy");
            CurrentHealth -= damage;
            InvincibilityTimeLeft = InvincibilityTime;
            if (CurrentHealth <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
