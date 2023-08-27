namespace HealthTracker.DTOs;

public record UserDto
{

    public string Name { get; set; }
    public DateTime DOB { get; set; }
    public double Height { get; set; }

    public double Weight { get; set; }
    public string Gender { get; set; }
}