using Microsoft.EntityFrameworkCore;
using Offers.Database;
using Database.Tables;

namespace Offers.Services
{
    public class TripsService : ITripsService
    {
        private readonly OffersContext _context;
        public TripsService(OffersContext context)
        {
            _context = context;
        }
        // TODO trips filtering
        public IEnumerable<Trip> GetTrips()
        {
            return _context.Trips
                .Select(t => t)
                .ToList();
        }

        public IEnumerable<Trip> GetTrips(DateTime beginDate, DateTime endDate, string destination, string departure)
        {
            return _context.Trips
                .Where(t => t.BeginDate == beginDate && t.EndDate == endDate && t.Destination == destination && t.Departure == departure)
                .Select(t => t)
                .ToList();
        }

        public void SaveTrips(IEnumerable<Trip> trips)
        {
            // remove previous trips with same parameters
            var beginDate = trips.First().BeginDate.ToUniversalTime();
            var endDate = trips.First().EndDate.ToUniversalTime();
            var destination = trips.First().Destination;
            var departure = trips.First().Departure;
            var adults = trips.First().Adults;
            var childrenUnder3 = trips.First().ChildrenUnder3;
            var childrenUnder10 = trips.First().ChildrenUnder10;
            var childrenUnder18 = trips.First().ChildrenUnder18;
            var numberOfPeople = trips.First().NumberOfPeople;
            var previousTrips = _context.Trips.Where(t => t.BeginDate == beginDate && t.EndDate == endDate
                && t.Destination.Equals(destination) && t.Adults == adults && t.ChildrenUnder3 == childrenUnder3
                && t.ChildrenUnder10 == childrenUnder10 && t.ChildrenUnder18 == childrenUnder18 && t.NumberOfPeople == numberOfPeople)
                .Select(t => t).ToList();
            _context.Trips.RemoveRange(previousTrips);
            // add new trips
            foreach(var trip in trips)
            {
                trip.BeginDate = trip.BeginDate.ToUniversalTime();
                trip.EndDate = trip.EndDate.ToUniversalTime();
                trip.DepartureTime = trip.DepartureTime.ToUniversalTime();
                _context.Trips.Add(trip);
            }
            _context.SaveChanges();
        }
        public void UpdateTrip(Trip trip)
        {
            var foundTrip = _context.Trips.FirstOrDefault(t => t.Id == trip.Id);
            foundTrip.SetFields(trip);
            _context.Trips.Update(trip);
            _context.SaveChanges();
        }
    }
}
