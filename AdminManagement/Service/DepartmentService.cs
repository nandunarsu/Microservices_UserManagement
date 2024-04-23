using AdminManagement.Context;
using AdminManagement.Interface;
using Dapper;

namespace AdminManagement.Service
{
    public class DepartmentService : IDepartment
    {
       
            private readonly AdminContext _context;

        public DepartmentService(AdminContext context)
        {
            _context = context;
        }

        public async  Task<int> CreateDepartMent(string departmentname)
        {
            var parameters = new DynamicParameters();
            parameters.Add("Departmentname", departmentname);
            var query = "INSERT INTO DEPARTMENT(Departmentname) VALUES (@Departmentname);";
            using (var connection = _context.CreateConnection())
            {
                int rowaffected =  await connection.ExecuteAsync(query, parameters);
                return rowaffected;
            }
        }
    }
    
}
