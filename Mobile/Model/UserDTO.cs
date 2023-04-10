namespace GripMobile.Model
{
    /// <summary>
    /// Class <c>UserDTO</c> represents a Data Transfer Object and stores data which came from or needed for the database. Used for registering or querying/modifying a user by ID.
    /// </summary>
    public class UserDTO
    {
        /// <value>Property <c>Id</c> represents the user ID in the database.</value>
        public int Id { get; set; }

        /// <value>Property <c>Email</c> represents the user's e-mail address.</value>
        public string Email { get; set; }

        /// <value>Property <c>UserName</c> represents the user's real name.</value>
        public string UserName { get; set; }

        /// <value>Property <c>EmailConfirmed</c> shows whether the user has previously confirmed his/her e-mail address or not.</value>
        public bool EmailConfirmed { get; set; }
    }
}
