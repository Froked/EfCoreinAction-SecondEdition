﻿// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Data.Common;
using System.Linq;
using DataLayer.EfClasses;
using DataLayer.EfCode;
using Microsoft.EntityFrameworkCore;
using Test.TestHelpers;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.TestDataLayer
{
    public class Ch06_QueryFilters
    {
        public Ch06_QueryFilters(ITestOutputHelper output)
        {
            _output = output;
        }

        private readonly ITestOutputHelper _output;

        [Fact]
        public void TestCountReviewsBadOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<EfCoreContext>();
            using var context = new EfCoreContext(options);
            context.Database.EnsureCreated();
            var bookWithReviews = context.SeedDatabaseFourBooks().First(x => x.Reviews?.Any() ?? false);
            bookWithReviews.SoftDeleted = true;
            context.SaveChanges();

            context.ChangeTracker.Clear();

            //ATTEMPT
            var numReviews = context.Set<Review>().Count();

            //VERIFY
            numReviews.ShouldEqual(2);
        }

        [Fact]
        public void TestCountReviewsGoodOk()
        {
            //SETUP
            var showlog = false;
            var options = SqliteInMemory.CreateOptionsWithLogging<EfCoreContext>(log =>
            {
                if (showlog)
                    _output.WriteLine(log.Message);
            });
            using var context = new EfCoreContext(options);
            context.Database.EnsureCreated();
            var bookWithReviews = context.SeedDatabaseFourBooks().First(x => x.Reviews?.Any() ?? false);
            bookWithReviews.SoftDeleted = true;
            context.SaveChanges();

            context.ChangeTracker.Clear();

            //ATTEMPT
            showlog = true;
            var numReviews = context.Books.SelectMany(x => x.Reviews).Count();

            //VERIFY
            numReviews.ShouldEqual(0);
        }

        [Fact]
        public void TestTwoPriceOffers()
        {
            //SETUP
            int bookId;
            var options = SqliteInMemory.CreateOptions<EfCoreContext>();
            using var context = new EfCoreContext(options);
            context.Database.EnsureCreated();
            bookId = context.SeedDatabaseFourBooks().First(x => x.Promotion != null).BookId;

            context.ChangeTracker.Clear();

            //ATTEMPT
            var book = context.Books.Single(x => x.BookId == bookId);
            book.Promotion = new PriceOffer();
            var ex = Assert.Throws<DbUpdateException>(() => context.SaveChanges());

            //VERIFY
            ex.InnerException.Message.ShouldEqual("SQLite Error 19: 'UNIQUE constraint failed: PriceOffers.BookId'.");
        }

    }
}