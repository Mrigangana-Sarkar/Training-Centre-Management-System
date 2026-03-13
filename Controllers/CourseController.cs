using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using TrainingCentreManagementSystem.Models;

namespace TrainingCentreManagementSystem.Controllers
{
    public class CourseController : Controller
    {
        private readonly IConfiguration _configuration;

        public CourseController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        }

        public IActionResult Index()
        {
            string role = HttpContext.Session.GetString("Role");

            List<Course> courses = new List<Course>();

            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd;

                if (role == "Trainer")
                {
                    int trainerId = HttpContext.Session.GetInt32("TrainerId").Value;

                    cmd = new SqlCommand(
                    "SELECT DISTINCT c.* FROM Course c " +
                    "JOIN Batch b ON c.CourseId = b.CourseId " +
                    "WHERE b.TrainerId = @TrainerId", con);

                    cmd.Parameters.AddWithValue("@TrainerId", trainerId);
                }
                else if (role == "Student")
                {
                    int studentId = HttpContext.Session.GetInt32("StudentId").Value;

                    cmd = new SqlCommand(
                    "SELECT c.* FROM Course c " +
                    "JOIN Student s ON c.CourseId = s.CourseId " +
                    "WHERE s.StudentId = @StudentId", con);

                    cmd.Parameters.AddWithValue("@StudentId", studentId);
                }
                else
                {
                    cmd = new SqlCommand("sp_GetCourses", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                }

                con.Open();

                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    courses.Add(new Course
                    {
                        CourseId = Convert.ToInt32(dr["CourseId"]),
                        CourseName = dr["CourseName"].ToString(),
                        Duration = dr["Duration"].ToString(),
                        Fee = Convert.ToDecimal(dr["Fee"])
                    });
                }
            }

            return View(courses);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Course course)
        {
            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("sp_AddCourse", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@CourseName", course.CourseName);
                cmd.Parameters.AddWithValue("@Duration", course.Duration);
                cmd.Parameters.AddWithValue("@Fee", course.Fee);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            Course course = new Course();

            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("sp_GetCourseById", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", id);

                con.Open();

                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    course.CourseId = Convert.ToInt32(dr["CourseId"]);
                    course.CourseName = dr["CourseName"].ToString();
                    course.Duration = dr["Duration"].ToString();
                    course.Fee = Convert.ToDecimal(dr["Fee"]);
                }
            }

            return View(course);
        }

        [HttpPost]
        public IActionResult Edit(Course course)
        {
            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("sp_UpdateCourse", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", course.CourseId);
                cmd.Parameters.AddWithValue("@CourseName", course.CourseName);
                cmd.Parameters.AddWithValue("@Duration", course.Duration);
                cmd.Parameters.AddWithValue("@Fee", course.Fee);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("sp_DeleteCourse", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", id);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index");
        }
    }
}