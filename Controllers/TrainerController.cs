using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using TrainingCentreManagementSystem.Models;

namespace TrainingCentreManagementSystem.Controllers
{
    public class TrainerController : Controller
    {
        private readonly IConfiguration _configuration;

        public TrainerController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        }

        public IActionResult Index()
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
                        TrainerName = dr["TrainerName"].ToString(),
                        Email = dr["Email"].ToString(),
                        Phone = dr["Phone"].ToString(),
                        Specialization = dr["Specialization"].ToString()
                    });
                }
            }

            return View(trainers);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Trainer trainer)
        {
            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("sp_AddTrainer", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@TrainerName", trainer.TrainerName);
                cmd.Parameters.AddWithValue("@Email", trainer.Email);
                cmd.Parameters.AddWithValue("@Phone", trainer.Phone);
                cmd.Parameters.AddWithValue("@Specialization", trainer.Specialization);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            Trainer trainer = new Trainer();

            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("sp_GetTrainerById", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", id);

                con.Open();

                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    trainer.TrainerId = Convert.ToInt32(dr["TrainerId"]);
                    trainer.TrainerName = dr["TrainerName"].ToString();
                    trainer.Email = dr["Email"].ToString();
                    trainer.Phone = dr["Phone"].ToString();
                    trainer.Specialization = dr["Specialization"].ToString();
                }
            }

            return View(trainer);
        }

        [HttpPost]
        public IActionResult Edit(Trainer trainer)
        {
            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("sp_UpdateTrainer", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", trainer.TrainerId);
                cmd.Parameters.AddWithValue("@TrainerName", trainer.TrainerName);
                cmd.Parameters.AddWithValue("@Email", trainer.Email);
                cmd.Parameters.AddWithValue("@Phone", trainer.Phone);
                cmd.Parameters.AddWithValue("@Specialization", trainer.Specialization);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("sp_DeleteTrainer", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", id);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index");
        }
    }
}