using Microsoft.EntityFrameworkCore;
using Database.Tables;

namespace Offers.Services
{
    public interface ITripsService
    {
        public IEnumerable<Trip> GetTrips();
    }
}
