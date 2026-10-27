using DevOpsPracticeApi.Data;
using DevOpsPracticeApi.Models;
using Microsoft.EntityFrameworkCore;
using DevOpsPracticeApi.DTOs;

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Database connection string is not configured.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Employee CRUD endpoints
// Document employee validation endpoints

app.MapGet("/employees", async (AppDbContext dbContext) =>
{
    var employees = await dbContext.Employees.ToListAsync();

    return Results.Ok(employees);
});

app.MapGet("/employees/{id:int}", async (
    int id,
    AppDbContext dbContext) =>
{
    var employee = await dbContext.Employees
        .FirstOrDefaultAsync(e => e.Id == id);

    if (employee is null)
    {
        return Results.NotFound(new
        {
            message = $"Employee with ID {id} was not found."
        });
    }

    return Results.Ok(employee);
});

app.MapPost("/employees", async (
    Employee employee,
    AppDbContext dbContext) =>
{
    dbContext.Employees.Add(employee);

    await dbContext.SaveChangesAsync();

    return Results.Created(
        $"/employees/{employee.Id}",
        employee);
});

app.MapPut("/employees/{id:int}", async (
    int id,
    UpdateEmployeeRequest request,
    AppDbContext dbContext) =>
{
    if (string.IsNullOrWhiteSpace(request.Name) ||
        string.IsNullOrWhiteSpace(request.Department))
    {
        return Results.BadRequest(new
        {
            message = "Name and department are required."
        });
    }

    var employee = await dbContext.Employees
        .FirstOrDefaultAsync(e => e.Id == id);

    if (employee is null)
    {
        return Results.NotFound(new
        {
            message = $"Employee with ID {id} was not found."
        });
    }

    employee.Name = request.Name;
    employee.Department = request.Department;

    await dbContext.SaveChangesAsync();

    return Results.Ok(employee);
});

app.MapDelete("/employees/{id:int}", async (
    int id,
    AppDbContext dbContext) =>
{
    var employee = await dbContext.Employees
        .FirstOrDefaultAsync(e => e.Id == id);

    if (employee is null)
    {
        return Results.NotFound(new
        {
            message = $"Employee with ID {id} was not found."
        });
    }

    dbContext.Employees.Remove(employee);

    await dbContext.SaveChangesAsync();

    return Results.NoContent();
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
