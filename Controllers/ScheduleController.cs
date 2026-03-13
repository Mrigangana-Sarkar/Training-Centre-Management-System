using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using TrainingCentreManagementSystem.Models;

namespace TrainingCentreManagementSystem.Controllers
{
    public class ScheduleController : Controller
    {
        private readonly IConfiguration _configuration;

        public ScheduleController(IConfiguration configuration)
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

            List<Schedule> schedules = new List<Schedule>();

            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd;

                if (role == "Trainer")
                {
                    int trainerId = HttpContext.Session.GetInt32("TrainerId").Value;

                    cmd = new SqlCommand(
                    "SELECT ts.* FROM TrainingSchedule ts " +
                    "JOIN Batch b ON ts.BatchId = b.BatchId " +
                    "WHERE b.TrainerId = @TrainerId", con);

                    cmd.Parameters.AddWithValue("@TrainerId", trainerId);
                }
                else if (role == "Student")
                {
                    int studentId = HttpContext.Session.GetInt32("StudentId").Value;

                    cmd = new SqlCommand(
                    "SELECT ts.* FROM TrainingSchedule ts " +
                    "JOIN Batch b ON ts.BatchId = b.BatchId " +
                    "JOIN Student s ON b.CourseId = s.CourseId " +
                    "WHERE s.StudentId = @StudentId", con);

                    cmd.Parameters.AddWithValue("@StudentId", studentId);
                }
                else
                {
                    cmd = new SqlCommand("sp_GetSchedules", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                }

                con.Open();

                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    schedules.Add(new Schedule
                    {
                        ScheduleId = Convert.ToInt32(dr["ScheduleId"]),
                        BatchId = Convert.ToInt32(dr["BatchId"]),
                        TrainingDate = Convert.ToDateTime(dr["TrainingDate"]),
                        Topic = dr["Topic"].ToString()
                    });
                }
            }

            return View(schedules);
        }

        public IActionResult Create()
        {
            LoadBatches();
            return View();
        }

        [HttpPost]
        public IActionResult Create(Schedule schedule)
        {
            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("sp_AddSchedule", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@BatchId", schedule.BatchId);
                cmd.Parameters.AddWithValue("@TrainingDate", schedule.TrainingDate);
                cmd.Parameters.AddWithValue("@Topic", schedule.Topic);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            Schedule schedule = new Schedule();

            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("sp_GetScheduleById", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", id);

                con.Open();

                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    schedule.ScheduleId = Convert.ToInt32(dr["ScheduleId"]);
                    schedule.BatchId = Convert.ToInt32(dr["BatchId"]);
                    schedule.TrainingDate = Convert.ToDateTime(dr["TrainingDate"]);
                    schedule.Topic = dr["Topic"].ToString();
                }
            }

            LoadBatches();

            return View(schedule);
        }

        [HttpPost]
        public IActionResult Edit(Schedule schedule)
        {
            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("sp_UpdateSchedule", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", schedule.ScheduleId);
                cmd.Parameters.AddWithValue("@BatchId", schedule.BatchId);
                cmd.Parameters.AddWithValue("@TrainingDate", schedule.TrainingDate);
                cmd.Parameters.AddWithValue("@Topic", schedule.Topic);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("sp_DeleteSchedule", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", id);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index");
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