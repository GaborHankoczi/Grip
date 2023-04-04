namespace GripMobile.Model
{
    public class User
    {
        public int Id { get; set; } = -1;
        public string UserName { get; set; } = null;
        public string Email { get; set; } = null;
        public bool EmailConfirmed { get; set; } = false;
        public string[] Roles { get; set; } = Array.Empty<string>();
    }
}
