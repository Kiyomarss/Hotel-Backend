using Entities;

namespace ContactsManager.Core.DTO;

public class CabinResponse
{
    public Guid Id { get; set; }
    
    public string Name { get; set; }
    
    public int MaxCapacity { get; set; }
    
    public int RegularPrice { get; set; }
    
    public int Discount { get; set; }
    
    public string Description { get; set; }
    
    public string Image { get; set; }
    
    public DateTime CreateAt { get; set; }
}


public static class CabinExtensions
{
    public static CabinResponse ToCabinResponse(this Cabin cabin)
    {
        var base64Image = Convert.ToBase64String(cabin.Image);
        const string mimeType = "image/jpeg";
        var imageUrl = $"data:{mimeType};base64,{base64Image}";

        return new CabinResponse()
        {
            Id = cabin.Id, 
            Name = cabin.Name, 
            MaxCapacity = cabin.MaxCapacity, 
            RegularPrice = cabin.RegularPrice, 
            Discount = cabin.Discount, 
            Image = imageUrl, 
            Description = cabin.Description, 
            CreateAt = cabin.CreateAt
        };
    }
}