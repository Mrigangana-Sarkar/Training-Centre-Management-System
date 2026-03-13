using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using TrainingCentreManagementSystem.Models;

namespace TrainingCentreManagementSystem.Controllers
{
    public class StudentController : Controller
    {
        private readonly IConfiguration _configuration;

        public StudentController(IConfiguration configuration)
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

            List<Student> students = new List<Student>();

            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd;

                if (role == "Trainer")
                {
                    int trainerId = HttpContext.Session.GetInt32("TrainerId").Value;

                    cmd = new SqlCommand(
                    "SELECT s.* FROM Student s JOIN Batch b ON s.CourseId=b.CourseId WHERE b.TrainerId=@TrainerId", con);

                    cmd.Parameters.AddWithValue("@TrainerId", trainerId);
                }
                else
                {
                    cmd = new SqlCommand("sp_GetStudents", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                }

                con.Open();

                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    students.Add(new Student
                    {
                        StudentId = Convert.ToInt32(dr["StudentId"]),
                        StudentName = dr["StudentName"].ToString(),
                        Email = dr["Email"].ToString(),
                        Phone = dr["Phone"].ToString(),
                        CourseId = Convert.ToInt32(dr["CourseId"])
                    });
                }
            }

            return View(students);
        }

        public IActionResult Create()
        {
            LoadCourses();
            return View();
        }

        [HttpPost]
        public IActionResult Create(Student student)
        {
            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("sp_AddStudent", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@StudentName", student.StudentName);
                cmd.Parameters.AddWithValue("@Email", student.Email);
                cmd.Parameters.AddWithValue("@Phone", student.Phone);
                cmd.Parameters.AddWithValue("@CourseId", student.CourseId);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            Student student = new Student();

            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("sp_GetStudentById", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", id);

                con.Open();

                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    student.StudentId = Convert.ToInt32(dr["StudentId"]);
                    student.StudentName = dr["StudentName"].ToString();
                    student.Email = dr["Email"].ToString();
                    student.Phone = dr["Phone"].ToString();
                    student.CourseId = Convert.ToInt32(dr["CourseId"]);
                }
            }

            LoadCourses();
            return View(student);
        }

        [HttpPost]
        public IActionResult Edit(Student student)
        {
            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("sp_UpdateStudent", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", student.StudentId);
                cmd.Parameters.AddWithValue("@StudentName", student.StudentName);
                cmd.Parameters.AddWithValue("@Email", student.Email);
                cmd.Parameters.AddWithValue("@Phone", student.Phone);
                cmd.Parameters.AddWithValue("@CourseId", student.CourseId);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("sp_DeleteStudent", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", id);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index");
        }

        private void LoadCourses()
        {
            List<Course> courses = new List<Course>();

            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("sp_GetCourses", con);
                cmd.CommandType = CommandType.StoredProcedure;

                con.Open();

                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    courses.Add(new Course
                    {
                        CourseId = Convert.ToInt32(dr["CourseId"]),
                        CourseName = dr["CourseName"].ToString()
                    });
                }
            }

            ViewBag.Courses = courses;
        }
    }
}