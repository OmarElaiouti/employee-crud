using crud.Core.Interfaces;
using crud.Core.Models;
using crud.Infrastructure.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace crud.Infrastructure.Services
{
    public class EmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;

        public EmployeeService(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public async Task<EmployeeDto> GetByIdAsync(int id)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);
            return employee == null ? null : MapToDto(employee);
        }

        public async Task<IEnumerable<EmployeeDto>> SearchByNameAsync(string name)
        {
            var employees = await _employeeRepository.SearchByNameAsync(name);
            return employees.Select(MapToDto);
        }

        public async Task<IEnumerable<EmployeeDto>> GetAllAsync()
        {
            var employees = await _employeeRepository.GetAllAsync();
            return employees.Select(MapToDto);
        }

        public async Task AddAsync(EmployeeDto employeeDto)
        {
            var employee = MapToEntity(employeeDto);
            await _employeeRepository.AddAsync(employee);
        }

        public async Task DeleteAsync(int id)
        {
            await _employeeRepository.DeleteAsync(id);
        }

        public async Task DeleteRangeAsync(IEnumerable<int> ids)
        {
            await _employeeRepository.DeleteRangeAsync(ids);
        }

        public async Task UpdateAsync(EmployeeDto employeeDto)
        {
            var employee = MapToEntity(employeeDto);
            await _employeeRepository.UpdateAsync(employee);
        }

        private EmployeeDto MapToDto(Employee employee) => new EmployeeDto
        {
            Id = employee.Id,
            Name = employee.Name,
            Email = employee.Email,
            Address = employee.Address,
            Phone = employee.Phone
        };

        private Employee MapToEntity(EmployeeDto employeeDto) => new Employee
        {
            Id = employeeDto.Id,
            Name = employeeDto.Name,
            Email = employeeDto.Email,
            Address = employeeDto.Address,
            Phone = employeeDto.Phone

        };
    }

}
