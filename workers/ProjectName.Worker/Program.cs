using ProjectName.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<WorkerOptions>(builder.Configuration.GetSection(WorkerOptions.SectionName));

// Replace LoggingTickHandler with your own ITickHandler (consume a queue,
// project a read model, call an external system). Hosted services are
// Singletons: resolve Scoped dependencies through IServiceScopeFactory inside
// the handler, never through the constructor.
builder.Services.AddSingleton<ITickHandler, LoggingTickHandler>();
builder.Services.AddHostedService<PeriodicWorker>();

var host = builder.Build();
host.Run();
