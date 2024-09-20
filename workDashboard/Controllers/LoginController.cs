using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using workDashboard.Data; // Your data context
using workDashboard.Models; // Your models
using Microsoft.AspNetCore.Identity; // For password hashing
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class LoginController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IPasswordHasher<Admin> _adminPasswordHasher;
    private readonly IPasswordHasher<Company> _companyPasswordHasher;
    private readonly IPasswordHasher<Employee> _employeePasswordHasher;

    public LoginController(ApplicationDbContext context, IConfiguration configuration, 
        IPasswordHasher<Admin> adminPasswordHasher,
        IPasswordHasher<Company> companyPasswordHasher,
        IPasswordHasher<Employee> employeePasswordHasher)
    {
        _context = context;
        _configuration = configuration;
        _adminPasswordHasher = adminPasswordHasher;
        _companyPasswordHasher = companyPasswordHasher;
        _employeePasswordHasher = employeePasswordHasher;
    }

    private string GenerateToken(IEnumerable<Claim> claims, string audience)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = audience
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    [HttpPost("admin")]
    public async Task<IActionResult> LoginAdmin(LoginModel model)
    {
        var admin = await _context.Admins.SingleOrDefaultAsync(a => a.Email == model.Email);

        if (admin == null || _adminPasswordHasher.VerifyHashedPassword(admin, admin.Password, model.Password) == PasswordVerificationResult.Failed)
        {
            return Unauthorized("Invalid email or password.");
        }

        var department = await _context.Departmants.SingleOrDefaultAsync(d => d.Id == admin.DepartmantId);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, admin.Name),
            new Claim("id", admin.Id.ToString()),
            new Claim("departmant", department.Name),
            new Claim(ClaimTypes.Email, admin.Email),
        };

        var tokenString = GenerateToken(claims, "admin");

        return Ok(new { Token = tokenString });
    }

    [HttpPost("company")]
    public async Task<IActionResult> LoginCompany(LoginModel model)
    {
        var company = await _context.Companies.SingleOrDefaultAsync(c => c.Email == model.Email);

        if (company == null || _companyPasswordHasher.VerifyHashedPassword(company, company.Password, model.Password) == PasswordVerificationResult.Failed)
        {
            return Unauthorized("Invalid email or password.");
        }

        var department = await _context.Departmants.SingleOrDefaultAsync(d => d.Id == company.DepartmantId);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, company.Name),
            new Claim("id", company.Id.ToString()),
            new Claim("departmant", department.Name),
            new Claim("departmantId", department.Id.ToString()),
            new Claim(ClaimTypes.Email, company.Email),
        };

        var tokenString = GenerateToken(claims, "company");

        return Ok(new { Token = tokenString });
    }

    [HttpPost("employee")]
    public async Task<IActionResult> LoginEmployee(LoginModel model)
    {
        var employee = await _context.Employees.Include(e => e.Departmant)
            .SingleOrDefaultAsync(e => e.Email == model.Email);

        if (employee == null || _employeePasswordHasher.VerifyHashedPassword(employee, employee.Password, model.Password) == PasswordVerificationResult.Failed)
        {
            return Unauthorized("Invalid email or password.");
        }

        var department = await _context.Departmants.SingleOrDefaultAsync(d => d.Id == employee.DepartmantId);

        if (department == null)
        {
            return Unauthorized("Invalid department.");
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, employee.Name),
            new Claim("id", employee.Id.ToString()),
            new Claim("companyId", employee.CompanyId.ToString()),
            new Claim("departmant", department.Name),
            new Claim("departmantId", department.Id.ToString()),
            new Claim(ClaimTypes.Email, employee.Email),
        };

        var tokenString = GenerateToken(claims, "employee");

        return Ok(new { Token = tokenString });
    }
}
