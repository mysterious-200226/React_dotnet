namespace PHP.QARAdjustmentTool.API.DTOs
{
    public class UpdateUserDto
    {
        public string UserEmail { get; set; }
        public string UserFirstName { get; set; }
        public string Roles { get; set; }  // Comma-separated
    }
}
