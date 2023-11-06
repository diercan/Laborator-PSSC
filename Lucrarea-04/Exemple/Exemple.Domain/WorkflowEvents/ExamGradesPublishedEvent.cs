using CSharp.Choices;
using System;

namespace Exemple.Domain.Models
{
    [AsChoice]
    public static partial class ExamGradesPublishedEvent
    {
        public interface IExamGradesPublishedEvent { }

        public record ExamGradesPublishScucceededEvent : IExamGradesPublishedEvent 
        {
            public string Csv{ get;}
            public DateTime PublishedDate { get; }

            internal ExamGradesPublishScucceededEvent(string csv, DateTime publishedDate)
            {
                Csv = csv;
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
