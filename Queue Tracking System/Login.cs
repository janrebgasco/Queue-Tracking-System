using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using Newtonsoft.Json;
using System.Security.Cryptography;


namespace Queue_Tracking_System
{
    public partial class Login : Form
    {
        //Configure FirebaseConfig
        IFirebaseConfig config = new FirebaseConfig
        {

            AuthSecret = "jAPNREXqaNdnbrnztpmBqn6yGjp1myhFP4kieJj5",
            BasePath = "https://queue-management-system-202109-default-rtdb.firebaseio.com/"

        };
        //Firebase Client
        IFirebaseClient client;
        string establishmentID;
        public Login()
        {
            InitializeComponent();
        }

        private void btnSignup_Click(object sender, EventArgs e)
        {
            //Register Form
            Signup rf = new Signup();
            this.Hide();
            rf.ShowDialog();
            this.Close();
        }
        
        private void Login_Load(object sender, EventArgs e)
        {

            if (Properties.Settings.Default.userName != null)
            {
                queueManager home = new queueManager();
                this.Hide();
                home.ShowDialog();
                this.Close();
            }
            else
            {
                //MessageBox.Show(Properties.Settings.Default.userName);
                try
                {
                    //Getting the Connection
                    client = new FireSharp.FirebaseClient(config);

                    if (client != null)
                    {


                        this.CenterToScreen();
                        //this.Size = Screen.PrimaryScreen.WorkingArea.Size;
                        //this.WindowState = FormWindowState.Normal;

                    }

                }
                catch
                {
                    MessageBox.Show("Connection Failed.");
                }
                txtBoxPass._TextBox.PasswordChar = '*';
            }
        }
        

        private void txtBoxUsername_Enter(object sender, EventArgs e)
        {
            MessageBox.Show("nagtrue");
            if (txtBoxUname.text == "Enter Username")
            {
                txtBoxUname.text = "";
            }
        }

        private void txtBoxUsername_Leave(object sender, EventArgs e)
        {
            if (txtBoxUname.text == "")
            {
                txtBoxUname.text = "Enter Username";
            }
        }

        //Declare a static string
        public static string usernamepass;

        private void btnLogin_Click(object sender, EventArgs e)
        {
            //Login

            if (string.IsNullOrEmpty(txtBoxUname.text) || string.IsNullOrEmpty(txtBoxPass.text))
            {
                // Check if textbox is Empty
                MessageBox.Show("Please Put Username or Password.");
            }
            else
            {

                string uname = hashGen(txtBoxUname.text);
                string pword = txtBoxPass.text;
                FirebaseResponse uRes = client.Get("Users/" + uname + "/" + "username");
                FirebaseResponse pRes = client.Get("Users/" + uname + "/" + "password");
                

                string userresult = uRes.ResultAs<string>();
                string passresult = pRes.ResultAs<string>();

                if (hashGen(txtBoxUname.text) == userresult)
                {

                    if (sha256(txtBoxPass.text) == passresult)
                    {
                        MessageBox.Show("Logged in successfully\nWelcome to QTMS " + txtBoxUname.text);
                        Properties.Settings.Default.userName = txtBoxUname.text;
                        //Declare some public string so you can pass the data to the another Frame.
                        usernamepass = txtBoxUname.text;
                        
                        queueManager home = new queueManager();
                        this.Hide();
                        home.ShowDialog();
                        this.Close();

                    }
                    else 
                    {
                        MessageBox.Show("Username or password is incorrect");
                    }

                }
                else 
                {
                    MessageBox.Show("User does not exist ");
                }

            }
        }
        /*
        private void showPass_CheckedChanged(object sender, EventArgs e)
        {
            //Show password using Checkbox
            if (showPass.Checked)
            {
                //txtBoxPass.UseSystemPasswordChar = false;
            }
            else
            {
                //txtBoxPass.UseSystemPasswordChar = true;
            }
        }
        */
        static string sha256(string randomString)
        {
            var crypt = new SHA256Managed();
            string hash = String.Empty;
            byte[] crypto = crypt.ComputeHash(Encoding.ASCII.GetBytes(randomString));
            foreach (byte theByte in crypto)
            {
                hash += theByte.ToString("x2");
            }
            return hash;
        }
        private String hashGen(string strInput)
        {
            string hashCode = String.Format("{0:X}", strInput.GetHashCode());
            Console.WriteLine(hashCode);

            return hashCode;

        }

        private void txtBoxPass_TextChanged(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void txtBoxPass_OnTextChange(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {
            //Register Form
            Signup rf = new Signup();
            this.Hide();
            rf.ShowDialog();
            this.Close();
        }
        

        private void bunifuFlatButton1_Click(object sender, EventArgs e)
        {
            //Login
            String username = this.txtBoxUname.text;
            String password = this.txtBoxPass.text;
            if (username.Length == 0) {
                epTb1.SetError(txtBoxUname,"Username is required");
            }
            if (password.Length == 0)
            {
                epTb1.SetError(txtBoxPass, "Password is required");
            }
            else
            {

                string uname = hashGen(txtBoxUname.text);
                string pword = txtBoxPass.text;
                FirebaseResponse uRes = client.Get("Users/" + uname + "/" + "username");
                FirebaseResponse pRes = client.Get("Users/" + uname + "/" + "password");
                FirebaseResponse esRes = client.Get("Users/" + uname + "/" + "establishmentID");

                establishmentID = esRes.ResultAs<string>();

                string userresult = uRes.ResultAs<string>();
                string passresult = pRes.ResultAs<string>();

                if (hashGen(txtBoxUname.text) == userresult)
                {

                    if (sha256(txtBoxPass.text) == passresult)
                    {
                        MessageBox.Show("Welcome to Queue Tracking Management System ");
                        Properties.Settings.Default.userName = txtBoxUname.text;
                        Properties.Settings.Default.esID = establishmentID;
                        //Declare some public string so you can pass the data to the another Frame.
                        usernamepass = txtBoxUname.text;

                        var home = new queueManager();
                        this.Hide();
                        home.ShowDialog();
                        this.Close();

                    }
                    else
                    {
                        MessageBox.Show("Username or password is incorrect");
                    }

                }
                else
                {
                    MessageBox.Show("User does not exist ");
                }

            }
        }
    }
}
