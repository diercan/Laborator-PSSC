using Examples.Domain.Commands;
using Examples.Domain.Errors;
using Examples.Domain.Events;
using Examples.Domain.Operations;
using Examples.Domain.Repositories;
using Examples.Domain.States;
using Examples.Domain.ValueObjects;
using Examples.Functional;

namespace Examples.Domain.Workflows;

/// <summary>
/// Workflow-ul de publicare a notelor unui examen. Urmează structura "impur → pur → impur" (Wlaschin cap. 12):
/// citește starea necesară din afară, execută logica de afaceri ca o singură funcție pură (<see cref="Publish"/>),
/// apoi salvează rezultatul dacă a reușit. Nu prinde excepții: o excepție aici este o eroare de infrastructură,
/// nu un eșec de validare, și trebuie să propage către marginea aplicației (API, consolă).
/// </summary>
public sealed class PublishExamWorkflow(IStudentsRepository studentsRepository, IGradesRepository gradesRepository, TimeProvider clock)
{
    /// <summary>Execută workflow-ul complet: citește studenții cunoscuți, publică, salvează pe ramura de succes.</summary>
    public async Task<Result<ExamPublishedEvent, PublishExamError>> ExecuteAsync(PublishExamCommand command, CancellationToken cancellationToken)
    {
        IReadOnlySet<StudentRegistrationNumber> knownStudents =
            await studentsRepository.GetExistingAsync(command.RegistrationNumbers, cancellationToken);

        Result<Exam.Published, PublishExamError> published = Publish(command, knownStudents, clock.GetUtcNow());

        return await published
            .TapAsync(exam => gradesRepository.SaveAsync(exam, cancellationToken))
            .Map(exam => exam.ToEvent());
    }

    /// <summary>
    /// Nucleul pur al workflow-ului: validare → calcul → publicare, compuse cu <c>Map</c>/<c>MapError</c>.
    /// Nu are efecte secundare, deci este direct testabilă fără repository-uri sau bază de date.
    /// </summary>
    public static Result<Exam.Published, PublishExamError> Publish(
        PublishExamCommand command, IReadOnlySet<StudentRegistrationNumber> knownStudents, DateTimeOffset now) =>
        new Exam.Unvalidated(command.Grades)
            .Validate(knownStudents)
            // Cast explicit la PublishExamError (tipul de bază): metoda MapError adaugă propriul parametru de
            // tip (TOut), iar SDK-ul curent (10.0.401) nu rezolvă încă apelurile membrilor de extensie C# 14
            // cu argument de tip explicit (ex. .MapError<PublishExamError>(...)); tipul se deduce în schimb
            // din corpul lambdei, forțat aici printr-o conversie explicită.
            .MapError(errors => (PublishExamError)new PublishExamError.Validation(errors))
            .Map(validated => validated.Calculate())
            .Map(calculated => calculated.Publish(now));
}
