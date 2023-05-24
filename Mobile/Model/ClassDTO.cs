namespace GripMobile.Model
{
    /// <summary>
    /// Class <c>ClassDTO</c> represents a Data Transfer Object and stores data which came from or needed for the database. Used for getting classes from the database.
    /// </summary>
    public class ClassDTO
    {
        /// <value>Property <c>Id</c> represents the class' ID in the database.</value>
        public int Id { get; set; }

        /// <value>Property <c>Name</c> represents the class' name.</value>
        public string Name { get; set; }

        /// <value>Property <c>StartDateTime</c> represents the time when the class is held.</value>
        public string StartDateTime { get; set; }

        /// <value>Property <c>Teacher</c> represents the teacher who teaches the class.</value>
        public UserInfoDTO Teacher { get; set; }

        /// <value>Property <c>Group</c> represents the group which attends the particular class.</value>
        public GroupDTO Group { get; set; }
    }
}
