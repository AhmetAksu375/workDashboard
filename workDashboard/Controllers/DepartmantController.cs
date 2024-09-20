using Microsoft.AspNetCore.Mvc;
using workDashboard.Models;
using workDashboard.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using workDashboard.Dtos;

namespace workDashboard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]

    public class DepartmantController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DepartmantController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Departmant
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Departmant>>> GetDepartmants()
        {
            return await _context.Departmants.ToListAsync();
        }

        // GET: api/Departmant/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Departmant>> GetDepartmant(int id)
        {
            var departmant = await _context.Departmants.FindAsync(id);

            if (departmant == null)
            {
                return NotFound();
            }

            return departmant;
        }

        // POST: api/Departmant
        // POST: api/Departmant
        [HttpPost]
        public async Task<ActionResult<Departmant>> PostDepartmant(DepartmantDTO departmantDTO)
        {
            // Convert DTO to Departmant model
            var departmant = new Departmant
            {
                Name = departmantDTO.Name
            };

            _context.Departmants.Add(departmant);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDepartmant), new { id = departmant.Id }, departmant);
        }


        // PUT: api/Departmant/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDepartmant(int id, Departmant departmant)
        {
            if (id != departmant.Id)
            {
                return BadRequest();
            }

            _context.Entry(departmant).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DepartmantExists(id))
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

        // DELETE: api/Departmant/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDepartmant(int id)
        {
            var departmant = await _context.Departmants.FindAsync(id);
            if (departmant == null)
            {
                return NotFound();
            }

            _context.Departmants.Remove(departmant);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DepartmantExists(int id)
        {
            return _context.Departmants.Any(e => e.Id == id);
        }
    }
}
