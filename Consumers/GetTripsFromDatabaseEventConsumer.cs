using MassTransit;
using Offers.Services;
using Models.Offers;
using Models.Offers.Dto;
using Models;

namespace Offers.Consumers
{
    public class GetTripsFromDatabaseEventConsumer : IConsumer<GetTripsFromDatabaseEvent>
    {
        private readonly ITripsService _service;

        public GetTripsFromDatabaseEventConsumer(ITripsService service)
        {
            _service = service;
        }

        public async Task Consume(ConsumeContext<GetTripsFromDatabaseEvent> context)
        {
            // TODO filter trips
            Console.WriteLine($"Consumer: Received event to get trips from database with Id: {context.Message.Id} and CorrelationId: {context.Message.CorrelationId}");
            var correlationId = context.Message.CorrelationId;
            var id = context.Message.Id;
            var trips = _service.GetTrips();
            var tripsDto = new List<TripDto>();
            foreach(var trip in trips)
            {
                tripsDto.Add(trip.ToTripDto());
            }
            Console.WriteLine($"Consumer: found trips with first trip to: {tripsDto.First().Destination}");
            await context.Publish<GetTripsFromDatabaseReplyEvent>(new GetTripsFromDatabaseReplyEvent { Trips = tripsDto, CorrelationId = correlationId, Id=id});
            await context.Publish<EventModel>(new EventModel());
            Console.WriteLine("Consumer: published trips");
        }
    }
}
