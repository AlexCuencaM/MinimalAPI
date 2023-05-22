var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// builder.WebHost.ConfigureKestrel(serverOptions =>
// {
//     serverOptions.Limits.MaxRequestBodySize = long.MaxValue;
// });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapPost("/videos", async (HttpRequest res) =>
{
    byte[] fileBytes;
    string? Id = res.Form["Id"];
    IFormFile? file = res.Form.Files
        .FirstOrDefault(f => f.Length > 0 && f.Name.Contains("Video"));

    
    if(string.IsNullOrEmpty(Id)){
        return Results.BadRequest("Id field is required");
    }
    if(file is null){
        return Results.BadRequest("Video field is required");
    }
    // await using Stream stream = file!.();
        
    using(MemoryStream memoryStream = new()){
        await file!.CopyToAsync(memoryStream);
        fileBytes = memoryStream.ToArray();
    }
    return Results.Ok($"Video recorded successfully");
})
.WithName("CreateVideo")
.WithOpenApi();

app.Run();