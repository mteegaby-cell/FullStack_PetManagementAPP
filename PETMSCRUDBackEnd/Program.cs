//This is the integrated program which will have EVERYBODY's code integrated as cleanly as possible. 
//The code will be copy pasted instead of being merged using git, for a better understanding of what's going on, and the reduction of duplicate code.
//I will backup the main codebase before the merge so we can have a restore point that has no issues.
using Microsoft.EntityFrameworkCore;
using PETMSCRUD.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<PETMSDBContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DbConn")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
