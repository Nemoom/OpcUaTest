using Opc.Ua;
using OpcUaHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace OpcUaTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        class point_XY //point类
        {
            public point_XY(double x, short y)
            {
                mOADate = x;
                Value = y;
            }
            public double mOADate { get; set; }
            public short Value { get; set; }
        }
        //class point_XY //point类
        //{
        //    public point_XY(double x, short y)
        //    {
        //        mOADate = x.ToString();
        //        Value = y.ToString();
        //    }
        //    public string mOADate { get; set; }
        //    public string Value { get; set; }
        //}

        List<point_XY> listp = new List<point_XY>();

        OpcUaClient m_OpcUaClient;
        //定义全局变量
        public int currentCount = 0;
        //定义Timer类
        System.Timers.Timer timer;

        public bool b_keepRunning = false;
        //定义委托
        public delegate void SetControlValue(string value);
        private void Form1_Load(object sender, EventArgs e)
        {
            m_OpcUaClient = new OpcUaClient();
            //设置匿名连接
            m_OpcUaClient.UserIdentity = new UserIdentity(new AnonymousIdentityToken());
            //设置用户名连接
            //m_OpcUaClient.UserIdentity = new UserIdentity( "user", "password" );

            //使用证书连接
            //X509Certificate2 certificate = new X509Certificate2("[证书的路径]", "[密钥]", X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);
            //m_OpcUaClient.UserIdentity = new UserIdentity(certificate);

            m_OpcUaClient.ConnectComplete += M_OpcUaClient_ConnectComplete;
            m_OpcUaClient.OpcStatusChange += M_OpcUaClient_OpcStatusChange;

            InitTimer();
        }

        /// <summary>
        /// 初始化Timer控件
        /// </summary>
        private void InitTimer()
        {
            //设置定时间隔(毫秒为单位)            
            timer = new System.Timers.Timer(0.001);
            //设置执行一次（false）还是一直执行(true)
            timer.AutoReset = true;
            //设置是否执行System.Timers.Timer.Elapsed事件
            timer.Enabled = false;
            //绑定Elapsed事件
            timer.Elapsed += new System.Timers.ElapsedEventHandler(TimerUp);
        }

        /// <summary>
        /// Timer类执行定时到点事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerUp(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                currentCount += 1;
                double now_OADate = DateTime.Now.ToOADate();
                short value = m_OpcUaClient.ReadNode<short>("ns=2;s=|var|CPX-CEC-S1-V3.Application.Valve1_FBs.SetValue2");
                //this.Invoke(new SetControlValue(SetTextBoxText), currentCount.ToString()+","+ value.ToString());
                listp.Add(new point_XY(now_OADate, value));
                //InvokeAddXY(chart1, DateTime.Now.ToOADate().ToString(), value.ToString());
                //if (chart1.Series[0].Points.Count>100)
                //{
                //    InvokeDeleteItem(chart1, 0);
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show("执行定时到点事件失败:" + ex.Message);
            }
        }

        private void UpdateData()
        {
            while (b_keepRunning)
            {
                try
                {
                    //double now_OADate = DateTime.Now.ToOADate();
                    short value = m_OpcUaClient.ReadNode<short>("ns=2;s=|var|CPX-CEC-S1-V3.Application.Valve1_FBs.SetValue2");
                    double now_OADate = DateTime.Now.ToOADate();

                    listp.Add(new point_XY(now_OADate, value));
                }
                catch (Exception)
                {
                }
            }
            //UpdateData();
        }

        /// <summary>
        /// 设置文本框的值
        /// </summary>
        /// <param name="strValue"></param>
        private void SetTextBoxText(string strValue)
        {
            //this.label2.Text = this.currentCount.ToString().Trim();
            label2.Text = strValue;
        }

        #region InvokeDeleteItem
        protected delegate void DeleteItemHandler(Chart chartCtrl, int SeriesIndex = 0, string Txt = "");
        void InvokeDeleteItem(Chart chartCtrl, int SeriesIndex = 0, string Txt = "")
        {
            chartCtrl.Invoke((DeleteItemHandler)DeleteItem, chartCtrl, SeriesIndex, Txt);
        }
        void DeleteItem(Chart chartCtrl, int SeriesIndex = 0, string Txt = "")
        {
            if (Txt == "")
            {
                chartCtrl.Series[SeriesIndex].Points.RemoveAt(0);
            }
            else
            {
                for (int i = 0; i < chartCtrl.Series[SeriesIndex].Points.Count; i++)
                {
                    if (chartCtrl.Series[SeriesIndex].Points[i].YValues[0].ToString() == Txt)
                    {
                        chartCtrl.Series[SeriesIndex].Points.RemoveAt(i);
                    }
                }
            }
        }
        #endregion

        #region InvokeAddY
        protected delegate void AddYHandler(Chart chartCtrl, string Y, int SeriesIndex = 0);
        void InvokeAddY(Chart chartCtrl, string Y, int SeriesIndex = 0)
        {
            chartCtrl.Invoke((AddYHandler)AddY, chartCtrl, Y, SeriesIndex);
        }
        void AddY(Chart chartCtrl, string Y, int SeriesIndex = 0)
        {
            chartCtrl.Series[SeriesIndex].Points.AddY(Convert.ToDouble(Y));
        }
        #endregion

        #region InvokeAddXY
        protected delegate void AddXYHandler(Chart chartCtrl, string X, string Y, int SeriesIndex = 0);
        void InvokeAddXY(Chart chartCtrl, string X, string Y, int SeriesIndex = 0)
        {
            chartCtrl.Invoke((AddXYHandler)AddXY, chartCtrl, X, Y, SeriesIndex);
        }
        void AddXY(Chart chartCtrl, string X, string Y, int SeriesIndex = 0)
        {
            chartCtrl.Series[SeriesIndex].Points.AddXY(Convert.ToDouble(X), Convert.ToDouble(Y));
        }
        #endregion

        #region InvokeChangeButtonText
        protected delegate void ChangeButtonTextHandler(Button buttonCtrl, string Txt);
        void InvokeChangeButtonText(Button buttonCtrl, string Txt)
        {
            buttonCtrl.Invoke((ChangeButtonTextHandler)ChangeButtonCtrlText, buttonCtrl, Txt);
        }
        void ChangeButtonCtrlText(Button buttonCtrl, string Txt)
        {
            buttonCtrl.Text = Txt;
        } 
        #endregion

        private void M_OpcUaClient_OpcStatusChange(object sender, OpcUaStatusEventArgs e)
        {
            lbl_ConnectStatus.Text = e.ToString();
            if (m_OpcUaClient.Connected)
            {
                InvokeChangeButtonText(button1, "Disconnect");
                //button1.Text = "Disconnect";
            }
            else
            {
                InvokeChangeButtonText(button1, "Connect");
                //button1.Text = "Connect";
            }
        }

        private void M_OpcUaClient_ConnectComplete(object sender, EventArgs e)
        {
            //lbl_ConnectStatus.Text = "Connected to " + textBox1.Text;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text != "Disconnect")
            {
                // connect to server
                try
                {
                    await m_OpcUaClient.ConnectServer(textBox1.Text);
                    //lbl_ConnectStatus.Text = "Connected to " + textBox1.Text;
                    //button1.Text = "Disconnect";
                    //short value = m_OpcUaClient.ReadNode<short>("ns=2;s=|var|CPX-CEC-S1-V3.Application.Valve1_FBs.SetValue2");
                }
                catch (Exception ex)
                {
                    ClientUtils.HandleException("Connected Failed", ex);
                }
            }
            else
            {
                m_OpcUaClient.Disconnect();
                
            }
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            listp.Clear();
            chart1.Series[0].Points.Clear();
            if (!m_OpcUaClient.Connected)
            {
                button1_Click(sender, e);
            }
            while (!m_OpcUaClient.Connected)
            {
                Thread.Sleep(10);
            }
            b_keepRunning = true;
            ThreadStart start = delegate
            {
                UpdateData();
            };
            Thread tStart = new Thread(start);
            tStart.Priority= ThreadPriority.Highest;
            tStart.IsBackground = true;
            tStart.Start();
            timer1.Enabled = true;
            //start
            ////timer.Enabled = true;
            ////timer.Start();
            //chart1.DataSource = listp;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //stop
            //timer.Stop();

            //chart1.DataSource = listp;
            //chart1.DataBind();

            b_keepRunning = false;
            //int mmm = 0;
            //if (listp.Count - 1000 > 0)
            //{
            //    mmm = listp.Count - 1000;
            //}
            //for (int i = mmm; i < listp.Count; i++)
            //{
            //    chart1.Series[0].Points.AddXY(Convert.ToDouble(listp[i].mOADate), Convert.ToDouble(listp[i].Value));
            //}
        }

        private void chart1_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                HitTestResult hit = chart1.HitTest(e.X, e.Y);
                if (hit.Series != null)
                {
                    var xValue = hit.Series.Points[hit.PointIndex].XValue;
                    var yValue = hit.Series.Points[hit.PointIndex].YValues.First();
                    lbl_Value.ForeColor = Color.Orange;
                    lbl_Value.Text = DateTime.FromOADate(double.Parse(xValue.ToString())).ToString("HH:mm:ss:fff") + "，" + yValue.ToString();
                }
                else
                {
                    //if (chart1.Series[0].Points.Count>0)
                    //{
                    //    var area = chart1.ChartAreas[0];
                    //    double xValue = area.AxisX.PixelPositionToValue(e.X);
                    //    double yValue = area.AxisY.PixelPositionToValue(e.Y);
                    //    lbl_Value.ForeColor = Color.Black;
                    //    lbl_Value.Text = string.Format("{0:F0}{1:F0}", xValue, "，" + yValue);
                    //}                    
                }
            }
            catch (Exception)
            {

            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            List<point_XY> list_show = listp;
            double[] XAxis,YAxis;

            int mmm = 0;
            int length = list_show.Count;
            if (length - 1000 > 0)
            {
                mmm = length - 1000;
                XAxis = new double[1000];
                YAxis = new double[1000];
                for (int i = mmm; i < 1000 + mmm; i++)
                {
                    XAxis[i - mmm] = list_show[i].mOADate;
                    YAxis[i - mmm] = list_show[i].Value;
                }
            }
            else
            {
                XAxis = new double[length];
                YAxis = new double[length];
                for (int i = mmm; i < length; i++)
                {
                    XAxis[i - mmm] = list_show[i].mOADate;
                    YAxis[i - mmm] = list_show[i].Value;
                }
            }
            
            chart1.Series[0].Points.DataBindXY(XAxis, YAxis);
        }
    }
}
