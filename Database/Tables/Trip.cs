using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
    public DateOnly BeginDate { get; set; }
    public DateOnly EndDate { get; set; }
}