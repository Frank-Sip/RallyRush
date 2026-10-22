using System.Collections.Generic;
using UnityEngine;

public class SplashPool
{
    private readonly Transform parent;
    private readonly int initialCount;
    private readonly int expandCount;
    private readonly Dictionary<DeathEffect, SplashEntry> entries = new Dictionary<DeathEffect, SplashEntry>();
    private readonly Dictionary<DeathEffect, SplashEntry> effectEntries = new Dictionary<DeathEffect, SplashEntry>();

    public SplashPool(Transform parent, int initialCount, int expandCount)
    {
        this.parent = parent;
        this.initialCount = initialCount;
        this.expandCount = expandCount;
    }

    public void Play(DeathEffect effectPrefab, Transform source, Vector3 scale, Color color)
    {
        Preload(effectPrefab);
        SplashEntry entry = entries[effectPrefab];
        DeathEffect effect = entry.Get();
        effect.transform.SetPositionAndRotation(source.position, source.rotation);
        effect.Play(scale, color);
    }

    public void Preload(DeathEffect effectPrefab)
    {
        if (!entries.ContainsKey(effectPrefab))
        {
            SplashEntry entry = new SplashEntry(this, effectPrefab, parent, initialCount, expandCount);
            entries.Add(effectPrefab, entry);
        }
    }

    public void Return(DeathEffect effect)
    {
        effectEntries[effect].Return(effect);
    }

    private void RegisterEffect(DeathEffect effect, SplashEntry entry)
    {
        effect.CustomAwake(this);
        effectEntries.Add(effect, entry);
    }

    private class SplashEntry
    {
        private readonly SplashPool owner;
        private readonly ObjectPool<DeathEffect> pool;

        public SplashEntry(SplashPool owner, DeathEffect prefab, Transform parent, int initialCount, int expandCount)
        {
            this.owner = owner;
            pool = new ObjectPool<DeathEffect>(prefab, parent, initialCount, expandCount, RegisterEffect);
        }

        private void RegisterEffect(DeathEffect effect)
        {
            owner.RegisterEffect(effect, this);
        }

        public DeathEffect Get()
        {
            return pool.Get();
        }

        public void Return(DeathEffect effect)
        {
            pool.Return(effect);
        }
    }
}
