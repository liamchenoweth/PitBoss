namespace PitBoss {
    public interface IOperation
    {
    }

    public interface IOperation<TIn, TOut> : IOperation
    {
        TOut Execute(TIn input);
    }

    public interface INullOutputOperation<TIn> : IOperation
    {
        void Execute(TIn input);
    }

    public interface INullInputOperation<TOut> : IOperation
    {
        TOut Execute();
    }
}