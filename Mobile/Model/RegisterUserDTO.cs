namespace GripMobile.Model
{   
    /// <summary>
    /// Class <c>RegisterUserDTO</c> stores the necessary data from a user needed for registering.
    /// </summary>
    public class RegisterUserDTO
    {
        /// <value>Property <c>Email</c> represents the user's e-mail address.</value>
        public string Email { get; set; }

        /// <value>Property <c>Name</c> represent the user's real name.</value>
        public string Name { get; set; }
    }
}
