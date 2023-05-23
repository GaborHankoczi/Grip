namespace GripMobile.Model
{
    /// <summary>
    /// Class <c>UserInfoDTO</c> represents a Data Transfer Object and stores data which came from or needed for the database. Used for getting teacher's data for classes.
    /// </summary>
    public class UserInfoDTO
    {
        /// <value>Property <c>Id</c> represents the user ID in the database.</value>
        public int Id { get; set; }

        /// <value>Property <c>UserName</c> represents the user's real name.</value>
        public string UserName { get; set; }
    }
}
