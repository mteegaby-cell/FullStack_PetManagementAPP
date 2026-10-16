//This is the integrated program which will have EVERYBODY's code integrated as cleanly as possible. 
//The code will be copy pasted instead of being merged using git, for a better understanding of what's going on, and the reduction of duplicate code.
//I will backup the main codebase before the merge so we can have a restore point that has no issues.

//KEY : Thobile's changes = $$; Lithemba's Changes = &&; Ovayo's Changes = @@; Mpilwenhle's Changes = ^^;

//Dependencies - ^^ + $$:
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using PETMSCRUD.Models;//^^following 3 changed from PETCRUD to PETMSCRUD, added 6 errors lol
using PETMSCRUD.Data;
using PETMSCRUD.Services;
using PETMSCRUD.Models.Entities;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontEndPolicy", policy =>
    {
        policy
            .WithOrigins("http://localhost:5176")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

//^^ + $$ | Added the second line 
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//$$ + ^^ Versions didn't match, used AI for TLC ^^
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    // Use the lambda overload to access the generated 'document'
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        // Pass the string name and the document into the new reference type
        [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });
});


//^^ + $$ | PetOContext => PETMSDBCONTEXT in every instance 
builder.Services.AddDbContext<PETMSDBContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DbConn"),
            sql => sql.CommandTimeout(120)));

    //$$
    // ASP.NET Identity: users and roles live in the same database
    builder.Services.AddIdentity<ApplicationUser, IdentityRole>(o =>
    {
        o.Password.RequiredLength = 8;
        o.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<PETMSDBContext>()
    .AddDefaultTokenProviders();

    builder.Services.AddScoped<TokenService>();

    // JWT bearer authentication: React sends "Authorization: Bearer <token>"
    var jwt = builder.Configuration.GetSection("Jwt");
    builder.Services.AddAuthentication(o =>
    {
        o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwt["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

    builder.Services.AddAuthorization();

    // Allow the React dev server to call the API
    builder.Services.AddCors(o => o.AddPolicy("React", p =>
        p.WithOrigins("http://localhost:5173", "http://localhost:3000")
         .AllowAnyHeader().AllowAnyMethod()));

    //^^
    var app = builder.Build();

    // Apply migrations, create roles, seed demo data
    //$$
    using (var scope = app.Services.CreateScope())
    {
        await SeedData.InitializeAsync(scope.ServiceProvider);
    }

    // Configure the HTTP request pipeline.
    //^^
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    //$$
    app.UseCors("FrontEndPolicy");// NOTE: no UseHttpsRedirection here so the React dev server can call http://localhost:5228 directly
    app.UseAuthentication();// must come BEFORE UseAuthorization

    //^^
    app.UseAuthorization();
    app.MapControllers();
    app.Run();

//$$
public class AuthOperationFilter : Swashbuckle.AspNetCore.SwaggerGen.IOperationFilter
{
    public void Apply(OpenApiOperation op, Swashbuckle.AspNetCore.SwaggerGen.OperationFilterContext ctx)
    {
        var needsAuth =
            ctx.MethodInfo.GetCustomAttributes(true).OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>().Any() ||
            (ctx.MethodInfo.DeclaringType?.GetCustomAttributes(true)
                .OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>().Any() ?? false);
        var allowsAnon =
            ctx.MethodInfo.GetCustomAttributes(true).OfType<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>().Any() ||
            (ctx.MethodInfo.DeclaringType?.GetCustomAttributes(true)
                .OfType<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>().Any() ?? false);

        //^^ Updated to be compatible with .Net 10.0 OpenApi standards
        if (needsAuth && !allowsAnon)
        {
            op.Security = new List<OpenApiSecurityRequirement>
    {
        new()
        { 
            // Change Array.Empty<string>() to a List representation
            [new OpenApiSecuritySchemeReference("Bearer", null)] = []
        }
    };
        }
    }
}

