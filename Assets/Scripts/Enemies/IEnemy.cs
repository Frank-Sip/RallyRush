using UnityEngine;

public interface IEnemy
{
    float CollisionRadius { get; }
    void TakeDamage();
}
