using MassTransit;
using Database.Tables;

namespace Offers.Orchestration
{
    public class StatefulOffer : SagaStateMachineInstance
    {
        public int CurrentState { get; set; }
        public Guid CorrelationId { get; set; }
        public Guid? ProcessTravelsId { get; set; }
        public Guid TravelsProcessedId { get; set; }
        public Guid OffersId { get; set; }
        public string Destination { get; set; }
        public DateOnly BeginDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int NumberOfPeople { get; set; }
        public IEnumerable<Trip> Trips { get; set; }
    }
}
