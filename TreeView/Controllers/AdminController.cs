using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TreeView.Models;
using System.Security.Cryptography;
using System.Text;

namespace TreeView.Controllers
{
    public class AdminController : Controller
    {
        protected Tree treeProt;
        protected User userProt;
        // GET: Admin
        public ActionResult Index()
        {
            if (!IsLogged())
                return RedirectToAction("Signin", "Admin");

            treeProt = new Tree();

            ViewBag.treeList = treeProt.GetAll();
            ViewBag.IsLogged = IsLogged() ? 1 : 0;

            return View();
        }

        public ActionResult Add()
        {
            if (!IsLogged())
                return RedirectToAction("Signin", "Admin");

            treeProt = new Tree();
            ViewBag.treeList = treeProt.GetAll();
            ViewBag.str = treeProt.GetStrDict();
            ViewBag.IsLogged = IsLogged() ? 1 : 0;

            return View();
        }

        [HttpPost]
        public ActionResult Add(Tree tree)
        {
            if (!IsLogged())
                return RedirectToAction("Signin", "Admin");

            tree.Insert();
            return RedirectToAction("Index", "Admin");
        }

        public ActionResult Edit(int id = 0)
        {
            if (!IsLogged())
                return RedirectToAction("Signin", "Admin");

            treeProt = new Tree();
            List<int> allID = treeProt.GetAllID();

            if (id <= 0 || !allID.Exists(id_node => id_node == id))
                return RedirectToAction("Index", "Admin");

            var strDic = treeProt.GetStrDict();
            var tmpDic = treeProt.GetStrDict();

            foreach (var id_node in allID)
            {
                if (treeProt.SearchID(id, id_node))
                    strDic.Remove(id_node);
            }

            ViewBag.treeList = treeProt.GetAll();
            ViewBag.str = strDic;
            ViewBag.IsLogged = IsLogged() ? 1 : 0;

            return View(treeProt.GetTreeByID(id));
        }

        [HttpPost]
        public ActionResult Edit(Tree tree)
        {
            if (!IsLogged())
                return RedirectToAction("Signin", "Admin");

            if (tree != null)
                tree.Update();

            return RedirectToAction("Index", "Admin");
        }

        public ActionResult Remove(int id = 0)
        {
            if (!IsLogged())
                return RedirectToAction("Signin", "Admin");

            treeProt = new Tree();
            List<int> allID = treeProt.GetAllID();

            if (id <= 0 || !allID.Exists(id_node => id_node == id))
                return RedirectToAction("Index", "Admin");

            ViewBag.treeList = treeProt.GetAll();
            ViewBag.str = treeProt.GetStructure(id);
            ViewBag.IsLogged = IsLogged() ? 1 : 0;

            return View(treeProt.GetTreeByID(id));
        }

        [HttpPost]
        public ActionResult Remove(Tree tree, string Childs)
        {
            if (!IsLogged())
                return RedirectToAction("Signin", "Admin");

            tree = tree.GetTreeByID(tree.ID);

            if (tree != null)
            {
                if (Childs.Equals("save"))
                    tree.Delete(true);
                else if (Childs.Equals("remove"))
                    tree.Delete(false);
            }

            return RedirectToAction("Index", "Admin");
        }

        public ActionResult Signin()
        {
            if (IsLogged())
                return RedirectToAction("Signin", "Admin");

            treeProt = new Tree();

            ViewBag.treeList = treeProt.GetAll();
            ViewBag.IsLogged = IsLogged() ? 1 : 0;
            ViewBag.pwd = "";

            return View();
        }

        [HttpPost]
        public ActionResult Signin(User user)
        {
            if (IsLogged())
                return RedirectToAction("Signin", "Admin");

            MD5 md5 = MD5.Create();
            Byte[] oByte = Encoding.ASCII.GetBytes(user.Password);
            Byte[] eByte = md5.ComputeHash(oByte);
            user.Password = BitConverter.ToString(eByte);
            user.Password = user.Password.ToLower().Replace("-", "");

            int id = user.SignIn();

            if (id != 0)
                Session["user"] = user.Get(id);

            return RedirectToAction("Index", "Admin");
        }

        public ActionResult Signout()
        {
            Session["user"] = new User();

            return RedirectToAction("Signin", "Admin");
        }

        private bool IsLogged()
        {
            User user = Session["user"] as User;
            if (user == null)
            {
                Session["user"] = new User();
                return false;
            }
            else if (user.Exist())
                return true;
            else
                return false;
        }
    }
}