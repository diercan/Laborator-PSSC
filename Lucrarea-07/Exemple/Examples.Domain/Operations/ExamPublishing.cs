using Examples.Domain.Events;
using Examples.Domain.States;

namespace Examples.Domain.Operations;

/// <summary>Publicarea examenului: produce starea finală și, din ea, evenimentul de domeniu.</summary>
public static class ExamPublishing
{
    extension(Exam.Calculated exam)
    {
        /// <summary>
        /// Produce starea finală a examenului: notele calculate, exportul CSV și momentul publicării.
        /// Funcția este totală și pură — ceasul (<paramref name="publishedAt"/>) vine ca parametru, nu din <c>DateTime.Now</c>.
        /// </summary>
        public Exam.Published Publish(DateTimeOffset publishedAt) =>
            new(exam.Grades, GradesCsv.Render(exam.Grades), publishedAt);
    }

    extension(Exam.Published exam)
    {
        /// <summary>Convertește starea finală în evenimentul de domeniu anunțat celorlalte contexte.</summary>
        public ExamPublishedEvent ToEvent() => new(exam.Grades, exam.Csv, exam.PublishedAt);
    }
}
