using Amazon.DynamoDBv2.DataModel;
using HealthTracker.Data;

namespace HealthTracker.Models;

public class UserTracking
{
    [DynamoDBProperty("dietaryTracking")]
    public List<DietaryTracking> DietaryTracking { get; set; }

    [DynamoDBProperty("BPTracking")]
    public List<BPTracking> BPTracking { get; set; }

    [DynamoDBProperty("sleepTracking")]
    public List<SleepTracking> SleepTracking { get; set; }

    public UserTracking()
    {
        DietaryTracking = new List<DietaryTracking>();
        BPTracking = new List<BPTracking>();
        SleepTracking = new List<SleepTracking>();
    }
}

public class BaseTracking
{
    [DynamoDBProperty("date")] // time since epoch
    public long Date { get; set; }

    public BaseTracking()
    {
        Date = Helpers.TimeSinceEpoch(DateTime.UtcNow);
    }
}

public class DietaryTracking : BaseTracking
{
    [DynamoDBProperty("water")]
    public double Water { get; set; }

    [DynamoDBProperty("calories")]
    public double Calories { get; set; }

    public DietaryTracking() : base()
    {
    }
}

public class BPTracking : BaseTracking
{
    [DynamoDBProperty("systolic")]
    public double Systolic { get; set; }

    [DynamoDBProperty("diastolic")]
    public double Diastolic { get; set; }

    public BPTracking() : base()
    {
    }
}

public class SleepTracking : BaseTracking
{
    [DynamoDBProperty("hours")]
    public double Hours { get; set; }


    public SleepTracking() : base()
    {
    }
}