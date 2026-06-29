using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CampuraOne
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btn_Login_Click(sender, EventArgs.Empty);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e) {

        }
        private void btnLogin_Click(object sender, EventArgs e) {
            

        }

        private void LoginForm_Load(object sender, EventArgs e) {

        }

        private void btn_Login_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (username == "admin" && password == "123456") {
                MainForm mainForm = new MainForm();

                this.Hide();          // 隐藏登录页面
                mainForm.ShowDialog(); // 打开主页
                this.Close();         // 主页关闭后，关闭登录页面
            } else {
                MessageBox.Show("用户名或密码错误", "登录失败");
            }
        }

        private void txtUsername_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
