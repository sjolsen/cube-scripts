using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class CameraController : MonoBehaviour
{
    public GlobeTrack Track;

    public class GlobeTrack
    {
        public enum State
        {
            IDLE,
            GRABBED,
            RELEASED
        }

        public State state { get; private set; } = State.IDLE;

        private CameraController _controller;
        private Quaternion _releaseStart;
        private Quaternion _releaseEnd;
        private float _releaseStep;

        public GlobeTrack(CameraController controller) => _controller = controller;

        public void Grab()
        {
            // Assert.AreEqual(State.IDLE, state);
            state = State.GRABBED;
        }

        private static Plane[] _primaryPlanes = new Plane[] {
            new Plane(Vector3.right, 0),  // yz
            new Plane(Vector3.up, 0),  // xz
            new Plane(Vector3.forward, 0),  // xy
        };

        static private readonly float _maxDeviation = 25;

        static private Quaternion _reorientToPlane(Quaternion rotation, Plane plane, bool clamp)
        {
            Vector3 heading = rotation * Vector3.forward;
            Vector3 closest_point = plane.ClosestPointOnPlane(heading);
            if (clamp && Vector3.Angle(closest_point, heading) > _maxDeviation)
                heading = Vector3.RotateTowards(closest_point, heading, Mathf.Deg2Rad * _maxDeviation, Mathf.Infinity).normalized;
            Vector3[] up_candidates = {
                plane.normal,
                -plane.normal,
                Vector3.Cross(closest_point, plane.normal),
                -Vector3.Cross(closest_point, plane.normal),
            };
            Vector3 up = Nearest.Vector(rotation * Vector3.up, up_candidates);
            return Quaternion.LookRotation(heading, up);
        }

        static public Quaternion FixOrientation(Quaternion rotation)
        {
            Vector3 heading = rotation * Vector3.forward;
            Plane[] planes = Nearest.PlaneSort(heading, _primaryPlanes);
            rotation = _reorientToPlane(rotation, planes[0], true);
            heading = rotation * Vector3.forward;
            float angle0 = Vector3.Angle(planes[0].ClosestPointOnPlane(heading), heading);
            float angle1 = Vector3.Angle(planes[1].ClosestPointOnPlane(heading), heading);
            float slerp_region = _maxDeviation * 1.15f;  // Handles corner discontinuities
            if (angle1 < slerp_region)
            {
                // 0.5 when angle0 = angle1; 0 when angle1 = _maxDeviation + epsilon
                float interpolation = 0.5f - (angle1 - angle0) / (2 * (slerp_region - angle0));
                Quaternion secondary_rotation = _reorientToPlane(rotation, planes[1], false);
                rotation = Quaternion.Slerp(rotation, secondary_rotation, interpolation);
            }
            return rotation;
        }

        public void UpdateAnglesDelta(float delta_yaw, float delta_pitch)
        {
            Assert.AreEqual(State.GRABBED, state);
            Quaternion delta = Quaternion.Euler(-delta_pitch, delta_yaw, 0);
            Quaternion new_heading = FixOrientation(_controller.transform.rotation * delta);
            _controller.transform.rotation = new_heading;
            _controller.transform.position = new_heading * (20 * Vector3.back);
        }

        private static readonly Vector3[] _primaryAxes = {
            Vector3.right,
            Vector3.left,
            Vector3.up,
            Vector3.down,
            Vector3.forward,
            Vector3.back,
        };

        private static Quaternion _computeReleaseTarget(Quaternion rotation)
        {
            Vector3 forward = Nearest.Vector(rotation * Vector3.forward, _primaryAxes);
            var up_candidates = new List<Vector3>(_primaryAxes);
            up_candidates.Remove(forward);
            up_candidates.Remove(-forward);
            Vector3 up = Nearest.Vector(rotation * Vector3.up, up_candidates);
            Quaternion at_face = Quaternion.LookRotation(forward, up);
            Quaternion[] candidates = {
                FixOrientation(at_face * Quaternion.Euler(20, 30, 0)),
                FixOrientation(at_face * Quaternion.Euler(20, -30, 0)),
                FixOrientation(at_face * Quaternion.Euler(-20, 30, 0)),
                FixOrientation(at_face * Quaternion.Euler(-20, -30, 0)),
            };
            return Nearest.Quaternion(rotation, candidates);
        }

        public void Release()
        {
            Assert.AreEqual(State.GRABBED, state);
            _releaseStart = _controller.transform.rotation;
            _releaseEnd = _computeReleaseTarget(_releaseStart);
            _releaseStep = 0;
            // TODO: Implement release animation
            _controller.transform.rotation = _releaseEnd;
            _controller.transform.position = _releaseEnd * (20 * Vector3.back);
            state = State.RELEASED;
        }

        private static readonly float _releaseTime = 0.15f;
        private static readonly AnimationCurve _releaseEasing = AnimationCurve.EaseInOut(-1, -1, 1, 1);

        public void Update()
        {
            if (state == State.RELEASED)
            {
                _releaseStep += Time.deltaTime / _releaseTime;
                _controller.transform.rotation = Quaternion.Slerp(_releaseStart, _releaseEnd, _releaseEasing.Evaluate(_releaseStep));
                _controller.transform.position = _controller.transform.rotation * (20 * Vector3.back);  // TODO: Commonize
                if (_releaseStep >= 1)
                    state = State.IDLE;
            }
        }
    }

    void Start()
    {
        Track = new GlobeTrack(this);
        transform.rotation = GlobeTrack.FixOrientation(Quaternion.Euler(20, -30, 0));
        transform.position = transform.rotation * (20 * Vector3.back);
    }

    void Update()
    {
        Track.Update();
    }
}
