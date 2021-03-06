﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

namespace eNet编辑器.AddForm
{
    public partial class panelCheck : Form
    {
        public panelCheck()
        {
            InitializeComponent();
        }

        ClientAsync client;

        private string address;

        public string Address
        {
            get { return address; }
            set { address = value; }
        }

        private string rtAddress;

        public string RtAddress
        {
            get { return rtAddress; }
            set { rtAddress = value; }
        }

        /// <summary>
        /// 是否为感应打开 是则读取ini里面的io
        /// </summary>
        private bool isIO = false;

        public bool IsIO
        {
            get { return isIO; }
            set { isIO = value; }
        }

        public event Action<string> msghandleCallBack;

        //private delegate void MsgHandleCallBack(string msg);
        //private MsgHandleCallBack msghandleCallBack;

        private string ip= "";

        private void panelCheck_Load(object sender, EventArgs e)
        {
            client = new ClientAsync();
            msghandleCallBack += new Action<string>(dealDeletega);
            
            
            if (!string.IsNullOrEmpty(address))
            {
                string[] info = address.Split('.');
                cbDevNum.Text = info[0];
                cbKeyNum.Text = info[1];
            }
            if (IsIO)
            {
                ip = FileMesege.sensorSelectNode.Parent.Text.Split(' ')[0];
                //扫ini添加item信息io
                findIOPanel();
                findIONum();
              
            }
            else
            {
                ip = FileMesege.panelSelectNode.Parent.Text.Split(' ')[0];
                //扫ini添加item信息key
                findKeyPanel();
                findKeyNum();
                //扫描设备在线
                Init();
                //异步连接
                client.ConnectAsync(ip, 6003);
                string hearMsg = "SET;0000000A;{" + ip.Split('.')[3] + ".251.0.1};\r\n";
                client.SendAsync(hearMsg);
            }
            lbip.Text = ip;
            
        }

        /// <summary>
        /// 寻找有key的panel
        /// </summary>
        private void findKeyPanel()
        {
            //devices 里面ini的名字
            string keyVal = "";
            string path = Application.StartupPath + "\\devices\\";
            //从设备加载网关信息
            foreach (DataJson.Device d in FileMesege.DeviceList)
            {
                if (d.ip == ip)
                {
                    //加载设备
                    foreach (DataJson.Module m in d.module)
                    {
                        keyVal = IniConfig.GetValue(path + m.device + ".ini", "input", "key");
                        if (keyVal != "null")
                        {
                            cbDevNum.Items.Add(m.id);

                        }
                    }
                }

            }
        }

        /// <summary>
        /// 寻找有io的panel
        /// </summary>
        private void findIOPanel()
        {
            //devices 里面ini的名字
            string ioVal = "";
            string path = Application.StartupPath + "\\devices\\";
            //从设备加载网关信息
            foreach (DataJson.Device d in FileMesege.DeviceList)
            {
                if (d.ip == ip)
                {
                    //加载设备
                    foreach (DataJson.Module m in d.module)
                    {
                        ioVal = IniConfig.GetValue(path + m.device + ".ini", "input", "io");
                        if (ioVal != "null")
                        {
                            cbDevNum.Items.Add(m.id);

                        }
                    }
                }

            }
        }

        /// <summary>
        /// 异步连接TCP信息回调初始化
        /// </summary>
        private void Init()
        {

            client.Completed += new Action<System.Net.Sockets.TcpClient, ClientAsync.EnSocketAction>((c, enAction) =>
            {
                /*string key = "";

                try
                {
                    if ( c.Client.Connected)
                    {
                        IPEndPoint iep = c.Client.RemoteEndPoint as IPEndPoint;
                        key = string.Format("{0}:{1}", iep.Address.ToString(), iep.Port);
                    }
                }
                catch {}*/

                switch (enAction)
                {
                    case ClientAsync.EnSocketAction.Connect:
                        //MessageBox.Show("已经与" + key + "建立连接");
                        break;
                    case ClientAsync.EnSocketAction.SendMsg:

                        //MessageBox.Show(DateTime.Now + "：向" + key + "发送了一条消息");
                        break;
                    case ClientAsync.EnSocketAction.Close:

                        //MessageBox.Show("服务端连接关闭");
                        break;
                    case ClientAsync.EnSocketAction.Error:

                        MessageBox.Show("连接发生错误,请检查网络连接");
                       
                        break;
                    default:
                        break;
                }
            });
            //信息接收处理
            client.Received += new Action<string, string>((key, msg) =>
            {
                Invoke(msghandleCallBack, msg);
               
            });
            
         

        }

        /// <summary>
        /// 委托处理接收到的msg
        /// </summary>
        /// <param name="msg"></param>
        private void dealDeletega(string msg)
        {
            try
            {
                //获取FB开头的信息
                string[] strArray = msg.Split(new string[] { "FB", "ACK" }, StringSplitOptions.RemoveEmptyEntries);
                //MessageBox.Show(msg);
                Regex reg = new Regex(@"(\d+)\.(\d+)\.(\d+)\.(\d+)");
                for (int i = 0; i < strArray.Length; i++)
                {
                    //数组信息按IP提取 
                    Match match = reg.Match(strArray[i]);
                    string[] strs = strArray[i].Split(';');
                    //行数
                    if (match.Groups[4].Value == "0")
                    {
                        cbDevNum.Text = match.Groups[3].Value.ToString();

                        cbKeyNum.Text = (Convert.ToInt32(strs[1].Substring(0, 2)) * 256 + Convert.ToInt32(strs[1].Substring(4, 2))).ToString();
                    }

                }
            }
            catch
            {
                //报错不处理
                //MessageBox.Show("DgvName处理信息出错869行");
            }
        }

        #region 窗体样色


        #region 窗体样色2
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect, // x-coordinate of upper-left corner
            int nTopRect, // y-coordinate of upper-left corner
            int nRightRect, // x-coordinate of lower-right corner
            int nBottomRect, // y-coordinate of lower-right corner
            int nWidthEllipse, // height of ellipse
            int nHeightEllipse // width of ellipse
         );

        [DllImport("dwmapi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

        [DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        [DllImport("dwmapi.dll")]
        public static extern int DwmIsCompositionEnabled(ref int pfEnabled);

        private bool m_aeroEnabled;                     // variables for box shadow
        private const int CS_DROPSHADOW = 0x00020000;
        private const int WM_NCPAINT = 0x0085;
        private const int WM_ACTIVATEAPP = 0x001C;

        public struct MARGINS                           // struct for box shadow
        {
            public int leftWidth;
            public int rightWidth;
            public int topHeight;
            public int bottomHeight;
        }

        private const int WM_NCHITTEST = 0x84;          // variables for dragging the form
        private const int HTCLIENT = 0x1;
        private const int HTCAPTION = 0x2;

        protected override CreateParams CreateParams
        {
            get
            {
                m_aeroEnabled = CheckAeroEnabled();

                CreateParams cp = base.CreateParams;
                if (!m_aeroEnabled)
                    cp.ClassStyle |= CS_DROPSHADOW;

                return cp;
            }
        }

        private bool CheckAeroEnabled()
        {
            if (Environment.OSVersion.Version.Major >= 6)
            {
                int enabled = 0;
                DwmIsCompositionEnabled(ref enabled);
                return (enabled == 1) ? true : false;
            }
            return false;
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_NCPAINT:                        // box shadow
                    if (m_aeroEnabled)
                    {
                        var v = 2;
                        DwmSetWindowAttribute(this.Handle, 2, ref v, 4);
                        MARGINS margins = new MARGINS()
                        {
                            bottomHeight = 1,
                            leftWidth = 1,
                            rightWidth = 1,
                            topHeight = 1
                        };
                        DwmExtendFrameIntoClientArea(this.Handle, ref margins);

                    }
                    break;
                default:
                    break;
            }
            base.WndProc(ref m);

            if (m.Msg == WM_NCHITTEST && (int)m.Result == HTCLIENT)     // drag the form
                m.Result = (IntPtr)HTCAPTION;

        }
        #endregion
        private void panelCheck_Paint(object sender, PaintEventArgs e)
        {
            Rectangle myRectangle = new Rectangle(0, 0, this.Width, this.Height);
            //ControlPaint.DrawBorder(e.Graphics, myRectangle, Color.Blue, ButtonBorderStyle.Solid);//画个边框 
            ControlPaint.DrawBorder(e.Graphics, myRectangle,
                Color.DarkGray, 1, ButtonBorderStyle.Solid,
                Color.DarkGray, 1, ButtonBorderStyle.Solid,
                Color.DarkGray, 2, ButtonBorderStyle.Solid,
                Color.DarkGray, 2, ButtonBorderStyle.Solid
            );
        }

        private Point mPoint;

        private void plInfoTitle_MouseDown(object sender, MouseEventArgs e)
        {
            mPoint = new Point(e.X, e.Y);
        }

        private void plInfoTitle_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Location = new Point(this.Location.X + e.X - mPoint.X, this.Location.Y + e.Y - mPoint.Y);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.No;
            this.Close();
        }
        #endregion

        private void cbDevNum_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsIO)
            {
                //按照设备号 去搜索ini里面io值
                findIONum();
            }
            else
            {
                //按照设备号 去搜索ini里面key值
                findKeyNum();
            }
           
        }

        /// <summary>
        /// 寻找该设备号的信息按键数
        /// </summary>
        private void findKeyNum()
        {
            //devices 里面ini的名字
            string keyVal = "";
            string path = Application.StartupPath + "\\devices\\";
            //从设备加载网关信息
            foreach (DataJson.Device d in FileMesege.DeviceList)
            {
                if (d.ip == ip)
                {
                    //加载设备
                    foreach (DataJson.Module m in d.module)
                    {
                        if (m.id.ToString() == cbDevNum.Text)
                        {
                            keyVal = IniConfig.GetValue(path + m.device + ".ini", "input", "key");
                            //iniKEY的内容不为字符 null
                            if (keyVal != "null")
                            {
                                //keyVal不为空
                                dealInfoNum(cbKeyNum, keyVal);
                                return;
                            }
                            
                        }
                        
                    }
                }

            }
        }


        /// <summary>
        /// 寻找该设备号的信息按键数
        /// </summary>
        private void findIONum()
        {
            //devices 里面ini的名字
            string ioVal = "";
            string path = Application.StartupPath + "\\devices\\";
            //从设备加载网关信息
            foreach (DataJson.Device d in FileMesege.DeviceList)
            {
                if (d.ip == ip)
                {
                    //加载设备
                    foreach (DataJson.Module m in d.module)
                    {
                        if (m.id.ToString() == cbDevNum.Text)
                        {
                            ioVal = IniConfig.GetValue(path + m.device + ".ini", "input", "io");
                            //iniKEY的内容不为字符 null
                            if (ioVal != "null")
                            {
                                //keyVal不为空
                                dealInfoNum(cbKeyNum, ioVal);
                                return;
                            }

                        }

                    }
                }

            }
        }


        /// <summary>
        /// cb信息内容的判断1-9 或 1,2,3  或数字（链路类型）
        /// </summary>
        /// <param name="cb"></param>
        /// <param name="info"></param>
        private void dealInfoNum(ComboBox cb, string info)
        {
            cb.Items.Clear();
            if (info.Contains("-"))
            {
                string[] infos = info.Split('-');
                int j = Convert.ToInt32(infos[1]);
                if (j > 100)
                {
                    j = 100;
                }
                for (int i = Convert.ToInt32(infos[0]); i <= j; i++)
                {
                    cb.Items.Add(i.ToString());
                }
            }
            else if (info.Contains(","))
            {
                string[] infos = info.Split(',');

                for (int i = 0; i < infos.Length; i++)
                {
                    cb.Items.Add(infos[i]);
                }
            }
            else
            {
                cb.Items.Add(info);
            }

        }

        private void btnDecid_Click(object sender, EventArgs e)
        {
            try
            {
                string newobj = "";
                if (string.IsNullOrEmpty(cbDevNum.Text) || string.IsNullOrEmpty(cbKeyNum.Text))
                {
                    this.DialogResult = DialogResult.No;
                    return;
                }
                //newobj = ToolsUtil.strtohexstr(ip.Split('.')[3]);
                newobj = "FE";
                newobj = newobj + ToolsUtil.strtohexstr(cbDevNum.Text);


                //非设备类
                string tmp = ToolsUtil.strtohexstr(cbKeyNum.Text);
                while (tmp.Length < 4)
                {
                    tmp = tmp.Insert(0, "0");
                }
                newobj = newobj + tmp;
            
                if (newobj.Length == 8)
                {
                    this.rtAddress = newobj;
                    this.DialogResult = DialogResult.OK;
                    return;
                }
                this.DialogResult = DialogResult.No;
            }
            catch
            {
                this.DialogResult = DialogResult.No;
            }
        }


        private void panelCheck_FormClosed(object sender, FormClosedEventArgs e)
        {
            //断开tcp连接
            if (client!= null)
            {
                client.Dispoes();
            }
        }

        



    }
}
