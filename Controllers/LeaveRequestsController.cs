using LeaveManagementApi.Data;
using LeaveManagementApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagementApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaveRequestsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public LeaveRequestsController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LeaveRequest>>> GetLeaveRequests()
        {
            return await _context.LeaveRequests.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<LeaveRequest>> GetLeaveRequest(int id)
        {
            var leaveRequest = await _context.LeaveRequests
                                               .Include(lr => lr.Employee)  
                                               .FirstOrDefaultAsync(lr => lr.Id == id);
            if (leaveRequest == null)
            {
                return NotFound();
            }

            return leaveRequest;
        }

        [HttpPost]
        public async Task<ActionResult<LeaveRequest>> PostLeaveRequest(LeaveRequest leaveRequest)
        {
            _context.LeaveRequests.Add(leaveRequest);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLeaveRequest", new { id = leaveRequest.Id }, leaveRequest);
        }



        [HttpPut("{id}")]
        public async Task<IActionResult> PutLeaveRequest(int id, LeaveRequest leaveRequest)
        {
            if (id != leaveRequest.Id)
            {
                return BadRequest();
            }

            _context.Entry(leaveRequest).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LeaveRequestExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        private bool LeaveRequestExists(int id)
        {
            return _context.LeaveRequests.Any(e => e.Id == id);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLeaveRequest(int id)
        {
            var leaveRequest = await _context.LeaveRequests.FindAsync(id);
            if (leaveRequest == null)
            {
                return NotFound();
            }

            _context.LeaveRequests.Remove(leaveRequest);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<LeaveRequest>>> GetFilteredLeaveRequests(
         [FromQuery] int? employeeId,
         [FromQuery] LeaveType? leaveType,
         [FromQuery] LeaveStatus? status,
         [FromQuery] DateTime? startDate,
         [FromQuery] DateTime? endDate,
         [FromQuery] string? keyword,
         [FromQuery] int page = 1,
         [FromQuery] int pageSize = 10,
         [FromQuery] string? sortBy = "startDate",
         [FromQuery] string? sortOrder = "asc")
        {
            var leaveRequestsQuery = _context.LeaveRequests.AsQueryable();

            if (employeeId.HasValue) leaveRequestsQuery = leaveRequestsQuery.Where(r => r.EmployeeId == employeeId.Value);
            if (leaveType.HasValue) leaveRequestsQuery = leaveRequestsQuery.Where(r => r.LeaveType == leaveType.Value);
            if (status.HasValue) leaveRequestsQuery = leaveRequestsQuery.Where(r => r.Status == status.Value);
            if (startDate.HasValue) leaveRequestsQuery = leaveRequestsQuery.Where(r => r.StartDate >= startDate.Value);
            if (endDate.HasValue) leaveRequestsQuery = leaveRequestsQuery.Where(r => r.EndDate <= endDate.Value);
            if (!string.IsNullOrEmpty(keyword)) leaveRequestsQuery = leaveRequestsQuery.Where(r => r.Reason.Contains(keyword));

            leaveRequestsQuery = sortOrder.ToLower() == "asc"
                ? leaveRequestsQuery.OrderBy(r => EF.Property<object>(r, sortBy))
                : leaveRequestsQuery.OrderByDescending(r => EF.Property<object>(r, sortBy));

            var leaveRequests = await leaveRequestsQuery.ToListAsync();

            foreach (var leaveRequest in leaveRequests)
            {
                if (_context.LeaveRequests.Any(r => r.EmployeeId == leaveRequest.EmployeeId
                                                     && r.Id != leaveRequest.Id
                                                     && r.StartDate < leaveRequest.EndDate
                                                     && r.EndDate > leaveRequest.StartDate))
                {
                    return BadRequest("There is an overlap in leave dates for this employee.");
                }

                if (leaveRequest.LeaveType == LeaveType.Annual)
                {
                    var annualLeaveDaysTaken = _context.LeaveRequests
                        .Where(r => r.EmployeeId == leaveRequest.EmployeeId
                                    && r.LeaveType == LeaveType.Annual
                                    && r.StartDate.Year == DateTime.Now.Year)
                        .Sum(r => (r.EndDate - r.StartDate).Days);

                    if (annualLeaveDaysTaken + (leaveRequest.EndDate - leaveRequest.StartDate).Days > 20)
                    {
                        return BadRequest("The employee cannot take more than 20 annual leave days per year.");
                    }
                }

                if (leaveRequest.LeaveType == LeaveType.Sick && string.IsNullOrEmpty(leaveRequest.Reason))
                {
                    return BadRequest("A reason is required for sick leave.");
                }
            }

            var paginatedLeaveRequests = leaveRequests.Skip((page - 1) * pageSize).Take(pageSize);

            return Ok(paginatedLeaveRequests);
        }


    }
}
