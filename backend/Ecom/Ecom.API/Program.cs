using Ecom.infrastructure;
using Ecom.infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.infrastructureConfiguration(builder.Configuration);
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Dev-only seeding: HasData would run on every environment, including
    // Production. DbInitializer keeps this to the local dev database.
    using var scope = app.Services.CreateScope();
    await DbInitializer.SeedAsync(scope.ServiceProvider.GetRequiredService<AppDbContext>());

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();