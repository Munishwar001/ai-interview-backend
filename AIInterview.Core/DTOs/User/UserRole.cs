namespace AIInterview.Core.DTOs.User
{
    public class UserRole
    {
        public string UserID { get; set; }
        public string RoleName { get; set; }
        public int ContextID { get; set; }
        public string UserType { get; set; }
    }

    public class UserDTO
    {
        public string Email { get; set; }
        public string FullName { get; set; }
        public string UserRole { get; set; }

    }

}
