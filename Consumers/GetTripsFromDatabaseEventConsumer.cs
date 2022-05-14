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
            var beginDate = context.Message.BeginDate.ToUniversalTime();
            var endDate = context.Message.EndDate.ToUniversalTime();
            var destination = context.Message.Destination;
            var departure = context.Message.Departure;
            var trips = _service.GetTrips(beginDate: beginDate, endDate: endDate, destination: destination, departure: departure);
            var tripsDto = new List<TripDto>();
            foreach(var trip in trips)
            {
                tripsDto.Add(trip.ToTripDto());
            }
            await context.Publish<GetTripsFromDatabaseReplyEvent>(new GetTripsFromDatabaseReplyEvent { Trips = tripsDto, CorrelationId = correlationId, Id=id});
            Console.WriteLine("Consumer: published trips");
        }
    }
}
