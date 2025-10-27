namespace Examples.Domain.Operations
{
  public abstract class DomainOperation<TEntity, TState, TResult>
    where TEntity : notnull
    where TState : class
  {
    public abstract TResult Transform(TEntity entity, TState? state);
  }
}
