using System;

namespace DevelopProducts.SkillTree
{
    /// <summary>
    /// エッジに持たせる攻撃力のパラメーター
    /// </summary>
    [Serializable]
    public class AttackParamaterTest: IEdgeParameterCategory
    {
        public float Value { get; }
        public void Apply()
        {
            throw new NotImplementedException();
        }
    }
}