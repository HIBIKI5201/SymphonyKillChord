using System;
using UnityEngine;

namespace DevelopProducts.SkillTree
{
    /// <summary>
    /// スキルツリーのノードIDを表す値オブジェクトです。
    /// </summary>
    public readonly struct SkillNodeIdVOTest : IEquatable<SkillNodeIdVOTest>
    {
        /// <summary>
        /// スキルノードIDを生成します。
        /// </summary>
        /// <param name="id">スキルノードのID。</param>
        public SkillNodeIdVOTest(int id)
        {
            _id = id;
        }

        /// <summary>
        /// スキルノードのIDを取得します。
        /// </summary>
        public int Id => _id;

        /// <summary>
        /// スキルノードのID。
        /// </summary>
        private readonly int _id;

        /// <summary>
        /// 指定したスキルノードIDと現在の値が等しいかどうかを判定します。
        /// </summary>
        /// <param name="other">比較対象のスキルノードID。</param>
        /// <returns>同じIDの場合は true、それ以外の場合は false。</returns>
        public bool Equals(SkillNodeIdVOTest other)
        {
            return _id == other.Id;
        }

        /// <summary>
        /// 指定したオブジェクトと現在の値が等しいかどうかを判定します。
        /// </summary>
        /// <param name="obj">比較対象のオブジェクト。</param>
        /// <returns>同じスキルノードIDの場合は true、それ以外の場合は false。</returns>
        public override bool Equals(object obj)
        {
            return obj is SkillNodeIdVOTest other && Equals(other);
        }

        /// <summary>
        /// 現在のスキルノードIDのハッシュコードを取得します。
        /// </summary>
        /// <returns>スキルノードIDに基づくハッシュコード。</returns>
        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        /// <summary>
        /// 2つのスキルノードIDが等しいかどうかを判定します。
        /// </summary>
        /// <param name="left">比較する左辺のスキルノードID。</param>
        /// <param name="right">比較する右辺のスキルノードID。</param>
        /// <returns>等しい場合は true、それ以外の場合は false。</returns>
        public static bool operator ==(SkillNodeIdVOTest left, SkillNodeIdVOTest right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// 2つのスキルノードIDが異なるかどうかを判定します。
        /// </summary>
        /// <param name="left">比較する左辺のスキルノードID。</param>
        /// <param name="right">比較する右辺のスキルノードID。</param>
        /// <returns>異なる場合は true、それ以外の場合は false。</returns>
        public static bool operator !=(SkillNodeIdVOTest left, SkillNodeIdVOTest right)
        {
            return !(left == right);
        }
    }
}