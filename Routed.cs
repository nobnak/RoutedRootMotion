using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace RoutedRootMotion {

    public class Routed : MonoBehaviour {

        public Settings settings = new Settings();

        protected Animator animator;
        protected AnimatorAdapter animatorAdapter;

        #region unity
        protected virtual void OnEnable() {
            animator = GetComponentInChildren<Animator>();
            if ((animatorAdapter = animator.GetComponent<AnimatorAdapter>()) == null) 
                animatorAdapter = animator.gameObject.AddComponent<AnimatorAdapter>();
            animatorAdapter.SetListener(Listen);
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
            quaternion rot_curr_lc = transform.localRotation;

            float3 pos_next_lc = transform.InverseTransformVector(animator.rootPosition);
            pos_next_lc = math.select(pos_next_lc, pos_curr_lc, settings.constraints.pos);

            quaternion rot_next_lc = math.mul(animator.deltaRotation, rot_curr_lc);

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