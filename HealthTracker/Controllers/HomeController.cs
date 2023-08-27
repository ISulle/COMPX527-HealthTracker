using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;
using HealthTracker.DTOs;
using HealthTracker.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Amazon;
using HealthTracker.Data;

namespace HealthTracker.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly IDynamoDBContext _context;

    public HomeController(IDynamoDBContext context)
    {
        _context = context;
    }

    //public HomeController(IConfiguration config)
    //{
    //    var credentials = new BasicAWSCredentials(config["AWS:AccessKey"], config["AWS:SecretKey"]);
    //    var client = new AmazonDynamoDBClient(credentials, RegionEndpoint.APSoutheast2);
    //    _context = new DynamoDBContext(client);
    //}

    private string GetUsername()
        => User.Claims.First(x => x.Type == "cognito:username")?.Value;

    private async Task<UserInformation> GetUser()
        => await _context.LoadAsync<UserInformation>(GetUsername());

    public async Task<IActionResult> Index()
    {
        var user = await GetUser();
        user.Init();
        // Redirect user to finish registering if needed
        if (user == null)
            return RedirectToAction("Register");
        return View(user);
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
        // Create user in DynamoDB
        var user = new UserInformation(GetUsername(), dto.Name, dto.Gender, dto.DOB, dto.Height, dto.Weight);
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
