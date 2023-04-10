namespace GripMobile.Model
{
    /// <summary>
    /// Class <c>LoginResultDTO</c> represents a Data Transfer Object and gives back information about the user after a successful login.
    /// </summary>
    public class LoginResultDTO
    {
        /// <value>Property <c>UserName</c> represent the real name of the user.</value>
        public string UserName { get; set; } = null;

        /// <value>Property <c>Email</c> represents the user's e-mail address.</value>
        public string Email { get; set; } = null;

        /// <value>Property <c>Roles</c> stores every role that the actual user has.</value>
        public string[] Roles { get; set; } = null;
    }
}
