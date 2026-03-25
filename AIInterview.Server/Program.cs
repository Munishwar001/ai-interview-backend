using AIInterview.Server;

var builder = WebApplication.CreateBuilder(args);

ServerConfiguration.ConfigureServices(builder);

var app = builder.Build();

await ServerConfiguration.ConfigurePipeline(app);

app.Run();