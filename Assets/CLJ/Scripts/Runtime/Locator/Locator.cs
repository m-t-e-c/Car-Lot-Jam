using System;
using System.Collections.Generic;
using UnityEngine;

namespace CLJ
{
    public class Locator
    {
        static          Locator                  _instance;
        static readonly object                   SyncRoot  = new();
        static readonly Dictionary<Type, object> Managers = new();

        public static Locator Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                lock (SyncRoot)
                {
                    _instance ??= new Locator();
                }

                return _instance;
            }
        }

        public void Register<T>(object manager)
        {
            lock (SyncRoot)
            {
                if (Managers.ContainsKey(typeof(T)))
                {
                    Managers[typeof(T)] = manager;
                }
                else
                {
                    Managers.Add(typeof(T), manager);
                }
            }
        }

        public T Resolve<T>()
        {
            lock (SyncRoot)
            {
                if (!Managers.TryGetValue(typeof(T), out object manager))
                {
                    Debug.LogError($"Manager not found {(typeof(T).Name)}");
                }

                return (T)manager;
            }
        }

        public void Reset()
        {
            lock (SyncRoot)
            {
                Managers.Clear();
            }
        }
    }
}