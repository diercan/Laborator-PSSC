using Example.Dto.Events;
using Example.Events;
using Example.Events.Models;

namespace Example.Accomodation.EventProcessor
{
    internal class GradesPublishedEventHandler : AbstractEventHandler<GradesPublishedEvent>
    {
        public override string[] EventTypes => new string[]{typeof(GradesPublishedEvent).Name};

        protected override Task<EventProcessingResult> OnHandleAsync(GradesPublishedEvent eventData)
        {
            Console.WriteLine(eventData.ToString());
            return Task.FromResult(EventProcessingResult.Completed);
        }
    }
}
