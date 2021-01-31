using UnityEngine;

namespace Bewildered
{
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
