using CSharp.Choices;
using System;
using System.Collections.Generic;

namespace Exemple.Domain.Models
{
    [AsChoice]
    public static partial class ExamGradesPublishedEvent
    {
        public interface IExamGradesPublishedEvent { }

        public record ExamGradesPublishScucceededEvent : IExamGradesPublishedEvent 
        {
            public IEnumerable<PublishedStudentGrade> Grades{ get; }
            public DateTime PublishedDate { get; }

            internal ExamGradesPublishScucceededEvent(IEnumerable<PublishedStudentGrade> grades, DateTime publishedDate)
            {
                Grades = grades;
                PublishedDate = publishedDate;
            }
        }

        public record ExamGradesPublishFaildEvent : IExamGradesPublishedEvent 
        {
            public string Reason { get; }

            internal ExamGradesPublishFaildEvent(string reason)
            {
                Reason = reason;
            }
        }
    }
}
