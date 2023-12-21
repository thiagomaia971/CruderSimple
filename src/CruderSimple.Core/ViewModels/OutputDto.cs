namespace CruderSimple.Core.ViewModels;

public class OutputDto(string id, DateTime createdAt, DateTime? updatedAt) : BaseDto(id)
{
    public DateTime CreatedAt { get; set; } = createdAt;
    public DateTime? UpdatedAt { get; set; } = updatedAt;
}
