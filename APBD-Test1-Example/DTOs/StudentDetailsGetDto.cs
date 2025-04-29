namespace APBD_Test1_Example.DTOs;

public class StudentDetailsGetDto
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
    public List<StudentGroupGetDto> Groups { get; set; }
}