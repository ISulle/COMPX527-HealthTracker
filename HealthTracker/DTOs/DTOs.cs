using Amazon.DynamoDBv2.DataModel;
using HealthTracker.Data;
using HealthTracker.Models;

namespace HealthTracker.DTOs;

public record UserDto
{

    public string Name { get; set; }
    public DateTime DOB { get; set; }
    public double Height { get; set; }

    public double Weight { get; set; }
    public string Gender { get; set; }
}


public record BaseTrackingDto
{
    public DateTime Date { get; set; }
}

public record DietaryTrackingDto : BaseTrackingDto
{
    public double Water { get; set; }
    public double Calories { get; set; }
}

public record BPTrackingDto : BaseTrackingDto
{
    public double Systolic { get; set; }

    public double Diastolic { get; set; }
}

public record SleepTrackingDto : BaseTrackingDto
{
    public double Hours { get; set; }
}

public class DiabetesData
{
    public UserInformation User { get; set; }
    public bool Prediction { get; set; }

    public DiabetesData()
    {
    }

    public DiabetesData(UserInformation user, bool prediction)
    {
        User = user;
        Prediction = prediction;
    }
}