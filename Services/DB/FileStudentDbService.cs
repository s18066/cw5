using System;
using System.Threading.Tasks;
using Wyklad5.DTOs;
using Wyklad5.DTOs.Requests;

namespace Wyklad5.Services.DB
{
    public class FileStudentDbService : IStudentDbService
    {
        public Task<Status> EnrollStudent(EnrollStudentRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<Status> PromoteStudents(int semester, string studies)
        {
            throw new NotImplementedException();
        }
    }
}
