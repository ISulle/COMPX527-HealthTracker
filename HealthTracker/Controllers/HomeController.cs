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

namespace HealthTracker.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly IDynamoDBContext _context;

    public HomeController(IDynamoDBContext context, IConfiguration config)
    {
        var credentials = new BasicAWSCredentials(config["AWS:AccessKey"], config["AWS:SecretKey"]);
        var client = new AmazonDynamoDBClient(credentials, RegionEndpoint.APSoutheast2);
        _context = new DynamoDBContext(client);

        //_context = context;
        //string accessToken = HttpContext.GetTokenAsync("access_token").Result;

    }

    private string GetUsername()
        => User.Claims.First(x => x.Type == "cognito:username")?.Value;


    public async Task<IActionResult> Index()
    {
        // Get user from DynamoDB
        var user = await _context.LoadAsync<UserInformation>(GetUsername());
        user.Init();
        // Redirect user to finish registering if needed
        if (user == null)
            return RedirectToAction("Register");
        return View(user);
    }

    public async Task<IActionResult> Register()
    {
        // Prevents users from accessing this if already registered
        var user = await _context.LoadAsync<UserInformation>(GetUsername());
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

    public IActionResult Dashboard()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
