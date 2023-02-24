using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

namespace CustomExtensions
{
    public static class Extensions
    {
        #region Transform
        public static Transform[] GetChildAsArray(this Transform t)
        {
            if (t == null) return new Transform[0];

            Transform[] result = new Transform[t.childCount];
            for(int i = 0; i < t.childCount; i++)
            {
                result[0] = t.GetChild(i);
            }
            return result;
        }
        #endregion

        #region Vectors
        public static Vector3 setX(this Vector3 v, float x = 0) => new Vector3(x, v.y, v.z);

        public static Vector3 setY(this Vector3 v, float y = 0) => new Vector3(v.x, y, v.z);

        public static Vector3 setZ(this Vector3 v, float z = 0) => new Vector3(v.x, v.y, z);

        public static Vector3 getLookRotationInEuler(this Transform t, Vector3 targetPosition)
        {
            Vector3 lookDir = (targetPosition - t.position).setY(0).normalized;
            return Quaternion.LookRotation(lookDir).eulerAngles;
        }

        public static float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
        {
            Vector3 AB = b - a;
            Vector3 AV = value - a;
            return Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB);
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

        public static void executeAction<T>(this GameObject g, System.Action<T> action) where T : Component
        {
            T t = (T)g.GetComponent<T>();
            if (t != null)
                action?.Invoke(t);
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

        public static List<ITimelineBehaviour> Clone<ITimelineBehaviour>(this List<ITimelineBehaviour> target)
        {
            List<ITimelineBehaviour> result = new List<ITimelineBehaviour>();
            for (int i = 0; i < target.Count; i++)
            {
                result.Add(target[i]);
            }
            return result;
        }
        #endregion

        #region Color
        public static void ChangeAlpha(this Image img, float alpha)
        {
            alpha = Mathf.Clamp(alpha, 0f, 1f);
            img.color = new Color(img.color.r, img.color.g, img.color.b, alpha);
        }

        public static void ChangeAlpha(this TMPro.TMP_Text text, float alpha)
        {
            alpha = Mathf.Clamp(alpha, 0f, 1f);
            text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
        }

        public static Color ToColor(this Color32 c)
        {
            return new Color(c.r / 255f, c.g / 255f, c.b / 255f);
        }
        #endregion

        #region Animation Curve
        public static AnimationCurve clone(this AnimationCurve curve)
        {
            AnimationCurve result = new AnimationCurve();

            for(int i = 0; i < curve.length; i++)
            {
                result.AddKey(curve.keys[i].time, curve.keys[i].time);
            }
            result.preWrapMode = curve.preWrapMode;
            result.postWrapMode = curve.postWrapMode;
            return result;
        }

        #endregion
    }

    public static class CEditor
    {
        public static void ShowVector(ref Vector3 v, PlayableAsset asset, string label, string undoName)
        {
#if UNITY_EDITOR
            Vector3 s = v, e = v;

            e = UnityEditor.EditorGUILayout.Vector3Field(label, v);

            if (s != e)
            {
                UnityEditor.Timeline.UndoExtensions.RegisterPlayableAsset(asset, undoName);
                v = e;
            }
#endif
        }

        public static AnimationClip getClipFromStateName(this Animator anim, string _name, int layer)
        {
#if UNITY_EDITOR
            AnimationClip result = null;
            //var n = anim.runtimeAnimatorController as AnimatorController;
            AnimatorControllerLayer[] layers = (anim.runtimeAnimatorController as AnimatorController).layers;

            for (int i = 0; i < layers.Length; i++)
            {
                if (i == layer)
                {
                    foreach (ChildAnimatorState j in layers[i].stateMachine.states)
                    {
                        //Debug.Log("states name : " + j.state.name);
                        if (j.state.name == _name)
                        {
                            AnimationClip c = j.state.motion as AnimationClip;
                            //Debug.Log("Result found : " + (result != null ? result.name : "NO clip found"));
                            return c;
                        }
                    }
                }
            }
            return result;
#endif
        }
    }

    public static class Bezier
    {
        public static Vector3 getQuadraticPoint(Vector3 startPoint, Vector3 endPoint, Vector3 p, float t)
        {
            return Mathf.Pow((1f - t),2) * startPoint + 2f* (1f-t) * t * p + Mathf.Pow(t, 2) * endPoint;
        }

        public static Vector3 getQuadraticTangent(Vector3 startPoint, Vector3 endPoint, Vector3 p, float t)
        {
            return 2 * (1f - t) * (p - startPoint) + 2 * t * (endPoint - p);
        }

        public static Vector3 getCubicPoint(Vector3 startPoint, Vector3 endPoint, Vector3 p1, Vector3 p2, float t)
        {
            return Mathf.Pow(1f - t, 3) * startPoint + 3 * Mathf.Pow(1f - t, 2) * t * p1 + 3 * (1f - t) * t * t * p2 + Mathf.Pow(t, 3) * endPoint;
        }

        public static Vector3 getCubicTangent(Vector3 startPoint, Vector3 endPoint, Vector3 p1, Vector3 p2, float t)
        {
            return 3 * Mathf.Pow(1f - t, 2) * (p1 - startPoint) + 6 * (1f - t) * t * (p2 - p1) + 2 * t * t * (endPoint - p2);
        }

        //public static Vector3 getQuadraticPoint(Vector3 startPoint, Vector3 endPoint, Vector3 p, float t)
        //{
        //    if (t < 0 || t > 1) Debug.LogError("Quadratic bezier interpolation value is invalid : " + t);

        //    t = Math.Clamp(t, 0.0f, 1.0f);

        //    Vector3 a = Vector3.Lerp(startPoint, p, t);
        //    Vector3 b = Vector3.Lerp(p, endPoint, t);
        //    return Vector3.Lerp(a, b, t);
        //}

        //public static Vector3 getCubicPoint(Vector3 startPoint, Vector3 endPoint, Vector3 p1, Vector3 p2, float t)
        //{
        //    if (t < 0 || t > 1) Debug.LogError("Quadratic bezier interpolation value is invalid : " + t);

        //    t = Math.Clamp(t, 0.0f, 1.0f);

        //    Vector3 a = Vector3.Lerp(startPoint, p1, t);
        //    Vector3 b = Vector3.Lerp(p1, p2, t);
        //    Vector3 c = Vector3.Lerp(p2, endPoint, t);
        //    Vector3 d = Vector3.Lerp(a, b, t);
        //    Vector3 e = Vector3.Lerp(b, c, t);
        //    return Vector3.Lerp(d, e, t);
        //}
    }
}
