using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CirrusPlay.PortalLibrary.Extensions;

namespace CirrusPlay.PortalLibrary
{
    public class PortalManager : MonoBehaviour {

        /// <summary>
        /// Interface for retrieving visibility information.
        /// </summary>
        public interface IVisible
        {
            bool IsVisible();
        }
        
        /// <summary>
        /// Structure used to notify completion of occlusion update.
        /// </summary>
        public struct PortalManagerInfo
        {
            public PortalManager manager;
        }

        // After update finishes being called and if occlusion was tested, this will fire //
        public delegate void CompletedCallback(PortalManagerInfo info);
        public event CompletedCallback Completed;
        // After update, if the portal manager did not process occlusion, the Incomplete event will be fired //
        public delegate void IncompleteCallback(PortalManagerInfo info);
        public event IncompleteCallback Incomplete;

        // Should we display debug info to console? //
        public bool displayResults = false;

        // This is the single instance of our object //
        private static PortalManager manager = null;
        private static bool applicationIsQuitting = false;
        private static object internalLock = new object();

        private readonly HashSet<Portal> portals = new HashSet<Portal>();
        private readonly HashSet<Room> rooms = new HashSet<Room>();
        public static readonly PortalSharedItemManager<Renderer> renderers = new PortalSharedItemManager<Renderer>();
        public static readonly PortalSharedItemManager<Light> lights = new PortalSharedItemManager<Light>();
        private Room cameraRoom = null;
        private Portal cameraPortal = null;
        private bool processedUpdate = false;
        private bool enableProcessing = true;

        private int debugRoomsConsidered = 0;
        private int debugRoomsRendered = 0;
        private int debugRoomsIndirect = 0;

        private HashSet<PortalObjectConfiguration> exteriorObjects = new HashSet<PortalObjectConfiguration>();
        private int visibleExteriors = 0;
        private bool lastVisibleExteriors = true;

        /// <summary>
        /// Register an object as an external (outside scenery) object.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static PortalObjectConfiguration RegisterExteriorObject(PortalObjectConfiguration obj)
        {
            var manager = Instance();
            if (obj != null && manager != null)
            {
                // Show or hide new object based on visibleExteriors value //
                if (!manager.exteriorObjects.Contains(obj))
                    ShowExteriorObject(obj, manager.visibleExteriors > 0);
                manager.exteriorObjects.Add(obj);
                
            }
            return obj;
        }

        /// <summary>
        /// Unregister an object as an external (outside scenery) object.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static PortalObjectConfiguration UnregisterExteriorObject(PortalObjectConfiguration obj)
        {
            var manager = Instance();
            if (obj != null && manager != null)
            {
                // Show the object being removed as we are no longer managing it //
                if (manager.exteriorObjects.Contains(obj))
                    ShowExteriorObject(obj, true);
                manager.exteriorObjects.Remove(obj);
            }
            return obj;
        }

        /// <summary>
        /// Determine if the object is already registered as an external (outside scenery) object.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool IsRegisteredExteriorObject(PortalObjectConfiguration obj)
        {
            return manager.exteriorObjects.Contains(obj);
        }

        /// <summary>
        /// Display or hide the exterior (outdoor scenery) objects.
        /// </summary>
        /// <param name="visible"></param>
        private static void ShowExteriorObjects(bool visible)
        {
            var manager = Instance();
            if (manager != null)
            {
                foreach (var exterior in manager.exteriorObjects)
                {
                    ShowExteriorObject(exterior, visible);
                }
            }
        }

        /// <summary>
        /// Display or hide an exterior object.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="visible"></param>
        private static void ShowExteriorObject(PortalObjectConfiguration obj, bool visible)
        {
            foreach (var rend in obj.gameObject.GetComponentsInChildren<Renderer>())
                rend.enabled = visible;
            foreach (var light in obj.gameObject.GetComponentsInChildren<Light>())
                light.enabled = visible;
            foreach (var terrain in obj.gameObject.GetComponentsInChildren<Terrain>())
            {
                terrain.drawHeightmap = visible;
                terrain.drawTreesAndFoliage = visible;
            }
        }

        /// <summary>
        /// Notify that an external object (outdoor scenery) has been flagged for visibility.
        /// </summary>
        public static void NotifyExteriorObject()
        {
            var manager = Instance();
            if (manager != null)
                manager.visibleExteriors++;
        }
        
        /// <summary>
        /// Determine if portal managing is available for processing.
        /// </summary>
        /// <returns></returns>
        public static bool Available()
        {
            return !applicationIsQuitting && Instance() != null;
        }

        /// <summary>
        /// Register a portal for this portal manager.
        /// </summary>
        /// <param name="portal"></param>
        public static void Register(Portal portal)
        {
            if (Available() && portal != null) manager.portals.Add(portal);
        }

        /// <summary>
        /// Register a room for this room manager.
        /// </summary>
        /// <param name="room"></param>
        public static void Register(Room room)
        {
            if (Available() && room != null) manager.rooms.Add(room);
        }

        /// <summary>
        /// Unregister a portal from the portal manager.
        /// </summary>
        /// <param name="portal"></param>
        public static void Unregister(Portal portal)
        {
            if (Available() && portal != null) manager.portals.Remove(portal);
        }

        /// <summary>
        /// Unregister a room from the portal manager.
        /// </summary>
        /// <param name="room"></param>
        public static void Unregister(Room room)
        {
            if(Available() && room != null) manager.rooms.Remove(room);
        }

        /// <summary>
        /// Retrieve the single instance of the portal manager.
        /// </summary>
        /// <returns></returns>
        private static PortalManager Instance()
        {
            if (applicationIsQuitting || !Application.isPlaying)
            {
                return null;
            }

            // Unity's prefered singleton pattern for this manager //
            if (manager == null)
            {
                lock (internalLock)
                {
                    manager = (PortalManager)FindObjectOfType(typeof(PortalManager));

                    if (FindObjectsOfType(typeof(PortalManager)).Length > 1)
                    {
                        Debug.LogError("[PortalManager] Something went really wrong " +
                            " - there should never be more than 1 PortalManager!" +
                            " Reopening the scene might fix it.");
                        return manager;
                    }

                    if (manager == null)
                    {
                        GameObject singleton = new GameObject();
                        manager = singleton.AddComponent<PortalManager>();
                        singleton.name = "(singleton) " + typeof(PortalManager).ToString();

                        DontDestroyOnLoad(singleton);

                        Debug.Log("[PortalManager] An instance of " + typeof(PortalManager) +
                            " is needed in the scene, so '" + singleton +
                            "' was created with DontDestroyOnLoad.");
                    }
                    else
                    {
                        Debug.Log("[PortalManager] Using instance already created: " +
                            manager.gameObject.name);
                    }
                }
            }

            return manager;
        }

        private void OnDestroy()
        {
            // Notify manager that we are quitting so future requests don't spawn a new manager //
            applicationIsQuitting = true;
        }

        private void PrepareUpdate()
        {
            if (Available() && manager.enableProcessing)
            {
                rooms.EachNotNull(r => r.PrepareUpdate());
                portals.EachNotNull(p => p.PrepareUpdate());
                visibleExteriors = 0;
            }
        }

        /// <summary>
        /// Begins occlusion testing from the given room.
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="portal"></param>
        public static void BeginProcessRoom(Camera camera, Room room, bool force = false)
        {
            // For now, if the camera is not in a room, we leave early //
            if (room == null || !Available()) return;

            if (force || (manager != null && !manager.processedUpdate && manager.enableProcessing))
            {

                // Mark that an update is being processed //
                manager.processedUpdate = true;

                // Save this room as the camera room //
                manager.cameraRoom = room;

                // Reset portals and rooms //
                manager.PrepareUpdate();

                // If the camera is in a room, start doing occlusion from the camera's perspective //
                room.BeginPortalProcessing(camera);

                // Finalize processing //
                manager.EndProcessing();
            }
        }

        private void DisplayExternalObjects()
        {
            var available = visibleExteriors > 0;
            if (available != lastVisibleExteriors)
            {
                ShowExteriorObjects(available);
                lastVisibleExteriors = available;
            }
        }

        /// <summary>
        /// Begins occlusion testing from the given portal.
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="portal"></param>
        public static void BeginProcessPortal(Camera camera, Portal portal, bool force = false)
        {
            if (portal == null || !Available()) return;

            if (force || (!manager.processedUpdate && manager.enableProcessing))
            {

                // Mark the an update is being processed //
                manager.processedUpdate = true;

                // For now, if the camera is not in a room, we leave early //
                if (portal == null) return;

                // Save this portal as the room portal //
                manager.cameraPortal = portal;

                // Reset portals and rooms //
                manager.PrepareUpdate();

                // If the camera is in a portal, render the portal's contents //
                if (portal != null)
                {
                    portal.Process(camera, null);
                }
                
                // Finalize processing //
                manager.EndProcessing();
            }
        }

        private void LateUpdate()
        {
            if (manager != null) manager.processedUpdate = false;
        }

        /// <summary>
        /// Find the first portal used for occlusion checks.  This is likely going to be the portal with the main camera.
        /// NOTE: Currently no longer used.
        /// </summary>
        /// <param name="camera"></param>
        /// <returns></returns>
        private Portal FindPortalContainingCamera(Camera camera)
        {
            foreach (var portal in portals)
            {
                if (portal.ContainsCamera(camera)) return portal;
            }
            return null;
        }

        /// <summary>
        /// Retrieve the portal that camera resides in.  If the camera is not in a portal, this call returns null.
        /// </summary>
        /// <returns></returns>
        public static Portal CameraPortal() { return Available() ? manager.cameraPortal : null; }
        
        /// <summary>
        /// Retrieve the room that camera resides in.  If the camera is not in a room, this call returns null.
        /// </summary>
        /// <returns></returns>
        public static Room CameraRoom() { return Available() ? manager.cameraRoom : null; }
        
        /// <summary>
        /// Do end of frame cleanup and stats display.
        /// </summary>
        private void EndProcessing()
        {
            if (Available() && enableProcessing)
            {
                // Determine if an update was processed and notify listeners //
                if (processedUpdate && Completed != null)
                    Completed(new PortalManagerInfo { manager = this });
                else if (Incomplete != null)
                    Incomplete(new PortalManagerInfo { manager = this });

                // Display results if asked to do so //
                if (displayResults)
                    Debug.Log("Portal Rooms Rendered/Indirect/Considered: " + debugRoomsRendered.ToString() + "/" + debugRoomsIndirect.ToString() + "/" + debugRoomsConsidered.ToString());
            }

            foreach (var portal in portals)
                portal.UpdateDrawing();
            foreach (var room in rooms)
                room.UpdateDrawing();

            // Change the visibility of external object if different from last frame //
            manager.DisplayExternalObjects();

            // Now set these to null to prepare for next frame //
            cameraPortal = null;
            cameraRoom = null;

            // Reset rendering information for next frame //
            debugRoomsConsidered = 0;
            debugRoomsRendered = 0;
            debugRoomsIndirect = 0;

        }

        /// <summary>
        /// Process only external portals.  This will be called in LateUpdate if the camera is not in a room.
        /// </summary>
        /// <param name="camera"></param>
        void ProcessExternalPortals(Camera camera)
        {
            foreach (var portal in portals) {
                if (portal.IsExternal()) portal.Process(camera, null);
            }
        }
        
        /// <summary>
        /// Enable or disable rendering of portals.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="freezeView"></param>
        public static void EnableProcessing(bool value, bool freezeView = false)
        {
            if (Available())
            {
                manager.enableProcessing = value;

                if (!value && !freezeView)
                {
                    // Show everything //
                    manager.rooms.EachNotNull(room => room.Display(true));
                    manager.portals.EachNotNull(portal => portal.Display(true));
                }
                else if (value)
                {
                    manager.rooms.EachNotNull(room => room.Display(false));
                    manager.portals.EachNotNull(portal => portal.Display(false));
                }
            }
        }

        public static void NotifyRoomConsidered() { if (Available()) manager.debugRoomsConsidered++; }
        public static void NotifyRoomRendered() { if(Available()) manager.debugRoomsRendered++; }
        public static void NotifyRoomsIndirect() { if(Available()) manager.debugRoomsIndirect++; }

        public static void DebugDisplayResults(bool value) { if (Available()) manager.displayResults = value; }
    }
}