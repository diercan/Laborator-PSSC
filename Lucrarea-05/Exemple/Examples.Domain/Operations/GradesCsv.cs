using System.Text;
using Examples.Domain.States;

namespace Examples.Domain.Operations;

/// <summary>Randarea listei de note calculate ca text CSV (RFC 4180), pentru exportul publicat.</summary>
public static class GradesCsv
{
    /// <summary>Antetul coloanelor, prima linie a fiecărui export.</summary>
    public const string Header = "RegistrationNumber,ExamGrade,ActivityGrade,FinalGrade";

    /// <summary>Construiește textul CSV complet: antetul urmat de câte o linie pentru fiecare student, fără linie goală la final.</summary>
    public static string Render(IEnumerable<CalculatedStudentGrade> grades)
    {
        StringBuilder csv = new StringBuilder().Append(Header);
        foreach (CalculatedStudentGrade grade in grades)
        {
            csv.Append('\r').Append('\n').Append(RenderLine(grade));
        }

        return csv.ToString();
    }

    private static string RenderLine(CalculatedStudentGrade grade) => string.Join(
        ',',
        grade.RegistrationNumber.Value,
        grade.ExamGrade.ToString(),
        grade.ActivityGrade.ToString(),
        grade.FinalGrade.Match(final => final.ToString(), () => string.Empty));
}
