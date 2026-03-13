using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using TrainingCentreManagementSystem.Models;

namespace TrainingCentreManagementSystem.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly IConfiguration _configuration;

        public AttendanceController(IConfiguration configuration)
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

            List<Attendance> attendanceList = new List<Attendance>();

            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd;

                if (role == "Student")
                {
                    int studentId = HttpContext.Session.GetInt32("StudentId").Value;

                    cmd = new SqlCommand(
                    "SELECT * FROM Attendance WHERE StudentId=@StudentId", con);

                    cmd.Parameters.AddWithValue("@StudentId", studentId);
                }
                else if (role == "Trainer")
                {
                    int trainerId = HttpContext.Session.GetInt32("TrainerId").Value;

                    cmd = new SqlCommand(
                    "SELECT a.* FROM Attendance a " +
                    "JOIN Batch b ON a.BatchId = b.BatchId " +
                    "WHERE b.TrainerId = @TrainerId", con);

                    cmd.Parameters.AddWithValue("@TrainerId", trainerId);
                }
                else
                {
                    cmd = new SqlCommand("sp_GetAttendance", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                }

                con.Open();

                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    attendanceList.Add(new Attendance
                    {
                        AttendanceId = Convert.ToInt32(dr["AttendanceId"]),
                        StudentId = Convert.ToInt32(dr["StudentId"]),
                        BatchId = Convert.ToInt32(dr["BatchId"]),
                        Date = Convert.ToDateTime(dr["Date"]),
                        Status = dr["Status"].ToString()
                    });
                }
            }

            return View(attendanceList);
        }
        public IActionResult Create()
        {
            LoadStudents();
            LoadBatches();
            return View();
        }

        [HttpPost]
        public IActionResult Create(Attendance attendance)
        {
            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("sp_AddAttendance", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@StudentId", attendance.StudentId);
                cmd.Parameters.AddWithValue("@BatchId", attendance.BatchId);
                cmd.Parameters.AddWithValue("@Date", attendance.Date);
                cmd.Parameters.AddWithValue("@Status", attendance.Status);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            Attendance attendance = new Attendance();

            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("sp_GetAttendanceById", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", id);

                con.Open();

                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    attendance.AttendanceId = Convert.ToInt32(dr["AttendanceId"]);
                    attendance.StudentId = Convert.ToInt32(dr["StudentId"]);
                    attendance.BatchId = Convert.ToInt32(dr["BatchId"]);
                    attendance.Date = Convert.ToDateTime(dr["Date"]);
                    attendance.Status = dr["Status"].ToString();
                }
            }

            LoadStudents();
            LoadBatches();

            return View(attendance);
        }

        [HttpPost]
        public IActionResult Edit(Attendance attendance)
        {
            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("sp_UpdateAttendance", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", attendance.AttendanceId);
                cmd.Parameters.AddWithValue("@StudentId", attendance.StudentId);
                cmd.Parameters.AddWithValue("@BatchId", attendance.BatchId);
                cmd.Parameters.AddWithValue("@Date", attendance.Date);
                cmd.Parameters.AddWithValue("@Status", attendance.Status);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("sp_DeleteAttendance", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", id);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index");
        }

        private void LoadStudents()
        {
            List<Student> students = new List<Student>();

            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("sp_GetStudents", con);
                cmd.CommandType = CommandType.StoredProcedure;

                con.Open();

                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    students.Add(new Student
                    {
                        StudentId = Convert.ToInt32(dr["StudentId"]),
                        StudentName = dr["StudentName"].ToString()
                    });
                }
            }

            ViewBag.Students = students;
        }

        private void LoadBatches()
        {
            List<Batch> batches = new List<Batch>();

            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("sp_GetBatches", con);
                cmd.CommandType = CommandType.StoredProcedure;

                con.Open();

                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    batches.Add(new Batch
                    {
                        BatchId = Convert.ToInt32(dr["BatchId"]),
                        BatchName = dr["BatchName"].ToString()
                    });
                }
            }

            ViewBag.Batches = batches;
        }
    }
}