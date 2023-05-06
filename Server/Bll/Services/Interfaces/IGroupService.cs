using Grip.Bll.DTO;

namespace Grip.Bll.Services.Interfaces;
public interface IGroupService : IRestInterface<GroupDTO, GroupDTO, GroupDTO>
{
    public Task AddUserToGroup(int groupId, int userId);
    public Task RemoveUserFromGroup(int groupId, int userId);
    public Task<IEnumerable<UserInfoDTO>> GetUsersInGroup(int groupId);
}