using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;
using HealthTracker.DTOs;
using HealthTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Amazon;
using HealthTracker.Data;
using System.Net.Http;

namespace HealthTracker.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly IDynamoDBContext _context;
    //public HomeController(IDynamoDBContext context)
    //{
    //    _context = context;
    //}

    public HomeController(IConfiguration config, IWebHostEnvironment env)
    {
        var credentials = new BasicAWSCredentials(config["AWS:AccessKey"], config["AWS:SecretKey"]);
        var client = new AmazonDynamoDBClient(credentials, RegionEndpoint.APSoutheast2);
        _context = new DynamoDBContext(client);
    }

    private string GetUsername()
        => User.Claims.First(x => x.Type == "cognito:username")?.Value;

    private async Task<UserInformation> GetUser()
        => await _context.LoadAsync<UserInformation>(GetUsername());

    public async Task<IActionResult> Index()
    {
        var user = await GetUser();
        // Redirect user to finish registering if needed
        if (user == null)
            return RedirectToAction("Register");
        user.Init();
        return View(user);
    }

    [HttpPost]
    public async Task<IActionResult> Index(UserDto dto)
    {
        // Create user in DynamoDB
        var user = new UserInformation(GetUsername(), dto.Name, dto.Gender, dto.DOB, dto.Height, dto.Weight);
        await _context.SaveAsync(user);
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Register()
    {
        // Prevents users from accessing they are already registered
        var user = await GetUser();
        if (user == null)
            return View();
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Register(UserDto dto)
    {
        var user = await GetUser();
        user.Name = dto.Name;
        user.Gender = dto.Gender;
        user.DOB = dto.DOB.ToString("MM/dd/yyyy");
        user.Height = dto.Height;
        user.Weight = dto.Weight;
        // Create user in DynamoDB
        await _context.SaveAsync(user);
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Dashboard(DateTime from)
    {
        if (from == DateTime.MinValue)
            from = DateTime.UtcNow.AddDays(-7);
        var user = await GetUser();
        // Redirect user to finish registering if needed
        if (user == null)
            return RedirectToAction("Register");
        DateTime current = DateTime.UtcNow;
        user.CalculateRecommended();
        // Calculate date strings to display on X Axis
        List<string> dates = new List<string>();
        for (DateTime date = from; date <= current; date = date.AddDays(1))
        {
            dates.Add(date.ToString("MM/dd"));
        }
        user.Dates = dates;
        // Create Y Axis values
        var waterValues = new List<string>();
        var calorieValues = new List<string>();
        var sysValues = new List<string>();
        var diaValues = new List<string>();
        var sleepValues = new List<string>();
        foreach (var date in dates)
        {
            DateTime parsedDate;
            if (DateTime.TryParse(date, out parsedDate))
            {
                var dietary = user.DietaryTracking
                    .FirstOrDefault(x => DateTime.Parse(x.Date) == parsedDate);
                if (dietary != null)
                {
                    waterValues.Add(dietary.Water.ToString());
                    calorieValues.Add(dietary.Calories.ToString());
                }
                else
                {
                    waterValues.Add("0");
                    calorieValues.Add("0");
                }
                var bp = user.BPTracking
                    .FirstOrDefault(x => DateTime.Parse(x.Date) == parsedDate);
                if (bp != null)
                {
                    sysValues.Add(bp.Systolic.ToString());
                    diaValues.Add(bp.Diastolic.ToString());
                }
                else
                {
                    sysValues.Add("0");
                    diaValues.Add("0");
                }
                var sleep = user.SleepTracking
                    .FirstOrDefault(x => DateTime.Parse(x.Date) == parsedDate);
                if (sleep != null)
                {
                    sleepValues.Add(sleep.Hours.ToString());
                }
                else
                {
                    sleepValues.Add("0");
                }
            }
            else
            {
                waterValues.Add("0");
                calorieValues.Add("0");
                sysValues.Add("0");
                diaValues.Add("0");
                sleepValues.Add("0");
            }
        }
        user.WaterValues = waterValues;
        user.CalorieValues = calorieValues;
        user.SysValues = sysValues;
        user.DiaValues = diaValues;
        user.SleepValues = sleepValues;
        return View(user);
    }

    public async Task<IActionResult> Diabetes()
    {
        var user = await GetUser();
        // Redirect user to finish registering if needed
        if (user == null)
            return RedirectToAction("Register");
        var diabetesData = new DiabetesData(user, false);
        if (user.MedicalHistory.EnteredMedicalHistory)
        {
            user.Init();
            // Make prediction
            MakePrediction(user);
            //diabetesData.Prediction = prediction[0];
        }
        return View(diabetesData);
    }

    private void MakePrediction(UserInformation user)
    {
        string pythonExePath = @"C:\Path\To\Python\python.exe";
        string pythonScriptPath = @"C:\Path\To\Your\PythonScript.py";
        // Create a process to run the Python script
        using (Process process = new Process())
        {
            process.StartInfo.FileName = pythonExePath;
            process.StartInfo.Arguments = pythonScriptPath;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            // Start the process
            process.Start();
            // Read the standard output of the process
            string output = process.StandardOutput.ReadToEnd();
            // Wait for the process to exit
            process.WaitForExit();
            // Display the output from the Python script
            Console.WriteLine("Python Script Output:");
            Console.WriteLine(output);
        }
    }


    //public async Task<bool[]> MakePredictionAsync(UserInformation user)
    //{
    //    // Prepare the input data
    //    var input = new DiabetesInput
    //    {
    //        Age = user.Age,
    //        Gender = user.Gender == "Male" ? 1 : 0,
    //        Polyuria = user.MedicalHistory.Polyuria,
    //        Polydipsia = user.MedicalHistory.Polydipsia,
    //        SuddenWeightLoss = user.MedicalHistory.SuddenWeightLoss,
    //        Weakness = user.MedicalHistory.Weakness,
    //        Polyphagia = user.MedicalHistory.Polyphagia,
    //        GenitalThrush = user.MedicalHistory.GenitalThrush,
    //        VisualBlurring = user.MedicalHistory.VisualBlurring,
    //        Itching = user.MedicalHistory.Itching,
    //        Irritability = user.MedicalHistory.Irritability,
    //        DelayedHealing = user.MedicalHistory.DelayedHealing,
    //        PartialParesis = user.MedicalHistory.PartialParesis,
    //        MuscleStiffness = user.MedicalHistory.MuscleStiffness,
    //        Alopecia = user.MedicalHistory.Alopecia,
    //        Obesity = user.MedicalHistory.Obesity,
    //    };
    //    // Send the POST request to the Python API
    //    HttpResponseMessage response = await _client.PostAsJsonAsync("http://localhost:5000/predict", input);

    //    // Check if the request was successful
    //    if (response.IsSuccessStatusCode)
    //    {
    //        // Read the response content
    //        using (Stream responseStream = await response.Content.ReadAsStreamAsync())
    //        {
    //            // Deserialize the response using a JSON deserializer
    //            var predictionResult = await JsonSerializer.DeserializeAsync<PredictionResponse>(responseStream);
    //            return predictionResult.Prediction;
    //        }
    //    }
    //    else
    //    {
    //        throw new Exception($"API request failed with status code: {response.StatusCode}");
    //    }
    //}

    //public class PredictionResponse
    //{
    //    public bool[] Prediction { get; set; }
    //}


    [HttpPost]
    public async Task<IActionResult> Diabetes(DiabetesData dto)
    {
        var user = await GetUser();
        user.MedicalHistory = dto.User.MedicalHistory;
        user.MedicalHistory.EnteredMedicalHistory = true;
        await _context.SaveAsync(user);
        return RedirectToAction("Diabetes");
    }

    [HttpPost]
    public async Task<IActionResult> ResetDiabetes()
    {
        var user = await GetUser();
        user.MedicalHistory.EnteredMedicalHistory = false;
        await _context.SaveAsync(user);
        return RedirectToAction("Diabetes");
    }

    [HttpPost]
    public async Task<IActionResult> AddDietaryIntake(DietaryTrackingDto dto)
    {
        var user = await GetUser();
        user.DietaryTracking.Add(new DietaryTracking(dto.Date, dto.Water, dto.Calories));
        await _context.SaveAsync(user);
        return RedirectToAction("Dashboard", DateTime.UtcNow.AddDays(-7));
    }

    [HttpPost]
    public async Task<IActionResult> AddBloodPressure(BPTrackingDto dto)
    {
        var user = await GetUser();
        user.BPTracking.Add(new BPTracking(dto.Date, dto.Systolic, dto.Diastolic));
        await _context.SaveAsync(user);
        return RedirectToAction("Dashboard", DateTime.UtcNow.AddDays(-7));
    }

    [HttpPost]
    public async Task<IActionResult> AddDailySleep(SleepTrackingDto dto)
    {
        var user = await GetUser();
        user.SleepTracking.Add(new SleepTracking(dto.Date, dto.Hours));
        await _context.SaveAsync(user);
        return RedirectToAction("Dashboard", DateTime.UtcNow.AddDays(-7));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
