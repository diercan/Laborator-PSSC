using System.Globalization;
using Examples.Api.Models;
using Examples.Domain.States;

namespace Examples.Api.Mapping;

/// <summary>Conversia dintre forma JSON de la graniță și tipul brut acceptat de domeniu.</summary>
public static class InputGradeMapping
{
    extension(InputGrade grade)
    {
        /// <summary>
        /// Domeniul cere text (vezi <c>Grade.Parse</c>), nu <c>decimal?</c>: convertim aici, cu punct ca
        /// separator zecimal, indiferent de cultura serverului.
        /// </summary>
        public UnvalidatedStudentGrade ToUnvalidated() => new(
            grade.RegistrationNumber,
            grade.Exam?.ToString(CultureInfo.InvariantCulture),
            grade.Activity?.ToString(CultureInfo.InvariantCulture));
    }
}
