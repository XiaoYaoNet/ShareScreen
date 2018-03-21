using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.IO;

namespace TCPServer
{
    public class StateObject
    {
        // Client socket.     
        public Socket workSocket = null;
        // Size of receive buffer.     
        public const int BufferSize = 8193;
        // Receive buffer.     
        public byte[] buffer = new byte[BufferSize];
        // Received data string.     
        public StringBuilder sb = new StringBuilder();
    }

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            
        }
        public bool btnstatu = true;  //开始停止服务按钮状态
        public Thread myThread;       //声明一个线程实例
        public Socket newsock;        //声明一个Socket实例
        public Socket server1;
        public Socket Client;
        public delegate void MyInvoke(string str);
        public IPEndPoint localEP;    
        public int localPort;
        public EndPoint remote;
        public Hashtable _sessionTable;
        private static ManualResetEvent receiveDone = new ManualResetEvent(false);
        private static String response = String.Empty;
        
        public bool m_Listening;
        //用来设置服务端监听的端口号
        public int setPort            
        {
            get { return localPort; }
            set { localPort = value; }
        }
        
        //用来往richtextbox框中显示消息
        public void showClientMsg(string msg)
        {
            //在线程里以安全方式调用控件
            if (showinfo.InvokeRequired)
            {
                MyInvoke _myinvoke = new MyInvoke(showClientMsg);
                showinfo.Invoke(_myinvoke, new object[] { msg });
            }
            else
            {
                showinfo.AppendText(msg);
            }
        }
        public void userListOperate(string msg)
        {
            //在线程里以安全方式调用控件
            if (userList.InvokeRequired)
            {
                MyInvoke _myinvoke = new MyInvoke(userListOperate);
                userList.Invoke(_myinvoke, new object[] { msg });
            }
            else
            {
                userList.Items.Add(msg);
            }
        }
        public void userListOperateR(string msg)
        {
            //在线程里以安全方式调用控件
            if (userList.InvokeRequired)
            {
                MyInvoke _myinvoke = new MyInvoke(userListOperateR);
                userList.Invoke(_myinvoke, new object[] { msg });
            }
            else
            {
                userList.Items.Remove(msg);
            }
        }
        //监听函数
        public void Listen()
        {   //设置端口
            setPort=int.Parse(serverport.Text.Trim());
            //初始化SOCKET实例
            newsock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //允许SOCKET被绑定在已使用的地址上。
            newsock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            //初始化终结点实例
            localEP=new IPEndPoint(IPAddress.Any,setPort);
            try
            {
                _sessionTable = new Hashtable(53);
                //绑定
                newsock.Bind(localEP);
                //监听
                newsock.Listen(10);
               //开始接受连接，异步。
                newsock.BeginAccept(new AsyncCallback(OnConnectRequest), newsock);
             }
            catch (Exception ex)
            {
                showClientMsg(ex.Message);
            }

        }
        //当有客户端连接时的处理
        public void OnConnectRequest(IAsyncResult ar)
        {
           //初始化一个SOCKET，用于其它客户端的连接
            server1 = (Socket)ar.AsyncState;
            Client = server1.EndAccept(ar);
            //将要发送给连接上来的客户端的提示字符串
            DateTimeOffset now = DateTimeOffset.Now;
            string strDateLine = "欢迎登录到服务器";
            Byte[] byteDateLine = System.Text.Encoding.UTF8.GetBytes(strDateLine);
            //将提示信息发送给客户端,并在服务端显示连接信息。
            remote = Client.RemoteEndPoint;
            showClientMsg(Client.RemoteEndPoint.ToString() + "连接成功。" + now.ToString("G")+"\r\n");
            Client.Send(byteDateLine, byteDateLine.Length, 0);
            userListOperate(Client.RemoteEndPoint.ToString());
            //把连接成功的客户端的SOCKET实例放入哈希表
            _sessionTable.Add(Client.RemoteEndPoint, null);
            
            //等待新的客户端连接
            server1.BeginAccept(new AsyncCallback(OnConnectRequest), server1);
            Receive(Client);
            //receiveDone.WaitOne();
            /*while (true)
            {
                int recv = Client.Receive(byteDateLine);
                string stringdata = Encoding.UTF8.GetString(byteDateLine, 0, recv);
                string ip = Client.RemoteEndPoint.ToString();
                //获取客户端的IP和端口
                
                if (stringdata == "STOP")
                {
                    //当客户端终止连接时
                    showClientMsg(ip+"   "+now.ToString("G")+"  "+"已从服务器断开"+"\r\n");
                    _sessionTable.Remove(Client.RemoteEndPoint);
                    break; 
                }
                //显示客户端发送过来的信息
                showClientMsg(ip + "  " + now.ToString("G") + "   " + stringdata + "\r\n");
                //ChangeStringToImage(stringdata).Save("screen0.jpg");
                //MessageBox.Show(stringdata);
            }
            //byte[] buffer = new byte[4];
            //Client.BeginReceive(buffer, 0, buffer.Length, 0, new AsyncCallback(ReceiveCallback), Client);*/
                       
        }
       /* private static void ReceiveCallback(IAsyncResult ar)
        {
            Socket client = ar.AsyncWaitHandle.;
        }*/
        //以下实现发送广播消息
        public void SendBroadMsg()
        {
            string strDataLine = sendmsg.Text;
            Byte[] sendData = Encoding.UTF8.GetBytes(strDataLine);
            /*
            IDictionaryEnumerator myEnumerator = _sessionTable.GetEnumerator();
            while (myEnumerator.MoveNext())
            {
                EndPoint tempend = (EndPoint)_sessionTable.Values;
                Client.SendTo(sendData, tempend);
            }
             * */
            foreach (DictionaryEntry de in _sessionTable)
            {
                EndPoint temp = (EndPoint)de.Key;
                
                Client.SendTo(sendData, temp);
            }
            sendmsg.Text = "";


        }
      //开始停止服务按钮
        private void startService_Click(object sender, EventArgs e)
        {
            //新建一个委托线程
            ThreadStart myThreadDelegate = new ThreadStart(Listen);
            //实例化新线程
            myThread = new Thread(myThreadDelegate);
             
            if (btnstatu)
            {
               
                myThread.Start();
                statuBar.Text = "服务已启动，等待客户端连接";
                btnstatu = false;
                startService.Text = "停止服务";
            }
            else
            {
                //停止服务（绑定的套接字没有关闭,因此客户端还是可以连接上来）
                myThread.Interrupt();
                myThread.Abort();
                
                //showClientMsg("服务器已停止服务"+"\r\n");
                btnstatu = true;
                startService.Text = "开始服务";
                statuBar.Text = "服务已停止";
                
            }
             
        }
        //窗口关闭时中止线程。
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (myThread != null)
            {
                myThread.Abort();
            }
        }

        private void send_Click(object sender, EventArgs e)
        {
            SendBroadMsg();
        }

        public static Image ChangeStringToImage(string pic)
        {
            try
            {
                byte[] imageBytes = Convert.FromBase64String(pic);
                //读入MemoryStream对象
                MemoryStream memoryStream = new MemoryStream(imageBytes, 0, imageBytes.Length);
                memoryStream.Write(imageBytes, 0, imageBytes.Length);
                //转成图片
                Image image = Image.FromStream(memoryStream);

                return image;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                Image image = null;
                return image;
            }
        }

        private static void Receive(Socket client)
        {
            try
            {
                // Create the state object.     
                StateObject state = new StateObject();
                state.workSocket = client;
                // Begin receiving the data from the remote device.     
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize-1, 0, new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        private static void ReceiveCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            try
            {
                // Retrieve the state object and the client socket     
                // from the asynchronous state object.     
                Socket client = state.workSocket;
                // Read data from the remote device.     
                int bytesRead = client.EndReceive(ar);
                //MessageBox.Show(bytesRead +" "+ state.buffer.Length);
                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.     

                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                    // Get the rest of the data.     
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize-1, 0, new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    // All the data has arrived; put it in response.     
                    if (state.sb.Length > 1)
                    {
                        response = state.sb.ToString();
                        //ChangeStringToImage(response).Save("screen1.jpg");
                        //MessageBox.Show(state.sb.ToString());
                        
                    }
                    receiveDone.Set();
                    // Signal that all bytes have been received.     
                }
                //MessageBox.Show(state.sb.ToString());
                if (bytesRead < 8192)
                {
                    System.IO.File.Delete("screen2.jpg");
                    ChangeStringToImage(state.sb.ToString()).Save(@"screen2.jpg");
                    state.sb=new StringBuilder();
                    //receiveDone.Set();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            //MessageBox.Show(state.sb.ToString());
        }
    }


}
