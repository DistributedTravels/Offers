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
    }
}
