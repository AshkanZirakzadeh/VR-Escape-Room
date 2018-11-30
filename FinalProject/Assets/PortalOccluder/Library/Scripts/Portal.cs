using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.VR;
using CirrusPlay.PortalLibrary.Extensions;

namespace CirrusPlay.PortalLibrary
{

    [RequireComponent(typeof(BoxCollider))]
    [ExecuteInEditMode()]
    public class Portal : MonoBehaviour, PortalManager.IVisible
    {

        public enum VisibleState
        {
            Unchecked,
            Visible
        }

        public struct PortalRenderInfo
        {
            public readonly Portal portal;
            public PortalRenderInfo(Portal room) { this.portal = room; }
        }

        // Used to map frustum positions //
        const int PlaneLeft = 0;
        const int PlaneRight = 1;
        const int PlaneDown = 2;
        const int PlaneUp = 3;
        const int PlaneNear = 4;
        const int PlaneFar = 5;

        // Public tweakable values //
        public bool open = true;                      // When true, the portal will be considered open (transparent) and display the room(s) on the other side.  If false, the portal will not show rooms attached to it.  Useful for open/closed doors and windows.
        public bool onlyFullyEnclosedObjects = false; // When true, only objects whose render bounds are fully enclosed in the frustum with be added to the contained objects list, otherwise partially included objects will be added as well.

        public delegate void PortalRenderCallback(PortalRenderInfo info);

        /// <summary>
        /// Occurs when this portal becomes visible.
        /// </summary>
        public event PortalRenderCallback OnVisible;
        /// <summary>
        /// Occurs when this portal becomes invisible.
        /// </summary>
        public event PortalRenderCallback OnInvisible;

        // Private stuff needed for the implementation //
        private new BoxCollider collider;
        private HashSet<Room> rooms = new HashSet<Room>();
        private HashSet<PortalSharedItem<Renderer>> containedObjects = new HashSet<PortalSharedItem<Renderer>>();
        private HashSet<PortalSharedItem<Light>> containedLights = new HashSet<PortalSharedItem<Light>>();
        private VisibleState state;
        private VisibleState lastState = VisibleState.Unchecked;

        [Header("Additional Visibility Objects")]
        public GameObject[] additionalObjects = new GameObject[] { };

        void OnEnable()
        {
            if (!Application.isPlaying) return;
            PortalManager.Register(this);

            // Initially hide this portal //
            Display(false);

            // Detect rigid body //
            var rigidbody = GetComponent<Rigidbody>();
            if (rigidbody == null)
            {
                rigidbody = gameObject.AddComponent<Rigidbody>();
                rigidbody.isKinematic = true;
            }

            //OnVisible += info => Debug.Log("Portal Visible: " + info.portal.gameObject.name);
            //OnInvisible += info => Debug.Log("Portal Invisible: " + info.portal.gameObject.name);
        }

        void OnDisable()
        {
            if (!Application.isPlaying) return;
            PortalManager.Unregister(this);
        }

        private void OnDestroy()
        {
            if (!enabled)
            {
                rooms.Clear();
                containedObjects.Clear();
                containedLights.Clear();
                state = VisibleState.Unchecked;
            }
        }

        // Use this for initialization
        void Start()
        {
            collider = GetComponent<BoxCollider>();

            // Add all objects intersecting or contained within this portal to the portal's contained object list //
            FindAndLoadContainedObjects();
            FindAndLoadContainedLights();
        }

        /// <summary>
        /// Find all objects contained within this portal.  This is a costly operation!
        /// </summary>
        public void FindAndLoadContainedObjects()
        {
            var frustum = new Frustum(LocalBounds(), transform);
            containedObjects.Clear();
            PortalManager.renderers.Add(frustum.FindWithin<Renderer>(onlyFullyEnclosedObjects, PortalObjectConfiguration.CanParticipate), this).AddTo(containedObjects);
        }

        /// <summary>
        /// Finds all lights who are within the room frustum and saves those as the contained light set.  This is a costly operation!
        /// </summary>
        public void FindAndLoadContainedLights()
        {
            containedLights.Clear();
            var frustum = new Frustum(LocalBounds(), transform);
            PortalManager.lights.Add(frustum.FindWithin<Light>(this.onlyFullyEnclosedObjects, PortalObjectConfiguration.CanParticipate), this).AddTo(containedLights);
        }

        /// <summary>
        /// Determine the local bounds of this portal.
        /// </summary>
        /// <returns></returns>
        public Bounds LocalBounds()
        {
            var collider = this.collider ?? GetComponent<BoxCollider>();
            if (collider != null)
                return new Bounds(collider.center, collider.size);
            else
                return new Bounds();
        }

        /// <summary>
        /// If the portal connects to exactly one room, the portal is considered to be an external portal.
        /// </summary>
        /// <returns></returns>
        public bool IsExternal() { return rooms.Count == 1; }

        /// <summary>
        /// Update portal for this frame.
        /// </summary>
        void Update()
        {
            if (!Application.isPlaying) return;
            if (ContainsCamera(Camera.main)) PortalManager.BeginProcessPortal(Camera.main, this);
        }

        /// <summary>
        /// Set state in preparation for portal processing.
        /// </summary>
        public void PrepareUpdate()
        {
            // Reset this portal to unchecked so that portal and room visibility can be updated for this frame //
            state = VisibleState.Unchecked;
        }
        
        /// <summary>
        /// Determine if camera is inside this portal.
        /// </summary>
        /// <param name="camera"></param>
        /// <returns></returns>
        public bool ContainsCamera(Camera camera)
        {
            return camera != null ? new Frustum(LocalBounds(), transform).Contains(camera.transform.position) : false;
        }

        /// <summary>
        /// Handles room adjacency and dynamic objects entering the portal.
        /// </summary>
        /// <param name="other"></param>
        void OnTriggerEnter(Collider other)
        {
            // Add renderers for entering object //
            foreach (var rend in other.GetComponentsInChildren<Renderer>())
            {
                if (rend != null && PortalObjectConfiguration.CanParticipate(other))
                {
                    containedObjects.Add(PortalManager.renderers.Add(rend, this));
                }
            }

            // Add lights for entering objects //
            foreach (var light in other.GetComponentsInChildren<Light>())
            {
                if (light != null && PortalObjectConfiguration.CanParticipate(other))
                {
                    var data = PortalManager.lights.Add(light, this);
                    containedLights.Add(data);
                }
            }

            // Add any rooms being added to the portal //
            var room = other.gameObject.GetComponent<Room>();
            if (room != null)
                rooms.Add(room);
        }

        /// <summary>
        /// Handles room adjacency and dynamic objects leaving the portal.
        /// </summary>
        /// <param name="other"></param>
        void OnTriggerExit(Collider other)
        {
            // Remove renderers for exiting object //
            foreach (var rend in other.GetComponentsInChildren<Renderer>())
            {
                if (rend != null)
                {
                    var data = PortalManager.renderers.Remove(rend, this);
                    containedObjects.Remove(data);
                }
            }

            // Remove lights for exiting object //
            foreach (var light in other.GetComponentsInChildren<Light>())
            {
                if (light != null)
                {
                    var data = PortalManager.lights.Remove(light, this);
                    containedLights.Remove(data);
                }
            }

            // Remove any rooms being removed from the portal //
            var room = other.gameObject.GetComponent<Room>();
            if (room != null)
                rooms.Remove(room);
        }

        /// <summary>
        /// Determine if this portal is visible.
        /// </summary>
        /// <returns></returns>
        public bool IsVisible()
        {
            return state == VisibleState.Visible && open;
        }

        /// <summary>
        /// Determine if this Portal lies outside of a Frustum's bounds.
        /// </summary>
        /// <param name="frustum"></param>
        /// <returns></returns>
        public bool OutOfView(Frustum frustum)
        {
            // Make sure we have a collider, and compute the half-size of the portal. //
            if (collider == null) return true;
            var hs = collider.size * 0.5f;
            var points = new Vector3[]
            {
                this.transform.TransformPoint(new Vector3(hs.x, hs.y, hs.z)),
                this.transform.TransformPoint(new Vector3(-hs.x, hs.y, hs.z)),
                this.transform.TransformPoint(new Vector3(hs.x, -hs.y, hs.z)),
                this.transform.TransformPoint(new Vector3(-hs.x, -hs.y, hs.z)),
                this.transform.TransformPoint(new Vector3(hs.x, hs.y, -hs.z)),
                this.transform.TransformPoint(new Vector3(-hs.x, hs.y, -hs.z)),
                this.transform.TransformPoint(new Vector3(hs.x, -hs.y, -hs.z)),
                this.transform.TransformPoint(new Vector3(-hs.x, -hs.y, -hs.z))
            };

            return frustum.Outside(points);
        }

        /// <summary>
        /// Process this Portal.  Portals will automatically process when inside a room.  
        /// A processed portal will only process once per frame unless 'force' is set to 
        /// true when making this call.
        /// </summary>
        /// <param name="cam"></param>
        /// <param name="force"></param>
        /// <returns></returns>
        public void Process(Camera cam, Room origin, bool force = false)
        {
            // If this isn't enabled, we return false //
            if (!enabled) return;

            // We should always have a collider for our portal //
            if (collider == null) return;

            // If we've been processed this frame, don't reprocess //
            if (state == VisibleState.Visible) return;

            // This room is visible if it's being tested for processing //
            state = VisibleState.Visible;

            // If this is an external portal, notify that the outside world can be seen.
            if (IsExternal() && open) PortalManager.NotifyExteriorObject();
            
            // EXPERIMENTAL CODE
            // Calculate the view frustum.  If in VR mode, use center eye information for speed (might be inaccurate) //
            //var viewFrustum = VRSettings.enabled ? new Frustum(cam.projectionMatrix, cam.cameraToWorldMatrix, Vector3.zero, Quaternion.identity) : new Frustum(cam);
            //var viewFrustum = new Frustum(cam);

            // If this portal is open and not obscured, then we can draw its room, //
            // and recursively process the new room's portals //
            //if (open && (ContainsCamera(cam) || !OutOfView(viewFrustum)))  // EXPERIMENTAL CODE: causes shadow artifacts and bugs out in VR mode.
            if (open)
            {
                ProcessMyRooms(cam, origin);
            }
        }
        
        public void UpdateDrawing()
        {
            if (state != lastState && Application.isPlaying)
            {
                Display(state == VisibleState.Visible);
                lastState = state;

                // Do event callbacks for state changes //
                if (state == VisibleState.Visible && OnVisible != null) OnVisible(new PortalRenderInfo(this));
                if (state == VisibleState.Unchecked && OnInvisible != null) OnInvisible(new PortalRenderInfo(this));

            }
        }

        /// <summary>
        /// Marks this portal's objects for display.
        /// </summary>
        private void DisplayMyObjects(bool enable)
        {
            bool cleanUp = false;
            containedObjects.Each(rend => {
                if (rend != null)
                    rend.item.enabled = enable || rend.IsVisible();
                else
                    cleanUp = true;
            });

            foreach (var rend in GetComponentsInChildren<Renderer>())
                rend.enabled = enable;

            DisplayAdditionalRenderObjects(enable);

            if (cleanUp)
                CleanupNullObjects();
        }

        /// <summary>
        /// Marks this portal's lights for display.
        /// </summary>
        /// <param name="enable"></param>
        private void DisplayMyLights(bool enable)
        {
            bool cleanUp = false;
            //containedLights.Each(light => { if (light != null) light.item.enabled = enable || light.IsVisible(); else cleanUp = true; });
            containedLights.Each(light => {
                if (light != null)
                    light.item.enabled = enable || light.IsVisible();
                else
                    cleanUp = true;
            });

            if (cleanUp)
                CleanupNullLights();
        }

        private void DisplayAdditionalRenderObjects(bool enable)
        {
            foreach (var obj in additionalObjects)
            {
                if (obj != null)
                {
                    foreach (var rend in obj.GetComponentsInChildren<Renderer>())
                    {
                        rend.enabled = enable;
                    }
                }
            }
        }
        
        /// <summary>
        /// Display or hide all contained objects and lights.
        /// </summary>
        /// <param name="value"></param>
        public void Display(bool value)
        {
            if (!Application.isPlaying) return;
            DisplayMyObjects(value);
            DisplayMyLights(value);
        }

        /// <summary>
        /// Retrieve the trigger's transformation information.
        /// </summary>
        /// <returns></returns>
        public Transform TriggerTransform()
        {
            return collider != null ? collider.transform : null;
        }

        private void ProcessMyRooms(Camera cam, Room origin)
        {
            var cleanup = false;
            // This portal's rooms should be marked visible //
            //state = VisibleState.Visible;

            // Go through all rooms and check their visibility //
            foreach (var room in rooms)
            {
                if (room != null)
                {
                    // Do not process the room we just came from again //
                    if (room != origin)
                    {
                        // Check visibility of next portal //
                        room.ProcessPortals(
                            this,
                            cam,
                            new Frustum(LocalBounds(), 1, 1, collider.transform, cam.transform.position),
                            new Frustum(LocalBounds(), -1, -1, collider.transform, cam.transform.position),
                            new Frustum(LocalBounds(), 1, -1, collider.transform, cam.transform.position),
                            new Frustum(LocalBounds(), -1, 1, collider.transform, cam.transform.position)
                            );
                    }
                }
                else
                    cleanup = true;

            }

            if (cleanup)
                CleanupNullRooms();
        }
        
        ///// <summary>
        ///// Displays all connected rooms for this portal.
        ///// </summary>
        ///// <param name="cam"></param>
        //private void DisplayMyRooms(Camera cam)
        //{
        //    var cleanup = false;
        //    // This portal's rooms should be marked visible //
        //    foreach (var room in rooms)
        //    {
        //        if (room != null)
        //        {
        //            // Check visibility of next portal //
        //            room.ProcessPortals(
        //                this,
        //                cam,
        //                new Frustum(LocalBounds(), 1, collider.transform, cam.transform.position),
        //                new Frustum(LocalBounds(), -1, collider.transform, cam.transform.position)
        //                );
        //        }
        //        else
        //            cleanup = true;
                
        //    }

        //    if (cleanup)
        //        CleanupNullRooms();
        //}

        private void ProcessRooms(Camera cam)
        {
            var cleanup = false;
            // This portal's rooms should be marked visible //
            foreach (var room in rooms)
            {
                if (room != null)
                {
                    // Check visibility of next portal //
                    room.ProcessPortals(
                        this,
                        cam,
                        new Frustum(LocalBounds(), 1, 1, collider.transform, cam.transform.position),
                        new Frustum(LocalBounds(), -1, -1, collider.transform, cam.transform.position),
                        new Frustum(LocalBounds(), 1, -1, collider.transform, cam.transform.position),
                        new Frustum(LocalBounds(), -1, 1, collider.transform, cam.transform.position)
                        );
                }
                else
                    cleanup = true;

            }

            if (cleanup)
                CleanupNullRooms();
        }

        /// <summary>
        /// Cleanup any objects which may have become invalidated.
        /// </summary>
        void CleanupNullObjects()
        {
            Debug.Log("Portal(" + name + "): Cleaning up objects...");
            var newSet = new HashSet<PortalSharedItem<Renderer>>();
            foreach (var obj in containedObjects)
                if (obj != null) newSet.Add(obj);
            this.containedObjects = newSet;
        }

        /// <summary>
        /// Cleanup any rooms which may have become invalidated.
        /// </summary>
        void CleanupNullRooms()
        {
            Debug.Log("Portal(" + name + "): Cleaning up rooms...");
            var newSet = new HashSet<Room>();
            foreach (var room in rooms)
                if (room != null) newSet.Add(room);
            this.rooms = newSet;
        }

        /// <summary>
        /// Cleanup any lights which may have become invalidated.
        /// </summary>
        void CleanupNullLights()
        {
            Debug.Log("Portal(" + name + "): Cleaning up lights...");
            var newSet = new HashSet<PortalSharedItem<Light>>();
            foreach (var light in containedLights)
                if (light != null) newSet.Add(light);
            this.containedLights = newSet;
        }
        
        /// <summary>
        /// Attach a room.  Should only be used by Room.
        /// </summary>
        /// <param name="room"></param>
        public void Attach(Room room)
        {
            rooms.Add(room);
        }

        /// <summary>
        /// Detatch a room.  Should only be used by Room.
        /// </summary>
        /// <param name="room"></param>
        public void Detach(Room room)
        {
            rooms.Remove(room);
        }

        /// <summary>
        /// Displays the box in the editor.
        /// </summary>
        private void OnDrawGizmos()
        {
            var bounds = LocalBounds();

            // Draw the entrance and exit to the portal in blue so that the designer knows how to place them //
            DrawEditorPortal(bounds, transform, 1);
            DrawEditorPortal(bounds, transform, -1);
        }

        static Color orange = new Color(1.0f, 0.3f, 0.1f, 1.0f);
        private void DrawEditorPortal(Bounds bounds, Transform transform, float direction)
        {

            Vector3 h = new Vector3(bounds.size.x, bounds.size.y, bounds.size.z) * 0.5f;
            Vector3 a = transform.TransformPoint(new Vector3(-h.x, h.y, h.z * direction) + bounds.center);
            Vector3 b = transform.TransformPoint(new Vector3(h.x, h.y, h.z * direction) + bounds.center);
            Vector3 c = transform.TransformPoint(new Vector3(h.x, -h.y, h.z * direction) + bounds.center);
            Vector3 d = transform.TransformPoint(new Vector3(-h.x, -h.y, h.z * direction) + bounds.center);

            Gizmos.color = new Color(orange.r, orange.g, orange.b, orange.a / 8);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(bounds.center, Abs(bounds.size));
            Gizmos.matrix = Matrix4x4.identity;

            Gizmos.color = orange;
            Gizmos.DrawLine(a, b);
            Gizmos.DrawLine(b, c);
            Gizmos.DrawLine(c, d);
            Gizmos.DrawLine(d, a);
        }

        private static Vector3 Abs(Vector3 vec)
        {
            return new Vector3(Mathf.Abs(vec.x), Mathf.Abs(vec.y), Mathf.Abs(vec.z));
        }

    }
}