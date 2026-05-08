namespace DevelopProducts.SkillTree
{
    public interface IEdgeParameterCategory
    {
        /// <summary>付与する値</summary>
        float Value { get; }

        void Apply();
    }
}