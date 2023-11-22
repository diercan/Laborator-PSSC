using Example.Dto.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Example.Dto.Events
{
    public record GradesPublishedEvent
    {
        public List<StudentGradeDto> Grades { get; init; }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            Console.WriteLine();
            Console.WriteLine("-----Published grades-------");

            foreach (var grade in Grades)
            {                
                stringBuilder.AppendLine($"Name: {grade.Name}");
                stringBuilder.AppendLine($"Student Registration Number: {grade.StudentRegistrationNumber}");
                stringBuilder.AppendLine($"Activity Grade: {grade.ActivityGrade}");
                stringBuilder.AppendLine($"Exam Grade: {grade.ExamGrade}");
                stringBuilder.AppendLine($"Final Grade: {grade.FinalGrade}");
                stringBuilder.AppendLine();
            }

            return stringBuilder.ToString();
        }
    }
}
