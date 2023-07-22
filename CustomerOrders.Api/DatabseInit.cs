using CustomerOrder.Infrastructure.Models;
using CustomerOrder.Infrastructure;
using Microsoft.AspNetCore.Identity;

namespace CustomerOrders.Api
{
    public static class DatabaseInit
    {
        public async static Task<IApplicationBuilder> SeedData(this IApplicationBuilder app)
        {
            ArgumentNullException.ThrowIfNull(app, nameof(app));

            using var scope = app.ApplicationServices.CreateScope();
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<ApplicationDbContext>();
                await SeedData(context);
            }
            catch (Exception ex)
            {
            }

            return app;
        }

        private async static Task SeedData(ApplicationDbContext context)
        {
            const string ADMIN_ID = "a18be9c0-aa65-4af8-bd17-00bd9344e575";
            context.Database.EnsureCreated();
            if (await context.Users.FindAsync(ADMIN_ID) == null)
            {
                var hasher = new PasswordHasher<IdentityUser>();
                Customer appUser = new Customer
                {
                    Id = ADMIN_ID,
                    Address = "Address",
                    UserName = "admin@CustomerOrders.com",
                    NormalizedUserName = "admin@CustomerOrders.com",
                    Email = "admin@CustomerOrders.com",
                    NormalizedEmail = "admin@CustomerOrders.com",
                    EmailConfirmed = true,
                    PasswordHash = hasher.HashPassword(new IdentityUser(), "123456aA!"),
                    PhoneNumber = "01002057457",
                    PhoneNumberConfirmed = true,
                    SecurityStamp = string.Empty
                };
                context.Users.Add(appUser);
            }
            
            
            
            List<Guid> Products = new List<Guid>() {
            new Guid("1E082A57-C3B7-41B6-AF56-179542087041"),
            new Guid("1E082A57-C3B7-41B6-AF56-179542087042"),
            new Guid("1E082A57-C3B7-41B6-AF56-179542087043") 
            };
            Random rnd = new Random();
            foreach (var p in Products)
            {
                if (await context.Products.FindAsync(p) == null)
                {
                    await context.Products.AddAsync(new Product()
                    {
                        Id = p,
                        Name = "Product " + rnd.Next(1, 100),
                        Description = "Description",
                        Price = rnd.Next(5, 50),
                        Quantity = rnd.Next(5, 50)
                    }
                    );
                }
            }
            await context.SaveChangesAsync();
        }
    }
}
