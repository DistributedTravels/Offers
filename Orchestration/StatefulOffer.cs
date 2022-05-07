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
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
        public int NumberOfPeople { get; set; }
        public IEnumerable<TripDto> Trips { get; set; }
        public IEnumerable<TravelDto> Travels { get; set; }
        public IEnumerable<HotelDto> Hotels { get; set; }
    }
}
