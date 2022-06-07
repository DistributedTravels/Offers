using Models.Offers;
using MassTransit;
using Offers.Services;
using Database.Tables;

namespace Offers.Consumers
{
    public class ChangesInOffersEventConsumer : IConsumer<ChangesInOffersEvent>
    {
        private readonly ITripsService _tripsService;
        private readonly IOfferChangesService _offerChangesService;
        public ChangesInOffersEventConsumer(ITripsService tripsService, IOfferChangesService offerChangesService)
        {
            _tripsService = tripsService;
            _offerChangesService = offerChangesService;
        }
        public async Task Consume(ConsumeContext<ChangesInOffersEvent> context)
        {
            var offerChanges = new OfferChangeEntity()
            {
                HotelId = context.Message.HotelId,
                HotelName = context.Message.HotelName,
                BigRoomsAvailable = context.Message.BigRoomsAvailable,
                SmallRoomsAvaialable = context.Message.SmallRoomsAvaialable,
                WifiAvailable = context.Message.WifiAvailable,
                BreakfastAvailable = context.Message.BreakfastAvailable,
                HotelPricePerPerson = context.Message.HotelPricePerPerson,
                TransportId = context.Message.TransportId,
                TransportPricePerSeat = context.Message.TransportPricePerSeat,
                PlaneAvailable = context.Message.PlaneAvailable,
                CreateDate = context.Message.CreationDate.ToUniversalTime()
            };
            // changes in hotel
            if (offerChanges.HotelId != -1)
            {
                var affectedTrips = _tripsService.GetTrips()
                    .Where(t => t.BeginDate >= offerChanges.CreateDate.ToUniversalTime() && t.HotelId == offerChanges.HotelId)
                    .Select(t => t)
                    .ToList();
                foreach (var t in affectedTrips)
                {
                    var changedOfferEvent = new ChangedOfferEvent();
                    changedOfferEvent.oldOffer = t.ToTripDto();
                    t.ApplyChanges(offerChanges);
                    changedOfferEvent.newOffer = t.ToTripDto();
                    _tripsService.UpdateTrip(t);
                    await context.Publish(changedOfferEvent);
                }
            }
            // changes in transport
            else
            {
                var affectedTrips = _tripsService.GetTrips()
                    .Where(t => t.BeginDate >= offerChanges.CreateDate && t.TransportId == offerChanges.TransportId)
                    .Select(t => t)
                    .ToList();
                foreach (var t in affectedTrips)
                {
                    var changedOfferEvent = new ChangedOfferEvent();
                    changedOfferEvent.oldOffer = t.ToTripDto();
                    t.ApplyChanges(offerChanges);
                    changedOfferEvent.newOffer = t.ToTripDto();
                    _tripsService.UpdateTrip(t);
                    await context.Publish(changedOfferEvent);
                }
            }
            _offerChangesService.AddChanges(offerChanges);
        }
    }
}
