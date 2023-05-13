using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grip.Bll.DTO;
using Grip.Bll.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Server.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _studentService;
        public StudentController(IStudentService studentService)
        {
            _studentService = studentService;
        }
        [HttpGet("Search")]
        [Authorize(Roles = "Teacher, Admin")]
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
        [Authorize(Roles = "Teacher, Admin")]
        [ProducesResponseType(typeof(StudentDetailDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<StudentDetailDTO>> GetStudentDetails(int id)
        {
            return Ok(await _studentService.GetStudentDetailsAsync(id));
        }
    }
}