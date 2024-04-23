using AdminManagement.Interface;
using AdminManagement.ModelClass;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AdminManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IDepartment _department;

        public AdminController(IDepartment department)
        {
            _department = department;
        }

        [HttpPost]
        public async Task<IActionResult> CreateDepartment(string departmentname)
        {
            try
            {
                var rowsaffected = await _department.CreateDepartMent(departmentname);
                var response = new ResponseModel<string>
                {

                    Message = "Department created Successfully",
                   

                };

                return Ok(response);
            }
            catch(Exception ex) 
            {
                return StatusCode(500, new ResponseModel<string>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
        }
    }
}
