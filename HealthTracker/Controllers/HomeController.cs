using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;
using HealthTracker.DTOs;
using HealthTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Http.Headers;
using Amazon;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.AspNetCore.Cors;
using Newtonsoft.Json;
using Microsoft.VisualBasic.FileIO;

namespace HealthTracker.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly IDynamoDBContext _context;
    private readonly IWebHostEnvironment _env;
    //public HomeController(IDynamoDBContext context)
    //{
    //    _context = context;
    //}

    public HomeController(IConfiguration config, IWebHostEnvironment env)
    {
        var credentials = new BasicAWSCredentials(config["AWS:AccessKey"], config["AWS:SecretKey"]);
        var client = new AmazonDynamoDBClient(credentials, RegionEndpoint.APSoutheast2);
        _context = new DynamoDBContext(client);
        _env = env;
    }

    private string GetUsername()
        => User.Claims.First(x => x.Type == "cognito:username")?.Value;

    private async Task<UserInformation> GetUser()
        => await _context.LoadAsync<UserInformation>(GetUsername());

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        return View();
    }

    public async Task<IActionResult> Profile()
    {
        var user = await GetUser();
        // Redirect user to finish registering if needed
        if (user == null)
            return RedirectToAction("Register");
        user.Init();
        return View(user);
    }

    private async Task SaveUser(UserDto dto)
    {
        // Create user in DynamoDB
        var user = await GetUser();
        user.Name = dto.Name;
        user.Gender = dto.Gender;
        user.DOB = dto.DOB.ToString("MM/dd/yyyy");
        user.Height = dto.Height;
        user.Weight = dto.Weight;
        await _context.SaveAsync(user);
    }

    [HttpPost]
    public async Task<IActionResult> Profile(UserDto dto)
    {
        await SaveUser(dto);
        return RedirectToAction("Profile");
    }


    public async Task<IActionResult> Register()
    {
        // Prevents users from accessing they are already registered
        var user = await GetUser();
        if (user == null)
            return View();
        return RedirectToAction("Profile");
    }

    [HttpPost]
    public async Task<IActionResult> Register(UserDto dto)
    {
        var username = GetUsername();
        var user = new UserInformation(username, dto.Name, dto.Gender, dto.DOB, dto.Height, dto.Weight);
        await _context.SaveAsync(user);
        return RedirectToAction("Profile");
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
            var prediction = await GetPrediction(user);
            diabetesData.Prediction = prediction;
        }
        return View(diabetesData);
    }

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

    public async Task<bool> GetPrediction(UserInformation user)
    {
        var apiUrl = "http://host.docker.internal:32000/predict";
        using (var client = new HttpClient())
        {
            // Construct the input JSON data
            var jsonData = new
            {
                Age = user.Age,
                Gender = user.Gender == "Male" ? 0 : 1,
                Polyuria = user.MedicalHistory.Polyuria,
                Polydipsia = user.MedicalHistory.Polydipsia,
                SuddenWeightLoss = user.MedicalHistory.SuddenWeightLoss,
                Weakness = user.MedicalHistory.Weakness,
                Polyphagia = user.MedicalHistory.Polyphagia,
                GenitalThrush = user.MedicalHistory.GenitalThrush,
                VisualBlurring = user.MedicalHistory.VisualBlurring,
                Itching = user.MedicalHistory.Itching,
                Irritability = user.MedicalHistory.Irritability,
                DelayedHealing = user.MedicalHistory.DelayedHealing,
                PartialParesis = user.MedicalHistory.PartialParesis,
                MuscleStiffness = user.MedicalHistory.MuscleStiffness,
                Alopecia = user.MedicalHistory.Alopecia,
                Obesity = user.MedicalHistory.Obesity
            };
            var jsonPayload = JsonConvert.SerializeObject(jsonData);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await client.PostAsync(apiUrl, content);
            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var r = JsonConvert.DeserializeObject<PredictionResponse>(jsonResponse);
                return r.Prediction;
            }
        }
        return false;
    }

    public class PredictionResponse
    {
        public bool Prediction { get; set; }
    }

    [HttpGet]
    public IActionResult GetCsvData()
    {
        string path = Path.Combine(_env.WebRootPath, "data", "diabetes_data.csv");
        var resultList = new List<Dictionary<string, string>>();
        using (var parser = new TextFieldParser(path))
        {
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(",");
            // Assuming the first row contains the column headers
            string[] headers = parser.ReadFields();
            while (!parser.EndOfData)
            {
                string[] fields = parser.ReadFields();
                if (fields != null && headers != null)
                {
                    var dict = new Dictionary<string, string>();
                    for (int i = 0; i < headers.Length; i++)
                    {
                        dict.Add(headers[i], fields[i]);
                    }
                    resultList.Add(dict);
                }
            }
        }
        return Json(resultList);
    }
}
