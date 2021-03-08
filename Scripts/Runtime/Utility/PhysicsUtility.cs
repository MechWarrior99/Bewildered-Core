using UnityEngine;

namespace Bewildered
{
    /// <summary>
    /// Various utility methods for helping work with physics related systems.
    /// </summary>
    public static class PhysicsUtility
    {
        private static Camera _camera;
        private static Vector3 _screenCenter = new Vector3(0.5f, 0.5f);

        private static Camera SceneCamera
        {
            get
            {
                if (_camera == null)
                    _camera = Camera.main;
                return _camera;
            }
        }

        /// <summary>
        /// Casts a ray forward from the center of the screen of the main <see cref="Camera"/> in the scene, against all colliders in the scene.
        /// </summary>
        /// <returns><c>true</c> if ray intersects with a <see cref="Collider"/>; otherwise, <c>false</c>.</returns>
        public static bool FirstPersonRaycast()
        {
            return Physics.Raycast(SceneCamera.WorldToScreenPoint(_screenCenter), SceneCamera.transform.forward);
        }

        public static bool FirstPersonRaycast(out RaycastHit hitInfo)
        {
            return Physics.Raycast(SceneCamera.WorldToScreenPoint(_screenCenter), SceneCamera.transform.forward, out hitInfo);
        }

        public static bool FirstPersonRaycast(out RaycastHit hitInfo, float maxDistance)
        {
            return Physics.Raycast(SceneCamera.WorldToScreenPoint(_screenCenter), SceneCamera.transform.forward, out hitInfo, maxDistance);
        }

        public static bool FirstPersonRaycast(out RaycastHit hitInfo, float maxDistance, int layerMask)
        {
            return Physics.Raycast(SceneCamera.WorldToScreenPoint(_screenCenter), SceneCamera.transform.forward, out hitInfo, maxDistance, layerMask);
        }

        /// <summary>
        /// Casts a ray forward from the center of the screen of the main <see cref="Camera"/> in the scene, against all colliders in the scene.
        /// If no collisions occure by the specified max distance, it casts a ray directly down from the max distance point, returning the results from that ray instead.
        /// </summary>
        /// <param name="hitInfo">If <c>true</c> is returned <paramref name="hitInfo"/> will contain more information about where the closest collider was hit.</param>
        /// <param name="maxDistance">The maximum distance the first ray should check for collisions.</param>
        /// <param name="layerMask">The <see cref="LayerMask"/> that is used to selectivly ignore <see cref="Collider"/>s when casting a ray.</param>
        /// <returns><c>true</c> if either of the rays intersect with a <see cref="Collider"/>; otherwise, false.</returns>
        public static bool FirstPersonGroundRaycast(out RaycastHit hitInfo, float maxDistance, int layerMask)
        {
            Vector3 viewportWorldPoint = SceneCamera.ViewportToWorldPoint(_screenCenter);

            if (Physics.Raycast(viewportWorldPoint, SceneCamera.transform.forward, out hitInfo, maxDistance, layerMask))
            {
                return true;
            }
            else
            {
                Ray ray = new Ray(viewportWorldPoint, SceneCamera.transform.forward);
                Vector3 pointAlongRay = ray.GetPoint(maxDistance);
                if (Physics.Raycast(pointAlongRay, Vector3.down, out hitInfo, Mathf.Infinity, layerMask))
                {
                    return true;
                }
            }

            return false;
        }
    } 
}
