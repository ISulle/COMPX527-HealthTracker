using Amazon.DynamoDBv2.DataModel;
using HealthTracker.Data;
using Microsoft.VisualBasic;

namespace HealthTracker.Models;

public class BaseTracking
{
    [DynamoDBProperty("date")]
    public string Date { get; set; }

    public BaseTracking()
    {
    }

    public BaseTracking(DateTime date)
    {
        Date = date.ToString("MM/dd/yyyy");
    }
}

public class DietaryTracking : BaseTracking
{
    [DynamoDBProperty("water")] // in liters
    public double Water { get; set; }

    [DynamoDBProperty("calories")]
    public double Calories { get; set; }

    public DietaryTracking() : base()
    {
    }

    public DietaryTracking(DateTime date, double water, double calories) : base(date)
    {
        Water = water;
        Calories = calories;
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

    public BPTracking(DateTime date, double systolic, double diastolic) : base(date)
    {
        Systolic = systolic;
        Diastolic = diastolic;
    }
}

public class SleepTracking : BaseTracking
{
    [DynamoDBProperty("hours")]
    public double Hours { get; set; }

    public SleepTracking() : base()
    {
    }

    public SleepTracking(DateTime date, double hours) : base(date)
    {
        Hours = hours;
    }
}