namespace Interfaces
{
    public interface IAuditable
    {
        bool? IsDeleted { get; set; }
        bool? IsModified { get; set; }
        bool? IsRestored { get; set; }

        int Id { get; set; }

        string? CreatedById { get; set; }
        string? DeletedById { get; set; }
        string? ModifiedById { get; set; }
        string? RestoredById { get; set; }

        DateTime? CreatedAt { get; set; }
        DateTime? DeletedAt { get; set; }
        DateTime? ModifiedAt { get; set; }
        DateTime? RestoredAt { get; set; }
    }
}
