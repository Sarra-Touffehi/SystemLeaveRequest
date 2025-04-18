namespace LeaveManagementApi.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public DateTime JoiningDate { get; set; }

        public ICollection<LeaveRequest>? LeaveRequests { get; set; }
    }
}
