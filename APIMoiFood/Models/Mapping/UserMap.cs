namespace APIMoiFood.Models.Mapping
{
    public class UserMap
    {
        public int UserId { get; set; }

        public string Username { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string PasswordHash { get; set; } = null!;

        public string? FullName { get; set; }

        public string? Phone { get; set; }

        public string? Avatar { get; set; }

        public string? Address { get; set; }

        public string? Role { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
