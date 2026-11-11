using Microsoft.EntityFrameworkCore;
using SchoolAdmission.Business.Options;
using SchoolAdmission.Data;
using SchoolAdmission.Data.Persistence;
using SchoolAdmission.Business;
using SchoolAdmission.API.BackgroundServices;
using SchoolAdmission.API.Mappings;
using SchoolAdmission.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(TimeProvider.System);

builder.Services.Configure<HoldOptions>(
    builder.Configuration.GetSection(HoldOptions.SectionName));

builder.Services.AddDataAccess(builder.Configuration);
builder.Services.AddBusinessServices();
builder.Services.AddHostedService<ExpiredHoldCleanupService>();
builder.Services.AddAutoMapper(cfg => { }, typeof(MappingProfile));

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Automatically apply database migrations.
await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    await dbContext.Database.MigrateAsync();
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Docker uses HTTP internally; local development can use HTTPS.
if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

app.Run();