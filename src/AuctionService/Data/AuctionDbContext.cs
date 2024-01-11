using AuctionService.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Data;

// this is used to generate migration schema - using dotnet ef tool
// this is the ORM (Object Relational Mapper) that maps between C# and SQL
public class AuctionDbContext : DbContext
{
    public AuctionDbContext(DbContextOptions options) : base(options)
    {
    }


    public DbSet<Auction> Auctions { get; set; }
}
