using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeaveManagementApi.Models
{
    public enum LeaveType
    {
        Annual,
        Sick,
        Other
    }

    public enum LeaveStatus
    {
        Pending,
        Approved,
        Rejected
    }

    public class LeaveRequest
    {
        public int Id { get; set; }

        [Required]  
        public int EmployeeId { get; set; }

        [Required]  
        public LeaveType LeaveType { get; set; }

        [Required] 
        public DateTime StartDate { get; set; }

        [Required]  
        public DateTime EndDate { get; set; }

        [Required] 
        public LeaveStatus Status { get; set; }

        [MaxLength(500)] 
        public string Reason { get; set; } = string.Empty;

        [Required] 
        public DateTime CreatedAt { get; set; }

        [ForeignKey("EmployeeId")]
        public Employee? Employee { get; set; }

    }
}
