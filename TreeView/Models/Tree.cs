using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using MySql.Data.MySqlClient;
using System.ComponentModel.DataAnnotations;

namespace TreeView.Models
{
    public class Tree
    {
        public int ID { get; set; }
        [Display(Name = "Node")]
        [Required]
        public int ID_Root { get; set; }
        [Required]
        public string Name { get; set; }
        public List<Tree> Childs { get; set; }

        public Tree()
        {

        }

        public Tree(MySqlDataReader dataReader)
        {
            ID = dataReader.GetInt32(0);
            ID_Root = dataReader.GetInt32(1);
            Name = dataReader.GetString(2);
            Childs = new List<Tree>();
        }

        public List<Tree> GetAll()
        {
            return Get(0);
        }

        public List<Tree> Get(int id_root)
        {
            var TreeList = GetRoot(id_root);
            TreeList = GetChilds(TreeList);

            return TreeList;
        }

        public Tree GetTreeByID(int id)
        {
            Tree tree = new Tree();

            if (!WhiteList(id.ToString()))
                return tree;

            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                string query = "SELECT * FROM tree WHERE id = " + id.ToString();

                using (MySqlCommand cmd = new MySqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();

                    using (MySqlDataReader sdr = cmd.ExecuteReader())
                    {
                        sdr.Read();
                        tree = new Tree(sdr);
                    }

                    con.Close();
                }
            }

            return tree;
        }

        private List<Tree> GetRoot(int id_root)
        {
            var RootList = new List<Tree>();

            if (!WhiteList(id_root.ToString()))
                return RootList;

            if (!ExistID(id_root) && id_root < 0)
                return RootList;

            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                string query = "SELECT * FROM tree WHERE id_root = " + id_root.ToString();

                using (MySqlCommand cmd = new MySqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();

                    using (MySqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            RootList.Add(new Tree(sdr));
                        }
                    }
                    con.Close();
                }
            }
            return RootList;
        }

        private List<Tree> GetChilds(List<Tree> TreeList)
        {
            foreach(var tree in TreeList)
            {
                var childs = Get(tree.ID);
                tree.Childs = childs;
            }
            return TreeList;
        }

        public string GetStructure(int id)
        {
            if (!WhiteList(id.ToString()))
                return "";

            if (id < 0)
                return "";
            else if (id == 0)
                return "Main";
            else
            {
                int id_root = 0;
                string name = "";

                string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
                using (MySqlConnection con = new MySqlConnection(constr))
                {
                    string query = "SELECT id_root, name FROM tree WHERE id = " + id.ToString();

                    using (MySqlCommand cmd = new MySqlCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();

                        using (MySqlDataReader sdr = cmd.ExecuteReader())
                        {
                            sdr.Read();
                            id_root = sdr.GetInt32(0);
                            name = sdr.GetString(1);
                        }

                        con.Close();
                    }
                }
                return GetStructure(id_root) + "/" + name;
            }
        }

        public Dictionary<int,string> GetStrDict()
        {
            Dictionary<int, string> strList = new Dictionary<int, string>();
            List<int> idList = GetAllID();

            strList.Add(0, "Main");

            foreach (int id in idList)
            {
                strList.Add(id, GetStructure(id));
            }

            return strList.OrderBy(str => str.Value).ToDictionary(str => str.Key, str => str.Value);
        }

        public List<int> GetAllID()
        {
            List<int> allID = new List<int>();

            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                string query = "SELECT id FROM tree";

                using (MySqlCommand cmd = new MySqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();

                    using (MySqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            allID.Add(sdr.GetInt32(0));
                        }
                    }

                    con.Close();
                }
            }

            return allID;
        }

        public bool Insert()
        {
            if (!WhiteList(ID.ToString()) || !WhiteList(ID_Root.ToString()) || !WhiteList(Name))
                return false;

            if (ID_Root != 0)
            {
                if (!ExistID(ID_Root))
                    return false;
            }

            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                string query = "INSERT INTO tree (id_root, name) VALUES (@id_root, @name)";

                using (MySqlCommand cmd = new MySqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();

                    cmd.Parameters.AddWithValue("@id_root", ID_Root);
                    cmd.Parameters.AddWithValue("@name", Name);

                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }

            return true;
        }

        public bool Update()
        {
            if (!WhiteList(ID.ToString()) || !WhiteList(ID_Root.ToString()) || !WhiteList(Name))
                return false;

            if (!ExistID(ID) || !ExistID(ID_Root))
                return false;

            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                string query = "UPDATE tree SET id_root=@id_root, name=@name WHERE id=@id";

                using (MySqlCommand cmd = new MySqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();

                    cmd.Parameters.AddWithValue("@id_root", ID_Root);
                    cmd.Parameters.AddWithValue("@name", Name);
                    cmd.Parameters.AddWithValue("@id", ID);

                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }

            return true;
        }

        public bool Delete(bool saveChilds)
        {
            if (!WhiteList(ID.ToString()) || !WhiteList(ID_Root.ToString()) || !WhiteList(Name))
                return false;

            if (!ExistID(ID))
                return false;

            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                string query = "DELETE FROM tree WHERE id=@id";

                using (MySqlCommand cmd = new MySqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();

                    cmd.Parameters.AddWithValue("@id", ID);

                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }

            if (saveChilds)
                SaveChilds(ID, ID_Root);
            else
                RemoveChilds(ID);

            return true;
        }

        private void Delete(Tree tree)
        {
            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                string query = "DELETE FROM tree WHERE id=@id";

                using (MySqlCommand cmd = new MySqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();

                    cmd.Parameters.AddWithValue("@id", tree.ID);

                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }

            foreach(var tmp in tree.Childs)
            {
                Delete(tmp);
            }
        }

        private void RemoveChilds(int id_root)
        {
            List<Tree> treeList = Get(id_root);

            foreach(var tree in treeList)
            {
                Delete(tree);
            }
        }

        private void Update(Tree tree, int id_root_new)
        {
            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                string query = "UPDATE tree SET id_root=@id_root, name=@name WHERE id=@id";

                using (MySqlCommand cmd = new MySqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();

                    cmd.Parameters.AddWithValue("@id_root", id_root_new);
                    cmd.Parameters.AddWithValue("@name", tree.Name);
                    cmd.Parameters.AddWithValue("@id", tree.ID);

                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
        }

        private void SaveChilds(int id_root, int id_root_new)
        {
            List<Tree> treeList = GetRoot(id_root);

            foreach (var tree in treeList)
            {
                Update(tree, id_root_new);
            }
        }

        public bool SearchID(int id_s = 0, int id_tree = 0)
        {
            if (id_s <= 0 || id_tree <= 0)
                return false;

            var tree = GetTreeByID(id_tree);

            if (id_s == tree.ID || id_s == tree.ID_Root)
                return true;
            else
                return SearchID(id_s, tree.ID_Root) || false;
        }

        private bool ExistID(int id)
        {
            if (!WhiteList(id.ToString()))
                return false;

            var idList = GetAllID();

            return idList.Contains(id);
        }

        private bool WhiteList(string text)
        {
            var positiveExp = new System.Text.RegularExpressions.Regex(@"^[a-zA-Z0-9-_\\s]*$");

            return positiveExp.IsMatch(text);
        }
    }
}