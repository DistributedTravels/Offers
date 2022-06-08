using MassTransit;
using Models.Offers;
using Models.Offers.Dto;
using Models.Transport;
using Models.Hotels;
using Database.Tables;
using Offers.Database;
using Offers.Services;
using Microsoft.EntityFrameworkCore;

namespace Offers.Orchestration
{
    public class OfferStateMachine : MassTransitStateMachine<StatefulOffer>
    {

        public State ReceivedTravels { get; set; }
        public State AwaitingHotelsAndTravels { get; set; }
        public State AwaitingTravels { get; set; }
        public State AwaitingHotels { get; set; }
        public State AwaitingTrips { get; set; }
        public State ReceivedHotelsAndTravels { get; set; }
        public Event<GetOffersEvent> GetOffersEvent { get; set; }
        public Event<GetAvailableTravelsReplyEvent> GetAvailableTravelsReplyEvent { get; set; }
        public Event<GetTripsFromDatabaseReplyEvent> GetTripsFromDatabaseReplyEvent { get; set; }
        public Event<GetHotelsEventReply> GetHotelsEventReply { get; set; }
        public Event<CheckGetOffersEvent> CheckGetOffersEvent { get; set; }
        // TODO events for getting hotels

        public OfferStateMachine()
        {
            InstanceState(x => x.CurrentState, ReceivedTravels, AwaitingHotelsAndTravels, AwaitingTravels, AwaitingHotels, AwaitingTrips, ReceivedHotelsAndTravels);
            Event(() => GetOffersEvent, x => { x.CorrelateById(context => context.Message.CorrelationId); x.SelectId(context => context.Message.CorrelationId); });
            Event(() => GetTripsFromDatabaseReplyEvent, x => { x.CorrelateById(context => context.Message.CorrelationId); });
            Event(() => GetAvailableTravelsReplyEvent, x => { x.CorrelateById(context => context.Message.CorrelationId); });
            Event(() => GetHotelsEventReply, x => { x.CorrelateById(context => context.Message.CorrelationId); });
            Event(() => CheckGetOffersEvent, x => { x.CorrelateById(context => context.Message.CorrelationId); });
            // TODO check if IDs are correct
            // I might have made some mistakes...

            //Initially wait for GetOffersEvent
            Initially(
                When(GetOffersEvent)
                    .Then(async context =>
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
                        context.Saga.Destination = payload.Message.Destination;
                        context.Saga.Departure = payload.Message.Departure;
                        context.Saga.Adults = payload.Message.Adults;
                        context.Saga.ChildrenUnder3 = payload.Message.ChildrenUnder3;
                        context.Saga.ChildrenUnder10 = payload.Message.ChildrenUnder10;
                        context.Saga.ChildrenUnder18 = payload.Message.ChildrenUnder18;
                        // TODO get offers from database (if there are any)
                        var trips = new List<TripDto>();
                        var hotels = new List<HotelDto>();
                        var travels = new List<TravelDto>();
                        context.Saga.Hotels = hotels;
                        context.Saga.Travels = travels;
                        context.Saga.Trips = trips;
                        Console.WriteLine("Received request for trips");
                        context.Saga.RequestUri = payload.ResponseAddress;
                    })
                    //.RespondAsync(context => context.Init<GetOffersReplyEvent>(new GetOffersReplyEvent() { CorrelationId = context.Saga.CorrelationId, Trips = context.Saga.Trips }))
                    .PublishAsync(context => context.Init<GetTripsFromDatabaseEvent>(new GetTripsFromDatabaseEvent() { Destination = context.Saga.Destination,
                        BeginDate = context.Saga.BeginDate, EndDate = context.Saga.EndDate, NumberOfPeople = context.Saga.NumberOfPeople,
                        Departure = context.Saga.Departure, Adults = context.Saga.Adults, ChildrenUnder3 = context.Saga.ChildrenUnder3,
                        ChildrenUnder10 = context.Saga.ChildrenUnder10, ChildrenUnder18 = context.Saga.ChildrenUnder18, Id = context.Saga.OffersId, CorrelationId = context.Saga.CorrelationId }))
                    .TransitionTo(AwaitingTrips)
                    );

            WhenEnter(AwaitingTrips, binder => binder
            .Then(context => { Console.WriteLine("ENTERED AWAITING TRIPS"); }));

            // ask self for trips in database (that's why we used additional consumer before)
            During(AwaitingTrips,
                When(GetTripsFromDatabaseReplyEvent)
                    .Then(context =>
                    {
                        if (!context.TryGetPayload(out SagaConsumeContext<StatefulOffer, GetTripsFromDatabaseReplyEvent> payload))
                        {
                            throw new Exception("Unable to retrieve payload with requested trips");
                        }
                        context.Saga.Trips = payload.Message.Trips;
                    })
                    .IfElse(x => x.Saga.Trips.Count() < 3,
                        x => x.PublishAsync(context => context.Init<GetAvailableTravelsEvent>(new GetAvailableTravelsEvent(
                                departureTime: context.Saga.BeginDate,
                                freeSeats: context.Saga.NumberOfPeople,
                                source: context.Saga.Departure == "gdziekolwiek" ? "any" : context.Saga.Departure,
                                destination: context.Saga.Destination == "gdziekolwiek" ? "any" : context.Saga.Destination)
                                {
                                    Id = context.Saga.OffersId, 
                                    CorrelationId = context.Saga.CorrelationId 
                                }))
                              .PublishAsync(context => context.Init<GetHotelsEvent>(new GetHotelsEvent() 
                              { 
                                  Country = context.Saga.Destination == "gdziekolwiek" ? "any" : context.Saga.Destination,
                                  BeginDate = context.Saga.BeginDate.ToUniversalTime(),
                                  EndDate = context.Saga.EndDate.ToUniversalTime(),
                                  CorrelationId = context.Saga.CorrelationId, 
                                  Id = context.Saga.OffersId 
                              }))
                              .TransitionTo(AwaitingHotelsAndTravels),
                        x => x.TransitionTo(ReceivedHotelsAndTravels)));

            WhenEnter(AwaitingHotelsAndTravels, binder => binder
            .Then(context => { Console.WriteLine("ENTERED AWAITING HOTELS AND TRAVELS"); }));

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
                    .TransitionTo(AwaitingHotels),
                
                When(GetHotelsEventReply)
                    .Then(context =>
                    {
                        if (!context.TryGetPayload(out SagaConsumeContext<StatefulOffer, GetHotelsEventReply> payload))
                        {
                            throw new Exception("Unable to retrieve payload with travels");
                        }
                        var hotelsReceived = payload.Message.HotelItems;
                        var hotels = new List<HotelDto>();
                        foreach(var hotel in hotelsReceived)
                        {
                            hotels.Add(new HotelDto(hotel));
                        }
                        context.Saga.Hotels = hotels;
                        Console.WriteLine("Received hotels from Hotels");
                    })
                    .TransitionTo(AwaitingTravels));

            WhenEnter(AwaitingHotels, binder => binder
            .Then(context => { Console.WriteLine("ENTERED AWAITING HOTELS"); }));

            During(AwaitingHotels,
                When(GetHotelsEventReply)
                    .Then(context =>
                    {
                        if (!context.TryGetPayload(out SagaConsumeContext<StatefulOffer, GetHotelsEventReply> payload))
                        {
                            throw new Exception("Unable to retrieve payload with travels");
                        }
                        var hotelsReceived = payload.Message.HotelItems;
                        var hotels = new List<HotelDto>();
                        foreach (var hotel in hotelsReceived)
                        {
                            hotels.Add(new HotelDto(hotel));
                        }
                        context.Saga.Hotels = hotels;
                        Console.WriteLine("Received hotels from Hotels");
                    })
                    .TransitionTo(ReceivedHotelsAndTravels));

            During(AwaitingTravels,
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

            WhenEnter(ReceivedHotelsAndTravels, binder => binder
                .Then(context =>
                {
                    if (context.Saga.Hotels.Count() > 0)
                    {
                        Console.WriteLine("ENTERED FINAL STEP");
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
                                        Departure = context.Saga.Departure,
                                        BeginDate = context.Saga.BeginDate,
                                        DepartureTime = travel.DepartureTime,
                                        EndDate = context.Saga.EndDate,
                                        HotelId = hotel.HotelItemId,
                                        HotelName = hotel.HotelName,
                                        NumberOfPeople = context.Saga.NumberOfPeople,
                                        Adults = context.Saga.Adults,
                                        ChildrenUnder3 = context.Saga.ChildrenUnder3,
                                        ChildrenUnder10 = context.Saga.ChildrenUnder10,
                                        ChildrenUnder18 = context.Saga.ChildrenUnder18,
                                        TransportId = travel.TravelId,
                                        PlaneAvailable = travel.AvailableSeats > 0,
                                        WifiAvailable = hotel.WifiAvailable,
                                        BreakfastAvailable = hotel.BreakfastAvailable,
                                        OfferAvailable = hotel.BigRoomsAvailable * 4 + hotel.SmallRoomsAvailable * 2 > context.Saga.NumberOfPeople,
                                        HotelPrice = context.Saga.NumberOfPeople * hotel.PricePerPersonPerNight * (context.Saga.EndDate - context.Saga.BeginDate).Days,
                                        BreakfastPrice = context.Saga.NumberOfPeople * hotel.BreakfastPrice * (context.Saga.EndDate - context.Saga.BeginDate).Days,
                                        TransportPricePerSeat = travel.PricePerSeat,
                                        TotalPrice = context.Saga.NumberOfPeople * hotel.PricePerPersonPerNight * (context.Saga.EndDate - context.Saga.BeginDate).Days
                                    });
                                }
                            }
                        }
                        context.Saga.Trips = trips;
                    }
                    // TODO save new trips to database
                    context.Send(context.Saga.RequestUri, new GetOffersReplyEvent() { CorrelationId = context.Saga.CorrelationId, Trips = context.Saga.Trips });
                })
                .PublishAsync(context => context.Init<SaveOffersToDatabaseEvent>(
                    new SaveOffersToDatabaseEvent()
                    {
                        Trips = context.Saga.Trips,
                        CorrelationId = context.Saga.CorrelationId
                    })));
        }
    }
}
