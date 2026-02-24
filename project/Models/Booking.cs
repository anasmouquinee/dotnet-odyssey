using System.ComponentModel.DataAnnotations;

namespace project.Models;

public class Booking
{
    public int Id { get; set; }
    
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    
    public int TravelPackageId { get; set; }
    public TravelPackage TravelPackage { get; set; } = null!;
    
    [Required]
    public DateTime StartDate { get; set; }
    
    [Required]
    public DateTime EndDate { get; set; }
    
    [Range(1, 20)]
    public int NumberOfGuests { get; set; }
    
    public string? SpecialRequests { get; set; }
    
    public decimal TotalPrice { get; set; }
    
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    
    public DateTime BookedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
}

public enum BookingStatus
{
    Pending,
    Confirmed,
    Cancelled,
    Completed
}
