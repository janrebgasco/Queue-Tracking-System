using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Queue_Tracking_System
{
    public partial class Form1 : Form
    {
        System.Timers.Timer t;
        Queue<int> queue = new Queue<int>();
        List<int> RandList = new List<int>();
        List<object> customerList = new List<object>();
        int h, m, s;
        int served = 0, customerCount = 0;
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
        string User, counter;
        public static Bitmap qrCodeImage;
        List<string> timeList = new List<string>();
        List<double> ratingList = new List<double>();


        public Form1()
        {
            InitializeComponent();
            this.TopMost = true;
        }

        private void btnTransfer_Click(object sender, EventArgs e)
        {
            queueManager form = new queueManager();
            this.Hide();
            form.ShowDialog();
            this.Close();
        }

        private void panel5_Paint(object sender, PaintEventArgs e)
        {
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                client = new FireSharp.FirebaseClient(fcon);
            }
            catch
            {
                MessageBox.Show("There was a problem connecting to the database.");
            }

            User = Properties.Settings.Default.esID;
            counter = hashGen(uName);

            intializeQueue();
            client.Set(User + "/" + counter + "/servingToken", queue.Peek());
            FirebaseResponse res = client.Get(@User + "/" + counter + "/totalServedToken");
            served = res.ResultAs<int>();

            panel5.Paint += panel5_Paint;
            //panel10.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, panel1.Width, panel1.Height, 2, 2));


            t = new System.Timers.Timer();
            t.Interval = 1000;
            t.Elapsed += OnTimeEvent;


            //liveCustomerCount();
            liveCCount();
            getServingTimeList();
            getRatingList();



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
                    if (usedTime != null)
                    {
                        ServingTime stList = new ServingTime()
                        {
                            customer = queue.Peek(),
                            time = usedTime
                        };
                        timeList.Add(usedTime);
                        client.PushAsync(User + "/" + counter + "/serving time/", stList);
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

                    client.SetAsync(User + "/" + counter + "/servingToken", queue.Peek());
                    client.SetAsync(User + "/" + counter + "/totalServedToken", served);


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
                    client.Set("admin/" + esID + "/employees/" + uName + "/history/" + date, historyData);

                    client.Set("admin/" + esID + "/employees/" + uName + "/ratings/", averageRatings());

                }

            }
        }
        private int HistoryCCount()
        {
            FirebaseResponse response = client.Get(esID + "/history/" + date + "/customerCount");
            Nullable<int> customerCount = response.ResultAs<int>();
            if (customerCount != null)
            {
                return (int)customerCount;
            }
            else
            {
                return 0;
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            client.Set(@User + "/" + counter + "/queueList" + "/" + queue.Peek() + "/call", true);
            MessageBox.Show("Customer number " + queue.Peek() + " has been called");
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            t.Start();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            t.Stop();
        }
        private String hashGen(string strInput)
        {
            string hashCode = String.Format("{0:X}", strInput.GetHashCode());
            //Console.WriteLine(hashCode);

            return hashCode;

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
            else
            {
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
                if (data != null)
                {
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
                if (s == 60)
                {
                    s = 0;
                    m += 1;
                }
                if (m == 60)
                {
                    m = 0;
                    h += 1;
                }
                if (m == 5)
                {
                    txtTime.ForeColor = System.Drawing.ColorTranslator.FromHtml("#ff7003");
                }
                if (m == 10)
                {
                    txtTime.ForeColor = System.Drawing.ColorTranslator.FromHtml("#ff0000");
                }
                txtTime.Text = string.Format("{0}:{1}:{2}", h.ToString().PadLeft(2, '0'), m.ToString().PadLeft(2, '0'), s.ToString().PadLeft(2, '0'));
                usedTime = string.Format("{0}:{1}:{2}", h.ToString().PadLeft(2, '0'), m.ToString().PadLeft(2, '0'), s.ToString().PadLeft(2, '0'));

            }));

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
                    Console.WriteLine(liveAvailableToken.ToString() + "/" + RandList[customerCount + 1].ToString() + "/" + ccountHasChanged);

                    ccountHasChanged = false;
                    if (liveAvailableToken == RandList[customerCount + 1] && ccountHasChanged == false)//available token has been used
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
        private List<int> newList()
        {
            List<int> list = new List<int>();
            for (int i = 1001; i <= 2000; i++)
            {
                list.Add(i);
            }
            return list;
        }
    }
}
