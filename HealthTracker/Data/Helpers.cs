namespace HealthTracker.Data;
public static class Helpers
{
    // Blood Pressure Numbers sourced from:
    // https://www.stroke.org.nz/blood-pressure
    // https://www.baptisthealth.com/blog/heart-care/healthy-blood-pressure-by-age-and-gender-chart
    // https://www.heart.org/en/health-topics/high-blood-pressure/understanding-blood-pressure-readings
    // Daily Calorie Intake sourced from:
    // https://www.nhs.uk/common-health-questions/food-and-diet/what-should-my-daily-intake-of-calories-be/
    // Daily Water Intake sourced from:
    // https://www.mayoclinic.org/healthy-lifestyle/nutrition-and-healthy-eating/in-depth/water/art-20044256#:~:text=So%20how%20much%20fluid%20does,fluids%20a%20day%20for%20women
    // Daily Recommended Sleep sourced from:
    // https://www.mayoclinic.org/healthy-lifestyle/nutrition-and-healthy-eating/in-depth/water/art-20044256#:~:text=So%20how%20much%20fluid%20does,fluids%20a%20day%20for%20women

    /// <summary>
    /// Calculates time since epoch for a given date
    /// </summary>
    /// <param name="date">Date and time to calculate with</param>
    /// <returns>Total seconds since epoch</returns>
    public static long TimeSinceEpoch(DateTime date)
        => (long)(date - new DateTime(1970, 1, 1)).TotalSeconds;

    /// <summary>
    /// Calculates general blood pressure guideline
    /// </summary>
    /// <param name="systolic">Systolic blood pressure</param>
    /// <param name="diastolic">Diastolic blood pressure</param>
    /// <returns>Blood Pressure State</returns>
    public static string CalculateGeneralBloodPressure(int systolic, int diastolic)
    {
        if (systolic < 120 && diastolic < 80)
            return "Normal";
        if (systolic is >= 120 and <= 129 && diastolic < 80)
            return "Elevated";
        if (systolic is >= 130 and <= 139 || diastolic is >= 80 and <= 89)
            return "High Blood Pressure (Hypertension) Stage 1";
        if (systolic >= 140 || diastolic > 90)
            return "High Blood Pressure (Hypertension) Stage 2";
        return "Hypertensive Crisis (consult your doctor immediately)";
    }

    /// <summary>
    /// Calculates recommended daily calorie intake
    /// </summary>
    /// <param name="gender">Gender of person</param>
    /// <returns></returns>
    public static int CalculateDailyCalorieIntake(string gender)
        => gender == "Male" ? 2500 : 2000;

    /// <summary>
    /// Calculates recommended daily water calorie intake
    /// </summary>
    /// <param name="gender">Gender of person</param>
    public static double CalculateDailyWaterIntake(string gender)
        => gender == "Male" ? 3.7 : 2.7;

    /// <summary>
    /// Calculates recommended sleep
    /// </summary>
    /// <param name="age">Age of person</param>
    public static int CalculateDailySleep(int age)
    {
        if (age < 1)
            return 14;
        if (age is >= 1 and <= 2)
            return 13;
        if (age is >= 3 and <= 5)
            return 12;
        if (age is >= 6 and <= 12)
            return 11;
        if (age is >= 13 and <= 18)
            return 9;
        return 8;
    }
}
