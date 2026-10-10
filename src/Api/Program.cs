using Api.Data;
using Api.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=app.db"));

builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>();
builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.MapOpenApi();
app.MapHealthChecks("/healthz");
app.MapHealthChecks("/readyz");

app.MapGet("/", () => Results.Ok(new { service = "dotnet-backend-api", version = "1.0.0" }))
    .WithTags("Info")
    .WithSummary("Service metadata");

var products = app.MapGroup("/products").WithTags("Products");

products.MapGet("/", async (AppDbContext db, CancellationToken ct) =>
        Results.Ok(await db.Products.AsNoTracking().ToListAsync(ct)))
    .WithSummary("List all products");

products.MapGet("/{id:int}", async (int id, AppDbContext db, CancellationToken ct) =>
        await db.Products.FindAsync(new object?[] { id }, ct) is { } product
            ? Results.Ok(product)
            : Results.NotFound())
    .WithSummary("Get a product by id");

products.MapPost("/", async (Product input, AppDbContext db, CancellationToken ct) =>
{
    db.Products.Add(input);
    await db.SaveChangesAsync(ct);
    return Results.Created($"/products/{input.Id}", input);
}).WithSummary("Create a product");

products.MapPut("/{id:int}", async (int id, Product input, AppDbContext db, CancellationToken ct) =>
{
    var product = await db.Products.FindAsync(new object?[] { id }, ct);
    if (product is null)
    {
        return Results.NotFound();
    }

    product.Name = input.Name;
    product.Price = input.Price;
    product.Stock = input.Stock;
    await db.SaveChangesAsync(ct);
    return Results.NoContent();
}).WithSummary("Update a product");

products.MapDelete("/{id:int}", async (int id, AppDbContext db, CancellationToken ct) =>
{
    var product = await db.Products.FindAsync(new object?[] { id }, ct);
    if (product is null)
    {
        return Results.NotFound();
    }

    db.Products.Remove(product);
    await db.SaveChangesAsync(ct);
    return Results.NoContent();
}).WithSummary("Delete a product");

app.Run();

public partial class Program { }
