using Grip.Bll.DTO;

namespace Grip.Bll.Services.Interfaces;

/// <summary>
/// Interface representing the Group Service, which provides CRUD operations for groups as well as specific group operations.
/// </summary>
public interface IGroupService : IRestInterface<GroupDTO, GroupDTO, GroupDTO>
{
    // <summary>
    /// Adds a user to a group.
    /// </summary>
    /// <param name="groupId">The ID of the group.</param>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task AddUserToGroup(int groupId, int userId);
    /// <summary>
    /// Removes a user from a group.
    /// </summary>
    /// <param name="groupId">The ID of the group.</param>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task RemoveUserFromGroup(int groupId, int userId);

    /// <summary>
    /// Gets all users in a group.
    /// </summary>
    /// <param name="groupId">The ID of the group.</param>
    /// <returns>A task representing the asynchronous operation. The result is an IEnumerable of UserInfoDTO representing all users in the group.</returns>
    public Task<IEnumerable<UserInfoDTO>> GetUsersInGroup(int groupId);
}