using Grip.Bll.DTO;

namespace Grip.Bll.Services.Interfaces
{
    public interface IStudentService
    {
        /// <summary>
        /// Search students by name and group
        /// </summary>
        /// <param name="name">String that the name of the student should contain, null or empty if this filter shouldn't apply</param>
        /// <param name="groupId">Id of the group the students are part of, null if theus filter shouldn't apply</param>
        /// <returns>IEnumerable containing description of users</returns>
        public Task<IEnumerable<UserInfoDTO>> SearchStudentsAsync(string? name, int? groupId);
        /// <summary>
        /// Get details of a student
        /// </summary>
        /// <param name="id">Id of the student</param>
        /// <returns>Detail DTO of the student</returns>
        public Task<StudentDetailDTO> GetStudentDetailsAsync(int id);
    }
}