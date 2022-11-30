using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using System;
using System.Collections.Generic;

namespace CustomExtensions
{
    public static class Extensions
    {
        #region Vectors
        public static Vector3 setX(this Vector3 v, float x = 0) => new Vector3(x, v.y, v.z);

        public static Vector3 setY(this Vector3 v, float y = 0) => new Vector3(v.x, y, v.z);

        public static Vector3 setZ(this Vector3 v, float z = 0) => new Vector3(v.x, v.y, z);

        public static Vector3 getLookRotationInEuler(this Transform t, Vector3 targetPosition)
        {
            Vector3 lookDir = (targetPosition - t.position).setY(0).normalized;
            return Quaternion.LookRotation(lookDir).eulerAngles;
        }

        #endregion

        #region Quaternion
        public static Quaternion getLookRotation(this Transform t, Vector3 targetPosition)
        {
            Vector3 lookDir = (targetPosition - t.position).setY(0).normalized;
            return Quaternion.LookRotation(lookDir);
        }

        #endregion

        #region Timeline
        public static bool isPlayableCompleted(this Playable playable, FrameData info)
        {
            var duration = playable.GetDuration();
            var time = playable.GetTime();
            var count = time + info.deltaTime;

            if ((info.effectivePlayState == PlayState.Paused && count > duration) || Mathf.Approximately((float)time, (float)duration))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region Gameobject
        public static bool HasComponent<T>(this GameObject g, System.Action action) where T : Component
        {
            return g.GetComponent<T>() != null;
        }

        public static void RemoveComponent<T>(this GameObject g) where T : Component
        {
            if (g.GetComponent<T>() != null)
                UnityEngine.Object.Destroy(g.GetComponent<T>());
        }
        #endregion

        #region Maths
        public static float InverseLerp(float a, float b, float v)
        {
            return (v - a) / (b - a);
        }

        public static double InverseLerp(double a, double b, double v)
        {
            return (v - a) / (b - a);
        }
        #endregion

        #region Component
        public static bool IsValidComponent<T>(this object o) where T : PlayableBehaviour
        {
            T t = (T)o;
            if (t != null)
                return true;

            return false;
        }

        public static void executeAction<T>(this object o, Action action) where T : PlayableBehaviour
        {
            T t = (T)o;
            if (t != null)
                action?.Invoke();
        }

        public static void executeAction<T>(this object o, Action<T> action) where T : PlayableBehaviour
        {
            T t = (T)o;
            if (t != null)
                action?.Invoke(t);
        }
        #endregion

        #region iTween
        public static object[] getArgs(this Vector3 v)
        {
            object[] result = { "x", v.x , "y" ,v.y, "z", v.z};
            return result;
        }

        #endregion

        #region List & Array
        public static List<T> clone<T>(this List<T> target) where T : Component
        {
            List<T> result = new List<T>();
            for(int i = 0; i < target.Count; i++)
            {
                result.Add(target[i]);
            }
            return result;
        }
        #endregion
        

    }
}
