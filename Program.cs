using Microsoft.AspNetCore.Builder;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Load the connection string named "DefaultConnection" from appsettings.json or Azure Configuration
var connStr = builder.Configuration.GetConnectionString("DefaultConnection");

// Add support for serving directory listings (optional)
builder.Services.AddDirectoryBrowser();

var app = builder.Build();

// Serve default files (e.g. index.html) from wwwroot/
app.UseDefaultFiles();

// Serve static files (JavaScript, CSS, images) from wwwroot/
app.UseStaticFiles();

// Simple DTO to bind incoming JSON payload
record AddDto(string Text);

// POST /api/add
// Accepts a JSON object { "text": "some text" }, inserts it into the database,
// and returns a JSON response indicating success or failure.
app.MapPost("/api/add", async (AddDto dto) =>
{
    // Reject empty or whitespace-only values
    if (string.IsNullOrWhiteSpace(dto.Text))
        return Results.BadRequest("Text cannot be empty");

    // Open a new connection to SQL Server
    await using var conn = new SqlConnection(connStr);
    await conn.OpenAsync();

    // Prepare and execute an INSERT command with a parameter to avoid SQL injection
    await using var cmd = new SqlCommand(
        "INSERT INTO Entries (Text) VALUES (@text);", conn);
    cmd.Parameters.AddWithValue("@text", dto.Text);

    await cmd.ExecuteNonQueryAsync();

    // Return HTTP 200 OK with a success payload
    return Results.Ok(new { success = true });
});

app.Run();