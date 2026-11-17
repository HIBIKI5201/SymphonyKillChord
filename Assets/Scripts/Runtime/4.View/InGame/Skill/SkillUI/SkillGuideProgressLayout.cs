using UnityEngine;

namespace KillChord.Runtime.View.InGame.Skill
{
    /// <summary>
    ///     スキル入力進行UIの置き場所を変える処理をまとめたクラスです。
    /// </summary>
    public static class SkillGuideProgressLayout
    {
        /// <summary>
        ///     リズムゲージの下にあるスキル入力進行UIを、リズムゲージを挟んだ反対側（上）へ移す。
        ///     既に上にある場合は動かさない。横の位置は変えないため、アイコンのジャスト位置への追従は保たれる。
        /// </summary>
        /// <param name="viewRoot"> スキル入力進行UIの親です。 </param>
        /// <param name="rhythmGauge"> リズムゲージのTransformです。 </param>
        /// <param name="extraGap"> ゲージとの間に足す余白（ワールド座標）です。 </param>
        public static void MoveAboveGauge(Transform viewRoot, Transform rhythmGauge, float extraGap)
        {
            if (viewRoot == null || rhythmGauge == null)
            {
                return;
            }

            // ゲージの中心からの縦の差を反転し、ゲージを挟んだ反対側に置く。
            float offsetY = viewRoot.position.y - rhythmGauge.position.y;
            if (offsetY >= 0f)
            {
                return;
            }

            Vector3 position = viewRoot.position;
            position.y = rhythmGauge.position.y - offsetY + extraGap;
            viewRoot.position = position;
        }
    }
}
