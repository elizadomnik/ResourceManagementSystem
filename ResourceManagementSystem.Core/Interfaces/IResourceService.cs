using ResourceManagementSystem.Core.DTOs.Resource;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ResourceManagementSystem.Core.Interfaces
{
    public interface IResourceService
    {
        Task<IEnumerable<ResourceDto>> GetAllResourcesAsync();
        Task<ResourceDto?> GetResourceByIdAsync(Guid id);
        Task<(bool Succeeded, ResourceDto? Data, string[] Errors)> CreateResourceAsync(CreateResourceDto resourceDto, Guid userId);
        Task<(bool Succeeded, ResourceDto? Data, string[] Errors)> UpdateResourceAsync(Guid id, UpdateResourceDto resourceDto, Guid userId);
        Task<(bool Succeeded, string[] Errors)> DeleteResourceAsync(Guid id, Guid userId);
    }
}