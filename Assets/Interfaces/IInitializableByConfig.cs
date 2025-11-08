namespace Assets.Interfaces
{
    public interface IInitializableByConfig<TConfig>
    {
        void Initialize(TConfig config);
    }
}
