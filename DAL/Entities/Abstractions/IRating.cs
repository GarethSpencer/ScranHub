namespace DAL.Entities.Abstractions;

public interface IRating
{
    public Guid RatingId { get; set; }
    public Guid GroupVenueId { get; set; }
    public Guid UserId { get; set; }
    public Guid OptionId { get; set; }
}
