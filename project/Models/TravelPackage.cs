using System.ComponentModel.DataAnnotations;

namespace project.Models;

public class TravelPackage
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Destination { get; set; } = string.Empty;
    
    [Required]
    [StringLength(200)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public decimal Price { get; set; }
    
    [Required]
    public string Season { get; set; } = string.Empty; // spring, summer, autumn, winter
    
    [Required]
    public string ImageUrl { get; set; } = string.Empty;
    
    public DateTime DefaultStartDate { get; set; }
    public DateTime DefaultEndDate { get; set; }
    
    public int DurationDays { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}
