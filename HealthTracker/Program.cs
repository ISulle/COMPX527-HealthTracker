using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using Amazon.Extensions.NETCore.Setup;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Authentication using AWS Cognito and OpenId
builder.Services.AddAuthentication(item =>
    {
        item.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        item.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie()
    .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        options.ResponseType = builder.Configuration["AWS:Cognito:ResponseType"];
        options.MetadataAddress = builder.Configuration["AWS:Cognito:MetadataAddress"];
        options.ClientId = builder.Configuration["AWS:Cognito:ClientId"];
        options.ClientSecret = builder.Configuration["AWS:Cognito:ClientSecret"];
        options.Events = new OpenIdConnectEvents()
        {
            OnRedirectToIdentityProviderForSignOut = OnRedirectToIdentityProviderForSignOut
        };
        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("email");
        options.Scope.Add("phone");
        options.SaveTokens = Convert.ToBoolean(builder.Configuration["AWS:Cognito:SaveToken"]);
    });

// AWS DynamoDB
var awsOptions = builder.Configuration.GetAWSOptions();
builder.Services.AddDefaultAWSOptions(awsOptions);
builder.Services.AddAWSService<IAmazonDynamoDB>();
builder.Services.AddScoped<IDynamoDBContext, DynamoDBContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

Task OnRedirectToIdentityProviderForSignOut(RedirectContext context)
{

    context.ProtocolMessage.Scope = "openid";
    context.ProtocolMessage.ResponseType = "code";

    var cognitoDomain = builder.Configuration["AWS:Cognito:CognitoDomain"];

    var clientId = builder.Configuration["AWS:Cognito:ClientId"];

    var logoutUrl = $"{context.Request.Scheme}://{context.Request.Host}{builder.Configuration["AWS:Cognito:AppSignOutUrl"]}";

    context.ProtocolMessage.IssuerAddress = $"{cognitoDomain}/logout?client_id={clientId}&logout_uri={logoutUrl}";

    return Task.CompletedTask;
}
