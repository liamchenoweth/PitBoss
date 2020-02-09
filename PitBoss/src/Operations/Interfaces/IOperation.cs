namespace PitBoss {
    public interface IOperation
    {
    }

    public interface IOperation<TIn, TOut> : IOperation
    {
        TOut Execute(TIn input);
    }

    public interface IOperation<TIn> : IOperation
    {
        void Execute(TIn input);
    }
}