using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grip.Bll.DTO;

namespace Grip.Bll.Services.Interfaces
{
    public interface IUserService
    {
        public Task<UserDTO> Create(RegisterUserDTO dto);
        public Task<UserDTO> Get(int id);
        public Task<IEnumerable<UserDTO>> GetAll();        
        public Task<LoginResultDTO> Login(LoginUserDTO dto);
        public Task Delete(int id);
        public Task ConfirmEmail(ConfirmEmailDTO dto);
        public Task ForgotPassword(ForgotPasswordDTO dto);
        public Task ResetPassword(ResetPasswordDTO dto);
        public Task Update(UserDTO dto);
        public Task AddRole(int userId, string role);
        public Task RemoveRole(int userId, string role);

    }
}