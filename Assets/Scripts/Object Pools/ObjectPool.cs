using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class ObjectPool<T> where T : Component
{
    private readonly T prefab;
    private readonly Transform parent;
    private readonly int expandCount;
    private readonly Action<T> onCreate;

    private readonly Queue<T> availableObjects = new Queue<T>();

    public ObjectPool(T prefab, Transform parent, int initialCount, Action<T> onCreate = null)
        : this(prefab, parent, initialCount, 0, onCreate)
    {
    }

    public ObjectPool(T prefab, Transform parent, int initialCount, int expandCount, Action<T> onCreate = null)
    {
        this.prefab = prefab;
        this.parent = parent;
        this.expandCount = expandCount;
        this.onCreate = onCreate;

        Expand(initialCount);
    }

    private void Expand(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            T instance = Object.Instantiate(prefab, parent);
            onCreate?.Invoke(instance);
            instance.gameObject.SetActive(false);
            availableObjects.Enqueue(instance);
        }
    }

    public T Get()
    {
        if (availableObjects.Count == 0)
        {
            if (expandCount == 0)
            {
                throw new InvalidOperationException($"The fixed pool for {typeof(T).Name} is empty.");
            }

            Expand(expandCount);
        }

        T instance = availableObjects.Dequeue();
        instance.gameObject.SetActive(true);
        return instance;
    }

    public void Return(T instance)
    {
        instance.gameObject.SetActive(false);
        instance.transform.SetParent(parent);
        availableObjects.Enqueue(instance);
    }
}
