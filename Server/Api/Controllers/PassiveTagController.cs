using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Grip.DAL;
using Grip.DAL.Model;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Grip.Bll.DTO;
using Grip.Bll.Services.Interfaces;

namespace Grip.Controllers
{
    /// <summary>
    /// API controller for managing passive tags.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PassiveTagController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IPassiveTagService _passiveTagService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PassiveTagController"/> class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        /// <param name="mapper">The mapper for DTO mapping.</param>
        /// <param name="passiveTagService">The service for managing passive tags.</param>
        public PassiveTagController(ApplicationDbContext context, IMapper mapper, IPassiveTagService passiveTagService)
        {
            _context = context;
            _mapper = mapper;
            _passiveTagService = passiveTagService;
        }

        /// <summary>
        /// Retrieves all passive tags.
        /// </summary>
        /// <returns>A collection of <see cref="PassiveTagDTO"/> representing the passive tags.</returns>
        /// <remarks>
        /// This function is accessible via HTTP GET request.
        /// The user must be authorized with the "Admin" role to access this endpoint.
        /// Returns 200 OK if the operation is successful.
        /// </remarks>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PassiveTagDTO>>> GetPassiveTags()
        {
            return Ok(await _passiveTagService.GetAll());
        }

        /// <summary>
        /// Retrieves a specific passive tag by its ID.
        /// </summary>
        /// <param name="id">The ID of the passive tag to retrieve.</param>
        /// <returns>An <see cref="ActionResult"/> representing the passive tag.</returns>
        /// <remarks>
        /// This function is accessible via HTTP GET request.
        /// The user must be authorized with the "Admin" role to access this endpoint.
        /// Returns 400 Bad Request if the ID parameter is invalid.
        /// Returns 404 Not Found if the passive tag with the specified ID is not found.
        /// Returns 200 OK if the operation is successful.
        /// </remarks>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PassiveTagDTO>> GetPassiveTag(int id)
        {
            return await _passiveTagService.Get(id);
        }

        /// <summary>
        /// Updates a specific passive tag.
        /// </summary>
        /// <param name="id">The ID of the passive tag to update.</param>
        /// <param name="passiveTag">The updated passive tag data.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// <remarks>
        /// This function is accessible via HTTP PUT request.
        /// The user must be authorized with the "Admin" role to access this endpoint.
        /// Returns 400 Bad Request if the ID parameter or the passive tag data is invalid.
        /// Returns 404 Not Found if the passive tag with the specified ID is not found.
        /// Returns 204 No Content if the operation is successful.
        /// </remarks>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> PutPassiveTag(int id, [FromBody] UpdatePassiveTagDTO passiveTag)
        {
            if (id != passiveTag.Id)
            {
                return BadRequest();
            }

            await _passiveTagService.Update(passiveTag);

            return NoContent();
        }

        /// <summary>
        /// Creates a new passive tag.
        /// </summary>
        /// <param name="passiveTag">The passive tag data to create.</param>
        /// <returns>An <see cref="ActionResult"/> representing the created passive tag.</returns>
        /// <remarks>
        /// This function is accessible via HTTP POST request.
        /// The user must be authorized with the "Admin" role to access this endpoint.
        /// Returns 400 Bad Request if the passive tag data is invalid.
        /// Returns 201 Created if the passive tag is created successfully.
        /// Returns 404 Not Found if the referenced entities in the passive tag data are not found.
        /// </remarks>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PassiveTagDTO>> PostPassiveTag([FromBody] CreatePassiveTagDTO passiveTag)
        {
            var created = await _passiveTagService.Create(passiveTag);

            return CreatedAtAction("GetPassiveTag", new { id = created.Id }, created);
        }

        /// <summary>
        /// Deletes a specific passive tag.
        /// </summary>
        /// <param name="id">The ID of the passive tag to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// <remarks>
        /// This function is accessible via HTTP DELETE request.
        /// The user must be authorized with the "Admin" role to access this endpoint.
        /// Returns 400 Bad Request if the ID parameter is invalid.
        /// Returns 204 No Content if the operation is successful.
        /// Returns 404 Not Found if the passive tag with the specified ID is not found.
        /// </remarks>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePassiveTag(int id)
        {
            await _passiveTagService.Delete(id);

            return NoContent();
        }

        private bool PassiveTagExists(int id)
        {
            return (_context.PassiveTags?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
