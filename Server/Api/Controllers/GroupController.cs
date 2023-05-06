using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Grip.DAL;
using Grip.DAL.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using AutoMapper;
using Grip.Bll.DTO;
using Grip.Bll.Services;
using Grip.Bll.Services.Interfaces;

namespace Grip.Controllers;

/// <summary>
/// Represents a controller for managing groups.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class GroupController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IMapper _mapper;
    private readonly IGroupService _groupService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GroupController"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="userManager">The user manager.</param>
    /// <param name="mapper">The mapper.</param>
    /// <param name="groupService">The group service.</param>
    public GroupController(ApplicationDbContext context, UserManager<User> userManager, IMapper mapper, IGroupService groupService)
    {
        _context = context;
        _userManager = userManager;
        _mapper = mapper;
        _groupService = groupService;
    }

    /// <summary>
    /// Retrieves all groups.
    /// </summary>
    /// <returns>An <see cref="ActionResult"/> containing the list of <see cref="GroupDTO"/> items.</returns>
    /// <remarks>
    /// This function is accessible via HTTP GET request.
    /// The user must be authorized to access this endpoint.
    /// Returns 200 OK if the request is successful.
    /// Returns 401 Unauthorized if the user is not authorized to access the endpoint.
    /// </remarks>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<GroupDTO>>> GetGroup()
    {
        return Ok(await _groupService.GetAll());
    }

    /// <summary>
    /// Retrieves a group by its ID.
    /// </summary>
    /// <param name="id">The ID of the group to retrieve.</param>
    /// <returns>An <see cref="ActionResult"/> containing the <see cref="GroupDTO"/> item.</returns>
    /// <remarks>
    /// This function is accessible via HTTP GET request.
    /// The user must be authorized to access this endpoint.
    /// Returns 200 OK if the request is successful.
    /// Returns 404 Not Found if the group with the specified ID is not found.
    /// Returns 401 Unauthorized if the user is not authorized to access the endpoint.
    /// Returns 400 Bad Request if the ID parameter is invalid.
    /// </remarks>
    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GroupDTO>> GetGroup(int id)
    {
        return Ok(await _groupService.Get(id));
    }

    /// <summary>
    /// Updates a group by its ID.
    /// </summary>
    /// <param name="id">The ID of the group to update.</param>
    /// <param name="group">The updated group data.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the update operation.</returns>
    /// <remarks>
    /// This function is accessible via HTTP PUT request.
    /// The user must be authorized with the "Admin" role to access this endpoint.
    /// Returns 204 No Content if the update operation is successful.
    /// Returns 400 Bad Request if the ID parameter is invalid or if the group object is malformed.
    /// Returns 404 Not Found if the group with the specified ID is not found.
    /// Returns 401 Unauthorized if the user is not authorized to access the endpoint
    /// </remarks>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> PutGroup(int id, [FromBody] GroupDTO @group)
    {
        if (id != @group.Id)
        {
            return BadRequest();
        }
        await _groupService.Update(@group);
        return NoContent();
    }

    /// <summary>
    /// Creates a new group.
    /// </summary>
    /// <param name="group">The group to create.</param>
    /// <returns>An <see cref="ActionResult"/> indicating the result of the create operation.</returns>
    /// <remarks>
    /// This function is accessible via HTTP POST request.
    /// The user must be authorized with the "Admin" role to access this endpoint.
    /// Returns 201 Created if the create operation is successful.
    /// Returns 400 Bad Request if the group object is malformed.
    /// Returns 401 Unauthorized if the user is not authorized to access the endpoint.
    /// </remarks>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> PostGroup([FromBody] GroupDTO @group)
    {
        var created = await _groupService.Create(@group);

        return CreatedAtAction("GetGroup", new { id = created.Id }, created);
    }

    /// <summary>
    /// Deletes a group by its ID.
    /// </summary>
    /// <param name="id">The ID of the group to delete.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the delete operation.</returns>
    /// <remarks>
    /// This function is accessible via HTTP DELETE request.
    /// The user must be authorized with the "Admin" role to access this endpoint.
    /// Returns 204 No Content if the delete operation is successful.
    /// Returns 404 Not Found if the group with the specified ID is not found.
    /// Returns 401 Unauthorized if the user is not authorized to access the endpoint.
    /// </remarks>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteGroup(int id)
    {
        await _groupService.Delete(id);

        return NoContent();
    }

    /// <summary>
    /// Adds a user to a group.
    /// </summary>
    /// <param name="groupId">The ID of the group.</param>
    /// <param name="userId">The ID of the user to add.</param>
    /// <returns>An <see cref="ActionResult"/> indicating the result of the operation.</returns>
    /// <remarks>
    /// This function is accessible via HTTP PATCH request.
    /// The user must be authorized with the "Admin" role to access this endpoint.
    /// Returns 200 OK if the operation is successful.
    /// Returns 404 Not Found if the group or user with the specified ID is not found.
    /// Returns 401 Unauthorized if the user is not authorized to access the endpoint.
    /// Returns 400 Bad Request if the ID parameters are invalid.
    /// </remarks>
    [HttpPatch("{groupId}/AddUser/{userId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> AddUserToGroup(int groupId, int userId)
    {
        await _groupService.AddUserToGroup(groupId, userId);

        return Ok();
    }

    /// <summary>
    /// Removes a user from a group.
    /// </summary>
    /// <param name="groupId">The ID of the group.</param>
    /// <param name="userId">The ID of the user to remove.</param>
    /// <returns>An <see cref="ActionResult"/> indicating the result of the operation.</returns>
    /// <remarks>
    /// This function is accessible via HTTP PATCH request.
    /// The user must be authorized with the "Admin" role to access this endpoint.
    /// Returns 200 OK if the operation is successful.
    /// Returns 404 Not Found if the group or user with the specified ID is not found.
    /// Returns 401 Unauthorized if the user is not authorized to access the endpoint.
    /// Returns 400 Bad Request if the ID parameters are invalid.
    /// </remarks>
    [HttpPatch("{groupId}/RemoveUser/{userId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> RemoveUserFromGroup(int groupId, int userId)
    {
        await _groupService.RemoveUserFromGroup(groupId, userId);

        return Ok();
    }

    private bool GroupExists(int id)
    {
        return (_context.Groups?.Any(e => e.Id == id)).GetValueOrDefault();
    }
}
