using MassTransit;
using Models.Offers;
using Models.Transport;
using Database.Tables;

namespace Offers.Orchestration
{
    public class OfferStateMachine : MassTransitStateMachine<StatefulOffer>
    {
        public State ReceivedTravels { get; set; }
        public Event<GetOffersEvent> GetOffers { get; set; }
        public Event<GetAvailableTravelsReplyEvent> GetTravelsReply { get; set; }
        public Request<StatefulOffer, GetAvailableTravelsEvent, GetAvailableTravelsReplyEvent> RequestTravels { get; set; }

        public OfferStateMachine()
        {
            InstanceState(x => x.CurrentState, ReceivedTravels);
            Event(() => GetOffers, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => GetTravelsReply, x => x.CorrelateById(context => context.Message.CorrelationId));
            Request(() => RequestTravels, requestedTravels => requestedTravels.ProcessTravelsId, config => { config.Timeout = TimeSpan.Zero; });
            // TODO check if IDs are correct
            // I might have made some mistakes...
            Initially(
                When(GetOffers)
                    .Then(context =>
                    {
                        //receive information about offers the user is looking for
                        context.Saga.CorrelationId = context.Message.CorrelationId;
                        context.Saga.ProcessTravelsId = Guid.NewGuid();
                        context.Saga.TravelsProcessedId = Guid.NewGuid();
                        if (!context.TryGetPayload(out SagaConsumeContext<StatefulOffer, GetOffersEvent> payload))
                        {
                            throw new Exception("Unable to retrieve payload with requested offers");
                        }
                        context.Saga.OffersId = payload.Message.Id;
                        context.Saga.NumberOfPeople = payload.Message.NumberOfPeople;
                        context.Saga.BeginDate = payload.Message.BeginDate;
                        context.Saga.EndDate = payload.Message.EndDate;
                        // TODO get offers from database (if there are any)
                        context.Saga.Trips = new List<Trip>();
                    })
                    .IfElse(x => x.Saga.Trips.Count() < 10,
                        x => x.Request(RequestTravels, context => new GetAvailableTravelsEvent(
                                departure: context.Saga.BeginDate.ToDateTime(TimeOnly.MinValue),
                                freeSeats: context.Saga.NumberOfPeople) { Id = context.Saga.TravelsProcessedId, ProcessingId = context.Saga.ProcessTravelsId!.Value})
                              .TransitionTo(RequestTravels.Pending),
                        x => x.TransitionTo(Final)));
            During(RequestTravels.Pending,
                When(RequestTravels.Completed)
                    .Then(context =>
                    {
                        if (!context.TryGetPayload(out SagaConsumeContext<StatefulOffer, GetAvailableTravelsReplyEvent> payload))
                        {
                            throw new Exception("Unable to retrieve payload with travels");
                        }
                        var travels = payload.Message.TravelItems;
                        var trips = new List<Trip>();
                        foreach (var travel in travels)
                        {
                            trips.Add(new Trip() { Destination = context.Saga.Destination, BeginDate = context.Saga.BeginDate,
                                EndDate = context.Saga.EndDate, HotelId = 0, HotelName = "hotel",
                                NumberOfPeople = context.Saga.NumberOfPeople, TransportId = travel.TravelId });
                        }
                        context.Saga.Trips = trips;
                    })
                    .TransitionTo(ReceivedTravels));
            WhenEnter(ReceivedTravels, binder => binder
                .Then(context => context.RespondAsync(new GetOffersEventReply() { Id = context.Saga.OffersId, CorrelationId = context.Saga.CorrelationId }))
                .Finalize());
        }
    }
}
