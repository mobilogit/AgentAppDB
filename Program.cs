using Microsoft.Data.SqlClient;    // SQL client

var builder = WebApplication.CreateBuilder(args);

// 1. read connection string named "DefaultConnection"
var connStr = builder.Configuration.GetConnectionString("DefaultConnection")!;

// 2. add services
//    (static-file middleware is part of Microsoft.AspNetCore.App by default)
builder.Services.AddRouting();

var app = builder.Build();

// 3. configure middleware
app.UseDefaultFiles();   // serves wwwroot/index.html
app.UseStaticFiles();    // serves JS, CSS, images from wwwroot/

// 4. minimal JSON POST endpoint at /api/add
app.MapPost("/api/add", async (AddDto dto) =>
{
    if (string.IsNullOrWhiteSpace(dto.Text))
        return Results.BadRequest("Text cannot be empty");

    // insert into SQL DB
    await using var conn = new SqlConnection(connStr);
    await conn.OpenAsync();

    await using var cmd = new SqlCommand(
        "INSERT INTO Entries (Text) VALUES (@text);", conn);
    cmd.Parameters.AddWithValue("@text", dto.Text);
    await cmd.ExecuteNonQueryAsync();

    return Results.Ok(new { success = true });
});

// 5. start the app
app.Run();

// 6. DTO type goes after all top-level statements
record AddDto(string Text);
