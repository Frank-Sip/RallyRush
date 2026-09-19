using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ServiceProvider
{
    private static readonly Dictionary<System.Type, object> services = new Dictionary<System.Type, object>();

    public static void RegisterService<T>(T service)
    {
        var type = typeof(T);
        if (!services.ContainsKey(type))
        {
            services[type] = service;
        }
    }

    public static T GetService<T>()
    {
        var type = typeof(T);
        if (services.TryGetValue(type, out var service))
        {
            return (T)service;
        }

        throw new System.Exception($"Service of type {type} not found.");
    }
}