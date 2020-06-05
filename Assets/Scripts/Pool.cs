using System.Collections.Generic;
using UnityEngine;

public interface IPoolItem
{
    void ResetItem();
    void Returned();
}

public class Pool<T>
    where T : MonoBehaviour, IPoolItem
{
    private readonly Stack<T> m_inactive;
    private readonly List<T> m_active;

    private readonly Transform m_inactiveObject;

    public Pool(string name, T prefab, int capacity)
    {
        m_inactive = new Stack<T>(capacity);
        m_active = new List<T>(capacity);

        m_inactiveObject = new GameObject(name).transform;

        for (int i = 0; i < capacity; i++)
        {
            T obj = Object.Instantiate(prefab);
            obj.transform.parent = m_inactiveObject;
            obj.name = $"{prefab.name} Pool Item #{i}";

            m_inactive.Push(obj);
        }
    }

    public T Get()
    {
        if (m_inactive.Count == 0)
        {
            return default;
        }

        T item = m_inactive.Pop();
        m_active.Add(item);

        item.ResetItem();

        return item;
    }

    public bool Return(T item)
    {
        if (!m_active.Contains(item))
        {
            // Attempted to return an item not in the pool!
            // Do nothing.
            return false;
        }

        m_active.Remove(item);
        m_inactive.Push(item);

        item.Returned();

        item.transform.parent = m_inactiveObject;

        return true;
    }
}
