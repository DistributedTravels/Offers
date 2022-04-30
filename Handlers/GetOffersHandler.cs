using Models;
using Models.Offers;
using Newtonsoft.Json;
using Offers.Database;
using System.Linq;

namespace Offers.Handlers;

public class GetOffersHandler : IHandler
{
    public GetOffersHandler(Action<EventModel> publish, WebApplication app) : base(publish, app)
    {
        //additional constructor actions
    }

    public override async Task HandleEvent(String content)
    {
        var @event = JsonConvert.DeserializeObject<GetOffersEvent>(content);
        Console.WriteLine($"Event received {@event.Id} msg: {content}");
        using (var contScope = this.app.Services.CreateScope())
        using (var context = contScope.ServiceProvider.GetRequiredService<OffersContext>())
        {
            // get cached trips
            var result = (from trip in context.Trips
                where trip.BeginDate.Equals(@event.BeginDate) &&
                      trip.EndDate.Equals(@event.EndDate) &&
                      trip.Destination.Equals(@event.Destination) &&
                      trip.NumberOfPeople.Equals(@event.NumberOfPeople)
                select trip)
                .ToList();
            // check if there are at least 5 available trips
            if (result.Count >= 5)
            {
                // TODO return list of trips to the one who asked
                Console.WriteLine($"First or Default Trip with Hotel named: {result[0].HotelName}");
            }
            else
            {
                // TODO create an orchestrator object which will get information from Transport and Hotels microservices
                Console.WriteLine("not found");
            }
        }
    }
}