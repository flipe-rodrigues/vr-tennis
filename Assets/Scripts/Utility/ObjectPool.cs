using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ObjectPool<T> where T : CachedGameObjectBhv
{
    private readonly Queue<T> _pool = new Queue<T>();

    public ObjectPool(T prefab, int poolSize)
    {
        string parentName = $"{prefab.name} Pool";

        GameObject parentGameObject = GameObject.Find(parentName);

        if (parentGameObject == null)
        {
            parentGameObject = new GameObject(parentName);
        }

        for (int i = 0; i < poolSize; i++)
        {
            T obj = GameObject.Instantiate(prefab, parent: parentGameObject.transform);

            obj.Active = false;

            this._pool.Enqueue(obj);
        }
    }

    public T Get(bool activate = true)
    {
        T obj = _pool.Dequeue();

        obj.Active = activate;

        return obj;
    }

    public void Return(T obj, bool deactivate = true)
    {
        obj.Active = !deactivate;

        _pool.Enqueue(obj);
    }
}

