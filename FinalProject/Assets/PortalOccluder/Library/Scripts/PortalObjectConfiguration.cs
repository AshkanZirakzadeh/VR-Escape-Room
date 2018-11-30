using UnityEngine;
using System.Collections;
using System;

namespace CirrusPlay.PortalLibrary { 
    /// <summary>
    /// This behavior is used to configure a renderable object or light specifically for the Portal occlusion system.
    /// </summary>
    public class PortalObjectConfiguration : MonoBehaviour {

        /// <summary>
        /// When true, this object will not participate in portal rendering.
        /// </summary>
        public bool ignore = false;
        public bool isExteriorObject = false;
        private bool lastIsExteriorObject = false;

        /// <summary>
        /// Determine if an object can participate in portal rendering.
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static bool CanParticipate(PortalObjectConfiguration config)
        {
            return config == null || config.ignore == false;
        }

        /// <summary>
        /// Determine if an object can participate in portal rendering.
        /// </summary>
        /// <typeparam name="Type"></typeparam>
        /// <param name="component"></param>
        /// <returns></returns>
        public static bool CanParticipate<Type>(Type component) where Type : Component
        {
            return CanParticipate(component.GetComponent<PortalObjectConfiguration>());
        }

        private void Update()
        {
            UpdateExteriorObjects();
        }

        /// <summary>
        /// Register or unregister if the state of this object has changed.
        /// </summary>
        private void UpdateExteriorObjects()
        {
            var changed = isExteriorObject != lastIsExteriorObject;
            lastIsExteriorObject = isExteriorObject;
            if (changed)
            {
                if (isExteriorObject)
                    PortalManager.RegisterExteriorObject(this);
                else
                    PortalManager.UnregisterExteriorObject(this);
            }
        }

        /// <summary>
        /// Determine if an object is marked as an external object for out-door rendering.
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        public static bool IsExteriorObject(Component component)
        {
            var obj = component != null ? component.GetComponent<PortalObjectConfiguration>() : null;
            return obj != null && obj.isExteriorObject;
        }

    }

}