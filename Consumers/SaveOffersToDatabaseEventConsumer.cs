using Models.Offers;
using Models.Offers.Dto;
using Database.Tables;
using Offers.Services;
using MassTransit;

namespace Offers.Consumers
{
    public class SaveOffersToDatabaseEventConsumer : IConsumer<SaveOffersToDatabaseEvent>
    {
        private readonly ITripsService _service;

        public SaveOffersToDatabaseEventConsumer(ITripsService service)
        {
            _service = service;
        }

        public async Task Consume(ConsumeContext<SaveOffersToDatabaseEvent> context)
        {
            Console.WriteLine($"Consumer: Received event to get trips from database with Id: {context.Message.Id} and CorrelationId: {context.Message.CorrelationId}");
            var tripsDto = context.Message.Trips;
            var trips = new List<Trip>();
            foreach (var tripDto in tripsDto)
            {
                var trip = new Trip();
                trip.SetFields(tripDto);
                trips.Add(trip);
            }
            _service.SaveTrips(trips);
        }
    }
}
