using Microsoft.Extensions.FileProviders;
using ShortLinks.Models;
using ShortLinks.Services;
using MongoDB.Driver;
using DotNetEnv;

DotNetEnv.Env.Load("./.env");

string redisConnection = Environment.GetEnvironmentVariable("REDIS_CONNECTION")!;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddSingleton<IMongoClient, MongoClient>();
builder.Services.AddSingleton<ILinkService, LinkService>();
builder.Services.AddSingleton<RedisCacheService>(sp => new RedisCacheService(redisConnection));
builder.Services.AddHttpContextAccessor();

var app = builder.Build();
app.MapControllers();
app.Run();
