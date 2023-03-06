using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using CustomExtensions;

namespace CustomTracks
{
    [System.Serializable]
    public class TweenBehaviour : PlayableBehaviour
    {
        const float transitionDuration = 1f;

        [HideInInspector] public PlayableDirector director;
        [HideInInspector] public Transform t;   //Target transform to move and rotate
        [HideInInspector] public TweenAsset asset;  //Clip (TweenAsset here) that holds this behaviour

        #region For resetting transform
        Vector3 resetPosition = Vector3.zero;
        Quaternion resetRotation = Quaternion.identity;
        #endregion

        bool positionInitialized = false;
        public bool useCurveRotation = false;   //Whether character should face the moving direction or not
        public TranslateType translateType = TranslateType.FromPreviousClip;

        public bool useTimeCurve = false;   //Uses separate animation curve to evaluate the values for movement and rotation

        public Vector3 startPosition, startRotation;
        public Vector3 endPosition, endRotation;
        public float rotationOffset = 0;

        public BezierType curveType = BezierType.Linear;    //Type of curve used for this clip
        public Vector3 point1, point2;

        [HideInInspector] public double startTime;
        [HideInInspector] public double endTime;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (!t || !director)
            {
                return;
            }

            double playTime = director.time;
            if (playTime <= startTime || playTime > endTime)
            {
                positionInitialized = false;
                return;
            }

            double i = Extensions.InverseLerp(startTime, endTime, playTime);

            if (translateType == TranslateType.HoldNewPosition || translateType == TranslateType.Hold)   //For this TranslateType, interpolation is not needed
            {
                t.position = startPosition;
                if (startRotation.x != TweenTrack.IGNORE_ROTATION_VALUE)
                {
                    t.rotation = Quaternion.Euler(startRotation);
                }
                return;
            }

            if (translateType == TranslateType.FromNewPosition && !positionInitialized)
            {
                positionInitialized = true;
                t.forward = (endPosition - startPosition).setY(0);
            }

            //If useTimeCurve is true, evaluate values from the animation curve
            if (useTimeCurve)
                move(asset.timeCurve.Evaluate((float)i));
            else
                move((float)i);


            if (useCurveRotation == false)   //if useCurveRotation is false, linearly interpolate start and end rotation values to set the rotation value
                t.rotation = Quaternion.Slerp(Quaternion.Euler(startRotation), Quaternion.Euler(endRotation), (float)i);
        }

        void move(float i)
        {
            //float lerpSpeed = 0.02f;
            float lerpSpeed = (float)(director.time - startTime) / transitionDuration;
            lerpSpeed = Mathf.Clamp(lerpSpeed, 0f, 1.0f);

            switch (curveType)
            {
                case BezierType.Linear:
                    t.position = Vector3.Lerp(startPosition, endPosition, i);
                    if (useCurveRotation)
                    {
                        Vector3 final = (endPosition - startPosition).setY(0);
                        final = Quaternion.Euler(Vector3.down * rotationOffset) * final;
                        t.forward = Vector3.Lerp(t.forward, final, lerpSpeed);
                    }
                    break;

                case BezierType.Quadratic:
                    t.position = Bezier.getQuadraticPoint(startPosition, endPosition, point1, i);
                    if (useCurveRotation)
                    {
                        Vector3 final = Bezier.getQuadraticTangent(startPosition, endPosition, point1, i).setY(0);
                        final = Quaternion.Euler(Vector3.down * rotationOffset) * final;
                        t.forward = Vector3.Lerp(t.forward, final, lerpSpeed);
                    }
                    break;

                case BezierType.Cubic:
                    t.position = Bezier.getCubicPoint(startPosition, endPosition, point1, point2, i);
                    if (useCurveRotation)
                    {
                        Vector3 final = Bezier.getCubicTangent(startPosition, endPosition, point1, point2, i).setY(0);
                        final = Quaternion.Euler(Vector3.down * rotationOffset) * final;
                        t.forward = Vector3.Lerp(t.forward, final, lerpSpeed);
                    }
                    break;
            }
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif

            if (playable.isPlayableCompleted(info))
            {
                positionInitialized = false;
            }
        }

        public override void OnGraphStart(Playable playable)
        {
            if (t == null) return;
            resetPosition = t.position;
            resetRotation = t.rotation;
        }

        public override void OnGraphStop(Playable playable)
        {
            if (t == null) return;
            t.position = resetPosition;
            t.rotation = resetRotation;
        }

        public Quaternion getEndRotation()
        {
            Quaternion result = Quaternion.Euler(endRotation);

            if (useCurveRotation)
            {
                Vector3 fwdDirection = Vector3.zero;
                switch (curveType)
                {
                    case BezierType.Linear:
                        if (useCurveRotation)
                        {
                            fwdDirection = (endPosition - startPosition).setY(0);
                        }
                        break;

                    case BezierType.Quadratic:
                        if (useCurveRotation)
                        {
                            fwdDirection = Bezier.getQuadraticTangent(startPosition, endPosition, point1, 1).setY(0);
                        }
                        break;

                    case BezierType.Cubic:
                        if (useCurveRotation)
                        {
                            fwdDirection = Bezier.getCubicTangent(startPosition, endPosition, point1, point2, 1).setY(0);
                        }
                        break;
                }
                result = Quaternion.LookRotation(fwdDirection);
            }

            return result;
        }

        //public override void OnPlayableDestroy(Playable playable)
        //{
        //    firstFrameHappened = false;

        //    t.position = resetPosition;
        //    t.rotation = resetRotation;
        //}

        public enum TranslateType
        {
            FromPreviousClip,   //Moves from previous clip end position and rotation to this clip end position and rotation
            FromNewPosition,    //Moves from this clip start position and rotation to this clip end position and rotation
            HoldNewPosition,    //Instantly moves the transform to this clip start position and rotation and holds this values until this clip ends
            Hold                //Holds the current position and rotation of the transform until this clip ends
        }

        public enum BezierType
        {
            Linear,     //Uses for straight line movement, linearly interpolates position and rotation values
            Quadratic,  //Uses quadratic bezier curve to estimate the position based on given time. Uses single control point to change the curviness
            Cubic       //Uses cubic bezier curve to estimate the position based on given time. Uses two control points and gives more control on the curviness
        }
    }
}