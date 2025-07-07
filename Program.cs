using Microsoft.AspNetCore.Builder;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

// 1) build the host
var builder = WebApplication.CreateBuilder(args);

// 2) read the connection string
var connStr = builder.Configuration.GetConnectionString("DefaultConnection");

// 3) optional: enable directory browsing
builder.Services.AddDirectoryBrowser();

var app = builder.Build();

// 4) static file support (serves wwwroot/index.html + *.js, css, etc.)
app.UseDefaultFiles();
app.UseStaticFiles();

// 5) JSON POST endpoint: accepts { "text": "…" }
app.MapPost("/api/add", async (AddDto dto) =>
{
    if (string.IsNullOrWhiteSpace(dto.Text))
        return Results.BadRequest("Text cannot be empty");

    await using var conn = new SqlConnection(connStr);
    await conn.OpenAsync();
    await using var cmd = new SqlCommand(
        "INSERT INTO Entries (Text) VALUES (@text);", conn);
    cmd.Parameters.AddWithValue("@text", dto.Text);
    await cmd.ExecuteNonQueryAsync();

    return Results.Ok(new { success = true });
});

// 6) start listening
app.Run();

// --- any type declarations go *after* all the top-level code above ---

// DTO for binding incoming JSON
record AddDto(string Text);
