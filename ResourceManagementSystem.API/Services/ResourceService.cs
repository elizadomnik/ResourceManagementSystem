using Microsoft.EntityFrameworkCore;
using ResourceManagementSystem.API.Data;
using ResourceManagementSystem.Core.DTOs.Resource;
using ResourceManagementSystem.Core.Entities;
using ResourceManagementSystem.Core.Interfaces;
using ResourceManagementSystem.API.Hubs;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ResourceManagementSystem.API.Services
{
    public class ResourceService : IResourceService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ResourceHub> _resourceHubContext;
        private readonly IRabbitMQProducerService _rabbitMQProducer;
        private const string ResourceExchangeName = "resource.events";
        

        public ResourceService(ApplicationDbContext context, IHubContext<ResourceHub> resourceHubContext, IRabbitMQProducerService rabbitMQProducer)
        {
            _context = context;
            _resourceHubContext = resourceHubContext;
            _rabbitMQProducer = rabbitMQProducer;
        }

        public async Task<IEnumerable<ResourceDto>> GetAllResourcesAsync()
        {

            var resources = await _context.Resources
                .Include(r => r.CreatedBy) 
                .Include(r => r.LastUpdatedBy) 
                .ToListAsync();

            return resources.Select(r => new ResourceDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                Location = r.Location,
                Type = r.Type,
                CreatedAt = r.CreatedAt,
                LastUpdatedAt = r.LastUpdatedAt,
                CreatedByUsername = r.CreatedBy?.Username,
                LastUpdatedByUsername = r.LastUpdatedBy?.Username
            });
        }

        public async Task<ResourceDto?> GetResourceByIdAsync(Guid id)
        {
            var resource = await _context.Resources
                .Include(r => r.CreatedBy)
                .Include(r => r.LastUpdatedBy)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (resource == null)
            {
                return null;
            }

            return new ResourceDto
            {
                Id = resource.Id,
                Name = resource.Name,
                Description = resource.Description,
                Location = resource.Location,
                Type = resource.Type,
                CreatedAt = resource.CreatedAt,
                LastUpdatedAt = resource.LastUpdatedAt,
                CreatedByUsername = resource.CreatedBy?.Username,
                LastUpdatedByUsername = resource.LastUpdatedBy?.Username
            };
        }

        public async Task<(bool Succeeded, ResourceDto? Data, string[] Errors)> CreateResourceAsync(CreateResourceDto resourceDto, Guid userId)
        {
            var resource = new Resource
            {
                Name = resourceDto.Name,
                Description = resourceDto.Description,
                Location = resourceDto.Location,
                Type = resourceDto.Type,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow,
                CreatedById = userId,
                LastUpdatedById = userId
            };

            _context.Resources.Add(resource);
            try
            {
                await _context.SaveChangesAsync();
                var createdResourceDto = await GetResourceByIdAsync(resource.Id);
                if (createdResourceDto != null)
                {
                    await _resourceHubContext.Clients.All.SendAsync("ReceiveResourceCreated", createdResourceDto);
                    _rabbitMQProducer.PublishMessage(ResourceExchangeName, "resource.created", createdResourceDto);
                }
                return (true, createdResourceDto, Array.Empty<string>());
            }
            catch (DbUpdateException ex)
            {
                return (false, null, new[] { "An error occurred while saving the resource." });
            }
        }


        public async Task<(bool Succeeded, ResourceDto? Data, string[] Errors)> UpdateResourceAsync(Guid id, UpdateResourceDto resourceDto, Guid userId)
        {
            var resource = await _context.Resources.FindAsync(id);

            if (resource == null)
            {
                return (false, null, new[] { "Resource not found." });
            }

            resource.Name = resourceDto.Name;
            resource.Description = resourceDto.Description;
            resource.Location = resourceDto.Location;
            resource.Type = resourceDto.Type;
            resource.LastUpdatedAt = DateTime.UtcNow;
            resource.LastUpdatedById = userId;

            try
            {
                await _context.SaveChangesAsync();
                var updatedResourceDto = await GetResourceByIdAsync(resource.Id);
                if (updatedResourceDto != null)
                {
                    await _resourceHubContext.Clients.All.SendAsync("ReceiveResourceUpdate", updatedResourceDto);
                    _rabbitMQProducer.PublishMessage(ResourceExchangeName, $"resource.updated.{resource.Id}",
                        updatedResourceDto);
                }
                return (true, updatedResourceDto, Array.Empty<string>());
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return (false, null, new[] { "The resource was modified by another user. Please refresh and try again." });
            }
            catch (DbUpdateException ex)
            {
                return (false, null, new[] { "An error occurred while updating the resource." });
            }
        }

        public async Task<(bool Succeeded, string[] Errors)> DeleteResourceAsync(Guid id, Guid userId)
        {
            var resource = await _context.Resources.FindAsync(id);

            if (resource == null)
            {
                return (false, new[] { "Resource not found." });
            }
            
            var resourceIdForMessage = resource.Id;
            var resourceNameForMessage = resource.Name; 

            _context.Resources.Remove(resource);
            try
            {
                await _context.SaveChangesAsync();
                await _resourceHubContext.Clients.All.SendAsync("ReceiveResourceDeleted", id);
                _rabbitMQProducer.PublishMessage(ResourceExchangeName, $"resource.deleted.{resourceIdForMessage}", 
                    new { ResourceId = resourceIdForMessage, Name = resourceNameForMessage, DeletedBy = userId, DeletedAt = DateTime.UtcNow });
                return (true, Array.Empty<string>());
            }
            catch (DbUpdateException ex)
            {
                return (false, new[] { "An error occurred while deleting the resource." });
            }
        }
    }
}
