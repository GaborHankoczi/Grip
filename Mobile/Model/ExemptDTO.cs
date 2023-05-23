namespace GripMobile.Model
{
    public class ExemptDTO
    {
        public int Id { get; set; }

        public UserInfoDTO IssuedBy { get; set; }

        public UserInfoDTO IssuedTo { get; set;}

        public string ValidFrom { get; set; }
        
        public string ValidTo { get; set; }
    }
}
