using System.Text.Json;

namespace project.Services;

public interface IDestinationApiService
{
    Task<List<DestinationSuggestion>> SearchDestinationsAsync(string query, string? season = null);
    Task<List<string>> GetDestinationImagesAsync(string destination);
    string ClassifySeasonByMonth(int month);
    string ClassifySeasonByLocation(string destination, double? latitude = null);
}

public class DestinationSuggestion
{
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string SuggestedSeason { get; set; } = string.Empty;
    public List<string> ImageUrls { get; set; } = new();
    public string Description { get; set; } = string.Empty;
}

public class DestinationApiService : IDestinationApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DestinationApiService> _logger;
    
    // Curated destination database with seasons
    private readonly Dictionary<string, List<CuratedDestination>> _curatedDestinations = new()
    {
        ["spring"] = new List<CuratedDestination>
        {
            new() { Name = "Kyoto", Country = "Japan", Description = "Cherry blossom viewing", Latitude = 35.0116, ImageKeyword = "kyoto cherry blossom" },
            new() { Name = "Keukenhof", Country = "Netherlands", Description = "Tulip gardens", Latitude = 52.2697, ImageKeyword = "keukenhof tulips" },
            new() { Name = "Washington D.C.", Country = "USA", Description = "Cherry blossom festival", Latitude = 38.9072, ImageKeyword = "washington dc cherry blossom" },
            new() { Name = "Provence", Country = "France", Description = "Lavender fields", Latitude = 43.9352, ImageKeyword = "provence lavender" },
            new() { Name = "Patagonia", Country = "Chile", Description = "Spring trekking", Latitude = -41.8101, ImageKeyword = "patagonia landscape" },
            new() { Name = "Amsterdam", Country = "Netherlands", Description = "Flower markets", Latitude = 52.3676, ImageKeyword = "amsterdam flowers" },
            new() { Name = "Seville", Country = "Spain", Description = "Orange blossom season", Latitude = 37.3891, ImageKeyword = "seville spring" },
            new() { Name = "New Zealand", Country = "New Zealand", Description = "Southern spring", Latitude = -40.9006, ImageKeyword = "new zealand landscape" }
        },
        ["summer"] = new List<CuratedDestination>
        {
            new() { Name = "Amalfi Coast", Country = "Italy", Description = "Mediterranean paradise", Latitude = 40.6333, ImageKeyword = "amalfi coast" },
            new() { Name = "Santorini", Country = "Greece", Description = "Island escape", Latitude = 36.3932, ImageKeyword = "santorini greece" },
            new() { Name = "Maldives", Country = "Maldives", Description = "Tropical luxury", Latitude = 3.2028, ImageKeyword = "maldives resort" },
            new() { Name = "Bali", Country = "Indonesia", Description = "Island adventures", Latitude = -8.3405, ImageKeyword = "bali indonesia" },
            new() { Name = "Ibiza", Country = "Spain", Description = "Beach parties", Latitude = 38.9067, ImageKeyword = "ibiza beach" },
            new() { Name = "Mykonos", Country = "Greece", Description = "Greek island life", Latitude = 37.4467, ImageKeyword = "mykonos greece" },
            new() { Name = "Côte d'Azur", Country = "France", Description = "French Riviera", Latitude = 43.7102, ImageKeyword = "french riviera" },
            new() { Name = "Hawaii", Country = "USA", Description = "Tropical paradise", Latitude = 19.8968, ImageKeyword = "hawaii beach" }
        },
        ["autumn"] = new List<CuratedDestination>
        {
            new() { Name = "Vermont", Country = "USA", Description = "Fall foliage", Latitude = 44.5588, ImageKeyword = "vermont fall foliage" },
            new() { Name = "Bavaria", Country = "Germany", Description = "Oktoberfest & castles", Latitude = 48.7904, ImageKeyword = "bavaria autumn" },
            new() { Name = "Kyoto", Country = "Japan", Description = "Momiji maple viewing", Latitude = 35.0116, ImageKeyword = "kyoto autumn maple" },
            new() { Name = "Tuscany", Country = "Italy", Description = "Harvest season", Latitude = 43.7711, ImageKeyword = "tuscany autumn" },
            new() { Name = "New England", Country = "USA", Description = "Fall colors", Latitude = 42.3601, ImageKeyword = "new england fall" },
            new() { Name = "Scottish Highlands", Country = "UK", Description = "Autumn landscapes", Latitude = 57.1497, ImageKeyword = "scottish highlands autumn" },
            new() { Name = "Quebec", Country = "Canada", Description = "Maple season", Latitude = 46.8139, ImageKeyword = "quebec autumn" },
            new() { Name = "Napa Valley", Country = "USA", Description = "Wine harvest", Latitude = 38.2975, ImageKeyword = "napa valley autumn" }
        },
        ["winter"] = new List<CuratedDestination>
        {
            new() { Name = "Lapland", Country = "Finland", Description = "Northern lights & snow", Latitude = 68.0000, ImageKeyword = "lapland finland aurora" },
            new() { Name = "Zermatt", Country = "Switzerland", Description = "Alpine skiing", Latitude = 46.0207, ImageKeyword = "zermatt matterhorn" },
            new() { Name = "Aspen", Country = "USA", Description = "Luxury ski resort", Latitude = 39.1911, ImageKeyword = "aspen colorado ski" },
            new() { Name = "Reykjavik", Country = "Iceland", Description = "Northern lights", Latitude = 64.1466, ImageKeyword = "iceland northern lights" },
            new() { Name = "Queenstown", Country = "New Zealand", Description = "Winter sports", Latitude = -45.0312, ImageKeyword = "queenstown new zealand" },
            new() { Name = "Hokkaido", Country = "Japan", Description = "Powder snow", Latitude = 43.0642, ImageKeyword = "hokkaido snow" },
            new() { Name = "Chamonix", Country = "France", Description = "Mont Blanc skiing", Latitude = 45.9237, ImageKeyword = "chamonix mont blanc" },
            new() { Name = "Tromsø", Country = "Norway", Description = "Arctic adventures", Latitude = 69.6492, ImageKeyword = "tromso norway aurora" }
        }
    };
    
    public DestinationApiService(HttpClient httpClient, ILogger<DestinationApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }
    
    public async Task<List<DestinationSuggestion>> SearchDestinationsAsync(string query, string? season = null)
    {
        var suggestions = new List<DestinationSuggestion>();
        
        try
        {
            // First, search our curated database
            var seasonsToSearch = string.IsNullOrEmpty(season) 
                ? _curatedDestinations.Keys.ToList() 
                : new List<string> { season.ToLower() };
            
            foreach (var s in seasonsToSearch)
            {
                if (_curatedDestinations.TryGetValue(s, out var destinations))
                {
                    var matches = destinations
                        .Where(d => string.IsNullOrEmpty(query) || 
                                   d.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                                   d.Country.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                                   d.Description.Contains(query, StringComparison.OrdinalIgnoreCase))
                        .Select(d => new DestinationSuggestion
                        {
                            Name = d.Name,
                            Country = d.Country,
                            FullName = $"{d.Name}, {d.Country}",
                            Latitude = d.Latitude,
                            SuggestedSeason = s,
                            Description = d.Description,
                            ImageUrls = GetUnsplashImages(d.ImageKeyword)
                        });
                    
                    suggestions.AddRange(matches);
                }
            }
            
            // If we have a query but few results, try external API (OpenStreetMap Nominatim)
            if (!string.IsNullOrEmpty(query) && query.Length >= 2 && suggestions.Count < 5)
            {
                try
                {
                    var externalResults = await SearchNominatimAsync(query);
                    foreach (var result in externalResults)
                    {
                        if (!suggestions.Any(s => s.Name.Equals(result.Name, StringComparison.OrdinalIgnoreCase)))
                        {
                            result.SuggestedSeason = ClassifySeasonByLocation(result.Name, result.Latitude);
                            result.ImageUrls = GetUnsplashImages(result.Name);
                            suggestions.Add(result);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "External API search failed, using curated results only");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching destinations for query: {Query}", query);
        }
        
        return suggestions.Take(20).ToList();
    }
    
    private async Task<List<DestinationSuggestion>> SearchNominatimAsync(string query)
    {
        var results = new List<DestinationSuggestion>();
        
        try
        {
            // Use OpenStreetMap Nominatim API (free, no API key required)
            var url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(query)}&format=json&limit=10&featuretype=city";
            
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "OdysseyTravelApp/1.0");
            
            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var places = JsonSerializer.Deserialize<List<NominatimPlace>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                if (places != null)
                {
                    foreach (var place in places)
                    {
                        var parts = place.DisplayName?.Split(',') ?? Array.Empty<string>();
                        var name = parts.FirstOrDefault()?.Trim() ?? query;
                        var country = parts.LastOrDefault()?.Trim() ?? "";
                        
                        results.Add(new DestinationSuggestion
                        {
                            Name = name,
                            Country = country,
                            FullName = place.DisplayName ?? $"{name}, {country}",
                            Latitude = double.TryParse(place.Lat, out var lat) ? lat : 0,
                            Longitude = double.TryParse(place.Lon, out var lon) ? lon : 0,
                            Description = $"Explore {name}"
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to search Nominatim API");
        }
        
        return results;
    }
    
    public async Task<List<string>> GetDestinationImagesAsync(string destination)
    {
        // Return Unsplash source URLs (free to use)
        return await Task.FromResult(GetUnsplashImages(destination));
    }
    
    private List<string> GetUnsplashImages(string keyword)
    {
        // Map specific destinations to curated Unsplash image IDs for better quality
        var imageMap = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
        {
            // Spring destinations
            ["kyoto cherry blossom"] = new() {
                "https://images.unsplash.com/photo-1493976040374-85c8e12f0c0e?w=800",
                "https://images.unsplash.com/photo-1545569341-9eb8b30979d9?w=800",
                "https://images.unsplash.com/photo-1528360983277-13d401cdc186?w=800",
                "https://images.unsplash.com/photo-1524413840807-0c3cb6fa808d?w=800"
            },
            ["keukenhof tulips"] = new() {
                "https://images.unsplash.com/photo-1589994160839-163cd867cfe8?w=800",
                "https://images.unsplash.com/photo-1520219306100-ec4afeeefe58?w=800",
                "https://images.unsplash.com/photo-1518709779341-56cf4535e94b?w=800",
                "https://images.unsplash.com/photo-1490750967868-88aa4486c946?w=800"
            },
            ["washington dc cherry blossom"] = new() {
                "https://images.unsplash.com/photo-1617581629397-a72507c3de9e?w=800",
                "https://images.unsplash.com/photo-1558005530-a7958896ec60?w=800",
                "https://images.unsplash.com/photo-1612222869049-d8ec83637a3c?w=800",
                "https://images.unsplash.com/photo-1586103516265-d0c41b7eaec7?w=800"
            },
            ["provence lavender"] = new() {
                "https://images.unsplash.com/photo-1499002238440-d264edd596ec?w=800",
                "https://images.unsplash.com/photo-1532099876310-5ac6f18b0df6?w=800",
                "https://images.unsplash.com/photo-1595069906974-f8ae7ffc3e5b?w=800",
                "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=800"
            },
            
            // Summer destinations
            ["amalfi coast"] = new() {
                "https://images.unsplash.com/photo-1534008897995-27a23e859048?w=800",
                "https://images.unsplash.com/photo-1612698093158-e07ac200d44e?w=800",
                "https://images.unsplash.com/photo-1516483638261-f4dbaf036963?w=800",
                "https://images.unsplash.com/photo-1506929562872-bb421503ef21?w=800"
            },
            ["santorini greece"] = new() {
                "https://images.unsplash.com/photo-1570077188670-e3a8d69ac5ff?w=800",
                "https://images.unsplash.com/photo-1613395877344-13d4a8e0d49e?w=800",
                "https://images.unsplash.com/photo-1533105079780-92b9be482077?w=800",
                "https://images.unsplash.com/photo-1504512485720-7d83a16ee930?w=800"
            },
            ["maldives resort"] = new() {
                "https://images.unsplash.com/photo-1514282401047-d79a71a590e8?w=800",
                "https://images.unsplash.com/photo-1573843981267-be1999ff37cd?w=800",
                "https://images.unsplash.com/photo-1540202404-a2f29016b523?w=800",
                "https://images.unsplash.com/photo-1512100356356-de1b84283e18?w=800"
            },
            ["bali indonesia"] = new() {
                "https://images.unsplash.com/photo-1537996194471-e657df975ab4?w=800",
                "https://images.unsplash.com/photo-1555400038-63f5ba517a47?w=800",
                "https://images.unsplash.com/photo-1518548419970-58e3b4079ab2?w=800",
                "https://images.unsplash.com/photo-1544644181-1484b3fdfc62?w=800"
            },
            ["ibiza beach"] = new() {
                "https://images.unsplash.com/photo-1539635278303-d4002c07eae3?w=800",
                "https://images.unsplash.com/photo-1507525428034-b723cf961d3e?w=800",
                "https://images.unsplash.com/photo-1520454974749-611b7248ffdb?w=800",
                "https://images.unsplash.com/photo-1519046904884-53103b34b206?w=800"
            },
            ["hawaii beach"] = new() {
                "https://images.unsplash.com/photo-1507876466758-bc54f384809c?w=800",
                "https://images.unsplash.com/photo-1559494007-9f5847c49d94?w=800",
                "https://images.unsplash.com/photo-1542259009477-d625272157b7?w=800",
                "https://images.unsplash.com/photo-1505852679233-d9fd70aff56d?w=800"
            },
            
            // Autumn destinations
            ["vermont fall"] = new() {
                "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=800",
                "https://images.unsplash.com/photo-1476820865390-c52aeebb9891?w=800",
                "https://images.unsplash.com/photo-1508193638397-1c4234db14d8?w=800",
                "https://images.unsplash.com/photo-1509316785289-025f5b846b35?w=800"
            },
            ["bavaria autumn"] = new() {
                "https://images.unsplash.com/photo-1527004013197-933c4bb611b3?w=800",
                "https://images.unsplash.com/photo-1516483638261-f4dbaf036963?w=800",
                "https://images.unsplash.com/photo-1467269204594-9661b134dd2b?w=800",
                "https://images.unsplash.com/photo-1504280390367-361c6d9f38f4?w=800"
            },
            ["tuscany vineyard"] = new() {
                "https://images.unsplash.com/photo-1523528283115-9bf9b1699245?w=800",
                "https://images.unsplash.com/photo-1534445867742-43195f401b6c?w=800",
                "https://images.unsplash.com/photo-1515542622106-78bda8ba0e5b?w=800",
                "https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=800"
            },
            ["new england fall"] = new() {
                "https://images.unsplash.com/photo-1508193638397-1c4234db14d8?w=800",
                "https://images.unsplash.com/photo-1476820865390-c52aeebb9891?w=800",
                "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=800",
                "https://images.unsplash.com/photo-1509316785289-025f5b846b35?w=800"
            },
            ["napa valley"] = new() {
                "https://images.unsplash.com/photo-1506377247377-2a5b3b417ebb?w=800",
                "https://images.unsplash.com/photo-1560493676-04071c5f467b?w=800",
                "https://images.unsplash.com/photo-1510812431401-41d2bd2722f3?w=800",
                "https://images.unsplash.com/photo-1474722883778-792e7990302f?w=800"
            },
            
            // Winter destinations
            ["lapland finland"] = new() {
                "https://images.unsplash.com/photo-1531366936337-7c912a4589a7?w=800",
                "https://images.unsplash.com/photo-1483921020237-2ff51e8e4b22?w=800",
                "https://images.unsplash.com/photo-1519681393784-d120267933ba?w=800",
                "https://images.unsplash.com/photo-1579003593419-98f949b9398f?w=800"
            },
            ["zermatt switzerland"] = new() {
                "https://images.unsplash.com/photo-1531310197839-ccf54634509e?w=800",
                "https://images.unsplash.com/photo-1520962922320-2038eebab146?w=800",
                "https://images.unsplash.com/photo-1548802673-380ab8ebc7b7?w=800",
                "https://images.unsplash.com/photo-1491555103944-7c647fd857e6?w=800"
            },
            ["aspen colorado ski"] = new() {
                "https://images.unsplash.com/photo-1551524559-8af4e6624178?w=800",
                "https://images.unsplash.com/photo-1605540436563-5bca919ae766?w=800",
                "https://images.unsplash.com/photo-1419242902214-272b3f66ee7a?w=800",
                "https://images.unsplash.com/photo-1516450360452-9312f5e86fc7?w=800"
            },
            ["iceland northern lights"] = new() {
                "https://images.unsplash.com/photo-1531366936337-7c912a4589a7?w=800",
                "https://images.unsplash.com/photo-1579033461380-adb47c3eb938?w=800",
                "https://images.unsplash.com/photo-1504893524553-b855bce32c67?w=800",
                "https://images.unsplash.com/photo-1520769945061-0a448c463865?w=800"
            },
            ["hokkaido snow"] = new() {
                "https://images.unsplash.com/photo-1491002052546-bf38f186af56?w=800",
                "https://images.unsplash.com/photo-1517299321609-52687d1bc55a?w=800",
                "https://images.unsplash.com/photo-1542640244-7e672d6cef4e?w=800",
                "https://images.unsplash.com/photo-1478436127897-769e1b3f0f36?w=800"
            },
            ["chamonix mont blanc"] = new() {
                "https://images.unsplash.com/photo-1522199710521-72d69614c702?w=800",
                "https://images.unsplash.com/photo-1520962922320-2038eebab146?w=800",
                "https://images.unsplash.com/photo-1548802673-380ab8ebc7b7?w=800",
                "https://images.unsplash.com/photo-1491555103944-7c647fd857e6?w=800"
            },
            ["tromso norway aurora"] = new() {
                "https://images.unsplash.com/photo-1531366936337-7c912a4589a7?w=800",
                "https://images.unsplash.com/photo-1483921020237-2ff51e8e4b22?w=800",
                "https://images.unsplash.com/photo-1579033461380-adb47c3eb938?w=800",
                "https://images.unsplash.com/photo-1520769945061-0a448c463865?w=800"
            }
        };
        
        // Try to find a match
        var keywordLower = keyword.ToLower();
        foreach (var kvp in imageMap)
        {
            if (keywordLower.Contains(kvp.Key.Split(' ')[0]) || kvp.Key.Contains(keywordLower.Split(' ')[0]))
            {
                return kvp.Value;
            }
        }
        
        // Fallback: Generate Unsplash search URLs with better parameters
        var searchQuery = Uri.EscapeDataString(keyword);
        return new List<string>
        {
            $"https://images.unsplash.com/photo-1500530855697-b586d89ba3ee?w=800&q=80",
            $"https://images.unsplash.com/photo-1476514525535-07fb3b4ae5f1?w=800&q=80",
            $"https://images.unsplash.com/photo-1530789253388-582c481c54b0?w=800&q=80",
            $"https://images.unsplash.com/photo-1469854523086-cc02fe5d8800?w=800&q=80"
        };
    }
    
    public string ClassifySeasonByMonth(int month)
    {
        return month switch
        {
            3 or 4 or 5 => "spring",
            6 or 7 or 8 => "summer",
            9 or 10 or 11 => "autumn",
            12 or 1 or 2 => "winter",
            _ => "summer"
        };
    }
    
    public string ClassifySeasonByLocation(string destination, double? latitude = null)
    {
        // Southern hemisphere has opposite seasons
        var isSouthern = latitude.HasValue && latitude.Value < 0;
        
        // Check for known seasonal destinations
        var lowerDest = destination.ToLower();
        
        // Winter destinations (skiing, snow)
        if (lowerDest.Contains("ski") || lowerDest.Contains("alps") || 
            lowerDest.Contains("aspen") || lowerDest.Contains("zermatt") ||
            lowerDest.Contains("lapland") || lowerDest.Contains("iceland"))
        {
            return isSouthern ? "summer" : "winter";
        }
        
        // Summer/Beach destinations
        if (lowerDest.Contains("beach") || lowerDest.Contains("coast") || 
            lowerDest.Contains("island") || lowerDest.Contains("maldives") ||
            lowerDest.Contains("bali") || lowerDest.Contains("hawaii"))
        {
            return "summer";
        }
        
        // Spring destinations (flowers, cherry blossoms)
        if (lowerDest.Contains("cherry") || lowerDest.Contains("tulip") ||
            lowerDest.Contains("blossom") || lowerDest.Contains("garden"))
        {
            return "spring";
        }
        
        // Autumn destinations (foliage, harvest)
        if (lowerDest.Contains("foliage") || lowerDest.Contains("harvest") ||
            lowerDest.Contains("vermont") || lowerDest.Contains("vineyard"))
        {
            return "autumn";
        }
        
        // Default based on latitude (tropical = summer, high lat = varies)
        if (latitude.HasValue)
        {
            var absLat = Math.Abs(latitude.Value);
            if (absLat < 23.5) return "summer"; // Tropical
            if (absLat > 60) return "winter"; // Arctic/Antarctic
        }
        
        return "summer"; // Default
    }
}

// Helper classes for API responses
public class NominatimPlace
{
    public string? DisplayName { get; set; }
    public string? Lat { get; set; }
    public string? Lon { get; set; }
    public string? Type { get; set; }
}

public class CuratedDestination
{
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public string ImageKeyword { get; set; } = string.Empty;
}
