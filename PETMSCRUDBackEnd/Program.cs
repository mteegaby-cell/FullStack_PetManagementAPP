// Integrated PETMSCRUD startup configuration.
// Ensure role names used here match the constants in PETMSCRUD.Data.Roles
// and any [Authorize(Roles = "...")] attributes throughout the application.

using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using PETMSCRUD.Data;
using PETMSCRUD.Models;
using PETMSCRUD.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontEndPolicy", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5176",
                "http://localhost:5173",
                "http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter a JWT bearer token.",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] =
                new List<string>()
        });
});

var connectionString = builder.Configuration.GetConnectionString("DbConn");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Database connection string 'DbConn' is missing.");
}

builder.Services.AddDbContext<PETMSDBContext>(options =>
    options.UseSqlServer(connectionString, sql => sql.CommandTimeout(120)));

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 8;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<PETMSDBContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<TokenService>();

var jwt = builder.Configuration.GetSection("Jwt");
var jwtKey = jwt["Key"];

if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException(
        "JWT signing key is missing. Configure Jwt:Key in appsettings or user secrets.");
}

if (string.IsNullOrWhiteSpace(jwt["Issuer"]) ||
    string.IsNullOrWhiteSpace(jwt["Audience"]))
{
    throw new InvalidOperationException(
        "JWT configuration must include Jwt:Issuer and Jwt:Audience.");
}

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwt["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Migrate and seed through one startup scope. DbSeeder creates the configured
// administrator and any missing base services. SeedData adds demo users and
// related sample data. Both seeders should be idempotent.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("StartupSeed");

    try
    {
        // SeedData applies EF Core migrations before it queries seed tables.
        // It also ensures Admin, Groomer and PetOwner roles exist.
        await SeedData.InitializeAsync(services);

        // This adds the configured SeedAdmin user and any missing base services.
        // It is safe to run after SeedData once DbSeeder has been made idempotent.
        await DbSeeder.SeedAsync(services);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
        throw;
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("FrontEndPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
