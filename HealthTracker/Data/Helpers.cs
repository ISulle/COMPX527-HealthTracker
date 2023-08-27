namespace HealthTracker.Data;
public static class Helpers
{
    // Blood Pressure numbers sourced from:
    // https://www.stroke.org.nz/blood-pressure
    // https://www.baptisthealth.com/blog/heart-care/healthy-blood-pressure-by-age-and-gender-chart
    // https://www.heart.org/en/health-topics/high-blood-pressure/understanding-blood-pressure-readings
    // Daily Calorie Intake sourced from:
    // https://www.nhs.uk/common-health-questions/food-and-diet/what-should-my-daily-intake-of-calories-be/
    // Daily Water Intake sourced from:
    // https://www.mayoclinic.org/healthy-lifestyle/nutrition-and-healthy-eating/in-depth/water/art-20044256#:~:text=So%20how%20much%20fluid%20does,fluids%20a%20day%20for%20women
    // Daily Recommended Sleep sourced from:
    // https://www.mayoclinic.org/healthy-lifestyle/nutrition-and-healthy-eating/in-depth/water/art-20044256#:~:text=So%20how%20much%20fluid%20does,fluids%20a%20day%20for%20women
    // BMI numbers sourced from:
    // https://www.cancer.org/cancer/risk-prevention/diet-physical-activity/body-weight-and-cancer-risk/adult-bmi.html

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

    /// <summary>
    /// Calculates BMI indicator
    /// </summary>
    /// <param name="bmi">BMI of person</param>
    public static string CalculateBMIState(double bmi)
    {
        if(bmi < 18.5)
            return "Underweight";
        if (bmi is >= 18.5 and < 25)
            return "Normal";
        if (bmi is >= 25 and < 30)
            return "Overweight";
        return "Obese";
    }

    /// <summary>
    /// Calculates CSS class for BMI indicator
    /// </summary>
    /// <param name="bmi">BMI of person</param>
    public static string CalculateBMIColor(double bmi)
    {
        if (bmi < 18.5)
            return "red";
        if (bmi is >= 18.5 and < 25)
            return "green";
        if (bmi is >= 25 and < 30)
            return "orange";
        return "red";
    }

    /// <summary>
    /// Calculates CSS class for BMI indicator
    /// </summary>
    /// <param name="bmi">BMI of person</param>
    public static string CalculateBloodPressure(int age, string gender)
    {
        var bp = "";
        if (age < 1)
            bp = "87/53";
        else if (age < 18)
            bp = "97/57";
        else if (age < 18)
            bp = "112/68";
        else if (gender == "Male")
        {
            if (age is >= 18 and < 40)
                bp = "119/70";
            else if(age is >= 40 and < 60)
                bp = "124/77";
            else if (age > 60)
                bp = "133/69";
        }
        else
        {
            if (age is >= 18 and < 40)
                bp = "110/68";
            else if (age is >= 40 and < 60)
                bp = "122/74";
            else if (age > 60)
                bp = "139/68";
            else
                bp = "133/69";
        }
        return bp;
    }
}
