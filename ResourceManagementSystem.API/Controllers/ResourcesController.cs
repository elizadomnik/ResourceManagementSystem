using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Mvc;
using ResourceManagementSystem.Core.DTOs.Resource;
using ResourceManagementSystem.Core.Interfaces;
using System;
using System.Linq; 
using System.Security.Claims;
using System.Threading.Tasks;

namespace ResourceManagementSystem.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class ResourcesController : ControllerBase
    {
        private readonly IResourceService _resourceService;

        public ResourcesController(IResourceService resourceService)
        {
            _resourceService = resourceService;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub");
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                throw new InvalidOperationException("User ID not found in token or is invalid.");
            }
            return userId;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllResources()
        {
            var resources = await _resourceService.GetAllResourcesAsync();
            return Ok(resources);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetResourceById(Guid id)
        {
            var resource = await _resourceService.GetResourceByIdAsync(id);
            if (resource == null)
            {
                return NotFound();
            }
            return Ok(resource);
        }

        [HttpPost]
        public async Task<IActionResult> CreateResource([FromBody] CreateResourceDto resourceDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userId = GetCurrentUserId();
            var result = await _resourceService.CreateResourceAsync(resourceDto, userId);

            if (!result.Succeeded || result.Data == null)
            {
                return BadRequest(new { Errors = result.Errors });
            }
            return CreatedAtAction(nameof(GetResourceById), new { id = result.Data.Id }, result.Data);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateResource(Guid id, [FromBody] UpdateResourceDto resourceDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userId = GetCurrentUserId();
            var result = await _resourceService.UpdateResourceAsync(id, resourceDto, userId);

            if (!result.Succeeded)
            {
                if (result.Errors.Contains("Resource not found.")) return NotFound();
                return BadRequest(new { Errors = result.Errors });
            }
            return Ok(result.Data);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteResource(Guid id)
        {
            var userId = GetCurrentUserId();
            var result = await _resourceService.DeleteResourceAsync(id, userId);

            if (!result.Succeeded)
            {
                if (result.Errors.Contains("Resource not found.")) return NotFound();
                return BadRequest(new { Errors = result.Errors });
            }
            return NoContent();
        }
    }
}