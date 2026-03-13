using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using TrainingCentreManagementSystem.Models;

namespace TrainingCentreManagementSystem.Controllers
{
    public class BatchController : Controller
    {
        private readonly IConfiguration _configuration;

        public BatchController(IConfiguration configuration)
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

            List<Batch> batches = new List<Batch>();

            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd;

                if (role == "Trainer")
                {
                    int trainerId = HttpContext.Session.GetInt32("TrainerId").Value;

                    cmd = new SqlCommand(
                    "SELECT * FROM Batch WHERE TrainerId=@TrainerId", con);

                    cmd.Parameters.AddWithValue("@TrainerId", trainerId);
                }
                else if (role == "Student")
                {
                    int studentId = HttpContext.Session.GetInt32("StudentId").Value;

                    cmd = new SqlCommand(
                    "SELECT b.* FROM Batch b JOIN Student s ON b.CourseId=s.CourseId WHERE s.StudentId=@StudentId", con);

                    cmd.Parameters.AddWithValue("@StudentId", studentId);
                }
                else
                {
                    cmd = new SqlCommand("sp_GetBatches", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                }

                con.Open();

                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    batches.Add(new Batch
                    {
                        BatchId = Convert.ToInt32(dr["BatchId"]),
                        BatchName = dr["BatchName"].ToString(),
                        CourseId = Convert.ToInt32(dr["CourseId"]),
                        TrainerId = Convert.ToInt32(dr["TrainerId"])
                    });
                }
            }

            return View(batches);
        }

        public IActionResult Create()
        {
            LoadCourses();
            LoadTrainers();
            return View();
        }

        [HttpPost]
        public IActionResult Create(Batch batch)
        {
            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("sp_AddBatch", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@BatchName", batch.BatchName);
                cmd.Parameters.AddWithValue("@CourseId", batch.CourseId);
                cmd.Parameters.AddWithValue("@TrainerId", batch.TrainerId);
                cmd.Parameters.AddWithValue("@StartDate", batch.StartDate);
                cmd.Parameters.AddWithValue("@EndDate", batch.EndDate);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            Batch batch = new Batch();

            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("sp_GetBatchById", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", id);

                con.Open();

                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    batch.BatchId = Convert.ToInt32(dr["BatchId"]);
                    batch.BatchName = dr["BatchName"].ToString();
                    batch.CourseId = Convert.ToInt32(dr["CourseId"]);
                    batch.TrainerId = Convert.ToInt32(dr["TrainerId"]);
                    batch.StartDate = Convert.ToDateTime(dr["StartDate"]);
                    batch.EndDate = Convert.ToDateTime(dr["EndDate"]);
                }
            }

            LoadCourses();
            LoadTrainers();

            return View(batch);
        }

        [HttpPost]
        public IActionResult Edit(Batch batch)
        {
            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("sp_UpdateBatch", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", batch.BatchId);
                cmd.Parameters.AddWithValue("@BatchName", batch.BatchName);
                cmd.Parameters.AddWithValue("@CourseId", batch.CourseId);
                cmd.Parameters.AddWithValue("@TrainerId", batch.TrainerId);
                cmd.Parameters.AddWithValue("@StartDate", batch.StartDate);
                cmd.Parameters.AddWithValue("@EndDate", batch.EndDate);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("sp_DeleteBatch", con);
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

        private void LoadTrainers()
        {
            List<Trainer> trainers = new List<Trainer>();

            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("sp_GetAllTrainers", con);
                cmd.CommandType = CommandType.StoredProcedure;

                con.Open();

                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    trainers.Add(new Trainer
                    {
                        TrainerId = Convert.ToInt32(dr["TrainerId"]),
                        TrainerName = dr["TrainerName"].ToString()
                    });
                }
            }

            ViewBag.Trainers = trainers;
        }
    }
}