namespace EcommerceAPI.Domain.Entities
{
    public class ServiceClient
    {
        public int Id { get; set; }
        public Guid Guid { get; set; } = Guid.NewGuid();
        public string ClientId { get; set; } = default!;
        public string ClientSecretHash { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string ScopesCsv { get; set; } = default!;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RevokedAt { get; set; }

        public IEnumerable<string> Scopes => ScopesCsv.Split(',', StringSplitOptions.RemoveEmptyEntries);
    }
}