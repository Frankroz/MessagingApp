using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

// 1. Load Environment Variables from .env file
Env.Load();

// 2. Add services to the container.
builder.Services.AddControllers();

// Register the FirestoreService as a Singleton. 
// This ensures we don't recreate the Firestore connection on every request.
builder.Services.AddSingleton<FirestoreService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();