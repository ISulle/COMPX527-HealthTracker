using Amazon.DynamoDBv2.DataModel;
using HealthTracker.Data;
using Microsoft.AspNetCore.Http;
using System.Reflection;

namespace HealthTracker.Models;

[DynamoDBTable("HealthTrackerUsers")]
public class UserInformation
{
    [DynamoDBHashKey("username")]
    public string Username { get; set; }

    [DynamoDBProperty("name")]
    public string Name { get; set; }

    [DynamoDBProperty("gender")]
    public string Gender { get; set; }

    [DynamoDBProperty("dob")] // time since epoch
    public string DOB { get; set; }

    [DynamoDBProperty("height")] // in cm
    public double Height { get; set; }

    [DynamoDBProperty("weight")] // in kg
    public double Weight { get; set; }

    [DynamoDBProperty("medicalHistory")]
    public MedicalHistory MedicalHistory { get; set; }

    [DynamoDBProperty("dietaryTracking")]
    public List<DietaryTracking> DietaryTracking { get; set; }

    [DynamoDBProperty("BPTracking")]
    public List<BPTracking> BPTracking { get; set; }

    [DynamoDBProperty("sleepTracking")]
    public List<SleepTracking> SleepTracking { get; set; }


    [DynamoDBIgnore]
    public int Age { get; set; }
    [DynamoDBIgnore]
    public double BMI { get; set; }
    [DynamoDBIgnore]
    public string BMIState { get; set; }
    [DynamoDBIgnore]
    public double RecommendedWater { get; set; }
    [DynamoDBIgnore]
    public double RecommendedCalories { get; set; }
    [DynamoDBIgnore]
    public int RecommendedSleep { get; set; }
    [DynamoDBIgnore]
    public string NormalBp { get; set; }
    [DynamoDBIgnore]
    public List<string> Dates { get; set; }
    [DynamoDBIgnore]
    public List<string> WaterValues { get; set; }
    [DynamoDBIgnore]
    public List<string> CalorieValues { get; set; }
    [DynamoDBIgnore]
    public List<string> SysValues { get; set; }
    [DynamoDBIgnore]
    public List<string> DiaValues { get; set; }
    [DynamoDBIgnore]
    public List<string> SleepValues { get; set; }

    public UserInformation()
    {
    }

    public UserInformation(string username, string name, string gender, DateTime dob, double height, double weight)
    {
        Username = username;
        Name = name;
        Gender = gender;
        Height = height;
        Weight = weight;
        DOB = dob.ToString("MM/dd/yyyy");
        MedicalHistory = new MedicalHistory();
        DietaryTracking = new List<DietaryTracking>();
        BPTracking = new List<BPTracking>();
        SleepTracking = new List<SleepTracking>();
    }

    public void Init()
    {
        var dob = DateTime.Parse(DOB);
        // Calculate age
        Age = DateTime.UtcNow.Year - dob.Year;
        // if the birth date has not occurred this year yet, subtract 1
        if (dob.Date > DateTime.UtcNow.Date.AddYears(-Age))
            Age--;
        // Calculate BMI
        BMI = Math.Round(Weight / Math.Pow((Height / 100), 2), 2);
        BMIState = Helpers.CalculateBMIState(BMI);
    }

    public void CalculateRecommended()
    {
        var dob = DateTime.Parse(DOB);
        RecommendedWater = Helpers.CalculateDailyWaterIntake(Gender);
        RecommendedCalories = Helpers.CalculateDailyCalorieIntake(Gender);
        Age = DateTime.UtcNow.Year - dob.Year;
        RecommendedSleep = Helpers.CalculateDailySleep(Age);
        NormalBp = Helpers.CalculateBloodPressure(Age, Gender);
    }
}
