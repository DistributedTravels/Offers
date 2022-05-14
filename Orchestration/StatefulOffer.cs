using MassTransit;
using Database.Tables;
using Models.Offers.Dto;

namespace Offers.Orchestration
{
    public class StatefulOffer : SagaStateMachineInstance
    {
        public int CurrentState { get; set; }
        public Guid CorrelationId { get; set; }
        public Guid OffersId { get; set; }
        public string Destination { get; set; }
        public string Departure { get; set; }
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime DepartureTime { get; set; }
        public int NumberOfPeople { get; set; }
        public int Adults { get; set; }
        public int ChildrenUnder3 { get; set; }
        public int ChildrenUnder10 { get; set; }
        public int ChildrenUnder18 { get; set; }
        public IEnumerable<TripDto> Trips { get; set; }
        public IEnumerable<TravelDto> Travels { get; set; }
        public IEnumerable<HotelDto> Hotels { get; set; }
        public Uri? RequestUri { get; set; }
    }
}
