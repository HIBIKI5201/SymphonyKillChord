using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Mock.MusicBattle.Camera
{
    public static class CamraUtil
    {
        public static (Transform transform, int index) GetTargetWithAxis(this Transform camera,
            Transform[] targets, float axis,
            params Transform[] ignore)
        {
            Vector3 forward = camera.forward;
            Vector3 up = camera.up;

            float minAngle = float.MaxValue;
            int index = -1;
            Transform closest = null;

            for (int i = 0; i < targets.Length; i++)
            {
                Transform t = targets[i];
                Vector3 dir = (t.position - camera.position).normalized;

                float signed = Vector3.SignedAngle(forward, dir, up);

                float angle = axis < 0f ?
                    signed >= 0 ? 360f - signed : -signed : // 右（時計回り）
                    signed >= 0 ? signed : 360f + signed; // 左（反時計回り）

                if (angle < minAngle &&
                    !ignore.Contains(t)) // 除外リストに含まれていない時。
                {
                    minAngle = angle;
                    closest = t;
                    index = i;
                }
            }

            return (closest, index);
        }
    }
}
