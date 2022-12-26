using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(setup => setup.SwaggerDoc("v1", new OpenApiInfo()
{
    Description = "A Free and Open Source API to fetch dad-jokes",
    Title = "Joke API",
    Version = "v1",
    Contact = new OpenApiContact()
    {
        Name = "anuraj",
        Url = new Uri("https://dotnetthoughts.net")
    }
}));

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Joke API v1");
    c.RoutePrefix = string.Empty;
});

var filename = Path.Combine(Directory.GetCurrentDirectory(), "source", "jokes.json");
var jokes = JsonSerializer.Deserialize<List<Joke>>(await File.ReadAllTextAsync(filename));

//Redirect to the Swagger endpoint
app.MapGet("/", () => Results.Redirect("/index.html")).ExcludeFromDescription();

app.MapGet("/jokes/random", () =>
{
    if (jokes == null)
    {
        return Results.Problem();
    }

    var random = new Random();
    var number = random.Next(1, jokes.Count);
    return Results.Ok(jokes[number]);
})
.Produces<Joke>(200, "application/json");

app.MapGet("/jokes/ten", () =>
{
    if (jokes == null)
    {
        return Results.Problem();
    }

    var random = new Random();
    var jokeList = new List<Joke>(10);
    for (int index = 0; index < 10; index++)
    {
        var number = random.Next(1, jokes.Count);
        jokeList.Add(jokes[number]);
    }

    return Results.Ok(jokeList);
})
.Produces<Joke>(200, "application/json")
.ProducesProblem(500);

app.MapGet("/prime/{start}/{end}", async (int start, int end) =>
{
    var numbers =
        Enumerable.Range(start, end - start)
            .Where(IsPrime)
            .Select(number => number)
            .ToList();
    app.Logger.LogInformation("Prime numbers between {start} and {end} are {numbers}", start, end, numbers);
    await Task.Delay(1000);
    return Results.Ok(numbers);
})
.Produces<Joke>(200, "application/json")
.ProducesProblem(500).ExcludeFromDescription();

bool IsPrime(int number)
{
    bool CalculatePrime(int value)
    {
        var possibleFactors = Math.Sqrt(number);
        for (var factor = 2; factor <= possibleFactors; factor++)
        {
            if (value % factor == 0)
            {
                return false;
            }
        }

        return true;
    }

    return number > 1 && CalculatePrime(number);
}

app.MapGet("/jokes/{number:int}", (int number) =>
{
    if (jokes == null)
    {
        return Results.StatusCode(500);
    }
    if ((number + 1) > jokes.Count)
    {
        return Results.NotFound();
    }

    return Results.Ok(jokes[(number + 1)]);
})
.Produces<Joke>(200, "application/json")
.ProducesProblem(404)
.ProducesProblem(500);

app.MapGet("/jokes/{type}/random", (string type) =>
{
    if (jokes == null)
    {
        return Results.StatusCode(500);
    }

    var jokesUnderType = jokes
        .Where(joke => joke.Type != null && joke.Type.Equals(type, StringComparison.OrdinalIgnoreCase)).ToList();
    var random = new Random();
    var number = random.Next(1, jokesUnderType.Count());
    return Results.Ok(jokesUnderType[number]);
})
.Produces<Joke>(200, "application/json")
.ProducesProblem(500);

app.MapGet("/jokes/{type}/ten", (string type) =>
{
    if (jokes == null)
    {
        return Results.StatusCode(500);
    }

    var jokesUnderType = jokes
        .Where(joke => joke.Type != null && joke.Type.Equals(type, StringComparison.OrdinalIgnoreCase)).ToList();
    if (jokesUnderType.Count < 10)
    {
        return Results.Ok(jokesUnderType);
    }
    var random = new Random();
    var jokeList = new List<Joke>(10);
    for (int index = 0; index < 10; index++)
    {
        var number = random.Next(1, jokesUnderType.Count());
        jokeList.Add(jokesUnderType[number]);
    }

    return Results.Ok(jokeList);
})
.Produces<List<Joke>>(200, "application/json")
.ProducesProblem(500);

app.MapGet("/jokes/{type}/{number:int}", (string type, int number) =>
{
    if (jokes == null)
    {
        return Results.StatusCode(500);
    }

    var jokesUnderType = jokes
        .Where(joke => joke.Type != null && joke.Type.Equals(type, StringComparison.OrdinalIgnoreCase)).ToList();
    if (jokesUnderType.Count < number)
    {
        return Results.Ok(jokesUnderType);
    }
    var random = new Random();
    var jokeList = new List<Joke>(10);
    for (int index = 0; index < 10; index++)
    {
        var randomNumber = random.Next(1, jokesUnderType.Count());
        jokeList.Add(jokesUnderType[randomNumber]);
    }

    return Results.Ok(jokeList);
})
.Produces<List<Joke>>(200, "application/json")
.ProducesProblem(500);


app.Run();

public class Joke
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("setup")]
    public string? Setup { get; set; }

    [JsonPropertyName("punchline")]
    public string? Punchline { get; set; }
}