using crud.Core.Interfaces;
using crud.Core.Models;
using crud.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace crud.Infrastructure.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly ApplicationDbContext _context;

        public EmployeeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Employee> GetByIdAsync(int id)
        {
            return await _context.Employees.FindAsync(id);
        }

        public async Task<IEnumerable<Employee>> SearchByNameAsync(string name)
        {
            return await _context.Employees
                .Where(e => e.Name.Contains(name))
                .ToListAsync();
        }

        public async Task<IEnumerable<Employee>> GetAllAsync()
        {
            return await _context.Employees.ToListAsync();
        }

        public async Task AddAsync(Employee employee)
        {
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteRangeAsync(IEnumerable<int> ids)
        {
            var employees = await _context.Employees
                .Where(e => ids.Contains(e.Id))
                .ToListAsync();
            _context.Employees.RemoveRange(employees);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Employee employee)
        {
            var employeeToUpdate = await _context.Employees.FindAsync(employee.Id);
            if (employee != null)
            {
                _context.Employees.Update(employee);
                await _context.SaveChangesAsync();
            }
        }
    }

}
