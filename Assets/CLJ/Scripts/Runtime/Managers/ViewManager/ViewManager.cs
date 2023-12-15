using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace CLJ.Runtime.Managers.ViewManager
{
    public class ViewManager : IViewManager
    {
        public event Action<Component> OnViewLoaded;

        readonly Dictionary<Type, GameObject> viewInstances = new Dictionary<Type, GameObject>();

        public T GetView<T>() where T : Component
        {
            Type viewType = typeof(T);

            if (viewInstances.TryGetValue(viewType, out GameObject viewObject))
            {
                return viewObject.GetComponent<T>();
            }

            Debug.LogError("View not found for type: " + viewType.Name);
            return null;
        }

        public void LoadView<T>(Action<T> onLoaded = null) where T : Component
        {
            Type viewType = typeof(T);

            if (viewInstances.TryGetValue(viewType, out GameObject viewObject))
            {
                viewObject.SetActive(true);
                onLoaded?.Invoke(viewObject.GetComponent<T>());
                return;
            }

            Addressables.LoadAssetAsync<GameObject>(viewType.Name).Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    GameObject prefab        = handle.Result;
                    GameObject newViewObject = Object.Instantiate(prefab);
                    T          viewComponent = newViewObject.GetComponent<T>();

                    if (viewComponent != null)
                    {
                        viewInstances[viewType] = newViewObject;
                        OnViewLoaded?.Invoke(viewComponent);
                        onLoaded?.Invoke(viewComponent);
                    }
                    else
                    {
                        Debug.LogError("View component not found on prefab for type: " + viewType.Name);
                    }
                }
                else
                {
                    Debug.LogError("Failed to load view prefab for type: " + viewType.Name);
                }
            };
        }

        public void CloseView<T>() where T : Component
        {
            Type viewType = typeof(T);

            if (viewInstances.TryGetValue(viewType, out GameObject viewObject))
            {
                viewInstances.Remove(viewType);
                Object.Destroy(viewObject);
            }
        }
    }
}