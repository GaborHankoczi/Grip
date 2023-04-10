namespace GripMobile.Model
{
    /// <summary>
    /// Class <c>ResetPasswordDTO</c> represents a Data Transfer Object and used when a user forgot his/her password.
    /// </summary>
    public class ResetPasswordDTO
    {
        /// <value>Property <c>Email</c> represents the user's e-mail address.</value>
        public string Email { get; set; }
        
        /// <value>Property <c>Token</c> is a 6 digit number. The user gets this from the server via email. Needed for authentication.</value>
        public string Token { get; set; }

        /// <value>Property <c>Password</c> represents the desired new password. After successful authentication this overwrites the old password.</value>
        public string Password { get; set; }
    }
}
