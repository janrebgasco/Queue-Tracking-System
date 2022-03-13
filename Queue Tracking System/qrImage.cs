using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Queue_Tracking_System
{
    public partial class qrImage : Form
    {
        public qrImage()
        {
            InitializeComponent();
        }
        Image img;
        private void Form1_Load(object sender, EventArgs e)
        {
            this.CenterToScreen();
            qrImgBox.Image = queueManager.qrCodeImage;
            img = queueManager.qrCodeImage;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SaveFileDialog sf = new SaveFileDialog();
            sf.Filter = "JPG(*.JPEG)|*.jpg";

            if(sf.ShowDialog() == DialogResult.OK){
                img.Save(sf.FileName);
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
        
    }
}
