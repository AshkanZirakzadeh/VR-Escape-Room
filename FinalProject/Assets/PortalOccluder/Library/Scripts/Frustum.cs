using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace CirrusPlay.PortalLibrary
{

    /// <summary>
    /// Represents an arbitrary frustum.  Generally, this will be constructed from
    /// either a Bounds object and a transform (creating a 3D, arbitrarily-aligned box) or
    /// from a 3D rectangle and a point of origin.
    /// </summary>
    public struct Frustum : IEnumerable<Plane>
    {
        public Plane[] planes;

        /// <summary>
        /// Create frustum based on a 3D Bounds object and its transformation.
        /// </summary>
        /// <param name="bounds"></param>
        /// <param name="transform"></param>
        public Frustum(Bounds bounds, Transform transform)
        {
            this.planes = new Plane[] {
                new Plane(transform.TransformDirection(1, 0, 0), transform.TransformPoint(-bounds.extents.x, 0, 0)),
                new Plane(transform.TransformDirection(-1, 0, 0), transform.TransformPoint(bounds.extents.x, 0, 0)),
                new Plane(transform.TransformDirection(0, -1, 0), transform.TransformPoint(0, bounds.extents.y, 0)),
                new Plane(transform.TransformDirection(0, 1, 0), transform.TransformPoint(0, -bounds.extents.y, 0)),
                new Plane(transform.TransformDirection(0, 0, 1), transform.TransformPoint(0, 0, -bounds.extents.z)),
                new Plane(transform.TransformDirection(0, 0, -1), transform.TransformPoint(0, 0, bounds.extents.z))
            };
        }

        /// <summary>
        /// Find all objects within the frustum.  This is a costly operation.
        /// </summary>
        /// <typeparam name="Type"></typeparam>
        /// <returns></returns>
        public Type[] FindWithin<Type>(bool onlyFullyEnclosedObjects = true, Func<Type, bool> condition = null) where Type : Component
        {

            var all = GameObject.FindObjectsOfType<Type>();
            var result = new List<Type>();
            foreach (var obj in all)
            {
                var rend = obj.GetComponent<Renderer>();
                if (rend != null && onlyFullyEnclosedObjects)
                {
                    if (Within(rend.bounds, null) && (condition == null || condition(obj)))
                    {
                        result.Add(obj);
                    }
                }
                else if (rend != null)
                {
                    if (!Outside(rend.bounds, null) && (condition == null || condition(obj)))
                    {
                        result.Add(obj);
                    }
                }
                else
                {
                    if (Contains(obj.transform.position) && (condition == null || condition(obj)))
                    {
                        result.Add(obj);
                    }
                }
            }
            return result.ToArray();
        }

        /// <summary>
        /// Create frustum based on a 3D rectangle, its transformation and a world-space point of origin (usually a camera position).
        /// </summary>
        /// <param name="rectangle"></param>
        /// <param name="rectangleTransform"></param>
        /// <param name="origin"></param>
        public Frustum(Bounds bounds, float faceDirection, float lengthDirection, Transform rectangleTransform, Vector3 origin)
        {
            Vector3 h = bounds.size * 0.5f;
            Vector3 a = rectangleTransform.TransformPoint(new Vector3(-h.x, h.y, h.z * lengthDirection));
            Vector3 b = rectangleTransform.TransformPoint(new Vector3(h.x, h.y, h.z * lengthDirection));
            Vector3 c = rectangleTransform.TransformPoint(new Vector3(h.x, -h.y, h.z * lengthDirection));
            Vector3 d = rectangleTransform.TransformPoint(new Vector3(-h.x, -h.y, h.z * lengthDirection));

            //if (Vector3.Dot(onorm, dir) < 0)
            if (faceDirection > 0)
            {
                this.planes = new Plane[]
                {
                new Plane(a, b, origin),
                new Plane(b, c, origin),
                new Plane(c, d, origin),
                new Plane(d, a, origin)
                };
            }
            else
            {
                this.planes = new Plane[]
                {
                new Plane(a, d, origin),
                new Plane(d, c, origin),
                new Plane(c, b, origin),
                new Plane(b, a, origin)
                };
            }
        }

        /// <summary>
        /// Calculate a frustum given a camera.
        /// </summary>
        /// <param name="camera"></param>
        public Frustum(Camera camera)
        {
            this.planes = GeometryUtility.CalculateFrustumPlanes(camera);
        }

        /// <summary>
        /// Calculate a frustum based on a projection matrix and a world position and rotation.
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        public Frustum(Matrix4x4 projection, Matrix4x4 world,  Vector3 position, Quaternion rotation)
        {
            Matrix4x4 trans = projection * world;// * Matrix4x4.TRS(position, rotation, Vector3.one) ;
            this.planes = GeometryUtility.CalculateFrustumPlanes(trans);
        }

        /// <summary>
        /// Create frustum based on an array of arbitrary planes.
        /// </summary>
        /// <param name="planes"></param>
        public Frustum(Plane[] planes)
        {
            if (planes == null) throw new ArgumentNullException("BoxFrustum must accept a non-null Plane array as first parameter.");
            this.planes = planes;
        }

        public bool Contains(Vector3 point)
        {
            if (this.planes == null || this.planes.Length == 0) return false;
            bool result = true;
            for (int i = 0; i < planes.Length; i++)
            {
                result &= planes[i].GetDistanceToPoint(point) >= 0;
            }
            return result;

        }

        /// <summary>
        /// Determine if point is within the frustum.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        [Obsolete("Use Contains instead")]
        public bool Within(Vector3 point)
        {
            return Contains(point);
        }

        /// <summary>
        /// Create 8 points from the corners of the given bounds after it has been transformed.
        /// </summary>
        /// <param name="bounds"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static Vector3[] PointsFromBounds(Bounds bounds, Transform transform)
        {
            var bx = bounds.extents.x;
            var by = bounds.extents.y;
            var bz = bounds.extents.z;
            var cx = bounds.center.x;
            var cy = bounds.center.y;
            var cz = bounds.center.z;
            if (transform != null)
            {
                return new Vector3[]
                {
                transform.TransformPoint(new Vector3(bx + cx, by + cy, bz + cz)),
                transform.TransformPoint(new Vector3(-bx+ cx, by + cy, bz + cz)),
                transform.TransformPoint(new Vector3(bx+ cx, -by + cy, bz + cz)),
                transform.TransformPoint(new Vector3(-bx+ cx, -by + cy, bz + cz)),
                transform.TransformPoint(new Vector3(bx+ cx, by + cy, -bz + cz)),
                transform.TransformPoint(new Vector3(-bx+ cx, by + cy, -bz + cz)),
                transform.TransformPoint(new Vector3(bx+ cx, -by + cy, -bz + cz)),
                transform.TransformPoint(new Vector3(-bx+ cx, -by + cy, -bz + cz))
                };
            }
            else
            {
                return new Vector3[]
                {
                new Vector3(bx + cx, by + cy, bz + cz),
                new Vector3(-bx+ cx, by + cy, bz + cz),
                new Vector3(bx+ cx, -by + cy, bz + cz),
                new Vector3(-bx+ cx, -by + cy, bz + cz),
                new Vector3(bx+ cx, by + cy, -bz + cz),
                new Vector3(-bx+ cx, by + cy, -bz + cz),
                new Vector3(bx+ cx, -by + cy, -bz + cz),
                new Vector3(-bx+ cx, -by + cy, -bz + cz)
                };
            }
        }

        /// <summary>
        /// Determine if a bounds lies entirely within this frustum.
        /// </summary>
        /// <param name="bounds"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public bool Within(Bounds bounds, Transform transform)
        {
            return Within(PointsFromBounds(bounds, transform));
        }

        /// <summary>
        /// Determine if all points are within the confines of a the frustum.
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public bool Within(IEnumerable<Vector3> points)
        {
            bool result = true;
            foreach (var point in points)
            {
                result &= Contains(point);
            }
            return result;
        }

        /// <summary>
        /// Determine if a transformed bounds lies entirely outside of the frustum.
        /// </summary>
        /// <param name="bounds"></param>
        /// <param name="transform"></param>
        /// <returns>Returns true if the transformed bounds is entirely outside the frustum.</returns>
        public bool Outside(Bounds bounds, Transform transform)
        {
            return Outside(PointsFromBounds(bounds, transform));
        }

        /// <summary>
        /// Determine if the volume defined by "points" is entirely outside of the frustum.  This is determined by simply
        /// checking if all points are on one side of any plane.  This isn't 100% accurate,
        /// but probably good enough and fails "inside" if it gets it wrong.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="plane"></param>
        /// <returns></returns>
        public bool Outside(IEnumerable<Vector3> points)
        {
            if (this.planes == null || this.planes.Length == 0) return false;
            for (int i = 0; i < planes.Length; i++)
            {
                if (Outside(points, planes[i]))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Determine if all points are on one side of a plane.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="plane"></param>
        /// <returns></returns>
        private bool Outside(IEnumerable<Vector3> points, Plane plane)
        {
            var outside = true;
            foreach (var point in points)
            {
                outside &= plane.GetDistanceToPoint(point) < 0;
            }
            return outside;
        }

        public IEnumerator<Plane> GetEnumerator()
        {
            return ((IEnumerable<Plane>)planes).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Plane>)planes).GetEnumerator();
        }
    }
}