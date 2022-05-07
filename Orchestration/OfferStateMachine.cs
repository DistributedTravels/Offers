using MassTransit;
using Models.Offers;
using Models.Offers.Dto;
using Models.Transport;
using Database.Tables;

namespace Offers.Orchestration
{
    public class OfferStateMachine : MassTransitStateMachine<StatefulOffer>
    {
        public State ReceivedTravels { get; set; }
        public State AwaitingHotelsAndTravels { get; set; }
        public State AwaitingTravels { get; set; }
        public State AwaitingHotels { get; set; }
        public State ReceivedHotelsAndTravels { get; set; }
        public Event<GetOffersEvent> GetOffersEvent { get; set; }
        public Event<GetAvailableTravelsReplyEvent> GetAvailableTravelsReplyEvent { get; set; }
        // TODO events for getting hotels

        public OfferStateMachine()
        {
            InstanceState(x => x.CurrentState, ReceivedTravels, AwaitingHotelsAndTravels, AwaitingTravels, AwaitingHotels, ReceivedHotelsAndTravels);
            Event(() => GetOffersEvent, x => { x.CorrelateById(context => context.Message.CorrelationId); x.SelectId(context => context.Message.Id); });
            Event(() => GetAvailableTravelsReplyEvent, x => { x.CorrelateById(context => context.Message.CorrelationId); });
            // TODO check if IDs are correct
            // I might have made some mistakes...

            //Initially wait for GetOffersEvent
            Initially(
                When(GetOffersEvent)
                    .Then(context =>
                    {
                        //receive information about offers the user is looking for
                        context.Saga.CorrelationId = context.Message.CorrelationId;
                        if (!context.TryGetPayload(out SagaConsumeContext<StatefulOffer, GetOffersEvent> payload))
                        {
                            throw new Exception("Unable to retrieve payload with requested offers");
                        }
                        context.Saga.OffersId = payload.Message.Id;
                        context.Saga.NumberOfPeople = payload.Message.NumberOfPeople;
                        context.Saga.BeginDate = payload.Message.BeginDate;
                        context.Saga.EndDate = payload.Message.EndDate;
                        // TODO get offers from database (if there are any)
                        context.Saga.Trips = new List<TripDto>();
                        Console.WriteLine("Received request for trips");
                    })
                    .IfElse(x => x.Saga.Trips.Count() < 10,
                        x => x.PublishAsync(context => context.Init<GetAvailableTravelsEvent>(new GetAvailableTravelsEvent(
                                departure: context.Saga.BeginDate,
                                freeSeats: context.Saga.NumberOfPeople) { Id = context.Saga.OffersId, CorrelationId = context.Saga.CorrelationId }))
                              //.PublishAsync(context => context.Init<GetAvailableHotelsEvent>(new GetAvailableHotelsEvent(...))
                              .TransitionTo(AwaitingHotelsAndTravels),
                        x => x.TransitionTo(ReceivedHotelsAndTravels)));

            // during AwaitingTravels wait for GetAvailableTravelsReplyEvent containing possible travels
            During(AwaitingHotelsAndTravels,
                When(GetAvailableTravelsReplyEvent)
                    .Then(context =>
                    {
                        if (!context.TryGetPayload(out SagaConsumeContext<StatefulOffer, GetAvailableTravelsReplyEvent> payload))
                        {
                            throw new Exception("Unable to retrieve payload with travels");
                        }
                        var travelsReceived = payload.Message.TravelItems;
                        var travels = new List<TravelDto>();
                        foreach (var travel in travelsReceived)
                        {
                            travels.Add(new TravelDto(travel));
                        }
                        context.Saga.Travels = travels;
                        Console.WriteLine("Received travels from Transport");
                    })
                    .TransitionTo(AwaitingHotels)
                /*,
                // TODO finish receiving hotels when service is ready
                When(GetAvailableHotelsReplyEvent)
                    .Then(context =>
                    {
                        if (!context.TryGetPayload(out SagaConsumeContext<StatefulOffer, GetAvailableHotelsReplyEvent> payload))
                        {
                            throw new Exception("Unable to retrieve payload with travels");
                        }
                        var hotelsReceived = payload.Message.Hotels;
                        var hotels = new List<HotelDto>();
                        foreach(var hotel in hotelsReceived)
                        {
                            hotels.Add(new HotelDto(...));
                        }
                        context.Saga.Hotels = hotels;
                        Console.WriteLine("Received hotels from Hotels");
                    })
                    .TransitionTo(AwaitingTravels)
                */
                );

            During(AwaitingHotels,
                When(GetAvailableTravelsReplyEvent)
                    .Then(context =>
                    {
                        if (!context.TryGetPayload(out SagaConsumeContext<StatefulOffer, GetAvailableTravelsReplyEvent> payload))
                        {
                            throw new Exception("Unable to retrieve payload with travels");
                        }
                        var travelsReceived = payload.Message.TravelItems;
                        var travels = new List<TravelDto>();
                        foreach (var travel in travelsReceived)
                        {
                            travels.Add(new TravelDto(travel));
                        }
                        context.Saga.Travels = travels;
                        Console.WriteLine("Received travels from Transport");
                    })
                    .TransitionTo(ReceivedHotelsAndTravels));
            /*
            During(AwaitingTravels,
                // TODO finish receiving hotels when service is ready
                When(GetAvailableHotelsReplyEvent)
                    .Then(context =>
                    {
                        if (!context.TryGetPayload(out SagaConsumeContext<StatefulOffer, GetAvailableHotelsReplyEvent> payload))
                        {
                            throw new Exception("Unable to retrieve payload with travels");
                        }
                        var hotelsReceived = payload.Message.Hotels;
                        var hotels = new List<HotelDto>();
                        foreach (var hotel in hotelsReceived)
                        {
                            hotels.Add(new HotelDto(...));
                        }
                        context.Saga.Hotels = hotels;
                        Console.WriteLine("Received hotels from Hotels");
                    })
                    .TransitionTo(ReceivedHotelsAndTravels));
            */

            WhenEnter(ReceivedHotelsAndTravels, binder => binder
                .Then(context => 
                {
                    // TODO check if trip generation is alright
                    var trips = new List<TripDto>();
                    foreach (var travel in context.Saga.Travels)
                    {
                        foreach (var hotel in context.Saga.Hotels)
                        {
                            if (hotel.Location.Equals(travel.Destination))
                            {
                                trips.Add(new TripDto()
                                {
                                    Destination = hotel.Location,
                                    BeginDate = context.Saga.BeginDate,
                                    EndDate = context.Saga.EndDate,
                                    HotelId = hotel.HotelId,
                                    HotelName = hotel.Name,
                                    NumberOfPeople = context.Saga.NumberOfPeople,
                                    TransportId = travel.TravelId,
                                });
                            }
                        }
                    }
                    context.Saga.Trips = trips;
                    // TODO save new trips to database
                    context.RespondAsync(new GetOffersEventReply() { Id = context.Saga.OffersId, CorrelationId = context.Saga.CorrelationId }); 
                })
                .Finalize());
        }
    }
}
