using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using MySql.Data.MySqlClient;
using System.ComponentModel.DataAnnotations;

namespace TreeView.Models
{
    public class User
    {
        public int ID { get; set; }
        [Display(Name = "Login")]
        [Required]
        public string Name { get; set; }
        [Required]
        public string Password { get; set; }

        public User()
        {

        }

        public User(MySqlDataReader dataReader)
        {
            ID = dataReader.GetInt32(0);
            Name = dataReader.GetString(1);
        }

        public List<User> GetAll()
        {
            List<User> userList = new List<User>();
            var idList = GetAllID();

            foreach (var id in idList)
            {
                userList.Add(Get(id));
            }

            return userList;
        }

        public User Get(int id)
        {
            User user = new User();

            if (!WhiteList(id.ToString()))
                return user;

            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                string query = "SELECT * FROM user WHERE id = " + id.ToString();

                using (MySqlCommand cmd = new MySqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();

                    using (MySqlDataReader sdr = cmd.ExecuteReader())
                    {
                        sdr.Read();
                        user = new User(sdr);
                    }

                    con.Close();
                }
            }

            return user;
        }

        public List<int> GetAllID()
        {
            List<int> idList = new List<int>();

            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                string query = "SELECT id FROM user";

                using (MySqlCommand cmd = new MySqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();

                    using (MySqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while(sdr.Read())
                        {
                            idList.Add(sdr.GetInt32(0));
                        }
                    }

                    con.Close();
                }
            }

            return idList;
        }

        public bool Exist()
        {
            var userList = GetAll();

            return userList.Exists(user => user.ID == ID && user.Name == Name);
        }

        public int SignIn()
        {
            if (!WhiteList(Name) || !WhiteList(Password))
                return 0;

            List<User> userList = new List<User>();

            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                string query = "SELECT id, name FROM user WHERE name=@name AND password=@pwd";

                using (MySqlCommand cmd = new MySqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();

                    cmd.Parameters.AddWithValue("@name", Name);
                    cmd.Parameters.AddWithValue("@pwd", Password);

                    using (MySqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while(sdr.Read())
                        {
                            userList.Add(new User(sdr));
                        }
                    }

                    con.Close();
                }
            }

            return (userList.Count == 1) ? userList[0].ID : 0;
        }

        private bool WhiteList(string text)
        {
            var positiveExp = new System.Text.RegularExpressions.Regex(@"^[a-zA-Z0-9]*$");

            return positiveExp.IsMatch(text);
        }
    }
}