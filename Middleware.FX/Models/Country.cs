namespace Middleware.FX.Models
{
    public class Country
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public bool IsSynced { get; set; } = false;
    }
}
