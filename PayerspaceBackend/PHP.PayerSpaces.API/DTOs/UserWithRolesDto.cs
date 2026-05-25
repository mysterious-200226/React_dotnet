namespace PHP.QARAdjustmentTool.API.DTOs
{
    public class UserWithRolesDto
    {
        public string UserFirstName { get; set; }
        public string UserEmail { get; set; }
        public string Roles { get; set; }
        public string? AvailityUserId { get; set; }
        public string? UserLastName { get; set; }
    }
}
