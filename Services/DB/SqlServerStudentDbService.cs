using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Wyklad5.DTOs;
using Wyklad5.DTOs.Requests;

namespace Wyklad5.Services.DB
{
    public class SqlServerStudentDbService : IStudentDbService
    {
        public SqlServerStudentDbService(/*.. */ )
        {

        }

        public async Task<Status> EnrollStudent(EnrollStudentRequest request)
        {
            await using var connection = new SqlConnection("Server=(LocalDb)\\MSSQLLocalDb;Database=master;Trusted_Connection=True;");
            await connection.OpenAsync();
            var transaction = connection.BeginTransaction();
            
            await using var command = new SqlCommand
            {
                Connection = connection,
                Transaction = transaction
            };
            
            try
            {
                var (studiesExist, studiesId) = await GetStudiesId(command, request.Studies);
                if (!studiesExist)
                {
                    await transaction.RollbackAsync();
                    return Status.Failed("Studia nie istnieja");
                }

                var (enrollmentExist, enrollmentId) = await EnrollmentExist(command, studiesId.Value);
                if (!enrollmentExist)
                {
                    enrollmentId = await CreateEnrollment(command, studiesId.Value);
                }

                var x = await CreateStudent(
                    command, 
                    request.IndexNumber, 
                    request.FirstName,
                    request.LastName,
                    request.Birthdate,
                    enrollmentId.Value);

                await transaction.CommitAsync();
                
                return Status.Succeeded();
            }
            catch (SqlException exc)
            {
                await transaction.RollbackAsync();
                return Status.Failed(exc.Message);
            }
        }

        public async Task<Status> PromoteStudents(int semester, string studies)
        {
            await using var connection = new SqlConnection("Server=(LocalDb)\\MSSQLLocalDb;Database=master;Trusted_Connection=True;");
            await connection.OpenAsync();
            var transaction = connection.BeginTransaction();
            
            await using var command = new SqlCommand
            {
                Connection = connection,
                Transaction = transaction
            };
            
            try
            {
                var (studiesExist, studiesId) = await GetStudiesId(command, studies);
                if (!studiesExist)
                {
                    await transaction.RollbackAsync();
                    return Status.Failed("Studia nie istnieja");
                }

                var (enrollmentExist, enrollmentId) = await EnrollmentExist(command, studiesId.Value);
                if (!enrollmentExist)
                {
                    return Status.Failed("Enrollment does not exist");
                }

                await UpdateStudentsEnrollments(command, semester, studies);
                return Status.Succeeded();
            }
            catch (SqlException exc)
            {
                await transaction.RollbackAsync();
                return Status.Failed(exc.Message);
            }
        }

        private static async Task<(bool studiesExist, int? studiesId)> GetStudiesId(SqlCommand command, string studiesName)
        {
            command.CommandText = "select IdStudy from dbo.Studies where Name = @name";
            command.Parameters.AddWithValue("name", studiesName);

            var dr = await command.ExecuteReaderAsync();
            
            if (!await dr.ReadAsync())
            {
                await dr.CloseAsync();
                return (false, null);
            }

            var studyId = (int)dr["IdStudy"];
            await dr.CloseAsync();
            return (true, studyId);
        }
        
        private static async Task<(bool exist, int? enrollmentId)> EnrollmentExist(SqlCommand command, int studiesId) => 
            await EnrollmentExist(command, studiesId, 1);

        private static async Task<(bool exist, int? enrollmentId)> EnrollmentExist(SqlCommand command, int studiesId, int semester)
        {
            command.CommandText = "select IdEnrollment from dbo.Enrollment where IdStudy = @studiesId AND Semester = @semester";
            command.Parameters.AddWithValue("studiesId", studiesId);
            command.Parameters.AddWithValue("semester", semester);

            var dr = await command.ExecuteReaderAsync();
            if (!await dr.ReadAsync())
            {
                await dr.CloseAsync();
                return (false, null);
            }

            var enrollmentId = (int)dr["IdEnrollment"];
            await dr.CloseAsync();
            return (true, enrollmentId);
        }

        private static async Task UpdateStudentsEnrollments(SqlCommand command, int semester, string studies)
        {
            command.CommandText = "EXEC UpdateEnrollment @studies, @semester";
            command.Parameters.AddWithValue("studies", studies);
            command.Parameters.AddWithValue("semester", semester);
            
            await command.ExecuteNonQueryAsync();
        }
        
        private static async Task<int> CreateEnrollment(SqlCommand command, int studiesId)
        {
            command.CommandText = "select max(IdEnrollment) as maxEnrollment from dbo.Enrollment";
            var dr = await command.ExecuteReaderAsync();
            await dr.ReadAsync();
            var maxCurrentEnrollmentId = (int)dr["maxEnrollment"];
            await dr.CloseAsync();

            command.CommandText = "insert into Enrollment(IdEnrollment, Semester, IdStudy, StartDate) values(@idEnrollment, 1, @studyId, @startDate)";
            command.Parameters.AddWithValue("idEnrollment", maxCurrentEnrollmentId + 1);
            command.Parameters.AddWithValue("studyId", studiesId);
            command.Parameters.AddWithValue("startDate", DateTime.Now);

            await command.ExecuteNonQueryAsync();

            return maxCurrentEnrollmentId + 1;
        }
        
        private static async Task<bool> CreateStudent(SqlCommand command, string indexNumber, string studentName, string studentLastName, DateTime studentBirthDate, int enrollmentId)
        {
            command.CommandText = "insert into Student(IndexNumber, FirstName, LastName, BirthDate, IdEnrollment) values (@indexNumber, @studentName, @studentLastName, @studentBirthDate, @enrollmentId)";
            command.Parameters.AddWithValue("indexNumber", indexNumber);
            command.Parameters.AddWithValue("studentName", studentName);
            command.Parameters.AddWithValue("studentLastName", studentLastName);
            command.Parameters.AddWithValue("studentBirthDate", studentBirthDate);
            command.Parameters.AddWithValue("enrollmentId", enrollmentId);

            return await command.ExecuteNonQueryAsync() is 0;
            
        }
    }
}
