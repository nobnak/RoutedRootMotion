using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace RoutedRootMotion {

    public class Routed : MonoBehaviour {

        public Settings settings = new Settings();

        protected float3 eulerAngles_init_lc;
        protected Animator animator;
        protected AnimatorAdapter animatorAdapter;

        #region unity
        protected virtual void OnEnable() {
            animator = GetComponentInChildren<Animator>();
            if ((animatorAdapter = animator.GetComponent<AnimatorAdapter>()) == null) 
                animatorAdapter = animator.gameObject.AddComponent<AnimatorAdapter>();
            animatorAdapter.SetListener(Listen);
            eulerAngles_init_lc = transform.localEulerAngles;
        }
        protected virtual void OnDisable() {
            if (animatorAdapter != null) {
                animatorAdapter.RemoveListener(Listen);
                Destroy(animatorAdapter);
            }
        }
        #endregion

        #region methods
        protected void Listen(Animator animator) {
            float3 pos_curr_lc = transform.localPosition;

            float3 pos_next_lc = transform.InverseTransformVector(animator.rootPosition);
            pos_next_lc = math.select(pos_next_lc, pos_curr_lc, settings.constraints.pos);

            quaternion rot_next_lc =  animator.rootRotation;
            if (transform.parent != null) rot_next_lc = math.mul(math.inverse(transform.parent.rotation), rot_next_lc);
            rot_next_lc = RotationUtil.ClosestRotationOnAxis(rot_next_lc, RotationUtil.Axis.Y);

            transform.localPosition = pos_next_lc;
            transform.localRotation = rot_next_lc;
        }

        #endregion

        #region declarations
        [System.Serializable]
        public class Constraints {
            public bool3 pos;
            public bool3 rot;
        }
        [System.Serializable]
        public class Settings {
            public Constraints constraints = new Constraints();
        }
        #endregion
    }
}