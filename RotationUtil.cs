using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace RoutedRootMotion {
    public static class RotationUtil {

        #region static
        public static quaternion ClosestRotationOnAxis(quaternion p, Axis axis) {
            float4 q_value = default;
            float4 p_value = p.value;
            int axis_int = (int)axis;
            var pOnAxis = new float2(p_value.w, p_value[axis_int]);
            var lenSq = math.lengthsq(pOnAxis);
            if (lenSq > 1e-3f) {
                var lenInv = math.rsqrt(lenSq);
                q_value.w = p_value.w / lenInv;
                q_value[axis_int] = p_value[axis_int] / lenInv;
            } else {
                q_value.w = 1f;
            }
            return new quaternion(q_value);
        }
        #endregion

        #region declarations
        public enum Axis {
            X = 0,
            Y = 1,
            Z = 2
        }
        #endregion
    }
}