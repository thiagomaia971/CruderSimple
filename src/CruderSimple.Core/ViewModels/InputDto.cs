namespace CruderSimple.Core.ViewModels;

public abstract class InputDto
{
    public string? Id { get; set; }

    public InputDto()
    {
        Id = Guid.NewGuid().ToString();
    }
}