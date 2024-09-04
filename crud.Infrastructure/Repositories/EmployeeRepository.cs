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
            return await _context.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<Employee>> SearchByNameAsync(string name)
        {
            return await _context.Employees
                .AsNoTracking()
                .Where(e => e.Name.Contains(name))
                .ToListAsync();
        }

        public async Task<IEnumerable<Employee>> GetAllAsync()
        {
            return await _context.Employees
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task AddAsync(Employee employee)
        {
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee != null)
            {
                employee.Archived = true;
                _context.Employees.Update(employee);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteRangeAsync(IEnumerable<int> ids)
        {
            var employees = await _context.Employees
                .Where(e => ids.Contains(e.Id))
                .ToListAsync();

            foreach (var employee in employees)
            {
                employee.Archived = true;
                _context.Employees.Update(employee);
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Employee employee)
        {
            var employeeToUpdate = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == employee.Id);

            if (employeeToUpdate != null)
            {
                _context.Employees.Update(employeeToUpdate);
                await _context.SaveChangesAsync();
            }
        }
    }


  

}
