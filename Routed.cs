using Codice.CM.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace RoutedRootMotion {

    public class Routed : MonoBehaviour {
        private const float EPSILON_DEG = 1e-2f;
        public Settings settings = new();

        protected float3 eulerAngles_init_lc;
        protected Animator animator;
        protected AnimatorAdapter animatorAdapter;

        protected Force force_requested = new();

        #region unity
        protected virtual void OnEnable() {
            animator = GetComponentInChildren<Animator>();
            if ((animatorAdapter = animator.GetComponent<AnimatorAdapter>()) == null) 
                animatorAdapter = animator.gameObject.AddComponent<AnimatorAdapter>();
            animatorAdapter.SetListener(UpdateOnAnimatorMove);
            eulerAngles_init_lc = transform.localEulerAngles;
        }
        protected virtual void OnDisable() {
            if (animatorAdapter != null) {
                animatorAdapter.RemoveListener(UpdateOnAnimatorMove);
                Destroy(animatorAdapter);
            }
        }
        void OnValidate() {
            if (settings.debug.force.enabled) {
                var dest_vec = settings.debug.force.dir;
                dest_vec = math.normalize(dest_vec);
                settings.debug.force.dir = dest_vec;
            }
        }
        #endregion

        #region methods
        protected void UpdateOnAnimatorMove(Animator animator) {
            var dt = Time.deltaTime;
            float3 pos_curr_lc = transform.localPosition;
            quaternion rot_curr_lc = transform.localRotation;

            float angle_deg = 0f;
            Force force;
            quaternion rot_force_lc = rot_curr_lc;
            if ((force = settings.debug.force).enabled || (force = this.force_requested).enabled) {
                rot_force_lc = quaternion.LookRotation(new float3(force.dir.x, 0f, force.dir.y), math.up());
                var rot = math.mul(rot_force_lc, math.inverse(rot_curr_lc));
                angle_deg = math.degrees(QuaternionUtil.Angle(rot));
            }

            float t = 0f;
            if (settings.constants.epsilon_min_rot_deg > float.Epsilon)
                t = math.saturate(math.unlerp(0f, settings.constants.epsilon_min_rot_deg, angle_deg));

            float3 pos_forward_lc;
            quaternion rot_forward_lc;
            MoveByRootMotion(animator, out pos_forward_lc, out rot_forward_lc);

            float3 pos_towrad_lc = pos_curr_lc;
            quaternion rot_toward_lc = rot_curr_lc;
            if (force.enabled) {
                var rotRatio = math.saturate(dt * settings.rotateToward.speed_rotate_deg / angle_deg);
                rot_toward_lc = math.slerp(rot_toward_lc, rot_force_lc, rotRatio);
            }

            transform.localPosition = math.lerp(pos_forward_lc, pos_towrad_lc, t);
            transform.localRotation = math.slerp(rot_forward_lc, rot_toward_lc, t);

            Debug.Log($"Angle={angle_deg:f} t={t}");
        }

        private void MoveByRootMotion(Animator animator, out float3 pos_next_lc, out quaternion rot_next_lc) {
            float3 pos_curr_lc = transform.localPosition;
            quaternion rot_curr_lc = transform.localRotation;

            pos_next_lc = animator.rootPosition;
            if (transform.parent != null) pos_next_lc = transform.parent.InverseTransformPoint(pos_next_lc);
            //pos_next_lc = math.select(pos_next_lc, pos_curr_lc, new bool3(false, true, false));

            rot_next_lc = animator.rootRotation;
            if (transform.parent != null) rot_next_lc = math.mul(math.inverse(transform.parent.rotation), rot_next_lc);
            var rot_const = RotationConstraint.Fixed;
            switch (rot_const) {
                case RotationConstraint.Fixed: {
                    rot_next_lc = rot_curr_lc;
                    break;
                }
                case RotationConstraint.YOnly: {
                    rot_next_lc = QuaternionUtil.ClosestRotationOnAxis(rot_next_lc, QuaternionUtil.Axis.Y);
                    break;
                }
            }
        }

        #endregion

        #region declarations
        public enum AnimationMode {
            RootMotion = 0,
            Script,
        }
        public enum RotationConstraint {
            Fixed = 0,
            YOnly = 2,
            Free = 7
        }
        [System.Serializable]
        public class Force {
            public bool enabled;
            public float2 dir;
            public float mag;
        }
        [System.Serializable]
        public class Constants {
            public float epsilon_min_rot_deg;
        }
        [System.Serializable]
        public class Constraints {
            public bool3 pos;
            public RotationConstraint rot;
        }
        [System.Serializable]
        public class DebugSettings {
            public Force force = new();
        }
        [System.Serializable]
        public class Driver {
            public AnimationMode animationMode;
            public string animationName;

            public float speed_forward;
            public float speed_rotate_deg;
        }
        [System.Serializable]
        public class Settings {
            public DebugSettings debug = new();
            public Constants constants = new();

            public Driver moveForward = new();
            public Driver rotateToward = new();
        }
        #endregion
    }
}