using APBD_Test1_Example.DTOs;
using APBD_Test1_Example.Exceptions;
using APBD_Test1_Example.Services;
using Microsoft.AspNetCore.Mvc;

namespace APBD_Test1_Example.Controllers;

[ApiController]
[Route("[controller]")]
public class StudentsController(IDbService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetStudentsDetails([FromQuery] string? fName)
    {
        return Ok(await service.GetStudentDetailsAsync(fName));
    }

    [HttpPost]
    public async Task<IActionResult> CreateStudent([FromBody] StudentCreateDto body)
    {
        try
        {
            var student = await service.CreateStudentAsync(body);
            return Created($"students/{student.Id}", student);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
}