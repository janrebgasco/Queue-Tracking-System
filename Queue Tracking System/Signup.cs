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
using System.Security.Cryptography;
using System.IO;
using System.Drawing.Imaging;


namespace Queue_Tracking_System
{
    public partial class Signup : Form
    {
        
        //Configure FirebaseConfig
        IFirebaseConfig config = new FirebaseConfig
        {

            AuthSecret = "jAPNREXqaNdnbrnztpmBqn6yGjp1myhFP4kieJj5",
            BasePath = "https://queue-management-system-202109-default-rtdb.firebaseio.com/"

        };
        
        //Firebase Client
        IFirebaseClient client;
        int uID;
        string base64Img;
        public Signup()
        {
            InitializeComponent();
        }

        private void Signup_Load(object sender, EventArgs e)
        {
            try
            {
                client = new FireSharp.FirebaseClient(config);

                if (client != null)
                {


                    this.CenterToScreen();
                    //this.Size = Screen.PrimaryScreen.WorkingArea.Size;
                    //this.WindowState = FormWindowState.Normal;

                }
                FirebaseResponse resP = client.Get("IDs");
                uID = resP.ResultAs<int>();

            }
            catch(Exception ex)
            {
                MessageBox.Show("Connection Failed."+ex.ToString());
            }


            boxPass._TextBox.PasswordChar = '*';
            iPass._TextBox.PasswordChar = '*';

        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            Login form = new Login();
            this.Hide();
            form.ShowDialog();
            this.Close();
        }

        private void btnSignup_Click(object sender, EventArgs e)
        {
            string header = esName.text;
            FirebaseResponse resDuplicate = client.Get("Users" + "/" + header + "/username");
            string unameRes = resDuplicate.ResultAs<string>();


            if (string.IsNullOrEmpty(boxUname.text) || string.IsNullOrEmpty(boxPass.text) || string.IsNullOrEmpty(esName.text))
            {
                //Check all textbox if some are Empty.
                MessageBox.Show("Please Specify All Data Needed.");
            }
            if(iPass.text != boxPass.text)
            {
                MessageBox.Show("Password does not matched.");
            }
            if (unameRes != null)
            {
                MessageBox.Show("User already exist");
            }
            else
            {
                string esID = esName.text;
                string pass = sha256(boxPass.text);
                string user = hashGen(boxUname.text);
                //The Register Class
                var register = new register
                {

                    username = user,
                    password = pass,
                    id = uID.ToString()

                };

                string esname = hashGen(esName.text);
                string counter = hashGen("counter A");

                //TODO: this should be in the counter registration part
                client.Set(header + "/" + counter + "/availableToken", 1001);
                client.Set(header + "/" + counter + "/customerCount", 0);
                client.SetAsync(header + "/" + counter + "/servingToken", 0);
                client.SetAsync(header + "/" + counter + "/totalServedToken", 0);
                client.SetAsync(header + "/" + counter + "/syncQueueList", false);
                client.SetAsync(header + "/" + counter + "/esImg", base64Img);

                FirebaseResponse response = client.Set("Users/" + esID, register);
                //client.Set(username+"/esName", esName.text);
                client.Set("IDs", uID += 1);

                register res = response.ResultAs<register>();
                MessageBox.Show("Account Registered Successfully");


                
                boxUname.text = string.Empty;
                boxPass.text = string.Empty;
                iPass.text = string.Empty;
                esName.text = string.Empty;

            }
        }
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
            //Console.WriteLine(hashCode);

            return hashCode;

        }
        

        private void boxUname_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void boxPass_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnUploadImg_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "Image Files(*.jpeg;*.bmp;*.png;*.jpg)|*.jpeg;*.bmp;*.png;*.jpg";

            if (open.ShowDialog() == DialogResult.OK)
            {

                Image uploadedImg = Image.FromFile(open.FileName);
                EsImg.Image = uploadedImg;
                MemoryStream ms = new MemoryStream();

                uploadedImg.Save(ms, uploadedImg.RawFormat);

                byte[] imageBytes = ms.ToArray();

                // Convert byte[] to Base64 String
                base64Img = Convert.ToBase64String(imageBytes);

                //Console.WriteLine(base64String);

            }
        }

        private void bunifuFlatButton1_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "Image Files(*.jpeg;*.bmp;*.png;*.jpg)|*.jpeg;*.bmp;*.png;*.jpg";

            if (open.ShowDialog() == DialogResult.OK)
            {

                Image uploadedImg = Image.FromFile(open.FileName);
                EsImg.Image = uploadedImg;
                MemoryStream ms = new MemoryStream();

                uploadedImg.Save(ms, uploadedImg.RawFormat);

                byte[] imageBytes = ms.ToArray();

                // Convert byte[] to Base64 String
                base64Img = Convert.ToBase64String(imageBytes);

                //Console.WriteLine(base64String);

            }
        }

        private void bunifuFlatButton2_Click(object sender, EventArgs e)
        {
            string esID = esName.text;
            FirebaseResponse resDuplicate = client.Get("Users" + "/" + esID + "/username");
            string unameRes = resDuplicate.ResultAs<string>();
            String ps = this.iPass.text;

            if (boxUname.text.Length == 0)
            {
                epTxt.SetError(boxUname, "Username is required");
            }
            if (esID.Length == 0)
            {
                epTxt.SetError(esName, "Establishment Name is required");
            }
            if (ps.Length == 0)
            {
                epTxt.SetError(iPass, "Password is required");
            }
            else if (iPass.text != boxPass.text)
            {
                MessageBox.Show("Password does not matched.");
            }
            else if (unameRes != null)
            {
                MessageBox.Show("User already exist");
            }
            else
            {

                Cursor = Cursors.WaitCursor; // change cursor to hourglass type
                string pass = sha256(boxPass.text);
                string user = hashGen(boxUname.text);
                string counter = hashGen("counter A");

                //The Register Class
                var register = new register
                {

                    username = user,
                    password = pass,
                    id = uID.ToString(),
                    rawUname = boxUname.text,
                    counter = counterBox.text,
                    address = " ",
                    contactNum = " ",
                    establishmentID = esID,
                    fullname = fullnameBox.text,
                    email = " ",
                    userImg = base64Img

                };


                var counterReg = new counterReg
                {

                    availableToken = 1001,
                    customerCount = 0,
                    servingToken = 0,
                    totalServedToken = 0,
                    syncQueueList = false,
                    esImg = base64Img

                };

                

                client.Set(esID + "/" + user, counterReg);

                client.Set("Users/" + hashGen(boxUname.text), register);

                client.Set("admin/" + esID +"/employees/"+ boxUname.text, register);

                client.Set("IDs", uID += 1);


                Cursor = Cursors.Arrow; // change cursor to normal type

                MessageBox.Show("Account Registered Successfully");



                boxUname.text = string.Empty;
                boxPass.text = string.Empty;
                iPass.text = string.Empty;
                esName.text = string.Empty;
                counterBox.text = string.Empty;
                fullnameBox.text = string.Empty;
                EsImg.Image = null;

            }
        }

        private void label4_Click(object sender, EventArgs e)
        {
            Login form = new Login();
            this.Hide();
            form.ShowDialog();
            this.Close();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
