using System.Linq;
using UnityEngine;

namespace Mock.MusicBattle.Camera
{
    public static class CamraUtil
    {
        public static Transform GetTargetWithAxis(this Transform camera, Transform[] targets, float axis)
        {
            if (camera == null || targets == null || targets.Length == 0)
                return null;

            Transform best = null;
            float bestAngle = float.MaxValue;
            axis = Mathf.Sign(axis);

            foreach (var t in targets)
            {
                if (t == null) continue;

                Vector3 toTarget = t.position - camera.position;

                // 左右判定
                float dotRight = Vector3.Dot(camera.right, toTarget);
                if (axis == 1 && dotRight <= 0f) continue;
                if (axis == -1 && dotRight >= 0f) continue;

                // 前方角度（回転量）
                float angle = Vector3.Angle(camera.forward, toTarget);

                if (angle < bestAngle)
                {
                    bestAngle = angle;
                    best = t;
                }
            }

            return best;
        }
    }
}
