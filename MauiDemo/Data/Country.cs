using SQLite;

namespace MauiDemo.Data
{
    public class Country
    {
        [PrimaryKey]
        public Guid Id { get; set; }

        public string Name { get; set; }

        public bool IsSynced { get; set; } = false;
    }
}
