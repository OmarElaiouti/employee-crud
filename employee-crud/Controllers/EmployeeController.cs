using crud.Infrastructure.Dtos;
using crud.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace employee_crud.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly EmployeeService _employeeService;

        public EmployeeController(EmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EmployeeDto>> GetById(int id)
        {
            var employee = await _employeeService.GetByIdAsync(id);
            if (employee == null)
                return NotFound();
            return Ok(employee);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<EmployeeDto>>> SearchByName(string name)
        {
            var employees = await _employeeService.SearchByNameAsync(name);
            return Ok(employees);
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<EmployeeDto>>> GetEmployees([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var employees = await _employeeService.GetAllAsync(page, pageSize);
            var totalEmployees = await _employeeService.GetTotalCountAsync();
            var result = new PagedResult<EmployeeDto>
            {
                Items = employees,
                TotalCount = totalEmployees
            };

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] EmployeeDto employeeDto)
        {
            await _employeeService.AddAsync(employeeDto);
            return CreatedAtAction(nameof(GetById), new { id = employeeDto.Id }, employeeDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _employeeService.DeleteAsync(id);
            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteRange([FromBody] IEnumerable<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                return BadRequest("No IDs provided.");
            }
            await _employeeService.DeleteRangeAsync(ids);
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] EmployeeDto employeeDto)
        {
            await _employeeService.UpdateAsync(employeeDto);
            return Ok();
        }
    }

}
