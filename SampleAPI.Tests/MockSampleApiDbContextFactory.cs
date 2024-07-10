using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SampleAPI.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleAPI.Tests
{
    /// <summary>
    /// Mock database provided for your convenience.
    /// </summary>
    internal static class MockSampleApiDbContextFactory
    {
        public static SampleApiDbContext GenerateMockContext()
        {
            var options = new DbContextOptionsBuilder<SampleApiDbContext>()
                .UseInMemoryDatabase(databaseName: "mock_SampleApiDbContext")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            var context = new SampleApiDbContext(options);

            SeedTestData(context);

            return context;
        }

        private static void SeedTestData(SampleApiDbContext context)
        {
            // Add test data
            var orders = new List<Order>
            {
                new Order
                {
                    Id = 1,
                    OrderDate = DateTime.Now.AddDays(-5),
                    Description = "Order 1 Description",
                    CustomerName = "Customer 1",
                    WasOrderInvoiced = true,
                    WasOrderDeleted = false
                },
                new Order
                {
                    Id = 2,
                    OrderDate = DateTime.Now.AddDays(-3),
                    Description = "Order 2 Description",
                    CustomerName = "Customer 2",
                    WasOrderInvoiced = false,
                    WasOrderDeleted = false
                }
            };

            context.Orders.AddRange(orders);
            context.SaveChanges();
        }
    }
}
