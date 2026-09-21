using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    public Runner Target { get; private set; }

    public bool HasTarget => Target != null;

    public virtual void SetTarget(Runner target)
    {
        Target = target;
    }
}
