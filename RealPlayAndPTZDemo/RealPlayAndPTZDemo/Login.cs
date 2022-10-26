using Dapper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RealPlayAndPTZDemo
{
    public partial class Login : Form
    {
        RealPlayAndPTZDemo real = new RealPlayAndPTZDemo();
        SqlConnection con = new SqlConnection("Data Source=192.168.1.101; Initial Catalog = SurveilAI; User ID = sa; Password=DE@2022");
        public Login()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void login_button_Click(object sender, EventArgs e)
        {
            var getlogin = GetLogin(textBox1.Text,textBox2.Text);
            if(getlogin.Count!=0)
            {
                this.Hide();
                real.Show();
            }
            else
            {
                label3.Text = "Invalid Email Or Password";
                
            }

        }




        public List<LoginClass> GetLogin(string email,string password)
        {

            try
            {
                var query = @"select UserID,Password from Users where UserID='"+email+"' and Password='"+password+"'";
                con.Open();
                var rs = con.Query<LoginClass>(query);
                con.Close();
                return rs.ToList();
            }

            catch (Exception ex)
            {
                con.Close();
                MessageBox.Show(ex.ToString());
                throw ex;
            }

        }

        private void Login_Load(object sender, EventArgs e)
        {
            textBox2.PasswordChar = '*';
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
