using BackgroundQueueService;
using Store;
var builder = WebApplication.CreateBuilder(args);
var videoConnection = builder.Configuration["Videos:StorageConnection"];
// Create a background queue service.
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = int.MaxValue;
});
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options => {
    options.MultipartBodyLengthLimit  = int.MaxValue;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
//for small files
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
app.MapPost("/v2/queue", async (HttpRequest req) =>
{
    MyStorageQueue myFirstQueue = new("my-first-queue",videoConnection ?? string.Empty);
    await myFirstQueue.CreateQueue();
    await myFirstQueue.InsertMessage("Hi there :)");
    string result = await myFirstQueue.PeekMessage();
    return Results.Ok(result);
})
.WithName("CreateQueue")
.WithOpenApi();

app.MapPost("/v2/Videos", async (HttpRequest req) =>
{
    try{
        var form = await req.ReadFormAsync();
        string? Id = form["Id"];
        IFormFile? file = form.Files["Video"];
        
        // return Results.Ok(file?.ContentDisposition);
        // form.Files
        //     .FirstOrDefault(f => f.Length > 0 && f.Name.Contains("Video"));
        if(string.IsNullOrEmpty(Id)){
            return Results.BadRequest("Id field is required");
        }
        if(file is null){
            return Results.BadRequest("Video field is required");
        }
        BlobStore myNewBlobStorage = new(videoConnection ?? string.Empty, new UploadFile(file.FileName));
        await myNewBlobStorage.CreateContainerClient();

        var response = await myNewBlobStorage.UploadBlob(file);

        return Results.Ok(response.Value);
    }
    catch(Exception e){
        return Results.BadRequest(e.Message);
    }
})
.WithName("CreateBlobVideo")
.WithOpenApi();

app.Run();