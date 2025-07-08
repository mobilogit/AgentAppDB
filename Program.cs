using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// Wczytaj connection string z konfiguracji (np. z Azure App Service)
var connStr = builder.Configuration.GetConnectionString("DefaultConnection")!;

builder.Services.AddRouting();

var app = builder.Build();

// Obsługa plików statycznych (np. index.html, app.js)
app.UseDefaultFiles();
app.UseStaticFiles();

// Endpoint POST do dodawania wpisów
app.MapPost("/api/add", async (AddDto dto) =>
{
    if (string.IsNullOrWhiteSpace(dto.Text))
        return Results.BadRequest("Text cannot be empty");

    try
    {
        await using var conn = new SqlConnection(connStr);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(
            "INSERT INTO Entries (Text) VALUES (@text);", conn);
        cmd.Parameters.AddWithValue("@text", dto.Text);
        await cmd.ExecuteNonQueryAsync();

        return Results.Ok(new { success = true });
    }
    catch (Exception ex)
    {
        Console.WriteLine("DB ERROR (add): " + ex.ToString());
        return Results.Problem("Database error: " + ex.Message);
    }
});

// Endpoint GET do testowania połączenia z bazą danych
app.MapGet("/api/test-db-connection", async () =>
{
    try
    {
        await using var conn = new SqlConnection(connStr);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand("SELECT 1", conn);
        var result = await cmd.ExecuteScalarAsync();

        return Results.Ok(new { connected = true, result });
    }
    catch (Exception ex)
    {
        Console.WriteLine("DB ERROR (test): " + ex.ToString());
        return Results.Problem("Database connection failed: " + ex.Message);
    }
});

app.Run();

// DTO do odbierania danych z JSON-a
record AddDto(string Text);
