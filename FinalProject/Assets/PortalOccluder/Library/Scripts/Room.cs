using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.VR;
using CirrusPlay.PortalLibrary.Extensions;

namespace CirrusPlay.PortalLibrary
{
    [RequireComponent(typeof(BoxCollider))]
    [ExecuteInEditMode()]
    public class Room : MonoBehaviour, PortalManager.IVisible
    {
        public enum VisibleState
        {
            Unchecked,
            Visible
        }

        public struct RoomRenderInfo
        {
            public readonly Room room;
            public RoomRenderInfo(Room room) { this.room = room; }
        }

        public delegate void RoomRenderCallback(RoomRenderInfo info);

        /// <summary>
        /// Occurs when this room becomes visible.
        /// </summary>
        public event RoomRenderCallback OnVisible;
        /// <summary>
        /// Occurs when this room becomes invisible.
        /// </summary>
        public event RoomRenderCallback OnInvisible;

        public bool onlyFullyEnclosedObjects = true; // When true, only objects whose render bounds are fully enclosed in the frustum with be added to the contained objects list, otherwise partially included objects will be added as well.
        //public bool forceDraw = false; // Will unconditionally draw this room.

        private HashSet<PortalSharedItem<Renderer>> containedObjects = new HashSet<PortalSharedItem<Renderer>>();
        private HashSet<PortalSharedItem<Light>> containedLights = new HashSet<PortalSharedItem<Light>>();
        private HashSet<Portal> portals = new HashSet<Portal>();
        private VisibleState state = VisibleState.Unchecked;
        private VisibleState lastState = VisibleState.Visible;

        new BoxCollider collider;
        //private bool isVisible = false;

        void OnEnable()
        {
            if (!Application.isPlaying) return;
            // Notify the portal manager of this room //
            PortalManager.Register(this);

            // Initially hide this room //
            Display(false);
            
            // Detect rigid body //
            var rigidbody = GetComponent<Rigidbody>();
            if (rigidbody == null)
            {
                rigidbody = gameObject.AddComponent<Rigidbody>();
                rigidbody.isKinematic = true;
            }
            
            //OnVisible += info => Debug.Log("Room Visible: " + info.room.gameObject.name);
            //OnInvisible += info => Debug.Log("Room Invisible: " + info.room.gameObject.name);
            
        }

        void OnDisable()
        {
            if (!Application.isPlaying) return;
            // Notify the portal manager that his room is no longer available //
            PortalManager.Unregister(this);

            if (!enabled)
            {
                containedObjects.Clear();
                containedLights.Clear();
                portals.Clear();
            }
        }

        // Use this for initialization
        void Start()
        {
            // We need a box collider marked as a trigger for this to work //
            collider = GetComponent<BoxCollider>();

            // Load all objects contained in this room.  Non-static objects will be allowed to leave the room if they choose. //
            FindAndLoadContainedObjects();
            FindAndLoadContainedLights();
        }

        /// <summary>
        /// Finds all objects with renderers whose render bounds are within the room frustum and saves those as the contained object set.  This is a costly operation!
        /// </summary>
        public void FindAndLoadContainedObjects()
        {
            // Clear the contained objects list (just to be safe) and go through every renderable object in the world. //
            // See which of these objects exist in the room's frustum, and add them to the list of contained objects. //
            containedObjects.Clear();
            var frustum = new Frustum(LocalBounds(), transform);

            var renderers = frustum.FindWithin<Renderer>(this.onlyFullyEnclosedObjects, PortalObjectConfiguration.CanParticipate);
            PortalManager.renderers.Add(renderers, this).AddTo(containedObjects);
            //AddSharedItems(frustum.FindWithin<Renderer>(this.onlyFullyEnclosedObjects, PortalObjectConfiguration.CanParticipate));
        }
        
        /// <summary>
        /// Finds all lights who are within the room frustum and saves those as the contained light set.  This is a costly operation!
        /// </summary>
        public void FindAndLoadContainedLights()
        {
            containedLights.Clear();
            var frustum = new Frustum(LocalBounds(), transform);
            PortalManager.lights.Add(frustum.FindWithin<Light>(this.onlyFullyEnclosedObjects, PortalObjectConfiguration.CanParticipate), this).AddTo(containedLights);
            //AddSharedItems(frustum.FindWithin<Light>(this.onlyFullyEnclosedObjects, PortalObjectConfiguration.CanParticipate));
        }
        
        /// <summary>
        /// Handles dynamic objects entering the room.  Dynamic objects will probably only work if they have a collider and rigid body attached to them.
        /// </summary>
        /// <param name="other"></param>
        void OnTriggerEnter(Collider other)
        {
            if (Application.isPlaying)
            {
                foreach (var rend in other.GetComponentsInChildren<Renderer>())
                {
                    if (rend != null && PortalObjectConfiguration.CanParticipate(other))
                    {
                        containedObjects.Add(PortalManager.renderers.Add(rend, this));
                    }
                }

                foreach (var light in other.GetComponentsInChildren<Light>())
                {
                    if (light != null && PortalObjectConfiguration.CanParticipate(other))
                    {
                        var data = PortalManager.lights.Add(light, this);
                        containedLights.Add(data);
                    }
                }

                foreach (var subPortal in other.gameObject.GetComponentsInChildren<Portal>())
                    subPortal.Attach(this);

                foreach (var portal in other.GetComponentsInChildren<Portal>())
                {
                    portal.Attach(this);
                    portals.Add(portal);
                }
            }
        }

        /// <summary>
        /// Handles dynamic objects leaving the room.  Dynamic objects will probably only work if they have a collider and rigid body attached to them.
        /// </summary>
        /// <param name="other"></param>
        void OnTriggerExit(Collider other)
        {
            if (Application.isPlaying)
            {
                foreach (var rend in other.GetComponentsInChildren<Renderer>())
                {
                    if (rend != null)
                    {
                        var data = PortalManager.renderers.Remove(rend, this);
                        containedObjects.Remove(data);
                    }
                }

                foreach (var light in other.GetComponentsInChildren<Light>())
                {
                    if (light != null)
                    {
                        var data = PortalManager.lights.Remove(light, this);
                        containedLights.Remove(data);
                    }
                }


                foreach (Portal subPortal in other.gameObject.GetComponentsInChildren<Portal>())
                    subPortal.Detach(this);

                var portal = other.GetComponent<Portal>();
                if (portal != null)
                    portals.Remove(portal);
            }
        }

        /// <summary>
        /// Retrieve the local bounds of this room.
        /// </summary>
        /// <returns></returns>
        Bounds LocalBounds()
        {
            if (collider != null)
            {
                return new Bounds(collider.center, collider.size);
            }
            else
            {
                return new Bounds();
            }
        }

        /// <summary>
        /// Update this room.
        /// </summary>
        void Update()
        {
            if (!Application.isPlaying) return;

            // If this room contains the main camera, begin portal occlusion tests from it //
            if (ContainsCamera(Camera.main))
            {
                PortalManager.BeginProcessRoom(Camera.main, this);
            }
        }

        /// <summary>
        /// Notifies the room that it needs to test for visibility changes and react to those changes by showing/hiding managed geometry.
        /// </summary>
        public void UpdateDrawing()
        {
            if (state != lastState && Application.isPlaying)
            {
                Display(state == VisibleState.Visible);
                lastState = state;

                // Do event callbacks for state changes //
                if (state == VisibleState.Visible && OnVisible != null) OnVisible(new RoomRenderInfo(this));
                if (state == VisibleState.Unchecked && OnInvisible != null) OnInvisible(new RoomRenderInfo(this));
            }
        }

        /// <summary>
        /// Sets the room state to be ready for portal processing.
        /// </summary>
        public void PrepareUpdate()
        {
            // Mark this room as unchecked at the beginning of the frame updating //
            state = VisibleState.Unchecked;
        }

        public void BeginPortalProcessing(Camera camera)
        {
            // Notify the portal manager that we are rendering this room //
            PortalManager.NotifyRoomConsidered();
            PortalManager.NotifyRoomRendered();

            // Mark room for drawing //
            state = VisibleState.Visible;

            foreach (var portal in portals)
            {
                portal.Process(camera, this);
            }

        }

        /// <summary>
        /// Determine if the camera is in this room.
        /// </summary>
        /// <param name="camera"></param>
        /// <returns></returns>
        public bool ContainsCamera(Camera camera)
        {
            return camera != null ? new Frustum(LocalBounds(), collider.transform).Contains(camera.transform.position) : false;
        }

        /// <summary>
        /// Determine if this room is visible from the camera or through another portal.
        /// </summary>
        /// <returns></returns>
        public bool IsVisible()
        {
            return state == VisibleState.Visible;
        }

        /// <summary>
        /// Display the room.  This is primarily used by portals.  If you need a room to unconditionally display, set forceDraw to true.
        /// </summary>
        /// <param name="value"></param>
        public void Display(bool value)
        {
            if (!Application.isPlaying) return;
            var cleanupObjects = false;
            var cleanupLights = false;
            //if (value) { status = VisibleState.Visible; }
            //isVisible = value;
            foreach (var obj in containedObjects)
            {
                if (obj != null)
                {
					try {

				
                    obj.item.enabled = value || obj.IsVisible();
					} catch (MissingReferenceException) {
						cleanupObjects = true;
					}
                }
                else
                {
                    cleanupObjects = true;
                }
            }
            foreach (var light in containedLights)
            {
                if (light != null)
                    light.item.enabled = value || light.IsVisible();
                else
                    cleanupLights = true;
            }

            if (cleanupObjects) CleanupNullObjects();
            if (cleanupLights) CleanupNullLights();
        }

        public void ProcessPortals(Portal thisPortal, Camera cam, Frustum frustum1, Frustum frustum2, Frustum frustum1Reverse, Frustum frustum2Reverse)
        {
            var cleanup = false;
            
            state = VisibleState.Visible;

            // We are rendering this room.  Let the manager know we found one. //
            PortalManager.NotifyRoomRendered();

            // Go through each portal connected to this room to see if we can see through it //
            foreach (var portal in portals)
            {
                // If the connection was destroyed, the portal may be null //
                if (portal != null)
                {
                    // If the portal was already marked as visible, we don't want to re-process it //
                    if (!portal.IsVisible())
                    {
                        // Tell the portal manager that we are considering rendering this room //
                        PortalManager.NotifyRoomConsidered();

                        // If we can see into a portal, process the portal's visibility //
                        if (portal != thisPortal && (!portal.OutOfView(frustum1) || !portal.OutOfView(frustum2) || !portal.OutOfView(frustum1Reverse) || !portal.OutOfView(frustum2Reverse)))
                        {
                            PortalManager.NotifyRoomsIndirect();
                            portal.Process(cam, this);
                        }
                    }
                }
                else
                {
                    // One or more portals is not valid //
                    cleanup = true;
                }
            }

            // Cleanup invalid portals //
            if (cleanup)
                CleanupNullPortals();
        }

        /// <summary>
        /// Cleanup any objects which may have become invalidated.
        /// </summary>
        void CleanupNullObjects()
        {
            var newSet = new HashSet<PortalSharedItem<Renderer>>();
            foreach (var obj in containedObjects)
                if (obj != null) newSet.Add(obj);
            this.containedObjects = newSet;
        }

        /// <summary>
        /// Cleanup any rooms which may have become invalidated.
        /// </summary>
        void CleanupNullPortals()
        {
            var newSet = new HashSet<Portal>();
            foreach (var portal in portals)
                if (portal != null) newSet.Add(portal);
            this.portals = newSet;
        }

        /// <summary>
        /// Cleanup any lights which may have become invalidated.
        /// </summary>
        void CleanupNullLights()
        {
            var newSet = new HashSet<PortalSharedItem<Light>>();
            foreach (var light in containedLights)
                if (light != null) newSet.Add(light);
            this.containedLights = newSet;
        }


        static Color cyan = new Color(0.1f, 1.0f, 0.9f, 1.0f);
        private void OnDrawGizmos()
        {
            
            DrawEditorRoom(LocalBounds(), transform, cyan);
        }
        
        private void DrawEditorRoom(Bounds bounds, Transform transform, Color color)
        {

            Vector3 h = new Vector3(bounds.size.x, bounds.size.y, bounds.size.z) * 0.5f;

            Vector3 a1 = transform.TransformPoint(new Vector3(-h.x, h.y, h.z) + bounds.center);
            Vector3 b1 = transform.TransformPoint(new Vector3(h.x, h.y, h.z) + bounds.center);
            Vector3 c1 = transform.TransformPoint(new Vector3(h.x, -h.y, h.z) + bounds.center);
            Vector3 d1 = transform.TransformPoint(new Vector3(-h.x, -h.y, h.z) + bounds.center);

            Vector3 a2 = transform.TransformPoint(new Vector3(-h.x, h.y, -h.z) + bounds.center);
            Vector3 b2 = transform.TransformPoint(new Vector3(h.x, h.y, -h.z) + bounds.center);
            Vector3 c2 = transform.TransformPoint(new Vector3(h.x, -h.y, -h.z) + bounds.center);
            Vector3 d2 = transform.TransformPoint(new Vector3(-h.x, -h.y, -h.z) + bounds.center);

            // Outside //
            Gizmos.color = new Color(color.r, color.g, color.b, color.a / 8);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(bounds.center, bounds.size);
            // Inside //
            Gizmos.DrawCube(bounds.center, -bounds.size);
            Gizmos.matrix = Matrix4x4.identity;

            Gizmos.color = color;
            Gizmos.DrawLine(a1, b1);
            Gizmos.DrawLine(b1, c1);
            Gizmos.DrawLine(c1, d1);
            Gizmos.DrawLine(d1, a1);

            Gizmos.DrawLine(a2, b2);
            Gizmos.DrawLine(b2, c2);
            Gizmos.DrawLine(c2, d2);
            Gizmos.DrawLine(d2, a2);

            Gizmos.DrawLine(a1, a2);
            Gizmos.DrawLine(b1, b2);
            Gizmos.DrawLine(c1, c2);
            Gizmos.DrawLine(d1, d2);

        }
    }
}