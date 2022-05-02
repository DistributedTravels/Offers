using Models;
using Models.Offers;
using Models.Transport;
using Newtonsoft.Json;
using Offers.Database.Tables;

namespace Offers.Orchestration;

public class GetOffersOrchestrator : Orchestrator<IEnumerable<Trip>>
{
    public GetOffersOrchestrator(Action<EventModel> publish, Func<EventModel, Task<string>> call) : base(publish,
        call) { }

    // TODO create the orchestrator for getting info from Transport and Hotels microservices
    public override async Task<IEnumerable<Trip>> Orchestrate(EventModel @event)
    {
        return await Orchestrate(@event as GetOffersEvent);
    }

    private Task<string> Call(EventModel @event)
    {
        return this.call(@event);
    }

    private async Task<IEnumerable<Trip>> Orchestrate(GetOffersEvent @event)
    {
        var destination = @event.Destination;
        var numberOfPeople = @event.NumberOfPeople;
        var beginDate = @event.BeginDate;
        var endDate = @event.EndDate;
        var getAvailableTravels = new GetAvailableTravelsEvent(beginDate.ToDateTime(new TimeOnly(0, 0)), numberOfPeople, "any", destination, "any");
        var availableTravelsResponse = await Call(getAvailableTravels);
        var travels =
            JsonConvert.DeserializeObject<GetAvailableTravelsReplyEvent>(availableTravelsResponse).TravelItems;
        var trips = new List<Trip>();
        foreach (var travel in travels)
        {
            trips.Add(new Trip(){Destination=destination, BeginDate = beginDate, EndDate = endDate, HotelId = 0, HotelName = "hotel", NumberOfPeople = numberOfPeople, TransportId = travel.TravelId});
        }

        return trips;
    }
}