using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Models.Offers.Dto;

namespace Database.Tables;

public class Trip
{
    [Key] [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public Guid TripId { get; set; }
    public int TransportId { get; set; }
    public string HotelName { get; set; }
    public int HotelId { get; set; }
    public string Destination { get; set; }
    public string Departure { get; set; }
    public int NumberOfPeople { get; set; }
    public int Adults { get; set; }
    public int ChildrenUnder3 { get; set; }
    public int ChildrenUnder10 { get; set; }
    public int ChildrenUnder18 { get; set; }
    public DateTime BeginDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime DepartureTime { get; set; }
    public double TransportPricePerSeat { get; set; }
    public double HotelPrice { get; set; }
    public double TotalPrice { get; set; }
    public bool WifiAvailable { get; set; }
    public bool BreakfastAvailable { get; set; }
    public int BigRoomsAvailable { get; set; }
    public int SmallRoomsAvailable { get; set; }
    public bool OfferAvailable { get; set; }
    public bool PlaneAvailable { get; set; }

    public void SetFields(TripDto tripDto)
    {
        this.TransportId = tripDto.TransportId;
        this.HotelId = tripDto.HotelId;
        this.HotelName = tripDto.HotelName;
        this.Destination = tripDto.Destination;
        this.NumberOfPeople = tripDto.NumberOfPeople;
        this.BeginDate = tripDto.BeginDate;
        this.EndDate = tripDto.EndDate;
        this.Adults = tripDto.Adults;
        this.ChildrenUnder3 = tripDto.ChildrenUnder3;
        this.ChildrenUnder10 = tripDto.ChildrenUnder10;
        this.ChildrenUnder18 = tripDto.ChildrenUnder18;
        this.Departure = tripDto.Departure;
        this.TripId = tripDto.TripId;
        this.DepartureTime = tripDto.DepartureTime;
        this.TransportPricePerSeat = tripDto.TransportPricePerSeat;
        this.PlaneAvailable = tripDto.PlaneAvailable;
        this.HotelPrice = tripDto.HotelPrice;
        this.TotalPrice = tripDto.TotalPrice;
        this.WifiAvailable = tripDto.WifiAvailable;
        this.BreakfastAvailable = tripDto.BreakfastAvailable;
        this.BigRoomsAvailable = tripDto.BigRoomsAvailable;
        this.SmallRoomsAvailable = tripDto.SmallRoomsAvailable;
        this.OfferAvailable = tripDto.OfferAvailable;
        
    }
    public void SetFields(Trip trip)
    {
        this.TransportId = trip.TransportId;
        this.HotelId = trip.HotelId;
        this.HotelName = trip.HotelName;
        this.Destination = trip.Destination;
        this.NumberOfPeople = trip.NumberOfPeople;
        this.BeginDate = trip.BeginDate;
        this.EndDate = trip.EndDate;
        this.Adults = trip.Adults;
        this.ChildrenUnder3 = trip.ChildrenUnder3;
        this.ChildrenUnder10 = trip.ChildrenUnder10;
        this.ChildrenUnder18 = trip.ChildrenUnder18;
        this.Departure = trip.Departure;
        this.TripId = trip.TripId;
        this.DepartureTime = trip.DepartureTime;
        this.TransportPricePerSeat = trip.TransportPricePerSeat;
        this.PlaneAvailable = trip.PlaneAvailable;
        this.HotelPrice = trip.HotelPrice;
        this.TotalPrice = trip.TotalPrice;
        this.WifiAvailable = trip.WifiAvailable;
        this.BreakfastAvailable = trip.BreakfastAvailable;
        this.BigRoomsAvailable = trip.BigRoomsAvailable;
        this.SmallRoomsAvailable = trip.SmallRoomsAvailable;
        this.OfferAvailable = trip.OfferAvailable;
    }

    public TripDto ToTripDto()
    {
        return new TripDto() 
        { 
            TransportId = this.TransportId, 
            HotelId = this.HotelId, 
            Destination = this.Destination, 
            NumberOfPeople = this.NumberOfPeople, 
            BeginDate = this.BeginDate, 
            EndDate = this.EndDate,
            HotelName = this.HotelName, 
            Id = this.Id, 
            Adults = this.Adults,
            ChildrenUnder3 = this.ChildrenUnder3,
            ChildrenUnder10 = this.ChildrenUnder10, 
            ChildrenUnder18 = this.ChildrenUnder18,
            Departure = this.Departure, 
            TripId = this.TripId,
            DepartureTime = this.DepartureTime,
            TransportPricePerSeat = this.TransportPricePerSeat,
            HotelPrice = this.HotelPrice,
            TotalPrice = this.TotalPrice,
            WifiAvailable = this.WifiAvailable,
            BreakfastAvailable = this.BreakfastAvailable,
            BigRoomsAvailable = this.BigRoomsAvailable,
            SmallRoomsAvailable = this.SmallRoomsAvailable,
            OfferAvailable = this.OfferAvailable,
            PlaneAvailable = this.OfferAvailable
        };
    }

    public void ApplyChanges(OfferChangeEntity offerChange)
    {
        // change from hotels
        if (offerChange.HotelId != -1)
        {
            this.HotelName = offerChange.HotelName;
            this.HotelPrice = offerChange.HotelPricePerPerson * this.NumberOfPeople * (this.EndDate - this.BeginDate).Days;
            this.OfferAvailable = offerChange.BigRoomsAvailable * 4 + offerChange.SmallRoomsAvaialable * 2 >= this.NumberOfPeople;
            this.BigRoomsAvailable = offerChange.BigRoomsAvailable;
            this.SmallRoomsAvailable = offerChange.SmallRoomsAvaialable;
            this.WifiAvailable = offerChange.WifiAvailable;
            this.BreakfastAvailable = offerChange.BreakfastAvailable;
        }
        // change from transport
        else
        {
            this.TransportPricePerSeat = offerChange.TransportPricePerSeat;
            this.PlaneAvailable = offerChange.PlaneAvailable;
        }
        this.TotalPrice = this.HotelPrice;
    }
}