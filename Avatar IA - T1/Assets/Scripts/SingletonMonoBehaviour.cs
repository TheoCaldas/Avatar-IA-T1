using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SingletonMonoBehaviour<T> : MonoBehaviour
    where T : MonoBehaviour
{
    public static T Instance { get; protected set; }

    protected virtual void Awake()
    {
        if (Instance == null)
        {
            Instance = this as T;
            DontDestroyOnLoad(this.gameObject);
        }
        else if (Instance != this)
        {
            Destroy(this.gameObject);
        }
    }
} 
