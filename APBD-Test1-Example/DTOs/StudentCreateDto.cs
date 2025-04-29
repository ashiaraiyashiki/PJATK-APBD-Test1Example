using System.ComponentModel.DataAnnotations;

namespace APBD_Test1_Example.DTOs;

public class StudentCreateDto
{
    [MaxLength(50)]
    public required string FirstName { get; set; }
    
    [MaxLength(50)]
    public required string LastName { get; set; }
    
    [Range(0, short.MaxValue)]
    public required short Age { get; set; }
    
    public List<int>? GroupAssignments { get; set; }
}