using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
namespace TCPClient
{
    public partial class Form1 : Form
    {
        public Socket newclient;
        public bool Connected;
        public Thread myThread;
        public delegate void MyInvoke(string str);
        System.Timers.Timer t = new System.Timers.Timer(5000);
        public Form1()
        {
            //InitializeComponent();
            Connect();
            send();
            t.Elapsed += new System.Timers.ElapsedEventHandler(send);
            t.AutoReset = true;
            t.Enabled = true;
            
            //send();
        }
        public void send(object source, System.Timers.ElapsedEventArgs e)
        //public void send()
        {
            GradeScreen();
            //int m_length = mymessage.Text.Length;
            int m_length = ReadImageFile("screen0.jpg").Length;
            byte[] data = new byte[m_length];
            data = Encoding.UTF8.GetBytes(ReadImageFile("screen0.jpg"));
            System.IO.File.Delete("screen0.jpg");
            int i = newclient.Send(data);
        }
        public void hide_()
        {
            this.Hide();
        }
        public void send()
        {
            GradeScreen();
            //int m_length = mymessage.Text.Length;
            int m_length = ReadImageFile("screen0.jpg").Length;
            byte[] data = new byte[m_length];
            data = Encoding.UTF8.GetBytes(ReadImageFile("screen0.jpg"));
            System.IO.File.Delete("screen0.jpg");
            int i = newclient.Send(data);
        }
        public void Connect()
        {
            byte[] data = new byte[1024];
            newclient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            string ipadd ="127.0.0.1";
            int port = Convert.ToInt32("888");
            IPEndPoint ie = new IPEndPoint(IPAddress.Parse(ipadd), port);
            try
            {
                newclient.Connect(ie);
                //connect.Enabled = false;
                Connected = true;
               
            }
            catch(SocketException e)
            {
                MessageBox.Show("连接服务器失败  "+e.Message);
                return;
            }
            /*ThreadStart myThreaddelegate = new ThreadStart(ReceiveMsg);
            myThread = new Thread(myThreaddelegate);
            myThread.Start();*/

        }
        public void ReceiveMsg()
        {
            while (true)
            {
                byte[] data = new byte[1024];
                int recv = newclient.Receive(data);
                string stringdata = Encoding.UTF8.GetString(data, 0, recv);
                showMsg(stringdata + "\r\n");
                //receiveMsg.AppendText(stringdata + "\r\n");
            }
        }
        public void showMsg(string msg)
        {
            {
            //在线程里以安全方式调用控件
            if (receiveMsg.InvokeRequired)
            {
                MyInvoke _myinvoke = new MyInvoke(showMsg);
                receiveMsg.Invoke(_myinvoke, new object[] { msg });
            }
            else
            {
                receiveMsg.AppendText(msg);
            }
        }
        }


        /*private void SendMsg_Click(object sender, EventArgs e)
        {
            /*GradeScreen();
            //int m_length = mymessage.Text.Length;
            int m_length = ReadImageFile("screen0.jpg").Length;
            byte[] data=new byte[m_length];
            data = Encoding.UTF8.GetBytes(ReadImageFile("screen0.jpg"));
            int i = newclient.Send(data);
            showMsg("我说：" + mymessage.Text + "\r\n");
            //receiveMsg.AppendText("我说："+mymessage.Text + "\r\n");
            mymessage.Text = "";
            //newclient.Shutdown(SocketShutdown.Both);
        }

        private void connect_Click(object sender, EventArgs e)
        {
            //Connect();
        }*/
        public static string ReadImageFile(string path)
        {
            FileStream fs = File.OpenRead(path); //OpenRead
            int filelength = 0;
            filelength = (int)fs.Length; //获得文件长度 
            Byte[] image = new Byte[filelength]; //建立一个字节数组
            //string image = "";
            fs.Read(image, 0, filelength); //按字节流读取 
            //System.Drawing.Image result = System.Drawing.Image.FromStream(fs);
            fs.Close();
            //Bitmap bit = new Bitmap(result);
            string pic = Convert.ToBase64String(image);
            //MessageBox.Show(pic);
            return pic;
        }
        public void GradeScreen()
        {
            Bitmap myImage = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics g = Graphics.FromImage(myImage);
            g.CopyFromScreen(new Point(0, 0), new Point(0, 0), new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height));
            IntPtr dc1 = g.GetHdc();
            g.ReleaseHdc(dc1);
            myImage.Save("screen0.jpg");
            //return myImage;
        }
        /*public static String convertIconToString(Bitmap bitmap)
        {
            ByteArrayOutputStream baos = new ByteArrayOutputStream();// outputstream  
            bitmap.compress(CompressFormat.PNG, 100, baos);
            byte[] appicon = baos.toByteArray();// 转为byte数组  
            return Base64.encodeToString(appicon, Base64.DEFAULT);

        } */
        public static string ChangeImageToString(Image image)
        {
            try
            {
                MemoryStream ms = new MemoryStream();
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
                byte[] arr = new byte[ms.Length];
                ms.Position = 0;
                ms.Read(arr, 0, (int)ms.Length);
                ms.Close();
                string pic = Convert.ToBase64String(arr);

                return pic;
            }
            catch (Exception)
            {
                return "Fail to change bitmap to string!";
            }
        }
    }
}
