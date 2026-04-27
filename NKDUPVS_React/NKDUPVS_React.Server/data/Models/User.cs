namespace NKDUPVS_React.Server.Models
{
    public class User
    {
        public string? Code { get; set; }
        public string? Username { get; set; } = string.Empty;
        public string? Password { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Name { get; set; } = string.Empty;
        public string? LastName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; } = string.Empty; 
        public bool? IsAdmin { get; set; }
        public bool? IsVerified { get; set; }
        public bool? IsSubmitted { get; set; }
        public bool? IsMentor { get; set; }
    }
}