using API.Data;
using API.Endpoints;
using API.Hubs;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(
    options =>
    {
        options.AddDefaultPolicy(builder =>
        {
            builder.WithOrigins("http://localhost:4200", "https://localhost:4200").AllowAnyHeader().AllowAnyMethod().AllowCredentials();
        });
    }
);
var JwtSetting = builder.Configuration.GetSection("JWTSetting");

builder.Services.AddDbContext<AppDbContext>(x => x.UseSqlite("Data Source=chat.db"));

builder.Services.AddIdentityCore<AppUser>()
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddScoped<TokenService>();

builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(option =>
{
    option.SaveToken = true;
    option.RequireHttpsMetadata = false;
    option.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSetting.GetSection("SecurityKey").Value!)),
        ValidateIssuer = false,
        ValidateAudience = false
    };

    option.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };

});

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSignalR();
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect("localhost:6379")); //change to actual port number

var multiplexer = ConnectionMultiplexer.Connect("localhost:6379");

if (multiplexer.IsConnected)
{
    Console.WriteLine("Connected to Redis successfully!");
}
else
{
    Console.WriteLine("Failed to connect to Redis!");
}

builder.Services.AddSingleton<RedisPubSubService>();
builder.Services.AddHostedService<RedisSubscriberService>();
//builder.Services.AddHttpClient<IDeviceUpdateService, DeviceUpdateService>();

builder.Services.AddHttpClient<IDeviceUpdateService, DeviceUpdateService>((sp, client) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var baseUrl = config["BackendApi:BaseUrl"];
    Console.WriteLine($"Backend API Base URL: {baseUrl}");
    client.BaseAddress = new Uri(baseUrl!);
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors(x => x.AllowAnyHeader()
.AllowAnyMethod().AllowCredentials().WithOrigins("http://localhost:4200", "https://localhost:4200"));

//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.MapHub<ChatHub>("hubs/chat"); //this is the hub url endpoint which will enable client to connect to this hub
app.MapHub<VideChatHub>("hubs/video-chat");
app.MapHub<DeltaPayloadHub>("hubs/delta-payload");
app.MapAccountEndpoint();
app.MapDeviceRegistrationEndpoint();
Console.WriteLine("WHAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAT");
app.Run();



