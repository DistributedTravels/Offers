using Microsoft.EntityFrameworkCore;
using Database.Tables;

namespace Offers.Services
{
    public interface ITripsService
    {
        public IEnumerable<Trip> GetTrips();
        public IEnumerable<Trip> GetTrips(DateTime beginDate, DateTime endDate, string destination, string departure);
        public void SaveTrips(IEnumerable<Trip> trips);
        public void UpdateTrip(Trip trip);
    }
}
