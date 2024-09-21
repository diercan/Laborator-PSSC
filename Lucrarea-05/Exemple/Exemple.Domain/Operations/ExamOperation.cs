using Examples.Domain.Exceptions;
using static Examples.Domain.Models.Exam;

namespace Examples.Domain.Operations
{
  internal abstract class ExamOperation<TState> : DomainOperation<IExam, TState, IExam>
    where TState : class
  {
    public override IExam Transform(IExam exam, TState? state) => exam switch
    {
      UnvalidatedExam unvalidatedExam => OnUnvalidated(unvalidatedExam, state),
      ValidatedExam validExam => OnValid(validExam, state),
      InvalidExam invalidExam => OnInvalid(invalidExam, state),
      CalculatedExam calculatedExam => OnCalculated(calculatedExam, state),
      PublishedExam publishedExam => OnPublished(publishedExam, state),
      _ => throw new InvalidExamStateException(exam.GetType().Name)
    };

    protected virtual IExam OnUnvalidated(UnvalidatedExam unvalidatedExam, TState? state) => unvalidatedExam;

    protected virtual IExam OnValid(ValidatedExam validExam, TState? state) => validExam;

    protected virtual IExam OnPublished(PublishedExam publishedExam, TState? state) => publishedExam;

    protected virtual IExam OnCalculated(CalculatedExam calculatedExam, TState? state) => calculatedExam;

    protected virtual IExam OnInvalid(InvalidExam invalidExam, TState? state) => invalidExam;
  }

  internal abstract class ExamOperation : ExamOperation<object>
  {
    internal IExam Transform(IExam exam) => Transform(exam, null);

    protected sealed override IExam OnUnvalidated(UnvalidatedExam unvalidatedExam, object? state) => OnUnvalidated(unvalidatedExam);

    protected virtual IExam OnUnvalidated(UnvalidatedExam unvalidatedExam) => unvalidatedExam;

    protected sealed override IExam OnValid(ValidatedExam validExam, object? state) => OnValid(validExam);

    protected virtual IExam OnValid(ValidatedExam validExam) => validExam;

    protected sealed override IExam OnPublished(PublishedExam publishedExam, object? state) => OnPublished(publishedExam);

    protected virtual IExam OnPublished(PublishedExam publishedExam) => publishedExam;

    protected sealed override IExam OnCalculated(CalculatedExam calculatedExam, object? state) => OnCalculated(calculatedExam);

    protected virtual IExam OnCalculated(CalculatedExam calculatedExam) => calculatedExam;

    protected sealed override IExam OnInvalid(InvalidExam invalidExam, object? state) => OnInvalid(invalidExam);

    protected virtual IExam OnInvalid(InvalidExam invalidExam) => invalidExam;
  }
}
