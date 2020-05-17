using System.Threading.Tasks;
using Wyklad5.DTOs;
using Wyklad5.DTOs.Requests;

namespace Wyklad5.Services.DB
{
    public interface IStudentDbService
    {
        Task<Status> EnrollStudent(EnrollStudentRequest request);
        Task<Status> PromoteStudents(int semester, string studies);
    }
}
