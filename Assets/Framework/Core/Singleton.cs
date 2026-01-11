using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static readonly object lockObject = new();

    private static T instance;
    public static T Instance
    {
        get
        {
            // 双重检查锁定模式
            if (instance == null)
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        T existingInstance = FindObjectOfType<T>();
                        
                        if (existingInstance != null)
                        {
                            instance = existingInstance;
                        }
                        else
                        {
                            // 创建新实例
                            GameObject singletonObject = new($"{typeof(T).Name} (Singleton)");
                            instance = singletonObject.AddComponent<T>();
                            DontDestroyOnLoad(singletonObject);
                        }
                    }
                }
            }

            return instance;
        }
    }
    public static bool HasInstance => instance != null;

    public static bool TryGetInstance(out T result)
    {
        result = instance;
        return HasInstance;
    }
    public static void DestroyInstance()
    {
        lock (lockObject)
        {
            if (instance != null)
            {
                Destroy(instance.gameObject);
                instance = null;
            }
        }
    }

    protected virtual void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogWarning($"[Singleton] Another instance of {typeof(T)} already exists. Destroying this duplicate: {gameObject.name}");
            Destroy(gameObject);
            return;
        }

        OnInit();
        DontDestroyOnLoad(gameObject);
    }
    protected virtual void OnInit()
    {
        // 子类重写此方法执行初始化
    }
    protected virtual void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
    protected virtual void OnApplicationQuit()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
