namespace Vending.Machine.Domain.Common
{
    public class Entity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
    }
}