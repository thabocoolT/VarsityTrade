using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore; // Provides DbContext and DbContextOptions
using Microsoft.EntityFrameworkCore.Design; // Provides IDesignTimeDbContextFactory — required for migrations at design time
using Microsoft.Extensions.Configuration; // Provides IConfiguration for reading appsettings.json

namespace VarsityTrade.Infrastructure.Data
{
    //This is used only at design time to create the DbContext for migrations. It is not used at runtime.
    //It tells EF Core how to create the DbContext when running commands like "dotnet ef migrations add" or "dotnet ef database update".
    //Package Manager Console commands like Add-Migration and Update-Database
    //Without this, EF Core cannot find the connection string during migrations
    public class VarsityTradeDbContextFactory : IDesignTimeDbContextFactory<VarsityTradeDbContext>

    {
        public VarsityTradeDbContext CreateDbContext(string[] args)
        {
            //Build the configuration by reading appsettings.json from the API project
            //This is how we get the connection string at design time
            IConfigurationRoot configuration = new ConfigurationBuilder()
                //set the base path to where appsettings.json is located. This is the API project folder
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../VarsityTrade.API"))
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            //Create the DbContextOptionsBuilder and configure it to use SQL Server with the connection string from appsettings.json
            var optionsBuilder =new DbContextOptionsBuilder<VarsityTradeDbContext>();

            //Read the connection string from ConnectionStrings section
            //This must match the name key in appsettings.json
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            //Tell EF Core to use SQL Server with our connection string
            optionsBuilder.UseSqlServer(connectionString);

            //Return anew instance of our DbContext with the configured options
            return new VarsityTradeDbContext(optionsBuilder.Options);



        }
    }
}
