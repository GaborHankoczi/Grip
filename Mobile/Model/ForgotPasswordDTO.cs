namespace GripMobile.Model
{
    /// <summary>
    /// Class <c>ForgotPasswordDTO</c> represents a Data Transfer Object and holds the e-mail address of the user where the server will send a token in order to change the password later.
    /// </summary>
    public class ForgotPasswordDTO
    {
        /// <value>Property <c>Email</c> represents the user's e-mail address.</value>
        public string Email { get; set; }
    }
}
