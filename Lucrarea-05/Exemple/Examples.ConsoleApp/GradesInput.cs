using Examples.Domain.States;

namespace Examples.ConsoleApp;

/// <summary>Citește de la consolă lista de note de publicat, un student pe rând, până la o linie goală.</summary>
internal static class GradesInput
{
    public static IReadOnlyList<UnvalidatedStudentGrade> Read()
    {
        List<UnvalidatedStudentGrade> grades = [];
        System.Console.WriteLine("Introduceți notele (număr matricol, notă examen, notă activitate). Linie goală pentru a termina.");

        while (true)
        {
            System.Console.Write("Număr matricol: ");
            string? registrationNumber = System.Console.ReadLine();
            if (string.IsNullOrWhiteSpace(registrationNumber))
            {
                break;
            }

            System.Console.Write("Notă examen: ");
            string? examGrade = System.Console.ReadLine();

            System.Console.Write("Notă activitate: ");
            string? activityGrade = System.Console.ReadLine();

            grades.Add(new UnvalidatedStudentGrade(registrationNumber, examGrade, activityGrade));
        }

        return grades;
    }
}
