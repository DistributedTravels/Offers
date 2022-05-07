using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Models.Offers.Dto;

namespace Database.Tables;

public class Trip
{
    [Key] [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public int TransportId { get; set; }
    public string? HotelName { get; set; }
    public int HotelId { get; set; }
    public string? Destination { get; set; }
    public int NumberOfPeople { get; set; }
    public DateTime BeginDate { get; set; }
    public DateTime EndDate { get; set; }

    public void SetFields(TripDto tripDto)
    {
        this.TransportId = tripDto.TransportId;
        this.HotelId = tripDto.HotelId;
        this.Destination = tripDto.Destination;
        this.NumberOfPeople = tripDto.NumberOfPeople;
        this.BeginDate = tripDto.BeginDate;
        this.EndDate = tripDto.EndDate;
    }
}