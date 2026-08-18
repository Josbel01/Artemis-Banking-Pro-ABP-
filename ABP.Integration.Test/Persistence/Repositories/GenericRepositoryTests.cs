using ABP.Core.Domain.Entities;
using ABP.Infrastructure.Persistence.Contexts;
using ABP.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ABP.Integration.Tests.Persistence.Repositories
{
    public class GenericRepositoryTests
    {
        private readonly DbContextOptions<ArtemisBankingAppContext> _dbOptions;

        public GenericRepositoryTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ArtemisBankingAppContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_GenericRepo_{Guid.NewGuid()}")
                .Options;
        }

        [Fact]
        public async Task AddAsync_Should_Add_Entity()
        {
            // Arrange
            using var context = new ArtemisBankingAppContext(_dbOptions);
            var repo = new GenericRepository<Commerce>(context);
            var entity = new Commerce { Id = 0, Name = "Test Commerce", Rnc = "123456789" };

            // Act
            var result = await repo.AddAsync(entity);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task AddRangeAsync_Should_Add_Multiple_Entities()
        {
            // Arrange
            using var context = new ArtemisBankingAppContext(_dbOptions);
            var repo = new GenericRepository<Commerce>(context);
            var items = new List<Commerce>
            {
                new Commerce { Id = 0, Name = "C1", Rnc = "111" },
                new Commerce { Id = 0, Name = "C2", Rnc = "222" }
            };

            // Act
            var result = await repo.AddRangeAsync(items);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Entity_When_Exists()
        {
            // Arrange
            using var context = new ArtemisBankingAppContext(_dbOptions);
            var entity = new Commerce { Id = 0, Name = "Test Commerce" };
            context.Commerces.Add(entity);
            await context.SaveChangesAsync();
            var repo = new GenericRepository<Commerce>(context);

            // Act
            var result = await repo.GetByIdAsync(entity.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Test Commerce");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotExists()
        {
            // Arrange
            using var context = new ArtemisBankingAppContext(_dbOptions);
            var repo = new GenericRepository<Commerce>(context);

            // Act
            var result = await repo.GetByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateAsync_Should_Modify_Existing_Entity()
        {
            // Arrange
            using var context = new ArtemisBankingAppContext(_dbOptions);
            var entity = new Commerce { Id = 0, Name = "Old Name" };
            context.Commerces.Add(entity);
            await context.SaveChangesAsync();

            var repo = new GenericRepository<Commerce>(context);
            entity.Name = "Updated Name";

            // Act
            var result = await repo.UpdateAsync(entity.Id, entity);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Updated Name");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Null_When_Entity_Not_Exists()
        {
            // Arrange
            using var context = new ArtemisBankingAppContext(_dbOptions);
            var repo = new GenericRepository<Commerce>(context);
            var entity = new Commerce { Id = 999, Name = "Nonexistent" };

            // Act
            var result = await repo.UpdateAsync(999, entity);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_Should_Remove_Entity()
        {
            // Arrange
            using var context = new ArtemisBankingAppContext(_dbOptions);
            var entity = new Commerce { Id = 0, Name = "To Delete" };
            context.Commerces.Add(entity);
            await context.SaveChangesAsync();
            var repo = new GenericRepository<Commerce>(context);

            // Act
            await repo.DeleteAsync(entity.Id);
            var result = await repo.GetByIdAsync(entity.Id);

            //Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_Should_Not_Throw_When_Entity_Not_Exists()
        {
            // Arrange
            using var context = new ArtemisBankingAppContext(_dbOptions);
            var repo = new GenericRepository<Commerce>(context);

            // Act
            Func<Task> act = async () => await repo.DeleteAsync(999);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task GetAllListAsync_Should_Return_All_Entities()
        {
            // Arrange
            using var context = new ArtemisBankingAppContext(_dbOptions);
            context.Commerces.AddRange(
                new Commerce { Id = 0, Name = "C1" },
                new Commerce { Id = 0, Name = "C2" }
            );
            await context.SaveChangesAsync();
            var repo = new GenericRepository<Commerce>(context);

            // Act
            var result = await repo.GetAllListAsync();

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllQuery_Should_Return_Queryable_Entities()
        {
            // Arrange
            using var context = new ArtemisBankingAppContext(_dbOptions);
            context.Commerces.Add(new Commerce { Id = 0, Name = "C1" });
            await context.SaveChangesAsync();
            var repo = new GenericRepository<Commerce>(context);

            // Act
            var query = repo.GetAllQuery();
            var result = await query.ToListAsync();

            // Assert
            result.Should().NotBeEmpty();
        }
    }
}
