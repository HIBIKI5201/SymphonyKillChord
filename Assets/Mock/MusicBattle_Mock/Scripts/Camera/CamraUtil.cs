using UnityEngine;

namespace Mock.MusicBattle.Camera
{
    public static class CamraUtil
    {
        public static (Transform transform, int index) GetTargetWithAxis(this Transform camera, Transform[] targets, float axis)
        {
            if (camera == null || targets == null || targets.Length == 0)
                return (null, -1);

            Transform best = null;
            int index = 0;
            float bestAngle = float.MaxValue;
            axis = Mathf.Sign(axis);

            for (int i = 0; i < targets.Length; i++)
            {
                Transform t = targets[i];
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
                    index = i;
                }
            }

            return (best, index);
        }
    }
}
