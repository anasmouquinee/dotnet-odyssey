using System.ComponentModel.DataAnnotations;

namespace project.Models;

public class CartItem
{
    public int Id { get; set; }
    
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    
    public int TravelPackageId { get; set; }
    public TravelPackage TravelPackage { get; set; } = null!;
    
    [Required]
    public DateTime SelectedStartDate { get; set; }
    
    [Required]
    public DateTime SelectedEndDate { get; set; }
    
    [Range(1, 20)]
    public int NumberOfGuests { get; set; } = 1;
    
    public string? SpecialRequests { get; set; }
    
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    
    public decimal TotalPrice => TravelPackage?.Price * NumberOfGuests ?? 0;
}
