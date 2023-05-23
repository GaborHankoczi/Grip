namespace GripMobile.Model
{
    /// <summary>
    /// Class <c>GroupDTO</c> represents a Data Transfer Object and stores data which came from or needed for the database. Used for getting group's data.
    /// </summary>
    public class GroupDTO
    {
        /// <value>Property <c>Id</c> represents the group's ID in the database.</value>
        public int Id { get; set; }

        /// <value>Property <c>Name</c> represents the group's name.</value>
        public string Name { get; set; }
    }
}
