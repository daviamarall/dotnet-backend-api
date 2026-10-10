using System.Net;
using System.Net.Http.Json;
using Api.Data;
using Api.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests;

public class ProductEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ProductEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the real (SQLite) EF Core registrations so we can swap in InMemory.
                var toRemove = services
                    .Where(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>)
                                || d.ServiceType == typeof(DbContextOptions)
                                || d.ServiceType == typeof(AppDbContext)
                                || (d.ServiceType.IsGenericType
                                    && d.ServiceType.GetGenericTypeDefinition().Name
                                        .StartsWith("IDbContextOptionsConfiguration")))
                    .ToList();

                foreach (var descriptor in toRemove)
                {
                    services.Remove(descriptor);
                }

                var databaseName = $"tests-{Guid.NewGuid()}";
                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase(databaseName));
            });
        });
    }

    [Fact]
    public async Task Root_returns_service_name()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/");
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        Assert.Equal("dotnet-backend-api", body!["service"]);
    }

    [Fact]
    public async Task Health_returns_success()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/healthz");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Ready_returns_success()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/readyz");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Product_crud_lifecycle()
    {
        var client = _factory.CreateClient();
        var input = new Product { Name = "Keyboard", Price = 199.90m, Stock = 10 };

        var created = await client.PostAsJsonAsync("/products", input);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        var product = await created.Content.ReadFromJsonAsync<Product>();
        Assert.NotNull(product);
        Assert.True(product!.Id > 0);

        var fetched = await client.GetFromJsonAsync<Product>($"/products/{product.Id}");
        Assert.Equal("Keyboard", fetched!.Name);

        var list = await client.GetFromJsonAsync<List<Product>>("/products");
        Assert.Contains(list!, p => p.Id == product.Id);

        var update = new Product { Name = "Mechanical Keyboard", Price = 349.90m, Stock = 5 };
        var put = await client.PutAsJsonAsync($"/products/{product.Id}", update);
        Assert.Equal(HttpStatusCode.NoContent, put.StatusCode);

        var updated = await client.GetFromJsonAsync<Product>($"/products/{product.Id}");
        Assert.Equal("Mechanical Keyboard", updated!.Name);

        var deleted = await client.DeleteAsync($"/products/{product.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);

        var missing = await client.GetAsync($"/products/{product.Id}");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    [Fact]
    public async Task Get_unknown_product_returns_404()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/products/999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
