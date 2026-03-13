using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using TrainingCentreManagementSystem.Models;

namespace TrainingCentreManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;

        public AccountController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        }

        // LOGIN PAGE
        public IActionResult Login()
        {
            return View();
        }

        // LOGIN POST
        [HttpPost]
        public IActionResult Login(string name, string email, string role)
        {
            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd;

                if (role == "Admin")
                {
                    cmd = new SqlCommand(
                    "SELECT * FROM Users WHERE Username=@Name AND Email=@Email", con);
                }
                else if (role == "Trainer")
                {
                    cmd = new SqlCommand(
                    "SELECT * FROM Trainer WHERE TrainerName=@Name AND Email=@Email", con);
                }
                else
                {
                    cmd = new SqlCommand(
                    "SELECT * FROM Student WHERE StudentName=@Name AND Email=@Email", con);
                }

                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@Email", email);

                con.Open();

                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    HttpContext.Session.SetString("Role", role);

                    if (role == "Admin")
                    {
                        return RedirectToAction("AdminDashboard");
                    }

                    if (role == "Trainer")
                    {
                        HttpContext.Session.SetInt32("TrainerId",
                            Convert.ToInt32(dr["TrainerId"]));

                        return RedirectToAction("TrainerDashboard");
                    }

                    if (role == "Student")
                    {
                        HttpContext.Session.SetInt32("StudentId",
                            Convert.ToInt32(dr["StudentId"]));

                        return RedirectToAction("StudentDashboard");
                    }
                }
            }

            ViewBag.Message = "Invalid Login";
            return View();
        }

        // CREATE ADMIN PAGE
        public IActionResult CreateAdmin()
        {
            return View();
        }

        // CREATE ADMIN POST
        [HttpPost]
        public IActionResult CreateAdmin(string name, string email, string phone, string password)
        {
            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand(
                "INSERT INTO Users (Username,Email,Phone,Password,Role) VALUES (@Name,@Email,@Phone,@Password,'Admin')", con);

                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Phone", phone);
                cmd.Parameters.AddWithValue("@Password", password);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Login");
        }

        // DASHBOARDS
        public IActionResult AdminDashboard()
        {
            return View();
        }

        public IActionResult TrainerDashboard()
        {
            return View();
        }

        public IActionResult StudentDashboard()
        {
            return View();
        }

        // LOGOUT
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}