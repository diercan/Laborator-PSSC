namespace Examples.Data.Queries;

/// <summary>Model de citire pentru catalogul curent, folosit de partea de raportare (de exemplu <c>GET /grades</c>).</summary>
public interface IGradesQuery
{
    Task<IReadOnlyList<StudentGradeRow>> GetAllAsync(CancellationToken cancellationToken);
}
