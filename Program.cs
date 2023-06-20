using Microsoft.Extensions.FileProviders;
using ShortLinks.Models;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddSingleton<IMongoClient, MongoClient>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();
app.MapControllers();
app.Run();
