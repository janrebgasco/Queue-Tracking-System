using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using FireSharp.Config;
using FireSharp.Response;
using FireSharp.Interfaces;
using QRCoder;
using Newtonsoft.Json;
using System.Collections;
using System.Threading;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;
using System.IO;

namespace Queue_Tracking_System
{
    public partial class queueManager : Form
    {
        Queue<int> queue = new Queue<int>();
        List<int> RandList = new List<int>();
        List<object> customerList = new List<object>();
        System.Timers.Timer t;
        int h, m, s;
        int served = 0,customerCount = 0;
        bool customerSync = false, ccountHasChanged = false;
        String esID = Properties.Settings.Default.esID;
        String date = DateTime.Now.ToString("yyyy-M-d");
        String day = System.DateTime.Now.DayOfWeek.ToString();



        IFirebaseClient client;
        IFirebaseConfig fcon = new FirebaseConfig() 
        {
            AuthSecret = "jAPNREXqaNdnbrnztpmBqn6yGjp1myhFP4kieJj5",
            BasePath = "https://queue-management-system-202109-default-rtdb.firebaseio.com/"
        };
        String usedTime;
        int liveAvailableToken = 0, liveServingToken = 0;
        string uName = Properties.Settings.Default.userName;
        string User,counter;
        public static Bitmap qrCodeImage;
        List<string> timeList = new List<string>();
        List<double> ratingList = new List<double>();
        public List<Person> People { get; set; }

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]

        private static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect,
            int nTopRect,
            int nRightRect,
            int nBottomRect,
            int nWidthEllipse,
            int nHeightEllipse
        );

        public queueManager()
        {
            InitializeComponent();
            
            
        }

        private List<Person> GetPeople()
        {
            var list = new List<Person>();

            try
            {
                FirebaseResponse response = client.Get(@User + "/" + counter + "/queueList");
                Dictionary<string, queueList> data = JsonConvert.DeserializeObject<Dictionary<string, queueList>>(response.Body.ToString());
                if (data != null) {
                    foreach (var item in data)
                    {
                        list.Add(new Person()
                        {
                            Customer_Number = item.Value.customerNumber,
                            Status = item.Value.status
                        });
                    }
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return list;
        }
        
        private static Random rng = new Random();
        private List<int> randomList() {//randomizing a list
            List<int> list = new List<int>();
            for (int i = 1001; i <= 2000; i++)
            {
                list.Add(i);
            }
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                int value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            return list;
        }
        private List<int> newList()
        {
            List<int> list = new List<int>();
            for (int i = 1001; i <= 2000; i++)
            {
                list.Add(i);
            }
            return list;
        }
       
        private void panel5_Paint(object sender, PaintEventArgs e)//making the panel5 border thick
        {
            if (panel5.BorderStyle == BorderStyle.FixedSingle)
            {
                int thickness = 3;//it's up to you
                int halfThickness = thickness / 2;
                using (Pen p = new Pen(Color.Gray, thickness))
                {
                    e.Graphics.DrawRectangle(p, new Rectangle(halfThickness,
                                                              halfThickness,
                                                              panel5.ClientSize.Width - thickness,
                                                              panel5.ClientSize.Height - thickness));
                }
            }
        }
       
        private void tbnNext_Click(object sender, EventArgs e)
        {
            getServingTimeList();
            getRatingList();
            if (liveServingToken == liveAvailableToken)
            {
                MessageBox.Show("No customer is on queue.");
            }
            else
            {
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to proceed?", "Message Box", MessageBoxButtons.YesNo);
            
            if (dialogResult == DialogResult.Yes)
            {
                if(usedTime != null){
                    ServingTime stList = new ServingTime()
                    {
                        customer = queue.Peek(),
                        time = usedTime
                    };
                    timeList.Add(usedTime);
                    client.PushAsync(User+"/"+counter+"/serving time/", stList);
                    timeAverage();
                    //reset the time
                    h = 0;
                    m = 0;
                    s = 0;
                    txtTime.ForeColor = System.Drawing.ColorTranslator.FromHtml("#000000");
                    txtTime.Text = string.Format("{0}:{1}:{2}", h.ToString().PadLeft(2, '0'), m.ToString().PadLeft(2, '0'), s.ToString().PadLeft(2, '0'));
                    usedTime = null;
                    t.Stop();

                }
                
                int removedNum = queue.Dequeue();
                queue.Enqueue(removedNum);
                txtTokenNum.Text = queue.Peek().ToString();

                served += 1;
                txtServed.Text = served.ToString();

                client.SetAsync(User+"/"+counter+"/servingToken", queue.Peek());
                client.SetAsync(User+"/"+counter+"/totalServedToken", served);


                var historyData = new historyData
                {

                    servingTime = averageServingTime(),
                    customerCount = HistoryCCount() + 1,
                    ratings = averageRatings(),
                    day = day,
                    date = date

                };
                client.Delete(esID +"/"+ counter + "/queueList/" + removedNum);
                client.Set(esID + "/" + counter + "/syncQueueList", true);


                client.Set(esID + "/history/" + date, historyData);
                client.Set("admin/" + esID +"/employees/" + uName + "/history/" + date, historyData);

                client.Set("admin/" + esID + "/employees/" + uName + "/ratings/", averageRatings());

                }

            }
            
            
        }
        private int HistoryCCount() {
            FirebaseResponse response = client.Get(esID + "/history/"+ date +"/customerCount");
            Nullable<int> customerCount = response.ResultAs<int>();
            if (customerCount != null)
            {
                return (int)customerCount;
            }
            else {
                return 0;
            }
        }
        private void btnStart_Click(object sender, EventArgs e)
        {
            t.Start();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            t.Stop();
        }
        private void queueManager_Load(object sender, EventArgs e)
        {
            try
            {
                client = new FireSharp.FirebaseClient(fcon);
            }
            catch
            {
                MessageBox.Show("There was a problem connecting to the database.");
            }

            username.Text = uName;
            User = Properties.Settings.Default.esID;
            counter = hashGen(uName); 

            intializeQueue();
            client.Set(User + "/" + counter + "/servingToken", queue.Peek());
            FirebaseResponse res = client.Get(@User + "/" + counter + "/totalServedToken");
            served = res.ResultAs<int>();
            txtServed.Text = served.ToString();
            
            panel5.Paint += panel5_Paint;
            //panel10.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, panel1.Width, panel1.Height, 2, 2));
            

            t = new System.Timers.Timer();
            t.Interval = 1000;
            t.Elapsed += OnTimeEvent;


            //liveCustomerCount();
            liveCCount();
            getServingTimeList();
            getRatingList();

            People = GetPeople();
            

            testDGV();
            liveQueueListBool();
            updateDataGridView();
            displayEsImg();
            
        }

        private void displayEsImg()
        {
            try
            {
                FirebaseResponse syncRes = client.Get(@User + "/" + counter + "/esImg");

                string base64Img = syncRes.ResultAs<string>();

                byte[] bytes = Convert.FromBase64String(base64Img);
                Image image;
                using (MemoryStream m = new MemoryStream(bytes))
                {
                    image = Image.FromStream(m);
                }
                System.Drawing.Drawing2D.GraphicsPath gp = new System.Drawing.Drawing2D.GraphicsPath();
                gp.AddEllipse(0, 0, esImg.Width - 3, esImg.Height - 3);
                Region rg = new Region(gp);
                esImg.Region = rg;

                esImg.Image = image;
                
            }
            catch 
            { 
            
            }
        }

        private void setupPicBox()
        {
            pictureBox3.SizeMode = PictureBoxSizeMode.StretchImage;
        }

        async void liveQueueListBool()
        {
            while (true)
            {

                try
                {
                    FirebaseResponse syncRes = await client.GetAsync(@User + "/" + counter + "/syncQueueList");

                    customerSync = syncRes.ResultAs<bool>();
                    
                    if (customerSync) {
                        People = GetPeople();
                        var people = this.People;

                        customerGridList.DataSource = people;

                        customerGridList.Update();
                        customerGridList.Refresh();

                        await client.SetAsync(@User + "/" + counter + "/syncQueueList", false);
                    }
                    Console.WriteLine(customerSync);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }

        private void testDGV()
        {
             var people = this.People;

             customerGridList.DataSource = people;

        }
        private void syncCustomer()
        {
            while (true)
            {
                customerGridList.Update();
                customerGridList.Refresh();
                var people = this.People;

                customerGridList.DataSource = people;

            }

        }

        private void updateDataGridView()
        {

            customerGridList.RowTemplate.Height = 50;
            customerGridList.Columns[0].DefaultCellStyle.ForeColor = Color.Black;
            customerGridList.Columns[1].DefaultCellStyle.ForeColor = Color.Black;
            customerGridList.Columns[0].Name = "Customer Number";
            customerGridList.Columns[1].Name = "Status";

            DataGridViewButtonColumn btnCol = new DataGridViewButtonColumn();
            btnCol.HeaderText = "Action";
            btnCol.Name = "btnAct";
            btnCol.Text = "CALL";
            btnCol.UseColumnTextForButtonValue = true;
            btnCol.FlatStyle = FlatStyle.Flat;
            btnCol.DefaultCellStyle.BackColor = Color.Green;
            customerGridList.Columns.Add(btnCol);

        }
        async void liveCustomerCount() {
            while (true)
            {
                
                try 
                {
                    FirebaseResponse aToken = await client.GetAsync(@User + "/" + counter + "/availableToken");
                    FirebaseResponse sToken = await client.GetAsync(@User + "/" + counter + "/servingToken");
                    FirebaseResponse cCount = await client.GetAsync(@User + "/" + counter + "/customerCount");

                    customerCount = cCount.ResultAs<int>();
                    liveAvailableToken = aToken.ResultAs<int>();
                    liveServingToken = sToken.ResultAs<int>();
                    Console.WriteLine(customerSync);

                    if (liveAvailableToken == 0)//available token has been used
                    {
                        client.Set(User + "/" + counter + "/customerCount", customerCount += 1);
                        client.Set(User + "/" + counter + "/availableToken", RandList[customerCount]);
                    }
                    if (liveAvailableToken == 1)//counter unavailable
                    {
                        client.Set(User + "/" + counter + "/availableToken", RandList[customerCount]);
                    }
                    
                }
                catch(Exception e)
                {
                    Console.WriteLine("availableToken is unreachable");
                    Console.WriteLine(e.ToString());
                }
                
            }
        }
        async void liveCCount()
        {
            while (true)
            {

                try
                {
                    FirebaseResponse aToken = await client.GetAsync(@User + "/" + counter + "/availableToken");
                    FirebaseResponse cCount = await client.GetAsync(@User + "/" + counter + "/customerCount");
                    FirebaseResponse sToken = await client.GetAsync(@User + "/" + counter + "/servingToken");

                    liveServingToken = sToken.ResultAs<int>();
                    customerCount = cCount.ResultAs<int>();
                    liveAvailableToken = aToken.ResultAs<int>();
                    Console.WriteLine(liveAvailableToken.ToString()+"/"+ RandList[customerCount + 1].ToString() + "/" + ccountHasChanged);

                    ccountHasChanged = false;
                    if (liveAvailableToken == RandList[customerCount+1] && ccountHasChanged == false)//available token has been used
                    {
                        client.Set(User + "/" + counter + "/customerCount", customerCount += 1);
                        ccountHasChanged = true;
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine("availableToken is unreachable");
                    Console.WriteLine(e.ToString());
                }

            }
        }
        private static async Task<int> getCustomerCount(IFirebaseClient client)//unused
        {
            FirebaseResponse response = await client.GetAsync("establishment/counter A/customerCount");
            int customerCount = response.ResultAs<int>(); //The response will contain the data being retreived
            EventStreamResponse res = await client.OnAsync("establishment/counter A/customerCount");
            MessageBox.Show(customerCount.ToString());
            return customerCount;
        }

        private void intializeQueue()
        {
            
            //RandList = randomList();//orig list
            RandList = newList();

            FirebaseResponse res = client.Get(User + "/" + counter + "/totalServedToken");
            Nullable<int> servingToken = res.ResultAs<int>();
            
            if (servingToken != null)
            {
                int st = res.ResultAs<int>();
                for (int i = st; i <= 999; i++)
                {
                    queue.Enqueue(RandList[i]);
                    //Console.WriteLine(RandList[i]);
                }

                txtTokenNum.Text = queue.Peek().ToString();
            }
            else 
            {
                for (int i = 0; i <= 999; i++)
                {
                    queue.Enqueue(RandList[i]);
                    //Console.WriteLine(RandList[i]);
                }

                txtTokenNum.Text = queue.Peek().ToString();
            }
            
            

        }
        private void OnTimeEvent(object sender, System.Timers.ElapsedEventArgs e)
        {
            s += 1;
            Invoke(new Action(() =>
            {
                if (s == 60){
                    s = 0;
                    m += 1;
                }
                if (m == 60){
                    m = 0;
                    h += 1;
                }
                if (m == 5) {
                    txtTime.ForeColor = System.Drawing.ColorTranslator.FromHtml("#ff7003");
                }
                if (m == 10) {
                    txtTime.ForeColor = System.Drawing.ColorTranslator.FromHtml("#ff0000");
                }
                txtTime.Text = string.Format("{0}:{1}:{2}",h.ToString().PadLeft(2,'0'),m.ToString().PadLeft(2,'0'),s.ToString().PadLeft(2,'0'));
                usedTime = string.Format("{0}:{1}:{2}",h.ToString().PadLeft(2,'0'),m.ToString().PadLeft(2,'0'),s.ToString().PadLeft(2,'0'));

            }));

        }
        private String hashGen(string strInput)
        {
            string hashCode = String.Format("{0:X}", strInput.GetHashCode());
            //Console.WriteLine(hashCode);
            
            return hashCode;

        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            Login login = new Login();
            this.Hide();
            login.ShowDialog();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string qrVal = User + "-" + counter;
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrVal, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            qrCodeImage = qrCode.GetGraphic(20);
            qrImage qrForm = new qrImage();
            qrForm.Show();
        }
        private void timeAverage() 
        {
            TimeSpan averageTime = new TimeSpan(Convert.ToInt64(timeList.Average(x => TimeSpan.Parse(x).Ticks)));
            string avTime = averageTime.ToString("hh':'mm':'s");

            client.Set(@User + "/" + counter + "/averageServingTime", avTime);
            Console.WriteLine(string.Join(",", timeList));
        }
        private string averageServingTime()
        {
            if (timeList.Any())
            {
                TimeSpan averageTime = new TimeSpan(Convert.ToInt64(timeList.Average(x => TimeSpan.Parse(x).Ticks)));
                string avTime = averageTime.ToString("hh':'mm':'s");
                return avTime;
            }
            else 
            {
                return "00:00:00";
            }
        }
        private double averageRatings()
        {
            if (ratingList.Any())
            {
                double average = ratingList.Average(); 

                return average;
            }
            else {
                return 0;
            }
        }
        private void getServingTimeList() 
        {
            try
            {
                timeList = new List<string>();
                FirebaseResponse response = client.Get(@User + "/" + counter + "/serving time");
                Dictionary<string, ServingTime> data = JsonConvert.DeserializeObject<Dictionary<string, ServingTime>>(response.Body.ToString());
                if (data != null) {
                    foreach (var item in data)
                    {
                        timeList.Add(item.Value.time);

                    }
                }
                
                //Console.WriteLine(string.Join("\t", timeList));
            }
            catch 
            {
                Console.WriteLine("Error getting time List, possibly empty");
            }
            
        }
        private void getRatingList()
        {
            try
            {
                ratingList = new List<double>();
                FirebaseResponse response = client.Get(@User + "/" + counter + "/rating");
                Dictionary<string, Ratings> data = JsonConvert.DeserializeObject<Dictionary<string, Ratings>>(response.Body.ToString());
                if (data != null)
                {
                    foreach (var item in data)
                    {
                        ratingList.Add(item.Value.rating);

                    }
                }

                //Console.WriteLine(string.Join("\t", timeList));
            }
            catch
            {
                Console.WriteLine("Error getting time List, possibly empty");
            }

        }

        private void customerGridList_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 2)
            {
                var index = Convert.ToInt32(customerGridList.CurrentRow.Cells[0].Value);

                DialogResult dialogResult = MessageBox.Show("Are you sure you want to call "+ index.ToString()+"?", "Confirm", MessageBoxButtons.YesNo);

                if (dialogResult == DialogResult.Yes)
                {
                    client.Set(@User + "/" + counter + "/queueList" + "/" + index.ToString() + "/call", true);
                    MessageBox.Show("Customer number " + index.ToString() + " has been called");
                }

             }
            if (e.ColumnIndex == 0)
            {
                var index = Convert.ToInt32(customerGridList.CurrentRow.Cells[1].Value);

                DialogResult dialogResult = MessageBox.Show("Are you sure you want to call " + index.ToString() + "?", "Confirm", MessageBoxButtons.YesNo);

                if (dialogResult == DialogResult.Yes)
                {
                    client.Set(@User + "/" + counter + "/queueList" + "/" + index.ToString() + "/call", true);
                    MessageBox.Show("Customer number " + index.ToString() + " has been called");
                }

            }
        }

        private void pictureBox5_Click_1(object sender, EventArgs e)
        {

            DialogResult dialogResult = MessageBox.Show("Are you sure you want to log out?", "Confirm", MessageBoxButtons.YesNo);

            if (dialogResult == DialogResult.Yes)
            {
                Properties.Settings.Default.userName = null;
                Login form = new Login();
                this.Hide();
                form.ShowDialog();
                this.Close();
            }
        }

        private void username_Click(object sender, EventArgs e)
        {

        }

        private void logOutBtn_Click(object sender, EventArgs e)
        {
            
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to log out?", "Confirm", MessageBoxButtons.YesNo);

            if (dialogResult == DialogResult.Yes)
            {
                Properties.Settings.Default.userName = null;
                Login form = new Login();
                this.Hide();
                form.ShowDialog();
                this.Close();
            }

    }

        private void btnSync_Click(object sender, EventArgs e)
        {
            syncCustomer();
        }

        private void btnCall_Click(object sender, EventArgs e)
        {
            client.Set(@User + "/" + counter + "/queueList" + "/" + queue.Peek() + "/call", true);
            MessageBox.Show("Customer number " + queue.Peek() + " has been called");
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            
        }

        private void btnTransfer_Click(object sender, EventArgs e)
        {
            Form1 form = new Form1();
            this.Hide();
            form.ShowDialog();
            this.Close();
        }
    }

}
