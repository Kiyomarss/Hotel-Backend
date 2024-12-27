namespace ServiceContracts
{
 public interface ICabinsDeleterService
 {
  Task<bool> DeleteCabin(Guid cabinId);
 }
}
