namespace PitBoss {
    public interface IPipelineOperation<T>
    {
        void Execute(T input);
    }
}