﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Net;
using System.Text.RegularExpressions;

namespace eNet编辑器.Tools
{
    public partial class masterIPchange : Form
    {
        public masterIPchange()
        {
            InitializeComponent();
        }


        //UDP客户端
        UdpSocket udp;

        //本地IP
        string Localip = "";
        //是否为刷新数据
        bool isInfoFind = false;

        public event Action<string> AppTxtShow;
        private event Action<string> udpreceviceDelegate;

        private void masterIPchange_Load(object sender, EventArgs e)
        {
            udpreceviceDelegate += new Action<string>(udpReceviceDelegateMsg);
            findOnlineGW(false);
        }


        #region UDP获取所有在线网关IP  UNP6002端口
        /// <summary>
        /// 寻找加载在线的网关
        /// </summary>
        private void findOnlineGW(bool flag)
        {
            isInfoFind = flag;
            try
            {

                //寻找加载在线的网关
                udp.udpClose();
            }
            catch
            {
            }
            udpIni();
            
            //获取本地IP
            Localip = ToolsUtil.GetLocalIP();
            lbLocalIP.Text = Localip;
            //udp 绑定
            udp.udpBing(Localip, ToolsUtil.GetFreePort().ToString());
            //绑定成功
            if (udp.isbing)
            {
                if (isInfoFind)
                {
                    if (!string.IsNullOrEmpty(cbOnlineIP.Text))
                    {
                        udp.udpSend("255.255.255.255", "6002", "getinf " + cbOnlineIP.Text);

                    }
                }
                else
                {
                    udp.udpSend("255.255.255.255", "6002", "search all");
                    udp.udpSend("255.255.255.255", "6002", "Search all");
                }

            }
        }

        /// <summary>
        /// udp 事件初始化
        /// </summary>
        private void udpIni()
        {
            //初始化UDP
            udp = new UdpSocket();
            udp.Received += new Action<string, string>((IP, msg) =>
            {
                try
                {
                    if (!String.IsNullOrWhiteSpace(msg))
                    {
                        //跨线程调用
                        this.Invoke(udpreceviceDelegate, msg);
                    }

                }
                catch
                {
                    //报错不处理
                }
            });
        }


        /// <summary>
        /// 网络信息 处理函数
        /// </summary>
        /// <param name="msg"></param>
        private void udpReceviceDelegateMsg(string msg)
        {

            try
            {
                if (msg.Contains("success"))
                {

                    //MessageBox.Show("数据更新完成");
                }
                if (msg.Contains("devIP"))
                {
                    if (isInfoFind)
                    {
                        //把信息添加到各个框里面
                        //网关加载到cb里面
                        string[] devInfos = msg.Split(' ');
                        for (int i = 0; i < devInfos.Length; i++)
                        {
                            string[] tmpInfo = devInfos[i].Split('=');
                            if (tmpInfo[0] == "devIP")
                            {
                                txtDevIP.Text = tmpInfo[1];
                            }
                            else if (tmpInfo[0] == "devMask")
                            {
                                txtDevMask.Text = tmpInfo[1];
                            }
                            else if (tmpInfo[0] == "devDNS")
                            {
                                txtDevDNS.Text = tmpInfo[1];
                            }
                            else if (tmpInfo[0] == "devMac")
                            {
                                txtDevMAC.Text = tmpInfo[1];
                            }

                        }
                    }
                    else
                    {
                        //网关加载到cb里面
                        string[] devInfos = msg.Split(' ');
                        //devIP = 0.0.0.0
                        string[] devIP = devInfos[0].Split('=');
                        bool isExeit = false;
                        for (int i = 0; i < cbOnlineIP.Items.Count; i++)
                        {
                            if (cbOnlineIP.Items[i].ToString() == devIP[1])
                            {
                                //确定item里面没有 该ip项就添加
                                isExeit = true;
                            }
                        }
                        if (!isExeit)
                        {
                            cbOnlineIP.Items.Add(devIP[1]);

                        }
                    }



                }

            }
            catch { }


        }
        #endregion



        #region 获取信息按钮
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            clearFormInfo();
            findOnlineGW(true);
        }

        /// <summary>
        /// 清除屏幕所有信息
        /// </summary>
        private void clearFormInfo()
        {
            txtDevIP.Text = "";
            txtDevDNS.Text = "";
            txtDevMask.Text = "";
            txtDevMAC.Text = "";
        }
        #endregion



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

        private void masterIPchange_Paint(object sender, PaintEventArgs e)
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
            //timer1.Stop();
            this.Close();
        }
        #endregion

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            //先设置mac 设置IP 都会引发重启 需要稍后 
            setIP();
        }

        private void setIP()
        {
            if (string.IsNullOrEmpty(cbOnlineIP.Text))
            {
                return;
            }
            try
            {
                //寻找加载在线的网关
                udp.udpClose();
            }
            catch
            {
            }
            udpIni();
            //获取本地IP
            Localip = ToolsUtil.GetLocalIP();
            //udp 绑定
            udp.udpBing(Localip, ToolsUtil.GetFreePort().ToString());
            //绑定成功
            if (udp.isbing)
            {


                TextBox[] txts = { txtDevIP, txtDevMask, txtDevDNS };
                for (int i = 0; i < 3; i++)
                {
                    if (string.IsNullOrEmpty(txts[i].Text))
                    {

                        AppTxtShow("地址不能为空");
                        return;
                    }
                    if (!Regex.IsMatch(txts[i].Text, @"^[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}$"))
                    {
                        AppTxtShow("地址格式错误");
                        return;
                    }

                }
                string setip = string.Format("setip {0} {1} {2} {3} {4}", cbOnlineIP.Text, txtDevIP.Text, txtDevMask.Text, txtDevDNS.Text, txtDevDNS.Text);
                //设置IP
                //udp.udpSend(cbOnlineIP.Text, "6002", setip);
                if (cbAutoGet.Checked)
                {
                    setip = setip + " on";
                }
                udp.udpSend("255.255.255.255", "6002", setip);
                AppTxtShow("修改IP地址成功！主机重启，请稍后...");

            }
        }

       
    }
}
