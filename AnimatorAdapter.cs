using System;
using UnityEngine;

namespace RoutedRootMotion {

    public class AnimatorAdapter : MonoBehaviour {

        public event Action<Animator> onAnimatorMove;

        public Animator animator;

        #region unity
        protected virtual void OnEnable() {
            animator = GetComponent<Animator>();
        }
        protected virtual void OnAnimatorMove() {
            onAnimatorMove?.Invoke(animator);
        }
        #endregion

        #region interfaces
        public void SetListener(Action<Animator> f) {
            RemoveListener(f);
            AddListener(f);
        }
        public void AddListener(Action<Animator> f) {
            onAnimatorMove += f;
        }
        public void RemoveListener(Action<Animator> f) {
            onAnimatorMove -= f;
        }
        #endregion

        #region declarations
        #endregion
    }

}