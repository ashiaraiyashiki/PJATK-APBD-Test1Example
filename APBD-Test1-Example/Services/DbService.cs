using System.Data;
using APBD_Test1_Example.DTOs;
using APBD_Test1_Example.Exceptions;
using Microsoft.Data.SqlClient;

namespace APBD_Test1_Example.Services;

public interface IDbService
{
    public Task<IEnumerable<StudentDetailsGetDto>> GetStudentDetailsAsync(string? searchName);
    public Task<StudentDetailsGetDto> CreateStudentAsync(StudentCreateDto studentData);
}

public class DbService(IConfiguration conf) : IDbService
{
    /* Helper method */
    private async Task<SqlConnection> GetConnectionAsync()
    {
        var connection = new SqlConnection(conf.GetConnectionString("Default-db"));
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }
        return connection;
    }


    public async Task<IEnumerable<StudentDetailsGetDto>> GetStudentDetailsAsync(string? searchName)
    {
        var studentsDict = new Dictionary<int, StudentDetailsGetDto>();
        
        await using var connection = await GetConnectionAsync();
        
        var sql = """
                  select S.Id, S.FirstName, S.LastName, S.Age, G.Id, G.Name 
                  from Student S
                  left join GroupAssignment GA on S.Id = GA.Student_Id
                  left join "Group" G on GA.Group_Id = G.Id
                  where @SearchName is null or FirstName like '%' + @SearchName + '%';
                  """;
        
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@SearchName", searchName is null ? DBNull.Value : searchName);
        
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var studentId = reader.GetInt32(0);
            
            if (!studentsDict.TryGetValue(studentId, out var studentDetails)) 
            {
                studentDetails = new StudentDetailsGetDto
                {
                    Id = studentId,
                    FirstName = reader.GetString(1),
                    LastName = reader.GetString(2),
                    Age = reader.GetInt32(3),
                    Groups = []
                };
                
                studentsDict.Add(studentId, studentDetails);
            }

            if (!await reader.IsDBNullAsync(4))
            {
                studentDetails.Groups.Add(new StudentGroupGetDto
                {
                    Id = reader.GetInt32(4),
                    Name = reader.GetString(5),
                });
            }
        }
        
        return studentsDict.Values;
    }

    public async Task<StudentDetailsGetDto> CreateStudentAsync(StudentCreateDto studentData)
    {
        await using var connection = await GetConnectionAsync();

        var groups = new List<StudentGroupGetDto>();
        
        if (studentData.GroupAssignments is not null && studentData.GroupAssignments.Count != 0)
        {
            foreach (var group in studentData.GroupAssignments)
            {
                var groupCheckSql = """
                                    select Id, Name 
                                    from "Group" 
                                    where Id = @Id;
                                    """;

                await using var groupCheckCommand = new SqlCommand(groupCheckSql, connection);
                groupCheckCommand.Parameters.AddWithValue("@Id", group);
                await using var groupCheckReader = await groupCheckCommand.ExecuteReaderAsync();

                if (!await groupCheckReader.ReadAsync())
                {
                    throw new NotFoundException($"Group with id {group} does not exist");
                }

                groups.Add(new StudentGroupGetDto
                {
                    Id = groupCheckReader.GetInt32(0),
                    Name = groupCheckReader.GetString(1),
                });
            }
        }

        await using var transaction = await connection.BeginTransactionAsync();

        try
        {

            var createStudentSql = """
                                   insert into student
                                   output inserted.Id
                                   values (@FirstName, @LastName, @Age);
                                   """;

            await using var createStudentCommand =
                new SqlCommand(createStudentSql, connection, (SqlTransaction)transaction);
            createStudentCommand.Parameters.AddWithValue("@FirstName", studentData.FirstName);
            createStudentCommand.Parameters.AddWithValue("@LastName", studentData.LastName);
            createStudentCommand.Parameters.AddWithValue("@Age", studentData.Age);

            var createdStudentId = Convert.ToInt32(await createStudentCommand.ExecuteScalarAsync());

            foreach (var group in groups)
            {
                var groupAssignmentSql = """
                                         insert into groupAssignment 
                                         values (@StudentId, @GroupId);
                                         """;
                await using var groupAssignmentCommand =
                    new SqlCommand(groupAssignmentSql, connection, (SqlTransaction)transaction);
                groupAssignmentCommand.Parameters.AddWithValue("@StudentId", createdStudentId);
                groupAssignmentCommand.Parameters.AddWithValue("@GroupId", group.Id);

                await groupAssignmentCommand.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();

            return new StudentDetailsGetDto
            {
                Id = createdStudentId,
                FirstName = studentData.FirstName,
                LastName = studentData.LastName,
                Age = studentData.Age,
                Groups = groups
            };

        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}