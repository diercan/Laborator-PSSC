using Examples.Domain.States;

namespace Examples.Domain.Repositories;

/// <summary>Port prin care workflow-ul persistă rezultatul publicării unui examen.</summary>
public interface IGradesRepository
{
    /// <summary>
    /// Salvează notele calculate ale examenului publicat: câte un rând pe student, actualizat dacă exista deja
    /// sau adăugat dacă nu (upsert după numărul matricol).
    /// </summary>
    Task SaveAsync(Exam.Published exam, CancellationToken cancellationToken);
}
