namespace GripMobile.Model
{
    /// <summary>
    /// Class <c>LoginUserDTO</c> represents a Data Transfer Object and used when a user tries to login. This class holds the data necessary for the authentication.
    /// </summary>
    public class LoginUserDTO
    {
        /// <value>Property <c>Email</c> represents the user's e-mail address.</value>
        public string Email { get; set; }

        /// <value>Property <c>Password</c> represents the password the user has given during the login process.</value>
        public string Password { get; set; }
    }
}
