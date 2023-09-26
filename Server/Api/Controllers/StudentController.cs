using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grip.Bll.DTO;
using Grip.Bll.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Grip.Api.Controllers
{
    /// <summary>
    /// Controller for managing students.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _studentService;

        /// <summary>
        /// Constructor for the student controller.
        /// </summary>
        /// <param name="studentService">Student service</param>
        public StudentController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        /// <summary>
        /// Options for the controller
        /// Only used for routing
        /// </summary>
        [HttpOptions]
        public IActionResult Options()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns students matching the search criteria.
        /// </summary>
        /// <param name="name">The string the students name should contain</param>
        /// <param name="groupId">Id of the student</param>
        /// <returns>List of the users matchin the criteria</returns>
        [HttpGet("Search")]
        [Authorize(Roles = "Teacher, Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(typeof(IEnumerable<UserInfoDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<UserInfoDTO>>> GetStudents([FromQuery] string? name, [FromQuery] int? groupId)
        {
            return Ok(await _studentService.SearchStudentsAsync(name, groupId));
        }

        /// <summary>
        /// Gets the details of a student, including their abs.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "Teacher, Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(typeof(StudentDetailDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<StudentDetailDTO>> GetStudentDetails(int id)
        {
            return Ok(await _studentService.GetStudentDetailsAsync(id));
        }
    }
}