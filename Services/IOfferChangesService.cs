using Database.Tables;

namespace Offers.Services
{
    public interface IOfferChangesService
    {
        public void AddChanges(OfferChangeEntity offerChange);
        public IEnumerable<OfferChangeEntity> GetAllChanges();
    }
}
