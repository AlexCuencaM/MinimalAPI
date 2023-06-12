using System.Threading.Channels;
using BackgroundQueueService;
var builder = WebApplication.CreateBuilder(args);
// The max memory to use for the upload endpoint on this instance.
var maxMemory = 500 << 20;

// The max size of a single message, staying below the default LOH size of 85K.
var maxMessageSize = 80 << 10;

// The max size of the queue based on those restrictions
var maxQueueSize = maxMemory / maxMessageSize;

// Create a channel to send data to the background queue.
builder.Services.AddSingleton<Channel<ReadOnlyMemory<byte>>>((_) =>
                     Channel.CreateBounded<ReadOnlyMemory<byte>>(maxQueueSize));

// Create a background queue service.
builder.Services.AddHostedService<BackgroundQueue>();
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

static string CreateTempfilePath()
{
    var filename = $"{Guid.NewGuid()}.tmp";
    var directoryPath = Path.Combine("temp", "uploads");
    if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

    return Path.Combine(directoryPath, filename);
}

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

app.MapPost("/v2/videos", async (HttpRequest req, Stream body) =>
{
    var a = maxMessageSize
        string tempfile = CreateTempfilePath();
        using Stream stream = File.OpenWrite(tempfile);
        // await body.CopyToAsync(stream);
        
       
    
})
.WithName("CreateVideoV2")
.WithOpenApi();
// app.MapPost("/register", async (HttpRequest req, Stream body,
//                                  Channel<ReadOnlyMemory<byte>> queue) =>
// {
//     if (req.ContentLength is not null && req.ContentLength > maxMessageSize)
//     {
//         return Results.BadRequest();
//     }

//     // We're not above the message size and we have a content length, or
//     // we're a chunked request and we're going to read up to the maxMessageSize + 1. 
//     // We add one to the message size so that we can detect when a chunked request body
//     // is bigger than our configured max.
//     var readSize = (int?)req.ContentLength ?? (maxMessageSize + 1);

//     var buffer = new byte[readSize];

//     // Read at least that many bytes from the body.
//     var read = await body.ReadAtLeastAsync(buffer, readSize, throwOnEndOfStream: false);

//     // We read more than the max, so this is a bad request.
//     if (read > maxMessageSize)
//     {
//         return Results.BadRequest();
//     }

//     // Attempt to send the buffer to the background queue.
//     if (queue.Writer.TryWrite(buffer.AsMemory(0..read)))
//     {
//         return Results.Accepted();
//     }

//     // We couldn't accept the message since we're overloaded.
//     return Results.StatusCode(StatusCodes.Status429TooManyRequests);
// });
app.Run();