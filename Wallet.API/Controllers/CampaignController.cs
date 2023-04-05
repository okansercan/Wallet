using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Wallet.API.Controllers
{
    [Route("api/[controller]")]
    public class CampaignController : Controller
    {
        private string connString = string.Empty;
        public readonly IConfiguration config;

        public CampaignController(IConfiguration configuration)
        {
            config = configuration;
            connString = config["ConnectionStrings:DefaultConnection"];
        }
        // GET: api/campaign
        [HttpGet]
        public IEnumerable<Campaign> Get()
        {
            connString = "Server=tcp:twinsoft.database.windows.net,1433;Initial Catalog=nova;Persist Security Info=False;User ID=twsa;Password=Twinbr0s;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

            List<Campaign> campaigns = new List<Campaign>();

            using (var connection = new SqlConnection(connString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("Select * from Campaign", connection);

                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    campaigns.Add(new Campaign
                    {
                        Brand = reader["Brand"].ToString(),
                        Sector = reader["Sector"].ToString(),
                        Title = reader["Title"].ToString(),
                        Description = reader["Description"].ToString(),
                        ImageUrl = reader["ImageUrl"].ToString(),
                        DetailUrl = reader["DetailUrl"].ToString(),
                        Logo = reader["Logo"].ToString()
                    });
                }
                connection.Close();
            }

            return campaigns;
        }
    }
}

