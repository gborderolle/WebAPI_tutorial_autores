using WebAPI_tutorial_recursos;

var builder = WebApplication.CreateBuilder(args);

// Curso Udemy: https://www.udemy.com/course/construyendo-web-apis-restful-con-aspnet-core/learn/lecture/13815592#overview

var startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services);

var app = builder.Build();

var serviceLogger = (ILogger<Startup>)app.Services.GetService(typeof(ILogger<Startup>));

startup.Configure(app, app.Environment);

app.Run();
