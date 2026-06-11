namespace DevelopProducts.BindingSystem
{
    public interface ISettingItem
    {
        string DisplayName { get; }
        void Apply();
        void Reset();
    }
}
