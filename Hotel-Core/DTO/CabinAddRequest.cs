using Entities;

namespace ContactsManager.Core.DTO;

public class CabinUpsertRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    
    public int MaxCapacity { get; set; }
    
    public int RegularPrice { get; set; }
    
    public int Discount { get; set; }
    
    public string Description { get; private set; }
    
    public byte[] Image { get; set; }
    
    public Cabin ToCabin()
    {
        return new Cabin
        {
            Name = Name,
            MaxCapacity = MaxCapacity,
            RegularPrice = RegularPrice,
            Discount = Discount,
            Image = Image
        };
    }
}