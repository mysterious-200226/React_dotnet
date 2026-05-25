namespace PHP.QARAdjustmentTool.API.DTOs
{
    public class UserResponseDto
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<string> Roles { get; set; }
    }
}
