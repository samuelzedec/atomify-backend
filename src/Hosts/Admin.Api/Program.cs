using Admin.Api.Setups;

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureSetup();

var app = builder.Build();
app.Configure();

await app.RunAsync();