using Microsoft.EntityFrameworkCore;

namespace Server.DB;

public class User
{
    public string UserId { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public DateOnly? LastSignIn { get; set; }
    public string? Note { get; set; }
};

internal class CosmosDBContext: DbContext
{
    public static Lazy<string> connectionStringFromFile = new Lazy<string>(() =>
    {
        return System.IO.File.ReadAllText("./keys/db_connection_string.txt");
    });

    public DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseCosmos(
                connectionString: connectionStringFromFile.Value,
                databaseName: "VolvoWroclawConf2025DB",
                cosmosOptionsAction: options =>
                {
                    options.ConnectionMode(Microsoft.Azure.Cosmos.ConnectionMode.Direct);
                    options.MaxRequestsPerTcpConnection(16);
                    options.MaxTcpConnectionsPerEndpoint(32);
                })
            //.LogTo(Console.WriteLine)
            ;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasManualThroughput(1000);
            modelBuilder.Entity<User>()
                .ToContainer(nameof(User))
                .HasPartitionKey(x => x.UserId)
                .HasKey(x => x.UserId);
    }
}
