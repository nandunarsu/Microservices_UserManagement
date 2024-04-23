using Dapper;
using System.Data;
using System.Text.RegularExpressions;
using UserHospital.Context;
using UserHospital.Entity;
using UserHospital.GlobalExceptions;
using UserHospital.Interface;
using UserHospital.Models;

namespace UserHospital.Service
{
    public class UserServicecs : IUser
    {
        private readonly UsermanagementContext _context;
        private readonly IAuthServices _authService;

        public UserServicecs(UsermanagementContext context, IAuthServices authService)
        {
            _context = context;
            _authService = authService;
           
        }
        public bool IsValidEmail(string email)
        {

            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return Regex.IsMatch(email, pattern);
        }
        public bool IsValidPassword(string password)
        {
            
            string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{6,}$";
            return Regex.IsMatch(password, pattern);
        }
        public async Task<bool> RegisterUser(UserRegistrationModel userRegModel,string Role)
        {
            var parametersToCheckEmailIsValid = new DynamicParameters();
            parametersToCheckEmailIsValid.Add("Email", userRegModel.Email, DbType.String);

            var querytoCheckEmailIsNotDuplicate = @"
                SELECT COUNT(*)
                FROM Users
                WHERE Email = @Email;
            ";

            var query = @"
                INSERT INTO Users (FirstName, LastName, Email, Password,Role)
                VALUES (@FirstName, @LastName, @Email, @Password,@Role);
            ";

            var parameters = new DynamicParameters();
            parameters.Add("FirstName", userRegModel.FirstName, DbType.String);
            parameters.Add("LastName", userRegModel.LastName, DbType.String);

            //Check Emailformat Using Regex
            if (!IsValidEmail(userRegModel.Email))
            {
                throw new InvalidEmailFormatException("Invalid email format");
            }

            parameters.Add("Email", userRegModel.Email, DbType.String);

            //convert Plain Password into cryptographic String 
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(userRegModel.Password);
            parameters.Add("Password", hashedPassword, DbType.String);

            parameters.Add("Role", Role, DbType.String);

            using (var connection = _context.CreateConnection())
            {
                // Check if table exists
                bool tableExists = await connection.QueryFirstOrDefaultAsync<bool>(
                    @"
                    SELECT COUNT(*)
                    FROM INFORMATION_SCHEMA.TABLES
                    WHERE TABLE_NAME = 'Users';
                ");

                // Create table if it doesn't exist
                if (!tableExists)
                {
                    await connection.ExecuteAsync(@"
                       CREATE TABLE Users (
                                         UserID INT IDENTITY(1, 1) PRIMARY KEY,
                                         FirstName NVARCHAR(50) NOT NULL,
                                         LastName NVARCHAR(50) NOT NULL,
                                         Email NVARCHAR(100) UNIQUE NOT NULL,
                                         Password NVARCHAR(100) NOT NULL,
                                         Role NVARCHAR(50) CHECK (Role IN ('Admin', 'Doctor', 'Patient')) NOT NULL,
                                         IsApproved BIT DEFAULT 0 NOT NULL
                                            );

                    ");
                }

                // Check if email already exists
                bool emailExists = await connection.QueryFirstOrDefaultAsync<bool>(querytoCheckEmailIsNotDuplicate, parametersToCheckEmailIsValid);
                if (emailExists)
                {
                    throw new DuplicateEmailException("Email address is already in use");
                }

                // Insert new user
                await connection.ExecuteAsync(query, parameters);
            }

            return true;
        }
        public async Task<string> UserLogin(UserLoginModel userLogin)
        {
            using (var connection = _context.CreateConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("Email", userLogin.Email);


                string query = @"
                        SELECT * FROM Users WHERE Email = @Email ;
                       ";


                var user = await connection.QueryFirstOrDefaultAsync<UserEntity>(query, parameters);

                if (user == null)
                {
                    throw new UserNotFoundException($"User with email '{userLogin.Email}' not found.");
                }

                if (!BCrypt.Net.BCrypt.Verify(userLogin.Password, user.Password))
                {
                    throw new InvalidPasswordException($"User with Password '{userLogin.Password}' not Found.");
                }
                //if password enterd from user and password in db match then generate Token 
                var token = _authService.GenerateJwtToken(user);
                return token;

            }
        }

        public async Task<IEnumerable<UserEntity>> GetUserDetails()
        {

            var query = " SELECT * FROM Users";
            using (var connection = _context.CreateConnection())
            {
                var registration = await connection.QueryAsync<UserEntity>(query);
                return registration.ToList();
            }

        }

    }
}
