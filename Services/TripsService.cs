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
                .Where(t => t.BeginDate == beginDate && t.EndDate == endDate && t.Destination == destination && t.Deprature == departure)
                .Select(t => t)
                .ToList();
        }

        public void SaveTrips(IEnumerable<Trip> trips)
        {
            foreach(var trip in trips)
            {
                trip.BeginDate = trip.BeginDate.ToUniversalTime();
                trip.EndDate = trip.EndDate.ToUniversalTime();
                trip.DepartureTime = trip.DepartureTime.ToUniversalTime();
                _context.Trips.Add(trip);
            }
        }
    }
}
