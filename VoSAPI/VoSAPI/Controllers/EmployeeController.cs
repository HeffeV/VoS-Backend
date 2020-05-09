using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VoSAPI.Models;
using VoSAPI.Services;

namespace VoSAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly VosContext _context;
        private readonly LogService _logService;

        public EmployeeController(VosContext context, LogService logService)
        {
            _context = context;
            _logService = logService;
        }

        // GET: api/Employee
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employee>>> Getemployees()
        {
            var email = User.Claims.First(i => i.Type == "Email").Value;

            //await _logService.AddLog(email + " requested all employees", "Info");
            return await _context.employees.ToListAsync();
        }

        // GET: api/Employee/5
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<Employee>> GetEmployee(long id)
        {
            var email = User.Claims.First(i => i.Type == "Email").Value;
            var employee = await _context.employees.FindAsync(id);

            if (employee == null)
            {
                await _logService.AddLog(email + " tried to request the data from employee id: "+id.ToString(), "Warning");
                return NotFound();
            }

            //await _logService.AddLog(email + " requested the data from employee id: " + employee.EmployeeID, "Info");
            return employee;
        }

        // PUT: api/Employee/5
        [Authorize]
        [HttpPut]
        public async Task<IActionResult> PutEmployee(Employee employee)
        {
            var email = User.Claims.First(i => i.Type == "Email").Value;
            Employee tmpEmployee = await _context.employees.Include(e=>e.EmployeeViolations).SingleOrDefaultAsync(e=>e.EmployeeID==employee.EmployeeID);

            tmpEmployee.Name = employee.Name;
            tmpEmployee.Firstname = employee.Firstname;

            if (!EmployeeExists(employee.EmployeeID))
                {
                await _logService.AddLog(email + " tried to update the data of employee id: " + employee.EmployeeID, "Warning");
                return NotFound();
                }
                else
                {
                //await _logService.AddLog(email + " updated the data from employee id: " + employee.EmployeeID, "Success");
                _context.Entry(tmpEmployee).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }

            return NoContent();
        }

        // POST: api/Employee
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Employee>> PostEmployee(Employee employee)
        {
            var email = User.Claims.First(i => i.Type == "Email").Value;
            employee.CreationDate = DateTime.Now;
            _context.employees.Add(employee);

            await _context.SaveChangesAsync();

            //await _logService.AddLog(email + " created a new employee with employee id: " + employee.EmployeeID, "Success");

            return CreatedAtAction("GetEmployee", new { id = employee.EmployeeID }, employee);
        }

        // DELETE: api/Employee/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult<Employee>> DeleteEmployee(long id)
        {
            var email = User.Claims.First(i => i.Type == "Email").Value;

            var employee = await _context.employees.Include(e => e.EmployeeViolations).SingleOrDefaultAsync(e => e.EmployeeID == id);
            if (employee == null)
            {
                await _logService.AddLog(email + " tried to delete an employee with employee id: " + id, "Warning");
                return NotFound();
            }

            //await _logService.AddLog(email + " deleted an employee with employee id: " +employee.EmployeeID, "Success");
            _context.employees.Remove(employee);
            await _context.SaveChangesAsync();

            return employee;
        }

        private bool EmployeeExists(long id)
        {
            return _context.employees.Any(e => e.EmployeeID == id);
        }
    }
}
