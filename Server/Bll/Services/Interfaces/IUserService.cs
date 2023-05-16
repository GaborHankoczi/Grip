using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grip.Bll.DTO;

namespace Grip.Bll.Services.Interfaces
{
    /// <summary>
    /// Represents an interface for the user service.
    /// </summary>
    public interface IUserService : IRestInterface<RegisterUserDTO, UserDTO, UserDTO>
    {
        /// <summary>
        /// Logs in a user with the specified login credentials.
        /// </summary>
        /// <param name="dto">The login credentials.</param>
        /// <returns>A task representing the asynchronous login operation.</returns>
        public Task<LoginResultDTO> Login(LoginUserDTO dto);

        /// <summary>
        /// Deletes a user with the specified ID.
        /// </summary>
        /// <param name="id">The ID of the user to delete.</param>
        /// <returns>A task representing the asynchronous deletion of the user.</returns>
        public Task Delete(int id);

        /// <summary>
        /// Confirms the email of a user with the specified data.
        /// </summary>
        /// <param name="dto">The data for confirming the email.</param>
        /// <returns>A task representing the asynchronous email confirmation.</returns>
        public Task ConfirmEmail(ConfirmEmailDTO dto);

        /// <summary>
        /// Sends a forgot password email to a user with the specified data.
        /// </summary>
        /// <param name="dto">The data for sending the forgot password email.</param>
        /// <returns>A task representing the asynchronous sending of the forgot password email.</returns>
        public Task ForgotPassword(ForgotPasswordDTO dto);

        /// <summary>
        /// Resets the password of a user with the specified data.
        /// </summary>
        /// <param name="dto">The data for resetting the password.</param>
        /// <returns>A task representing the asynchronous password reset.</returns>
        public Task ResetPassword(ResetPasswordDTO dto);

        /// <summary>
        /// Adds a role to a user with the specified ID.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="role">The role to add.</param>
        /// <returns>A task representing the asynchronous role addition.</returns>
        public Task AddRole(int userId, string role);

        /// <summary>
        /// Removes a role from a user with the specified ID.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="role">The role to remove.</param>
        /// <returns>A task representing the asynchronous role removal.</returns>
        public Task RemoveRole(int userId, string role);

    }
}