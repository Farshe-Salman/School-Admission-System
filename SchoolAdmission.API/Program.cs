using SchoolAdmission.Business.Options;
using SchoolAdmission.Data;
using SchoolAdmission.Business;
using SchoolAdmission.API.BackgroundServices;
using SchoolAdmission.API.Mappings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(TimeProvider.System);

builder.Services.Configure<HoldOptions>(
    builder.Configuration.GetSection(HoldOptions.SectionName));

builder.Services.AddDataAccess(builder.Configuration);
builder.Services.AddBusinessServices();
builder.Services.AddHostedService<ExpiredHoldCleanupService>();
builder.Services.AddAutoMapper(cfg => { }, typeof(MappingProfile));

// Register services
builder.Services.AddControllers();

// Register Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure HTTP pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
