using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.SignalR;
using ResourceManagementSystem.API.Data;
using ResourceManagementSystem.API.Services;
using ResourceManagementSystem.API.Hubs;
using ResourceManagementSystem.Core.DTOs.Resource;
using ResourceManagementSystem.Core.Entities;
using ResourceManagementSystem.Core.Interfaces; 
using System;
using System.Threading.Tasks;
using System.Linq;
using FluentAssertions;
using System.Collections.Generic;

namespace ResourceManagementSystem.API.Tests.Services
{
    public class ResourceServiceIntegrationTests : IDisposable
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly Mock<IHubContext<ResourceHub>> _mockHubContext;
        private readonly Mock<IRabbitMQProducerService> _mockRabbitMQProducerService;
        private readonly Mock<IHubClients> _mockHubClients;
        private readonly Mock<IClientProxy> _mockClientProxy; 
        private readonly ResourceService _resourceService;
        private readonly Guid _testUserId = Guid.NewGuid();

        public ResourceServiceIntegrationTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) 
                .Options;
            _dbContext = new ApplicationDbContext(options);

            _mockHubClients = new Mock<IHubClients>();
            _mockClientProxy = new Mock<IClientProxy>();
            _mockHubClients.Setup(clients => clients.All).Returns(_mockClientProxy.Object);
            
            _mockHubContext = new Mock<IHubContext<ResourceHub>>();
            _mockHubContext.Setup(x => x.Clients).Returns(_mockHubClients.Object);

            _mockRabbitMQProducerService = new Mock<IRabbitMQProducerService>();

            _resourceService = new ResourceService(_dbContext, _mockHubContext.Object, _mockRabbitMQProducerService.Object);

            SeedDatabase();
        }

        private void SeedDatabase()
        {
           
            _dbContext.Users.Add(new User { Id = _testUserId, Username = "testuser", Email = "test@example.com", PasswordHash="hash" });
            _dbContext.SaveChanges();
        }


        [Fact]
        public async Task CreateResourceAsync_ValidData_SavesToDb_SendsSignalRMessage_AndPublishesToRabbitMQ()
        {
            var createDto = new CreateResourceDto { Name = "Test Resource Create", Type = ResourceType.Hardware };
            
            var result = await _resourceService.CreateResourceAsync(createDto, _testUserId);

            result.Succeeded.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data?.Name.Should().Be(createDto.Name);

            var resourceInDb = await _dbContext.Resources.FindAsync(result.Data!.Id);
            resourceInDb.Should().NotBeNull();
            resourceInDb?.Name.Should().Be(createDto.Name);
            resourceInDb?.CreatedById.Should().Be(_testUserId);

            _mockClientProxy.Verify(
                clientProxy => clientProxy.SendCoreAsync(
                    "ReceiveResourceCreated", 
                    It.Is<object[]>(o => ((ResourceDto)o[0]).Id == result.Data!.Id && ((ResourceDto)o[0]).Name == createDto.Name),
                    default),
                Times.Once);
            
            _mockRabbitMQProducerService.Verify(
                producer => producer.PublishMessage(
                    "resource.events",
                    "resource.created",
                    It.Is<ResourceDto>(msg => msg.Id == result.Data!.Id && msg.Name == createDto.Name),
                    null),
                Times.Once);
        }

        [Fact]
        public async Task UpdateResourceAsync_ExistingResource_UpdatesInDb_SendsSignalRMessage_AndPublishesToRabbitMQ()
        {
            var initialResource = new Resource
            {
                Name = "Initial Resource",
                Type = ResourceType.Document,
                CreatedById = _testUserId,
                LastUpdatedById = _testUserId,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                LastUpdatedAt = DateTime.UtcNow.AddDays(-1)
            };
            _dbContext.Resources.Add(initialResource);
            await _dbContext.SaveChangesAsync();
            _dbContext.Entry(initialResource).State = EntityState.Detached; 

            var updateDto = new UpdateResourceDto { Name = "Updated Test Resource", Type = ResourceType.Other };
            
            var result = await _resourceService.UpdateResourceAsync(initialResource.Id, updateDto, _testUserId);

            result.Succeeded.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data?.Name.Should().Be(updateDto.Name);

            var resourceInDb = await _dbContext.Resources.FindAsync(initialResource.Id);
            resourceInDb.Should().NotBeNull();
            resourceInDb?.Name.Should().Be(updateDto.Name);
            resourceInDb?.LastUpdatedById.Should().Be(_testUserId);
            resourceInDb?.LastUpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));


            _mockClientProxy.Verify(
                clientProxy => clientProxy.SendCoreAsync(
                    "ReceiveResourceUpdate",
                    It.Is<object[]>(o => ((ResourceDto)o[0]).Id == initialResource.Id && ((ResourceDto)o[0]).Name == updateDto.Name),
                    default),
                Times.Once);
            
            _mockRabbitMQProducerService.Verify(
                producer => producer.PublishMessage(
                    "resource.events",
                    $"resource.updated.{initialResource.Id}",
                    It.Is<ResourceDto>(msg => msg.Id == initialResource.Id && msg.Name == updateDto.Name),
                    null),
                Times.Once);
        }

        [Fact]
        public async Task DeleteResourceAsync_ExistingResource_DeletesFromDb_SendsSignalRMessage_AndPublishesToRabbitMQ()
        {
            var resourceToDelete = new Resource
            {
                Name = "Resource to Delete",
                Type = ResourceType.Other,
                CreatedById = _testUserId,
                LastUpdatedById = _testUserId
            };
            _dbContext.Resources.Add(resourceToDelete);
            await _dbContext.SaveChangesAsync();
            var resourceId = resourceToDelete.Id; 

            var result = await _resourceService.DeleteResourceAsync(resourceId, _testUserId);

            result.Succeeded.Should().BeTrue();
            
            var resourceInDb = await _dbContext.Resources.FindAsync(resourceId);
            resourceInDb.Should().BeNull();

            _mockClientProxy.Verify(
                clientProxy => clientProxy.SendCoreAsync(
                    "ReceiveResourceDeleted",
                    It.Is<object[]>(o => (Guid)o[0] == resourceId),
                    default),
                Times.Once);
            
            _mockRabbitMQProducerService.Verify(
                producer => producer.PublishMessage(
                    "resource.events",
                    $"resource.deleted.{resourceId}",
                    It.Is<object>(msg => msg.GetType().GetProperty("ResourceId")!.GetValue(msg)!.Equals(resourceId)), // Sprawdź anonimowy obiekt
                    null),
                Times.Once);
        }
        
        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}