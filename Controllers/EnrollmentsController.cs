using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Wyklad5.DTOs.Requests;
using Wyklad5.DTOs.Responses;
using Wyklad5.Models;
using Wyklad5.Services;
using Wyklad5.Services.DB;

namespace Wyklad5.Controllers
{
    [Route("api/enrollments")]
    [ApiController]
    public class EnrollmentsController : ControllerBase
    {
        private readonly IStudentDbService _service;

        public EnrollmentsController(IStudentDbService service)
        {
            _service = service;
        }


        [HttpPost]
        public async Task<IActionResult> EnrollStudent(EnrollStudentRequest request)
        {
            var serviceStatus  = await _service.EnrollStudent(request);
            
            var response = new EnrollStudentResponse()
            {
                Semester = 1,
                LastName = request.LastName,
                StartDate = DateTime.Now
            };

            if (serviceStatus.IsSucceeded)
            {
                return BadRequest(serviceStatus.Reason);
            }

            return Created(string.Empty, response);
        }
        
        [HttpPost("promotions")]
        public async Task<IActionResult> Promote(PromoteRequest request)
        {
            var serviceStatus  = await _service.PromoteStudents(request.Semester, request.Studies);

            var response = new PromoteResponse()
            {
                Semester = request.Semester + 1
            };

            if (serviceStatus.IsSucceeded)
            {
                return NotFound();
            }

            return Created(string.Empty, response);
        }
    }
}