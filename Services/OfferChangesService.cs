using Database.Tables;
using Offers.Database;

namespace Offers.Services
{
    public class OfferChangesService : IOfferChangesService
    {
        private readonly OffersContext _context;
        public OfferChangesService(OffersContext context)
        {
            _context = context;
        }
        public void AddChanges(OfferChangeEntity offerChange)
        {
            _context.OfferChanges.Add(offerChange);
            _context.SaveChanges();
        }
        public IEnumerable<OfferChangeEntity> GetAllChanges()
        {
            var changes = _context.OfferChanges.Select(o => o).ToList();
            return changes;
        }
    }
}
