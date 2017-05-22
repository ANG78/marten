﻿using System;
using Marten.Testing.Examples;
using Shouldly;
using Xunit;

namespace Marten.Testing.MultiTenancy
{
    public class cannot_update_documents_across_tenants : IntegratedFixture
    {
        [Fact]
        public void will_not_cross_the_streams()
        {
            StoreOptions(_ =>
            {
                _.Connection(ConnectionSource.ConnectionString)
                    .MultiTenanted();
            });

            var user = new User {UserName = "Me"};
            user.Id = Guid.NewGuid();

            using (var red = theStore.OpenSession("Red"))
            {
                red.Store(user);
                red.SaveChanges();
            }

            using (var green = theStore.OpenSession("Green"))
            {
                var greenUser = new User
                {
                    UserName = "You",
                    Id = user.Id
                };

                // Nothing should happen here
                green.Store(greenUser);
                green.SaveChanges();
            }

            // Still got the original data
            using (var query = theStore.QuerySession("Red"))
            {
                query.Load<User>(user.Id).UserName.ShouldBe("Me");
            }
        }

        [Fact]
        public void patching_respects_tenancy_too()
        {
            StoreOptions(_ =>
            {
                _.Connection(ConnectionSource.ConnectionString)
                 .MultiTenanted();
            });

            var user = new User { UserName = "Me", FirstName = "Jeremy", LastName = "Miller"};
            user.Id = Guid.NewGuid();

            using (var red = theStore.OpenSession("Red"))
            {
                red.Store(user);
                red.SaveChanges();
            }

            using (var green = theStore.OpenSession("Green"))
            {
                green.Patch<User>(user.Id).Set(x => x.FirstName, "John");
                green.SaveChanges();
            }

            using (var red = theStore.QuerySession("Red"))
            {
                var final = red.Load<User>(user.Id);
                final.FirstName.ShouldBe("Jeremy");
            }
        }

        [Fact]
        public void patching_respects_tenancy_too_2()
        {
            StoreOptions(_ =>
            {
                _.Connection(ConnectionSource.ConnectionString)
                 .MultiTenanted();
            });

            var user = new User { UserName = "Me", FirstName = "Jeremy", LastName = "Miller" };
            user.Id = Guid.NewGuid();

            using (var red = theStore.OpenSession("Red"))
            {
                red.Store(user);
                red.SaveChanges();
            }

            using (var green = theStore.OpenSession("Green"))
            {
                green.Patch<User>(x => x.UserName == "Me").Set(x => x.FirstName, "John");
                green.SaveChanges();
            }

            using (var red = theStore.QuerySession("Red"))
            {
                var final = red.Load<User>(user.Id);
                final.FirstName.ShouldBe("Jeremy");
            }
        }
    }
}