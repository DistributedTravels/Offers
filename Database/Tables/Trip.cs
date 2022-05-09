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
    public string Deprature { get; set; }
    public int NumberOfPeople { get; set; }
    public int Adults { get; set; }
    public int ChildrenUnder3 { get; set; }
    public int ChildrenUnder10 { get; set; }
    public int ChildrenUnder18 { get; set; }
    public DateTime BeginDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime DepartureTime { get; set; }

    public void SetFields(TripDto tripDto)
    {
        this.TransportId = tripDto.TransportId;
        this.HotelId = tripDto.HotelId;
        this.Destination = tripDto.Destination;
        this.NumberOfPeople = tripDto.NumberOfPeople;
        this.BeginDate = tripDto.BeginDate;
        this.EndDate = tripDto.EndDate;
        this.Adults = tripDto.Adults;
        this.ChildrenUnder3 = tripDto.ChildrenUnder3;
        this.ChildrenUnder10 = tripDto.ChildrenUnder10;
        this.ChildrenUnder18 = tripDto.ChildrenUnder18;
        this.Deprature = tripDto.Departure;
        this.TripId = tripDto.TripId;
        this.DepartureTime = tripDto.DepartureTime;
    }

    public TripDto ToTripDto()
    {
        return new TripDto() { TransportId = this.TransportId, HotelId = this.HotelId, 
            Destination = this.Destination, NumberOfPeople = this.NumberOfPeople, 
            BeginDate = this.BeginDate, EndDate = this.EndDate, HotelName = this.HotelName, 
            Id = this.Id, Adults = this.Adults, ChildrenUnder3 = this.ChildrenUnder3,
            ChildrenUnder10 = this.ChildrenUnder10, ChildrenUnder18 = this.ChildrenUnder18,
            Departure = this.Deprature, TripId = this.TripId, DepartureTime = this.DepartureTime};
    }
}