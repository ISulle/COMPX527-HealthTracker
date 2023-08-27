using Amazon.DynamoDBv2.DataModel;
using HealthTracker.Data;

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
    public long DOB { get; set; }

    [DynamoDBProperty("height")] // in cm
    public double Height { get; set; }

    [DynamoDBProperty("weight")] // in kg
    public double Weight { get; set; }

    [DynamoDBProperty("medicalHistory")]
    public MedicalHistory MedicalHistory { get; set; }

    [DynamoDBProperty("userTracking")]
    public UserTracking UserTracking { get; set; }

    [DynamoDBIgnore]
    public int Age { get; set; }
    [DynamoDBIgnore]
    public double BMI { get; set; }

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
        DOB = Helpers.TimeSinceEpoch(dob);
        MedicalHistory = new MedicalHistory();
        UserTracking = new UserTracking();
    }

    public void Init()
    {
        // Calculate age
        DateTime dobDateTime = new DateTime(1970, 1, 1).AddSeconds(DOB);
        Age = DateTime.UtcNow.Year - dobDateTime.Year;
        // if the birth date has not occurred this year yet, subtract 1
        if (dobDateTime.Date > DateTime.UtcNow.Date.AddYears(-Age))
            Age--;
        // Calculate BMI
        BMI = Math.Round(Weight / Math.Pow((Height / 100), 2),2);
    }
}
