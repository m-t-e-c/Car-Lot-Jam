using System;
using UnityEngine;

namespace CLJ.Managers.ViewManager
{
    public interface IViewManager
    {
        public event Action<Component> OnViewLoaded;
		
        T GetView<T>() where T : Component;
		
        void LoadView<T>(Action<T> onLoaded = null) where T : Component;

        void CloseView<T>() where T : Component;
    }
}