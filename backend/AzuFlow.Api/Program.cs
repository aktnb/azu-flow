using AzuFlow.Azure;

var builder = WebApplication.CreateBuilder(NormalizeAuthenticationMethodArgs(args));

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddOpenApi();
builder.Services.AddAzuFlowAzure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseHttpsRedirection();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();

static string[] NormalizeAuthenticationMethodArgs(string[] args)
{
    var normalizedArgs = new List<string>(args.Length);

    for (var i = 0; i < args.Length; i++)
    {
        var arg = args[i];

        if (arg.StartsWith("--auth-method=", StringComparison.OrdinalIgnoreCase))
        {
            normalizedArgs.Add($"--Azure:AuthenticationMethod={arg["--auth-method=".Length..]}");
            continue;
        }

        if (arg.StartsWith("--azure-auth-method=", StringComparison.OrdinalIgnoreCase))
        {
            normalizedArgs.Add($"--Azure:AuthenticationMethod={arg["--azure-auth-method=".Length..]}");
            continue;
        }

        if (IsAuthenticationMethodSwitch(arg) && i + 1 < args.Length)
        {
            normalizedArgs.Add("--Azure:AuthenticationMethod");
            normalizedArgs.Add(args[++i]);
            continue;
        }

        normalizedArgs.Add(arg);
    }

    return normalizedArgs.ToArray();
}

static bool IsAuthenticationMethodSwitch(string arg) =>
    string.Equals(arg, "--auth-method", StringComparison.OrdinalIgnoreCase) ||
    string.Equals(arg, "--azure-auth-method", StringComparison.OrdinalIgnoreCase);
