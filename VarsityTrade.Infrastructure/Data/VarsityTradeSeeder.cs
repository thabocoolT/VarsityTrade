using Microsoft.EntityFrameworkCore; // Provides AnyAsync and other EF Core query methods
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using VarsityTrade.Core.Entities; // Provides access to all entity classes
using SystemSettings = VarsityTrade.Core.Entities.SystemSettings; // Resolves conflict with built-in SystemSettings type

namespace VarsityTrade.Infrastructure.Data
{
    //This class is responsible for seeding the database with initial data. It can be used to populate the database with default values, test data, or any other necessary information when the application is first run or during development.
    //It runs on application startup and only inserts data if the table is empty
    //This prevents duplicate data if the application restarts

    public class VarsityTradeSeeder
    {
        //The DbContxt is injected so we can interact with the database
        private readonly VarsityTradeDbContext _context;

        //Constructor receives the DbContext via dependency injection
        public VarsityTradeSeeder(VarsityTradeDbContext context)
        {
            _context = context;
        }

        //Main entry point-called on startup to seed the database
        //Each method checks if data already exists before inserting to avoid duplicates
        public async Task SeedAsync()
        {
            //Seed each table in order - lookup tables first
            //Order matters because some tables depend on others
            await SeedUniversitiesAsync();
            await SeedConditionsAsync();
            await SeedCategoriesAsync();
            await SeedListingStatusesAsync();
            await SeedSystemSettingsAsync();


        }
        //-------------------------------------------
        //Universities
        //-------------------------------------------

        private async Task SeedUniversitiesAsync()
        {
            //Check if Universities already exist in the database
            //This prevents duplicate data if the application restarts
            if (await _context.Universities.AnyAsync())
                return;

            //Create the list of all 21 South African universities to seed into the database
            var universities = new List<University>
            {
                new University { Name = "North-West University (Vaal Campus)", ShortName="VTC", City="Vanderbilpark", Province="Gauteng", IsActive=true},
                new University { Name = "North-West University", ShortName="NWU Potch", City="Potchefstroom", Province="North West",IsActive=true },
                new University { Name = "University of Cape Town", ShortName="UCT", City="Cape Town", Province="Western Cape",IsActive=true  },
                new University { Name = "University of the Witwatersrand",ShortName="Wits", City="Johannesburg", Province="Gauteng",IsActive=true  },
                new University {Name = "Stellenbosch University", ShortName = "SU", City = "Stellenbosch", Province = "Western Cape", IsActive = true},
                new University {Name = "University of Pretoria", ShortName = "UP", City = "Pretoria", Province = "Gauteng", IsActive = true},
                new University {Name = "University of KwaZulu-Natal", ShortName = "UKZN", City = "Durban", Province = "KwaZulu Natal", IsActive = true},
                new University {Name = "University of Johannesburg", ShortName = "UJ", City = "Johannesburg", Province = "Gauteng", IsActive = true},
                new University {Name = "University of the Free State", ShortName = "UFS", City = "Bloemfontein", Province = "Free State", IsActive = true},
                new University {Name = "Rhodes University", ShortName = "RU", City = "Makhanda", Province = "Eastern Cape", IsActive = true},
                new University {Name = "University of South Africa (UNISA)", ShortName = "UNISA", City = "Pretoria", Province = "Gauteng", IsActive = true},
                new University {Name = "Nelson Mandela University", ShortName = "NMU", City = "Gqeberha", Province = "Eastern Cape", IsActive = true},
                new University {Name = "University of Fort Hare", ShortName = "UFH", City = "Alice", Province = "Eastern Cape", IsActive = true},
                new University {Name = "Tshwane University of Technology", ShortName = "TUT", City = "Pretoria", Province = "Gauteng", IsActive = true},
                new University {Name = "Cape Peninsula University of Technology", ShortName = "CPUT", City = "Cape Town", Province = "Western Cape", IsActive = true},
                new University {Name = "Durban University of Technology", ShortName = "DUT", City = "Durban", Province = "KwaZulu Natal", IsActive = true},
                new University {Name = "Central University of Technology", ShortName = "CUT", City = "Bloemfontein", Province = "Free State", IsActive = true},
                new University {Name = "Vaal University of Technology", ShortName = "VUT", City = "Vanderbilpark", Province = "Gauteng", IsActive = true},
                new University {Name = "Mangosuthu University of Technology", ShortName = "MUT", City = "Durban", Province = "KwaZulu Natal", IsActive = true},
                new University {Name = "Walter Sisulu University", ShortName = "WSU", City = "Mthatha", Province = "KwaZulu Natal", IsActive = true},
                new University {Name = "Sol Plaatje University", ShortName = "SPU", City = "Kimberly", Province = "Northern Cape", IsActive = true},
                new University { Name = "University of Limpopo", ShortName = "UL",  City = "Polokwane",Province = "Limpopo", IsActive = true },
                new University { Name = "University of Mpumalanga", ShortName = "UMP",  City = "Mbombela",Province = "Mpumalanga", IsActive = true }

            };
            //Add the universities to the database context
            await _context.Universities.AddRangeAsync(universities);
            await _context.SaveChangesAsync(); //Save changes to the database
        }
        //________________________________________________________
        //CONDITIONS
        //________________________________________________________

        private async Task SeedConditionsAsync()
        {
            //skip if conditions already exist
            if (await _context.Condition.AnyAsync())
                return;

            //These are the only valid item conditions on the platform
            //Every item listed must be in one of these conditions
            var conditions = new List<Condition>
            {
                new Condition { Name = "New" },
                new Condition { Name = "Like New" },
                new Condition { Name = "Used" },
                new Condition { Name = "Fair" },
            };

            await _context.Condition.AddRangeAsync(conditions);
            await _context.SaveChangesAsync(); //Save changes to the database


        }
        //________________________
        //LISTING STATUSES
        //________________________
        private async Task SeedListingStatusesAsync()
        {
            //Skip if statuses already exist
            if (await _context.ListingStatuses.AnyAsync())
                return;

            //These stuses contro the full lifecycle of a listing on the platform
            var statuses = new List<ListingStatus>
                {
                    new ListingStatus { Name = "Active" },
                    new ListingStatus { Name = "Sold" },
                    new ListingStatus { Name = "Draft" },
                    new ListingStatus { Name = "Expired" },
                    new ListingStatus { Name = "Deleted" },
                    new ListingStatus { Name = "Reserved" }

                };
            await _context.ListingStatuses.AddRangeAsync(statuses);
            await _context.SaveChangesAsync(); //Save changes to the database
        }

        //----------------------------
        //CATEGORIES
        //---------------------------

        private async Task SeedCategoriesAsync()
        {
            //Skip if categories already exist
            if (await _context.Categories.AnyAsync())
                return;

            //Parent categories are the main categories on the platform. Each category can have multiple subcategories
            //Subcategories are then created referencing their parent
            //This supports the self-referencing relationship in the Category entity

            //Step 1: Create parent categories
            var electronics = new Category { Name = "Electronics", IconName = "ti-device-laptop", ParentCategoryId = null };
            var studyMaterials = new Category { Name = "Study Materials", IconName = "ti-book", ParentCategoryId = null };
            var furniture = new Category { Name = "Furniture", IconName = "ti-home", ParentCategoryId = null };
            var clothing = new Category { Name = "Clothing & Accessories", IconName = "ti-shirt", ParentCategoryId = null };
            var household = new Category { Name = "Household & Kitchen", IconName = "ti-home", ParentCategoryId = null };
            var sports = new Category { Name = "Sports & Outdoor", IconName = "ti-basketball", ParentCategoryId = null };
            var services = new Category { Name = "Services", IconName = "ti-dots", ParentCategoryId = null };
            var accomodation = new Category { Name = "Accommodation", IconName = "ti-building", ParentCategoryId = null };
            var other = new Category { Name = "Other", IconName = "ti-more", ParentCategoryId = null };

            //add parent categories to the database and save changes to generate their IDs
            //We must save before creating subcategories because subcategories reference their parent category's ID
            await _context.AddRangeAsync(
                electronics, studyMaterials, furniture, clothing, household, sports, services, accomodation, other

                );
            await _context.SaveChangesAsync();

            //Step 2: Create subcategories referencing their parent categories
            var subcategories = new List<Category>
            {
                //Electronics subcategories
                new Category { Name = "Laptop & Computers",   IconName = "ti-device-laptop",    ParentCategoryId = electronics.CategoryId },
                new Category { Name = "Phones & Tablets",     IconName = "ti-device-mobile",    ParentCategoryId = electronics.CategoryId },
                new Category { Name = "Audio & Headphones",   IconName = "ti-headphones",       ParentCategoryId = electronics.CategoryId },
                new Category { Name = "Gaming",               IconName = "ti-device-gamepad",   ParentCategoryId = electronics.CategoryId },
                new Category { Name = "Cameras",              IconName = "ti-camera",           ParentCategoryId = electronics.CategoryId },
                new Category { Name = "Accessories & Cables", IconName = "ti-cable",            ParentCategoryId = electronics.CategoryId },
                new Category { Name = "Printers",             IconName = "ti-printer",          ParentCategoryId = electronics.CategoryId },

                //Study Materials subcategories
                new Category { Name = "Textbooks",            IconName = "ti-book",             ParentCategoryId = studyMaterials.CategoryId },
                new Category { Name = "Notes & Study Guides", IconName = "ti-notes",            ParentCategoryId = studyMaterials.CategoryId },
                new Category { Name = "Stationery",           IconName = "ti-pencil",           ParentCategoryId = studyMaterials.CategoryId },
                new Category { Name = "Calculators",          IconName = "ti-calculator",       ParentCategoryId = studyMaterials.CategoryId },


                //Furniture subcategories
                new Category { Name = "Desks & Chairs",       IconName = "ti-armchair",         ParentCategoryId = furniture.CategoryId },
                new Category { Name = "Bedding",              IconName = "ti-bed",              ParentCategoryId = furniture.CategoryId },
                new Category { Name = "Storage",              IconName = "ti-archive",          ParentCategoryId = furniture.CategoryId },

                //Clothing subcategories
                new Category { Name = "Mens Clothing",        IconName = "ti-shirt",           ParentCategoryId = clothing.CategoryId },
                new Category { Name = "Womens Clothing",      IconName = "ti-shirt",           ParentCategoryId = clothing.CategoryId },
                new Category { Name = "Shoes",                IconName = "ti-shoe",            ParentCategoryId = clothing.CategoryId },
                new Category { Name = "Bags & Backpack",      IconName = "ti-backpack",      ParentCategoryId = clothing.CategoryId },

                //Sports subcategories
                new Category { Name = "Gym Equipment",         IconName = "ti-barbell",         ParentCategoryId = sports.CategoryId },
                new Category { Name = "Sports Gear",           IconName = "ti-barbell",         ParentCategoryId = sports.CategoryId },
                new Category { Name = "Outdoor Sports",        IconName = "ti-barbell",         ParentCategoryId = sports.CategoryId },
                new Category { Name = "Bicycles",              IconName = "ti-bicycles",         ParentCategoryId = sports.CategoryId },

                //Household subcategories
                new Category { Name = "Appliances",           IconName = "ti-plug",            ParentCategoryId = household.CategoryId },
                new Category { Name = "Kitchenware",          IconName = "ti-tools-kitchen",            ParentCategoryId = household.CategoryId },
                new Category { Name = "Decor",                IconName = "ti-lamp",            ParentCategoryId = household.CategoryId },
                new Category { Name = "Clean Supplies",       IconName = "ti-broom",            ParentCategoryId = household.CategoryId },

                //Services subcategories
                new Category { Name = "Tutoring",              IconName = "ti-school",          ParentCategoryId = services.CategoryId },
                new Category { Name = "Design & Creative",     IconName = "ti-creative",          ParentCategoryId = services.CategoryId },
                new Category { Name = "Programming & Tech",    IconName = "ti-laptop",          ParentCategoryId = services.CategoryId },
                new Category { Name = "Moving Assistance",     IconName = "ti-box",          ParentCategoryId = services.CategoryId },

                //Accommodation subcategories
                new Category { Name = "Room Takeover",          IconName = "ti-door",          ParentCategoryId = accomodation.CategoryId },
                new Category { Name = "Parking",                IconName = "ti-car",          ParentCategoryId = accomodation.CategoryId },
                new Category { Name = "Storage Space",          IconName = "ti-archive",          ParentCategoryId = accomodation.CategoryId },



            };
            //add all subcategories and save to the DB
            await _context.Categories.AddRangeAsync(subcategories);
            await _context.SaveChangesAsync();

        }


        //-------------------------------
        //System Settings
        //-------------------------------

        private async Task SeedSystemSettingsAsync()
        {
            //Skip if system settings already exist
            if (await _context.SystemSettings.AnyAsync())
                return;

            //These are the default platform-wide configuration values
            //Admin can update these values through the admin panel at runtime

            var settings = new List<SystemSettings>
            {
                new SystemSettings
                {
                    Key         ="MaintenancMode",
                    Value       ="false",
                    Description ="set to true to enaable platform maintenance mode"
                },

                new SystemSettings
                {
                    Key     ="MaxListingImages",
                    Value   ="6",
                    Description="Number of images allowed per listing"
                },

                new SystemSettings
                {
                    Key         ="EnableTrades",
                    Value       ="true",
                    Description ="Enable or disable trade offer functionality platform-wide",

                },

                new SystemSettings
                {
                    Key         ="ListingExpirydays",
                    Value       ="90",
                    Description ="Number of days before a listing automatically expires",
                },
            };

            await _context.SystemSettings.AddRangeAsync(settings);
            await _context.SaveChangesAsync();

        }



        
    }
}
