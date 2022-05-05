using Microsoft.EntityFrameworkCore;
using Offers.Orchestration;
using Offers.Database;
using Database.Tables;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// DB connection creation
// User, Password and Database are configured in Docker/init/db/initdb.sql file
// MariaDB -> var connectionString = "server=mariadb;user=Transport;password=transport;database=Transport";
//var connectionString = @"Host=psql;Username=Transport;Password=transport;Database=Transport";
// setting up DB as app service, some logging should be disabled for production
builder.Services.AddDbContext<OffersContext>(
    dbContextOptions => dbContextOptions
        .UseNpgsql(builder.Configuration.GetConnectionString("PsqlConnection"))
        // The following three options help with debugging, but should
        // be changed or removed for production.
        .LogTo(Console.WriteLine, LogLevel.Information)
        .EnableSensitiveDataLogging()
        .EnableDetailedErrors()
);
builder.Services.AddMassTransit(cfg =>
{
    cfg.UsingRabbitMq();
    cfg.AddSagaStateMachine<OfferStateMachine, StatefulOffer>().InMemoryRepository();
});
var app = builder.Build();
//var manager = new EventManager(app);
//manager.ListenForEvents();

// example of inserting new Data to Database, Ensure created should be called at init of service (?)
using (var contScope = app.Services.CreateScope())
using (var context = contScope.ServiceProvider.GetRequiredService<OffersContext>())
{
    // Ensure Deleted possible to use for testing
    //context.Database.EnsureDeleted();
    context.Database.EnsureCreated();
    var test = new Trip { TransportId = 0, Destination = "Destination", BeginDate = new DateOnly(2022, 03, 01), EndDate = new DateOnly(2022, 04, 01), HotelName = "Hotel", HotelId = 0, NumberOfPeople = 4};
    context.Trips.Add(test); // add new item
    context.SaveChanges(); // save to DB
    Console.WriteLine("Done inserting test data");
    // manager.Publish(new ReserveTransportEvent(1));
}
app.Run();