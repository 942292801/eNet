﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;

namespace eNet编辑器.AddForm
{
    public partial class LogicAddress : Form
    {
        public LogicAddress()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 传入地址
        /// </summary>
        private string obj;

        public string Obj
        {
            get { return obj; }
            set { obj = value; }
        }

        private string objType;

        /// <summary>
        /// 对象类型（中文）
        /// </summary>
        public string ObjType
        {
            get { return objType; }
            set { objType = value; }
        }

        /// <summary>
        /// 返回 类型（英文4.0_scene）
        /// </summary>
        private string rtType;

        public string RtType
        {
            get { return rtType; }
            set { rtType = value; }
        }

        /// <summary>
        /// 存放当前dev表中读取到的 ID+设备
        /// </summary>
        List<string> devName = null;

        private string ip = "";

        LinkType linkType = LinkType.Dev;

        /// <summary>
        /// 链路的类型
        /// </summary>
        public enum LinkType
        {
            /// <summary>
            /// 设备类型
            /// </summary>
            Dev = 1,
            /// <summary>
            /// 虚拟端口类型
            /// </summary>
            Var = 2,
            /// <summary>
            /// 场景 定时 面板 类型
            /// </summary>
            Com = 3,
            /// <summary>
            /// 局部变量
            /// </summary>
            localVar=4
            
        }

        /// <summary>
        /// address下 2 的内容（ 类型，xx,0 ）
        /// </summary>
        List<string> TypeList = null;

        //bool isLoad = false;
        private void LogicAddress_Load(object sender, EventArgs e)
        {
            try
            {
                cbTypeitemIni();
                getIp();
                //加载默认的类型到cb中
                iniInfo(objType);
                
                //加载地址信息
                if (!string.IsNullOrEmpty(obj))
                {
                    string[] infos = obj.Split('.');
                    if (linkType == LinkType.Com)
                    {
                        //不为设备类型
                        cb1.Text = infos[0];
                        //将超256 的号数操作
                        cb4.Text = (Convert.ToInt32(infos[2]) * 256 + Convert.ToInt32(infos[3])).ToString();
                        findNum();
                    }
                    else if (linkType == LinkType.Dev)
                    {
                        addNumForDevList();
                        cb1.Text = infos[0];
                        cb3.Text = infos[2];
                        cb4.Text = infos[3];
                        findPort(infos[2]);
                    }
                    else if (linkType == LinkType.Var)
                    {
                        //虚拟端口
                        cb1.Text = infos[0];
                        cb3.Text = infos[2];
                        cb4.Text = infos[3];
                        findVarNum();
                        cb3.Enabled = false;
                    }
                    else if (linkType == LinkType.localVar)
                    {
                        //局部变量
                        cb1.Text = infos[0];
                        int localvarNum = Convert.ToInt32(infos[2]) * 256 + Convert.ToInt32(infos[3]);
                        double tmp = Math.Ceiling((double)localvarNum/8);
                        cb3.Text = Convert.ToInt32(tmp).ToString();
                        //将超256 的号数操作
                        cb4.Text = localvarNum.ToString();
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            
            
        }

        /// <summary>
        /// 初始化加载ini里面的类型
        /// </summary>
        private void cbTypeitemIni()
        {
            //记录全部cb2的类型
            
            //循环读取INI里面的信息
            DirectoryInfo folder = new DirectoryInfo(Application.StartupPath + "//types");
            
            HashSet<string> nameList = new HashSet<string>();
            string tmp = "";
            foreach (FileInfo file in folder.GetFiles("*.ini"))
            {
                tmp = IniConfig.GetValue(file.FullName, "address", "2");
                if (!string.IsNullOrEmpty(tmp))
                {
                    nameList.Add(tmp);
                }
               
            }
            TypeList = nameList.ToList<string>();
            for (int i = 0; i < TypeList.Count; i++)
            {
                cb2.Items.Add(TypeList[i].Split(',')[1]);
            }
            cb2.Items.Add("局部变量");
        }

        /// <summary>
        /// 根据类型type（中文名） 加载Ini的信息
        /// </summary>
        private void iniInfo(string findType)
        {
            if (string.IsNullOrEmpty(findType))
            {
                return;
            }
            if (findType == "局部变量")
            {
                localVarMode();
                return;
            }
            
            linkType = LinkType.Dev;
            string fileName = IniHelper.findTypesIniTypebyName(findType);
            string path = string.Format("{0}//types//{1}.ini", Application.StartupPath, fileName);
       
            Label[] lbs = { lb1, lb2, lb3, lb4 };
            ComboBox[] cbs = { cb1, cb2, cb3, cb4 };
            //读取【address】1-4信息
            for (int i = 0; i < 4; i++)
            {
                string[] infos = IniConfig.GetValue(path, "address", (i + 1).ToString()).Split(',');
                if (infos[0] == "固定" && infos.Length == 2)
                {
                    //变量Ini处理 address:3=
                    lbs[i].Text = infos[0];
                    cbs[i].Text = infos[1];
                    cb1.Text = "254";
                    cb3.Enabled = false;
                    linkType = LinkType.Var;
                }
                else if (infos.Length == 2)
                {
                    lbs[i].Text = infos[0];
                    dealInfoNum(cbs[i], infos[1]);
                }
                else if (infos.Length == 3)
                {
                    //类型信息
                    cb2.Text = infos[1];
                    
                }
                else
                {
                    //场景 定时 面板 传感 即cb3关闭
                    lb3.Text = "";
                    cb3.Text = "";
                    cb3.Enabled = false;
                    linkType = LinkType.Com;
                }
            }///foreach 
            
        }

        /// <summary>
        /// 把cb的信息更新为局部变量信息
        /// </summary>
        private void localVarMode()
        {
            try
            {
                linkType = LinkType.localVar;
                lb3.Text = "逻辑号";
                lb4.Text = "局部变量号";
                cb2.Text = "局部变量";
                cb3.Text = "";
                cb4.Text = "";
                cb3.Enabled = true;
                rtType = "15.0_LocalVariable";
                cb3.Items.Clear();
                foreach (DataJson.Logic lg in FileMesege.logicList)
                {
                    //  添加该网关IP的子节点
                    if (lg.IP == ip)
                    {
                        foreach (DataJson.logics lgs in lg.logics)
                        {
                            cb3.Items.Add(lgs.id);

                        }

                    }
                }

            }
            catch{
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

        /// <summary>
        /// 加载Ip到cb1的框里面 254或者当前传进来的IP
        /// </summary>
        /// <param name="selectSumNode">选中子节点</param>
        private void getIp()
        {

            try
            {
                devName = new List<string>();
                switch (FileMesege.formType)
                {
                    ////////////////////////////////////还需要后续添加选中节点参数
                    case "name":

                        break;

                    case "point":
                        break;
                    case "scene":
                        ip = FileMesege.sceneSelectNode.Parent.Text.Split(' ')[0];
                        break;
                    case "timer":
                        ip = FileMesege.timerSelectNode.Parent.Text.Split(' ')[0];
                        break;
                    case "panel":
                        ip = FileMesege.panelSelectNode.Parent.Text.Split(' ')[0];
                        break;
                    case "sensor":
                        ip = FileMesege.sensorSelectNode.Parent.Text.Split(' ')[0];
                        break;
                    case "logic":
                        ip = FileMesege.logicSelectNode.Parent.Text.Split(' ')[0];
                        break;
                    
                    default: break;
                }

                cb1.Enabled = true;
                cb1.Items.Add( "254") ;
                if (string.IsNullOrEmpty(cb1.Text) || cb1.Text == "255")
                {
                    //cb1.Text = ip.Split('.')[3];
                    cb1.Text = "254";
                }
                
                
            }
            catch
            {

            }
           
        }

        /// <summary>
        /// 加载网关的设备到cb3里面
        /// </summary>
        private void addNumForDevList()
        {
            foreach (DataJson.Device devip in FileMesege.DeviceList)
            {
                if (devip.ip == ip)
                {
                    cb3.Items.Clear();
                    foreach (DataJson.Module md in devip.module)
                    {
                        devName.Add(md.id + " " + md.device);
                        cb3.Items.Add(md.id);
                    }
                    
                    break;
                }
            }
        }



        private void cb2_SelectedIndexChanged(object sender, EventArgs e)
        {
            cb3.Enabled = true;
            typeChange(cb2.Text);
            if (linkType == LinkType.Dev)
            {
                addNumForDevList();
                
            }
            else if (linkType == LinkType.Com)
            {
                findNum();
                cb3.Enabled = false;
            }
            else if (linkType == LinkType.Var)
            {
                findVarNum();
                cb3.Enabled = false;
            }
          
            
        }



        /// <summary>
        /// 当cb2类型改变cb3 cb4的格式  TypeList的格式
        /// </summary>
        /// <param name="cbType"></param>
        private void typeChange(string cbType)
        {
            if (string.IsNullOrEmpty(cbType))
            {
                return;
            }
            if (cbType == "局部变量")
            {
                localVarMode();
                return;
            }
            //循环读取INI里面的信息
            DirectoryInfo folder = new DirectoryInfo(Application.StartupPath + "//types");
            string type = "";
            cb3.Text = "";
            cb4.Text = "";
            linkType = LinkType.Dev;
            string tmp = "";
            foreach (FileInfo file in folder.GetFiles("*.ini"))
            {
                tmp = IniConfig.GetValue(file.FullName, "address", "2");
                if (string.IsNullOrEmpty(tmp))
                {
                    continue;
                }
                type = tmp.Split(',')[1];
                //找到类型一致
                if (type == cbType)
                {
                    //记录当前类型 场景 定时 面板 人感 编组
                    rtType = file.Name.Replace(".ini","");
                    Label[] lbs = { lb1, lb2, lb3, lb4 };
                    ComboBox[] cbs = { cb1, cb2, cb3, cb4 };
                    //读取【address】1-4信息
                    for (int i = 0; i < 4; i++)
                    {
                        string[] infos = IniConfig.GetValue(file.FullName, "address", (i + 1).ToString()).Split(',');
                        if (infos[0] == "固定" && infos.Length == 2)
                        {
                            lbs[i].Text = infos[0];
                            cbs[i].Text = infos[1];
                            cb1.Text = "254";
                            cb3.Enabled = false;
                            linkType = LinkType.Var;

                        }
                        else if (infos.Length == 2)
                        {
                            lbs[i].Text = infos[0];
                            dealInfoNum(cbs[i], infos[1]);
                        }
                        else if (infos.Length == 3)
                        {
                            //类型信息
                           //cb2.Text = infos[1];

                        }
                        else
                        {
                            //场景 定时 面板 传感 即cb3关闭
                            lbs[i].Text = "";
                            cb3.Items.Clear();
                            cb3.Text = "";
                            cb3.Enabled = false;
                            linkType = LinkType.Com;
                        }

                    }
                    break;
                }
                //不存在该个类型
                rtType = "";
            }///foreach 
             
        }

        private void cb3_TextChanged(object sender, EventArgs e)
        {
            
            if (cb3.Enabled && linkType == LinkType.Dev)
            {
                findPort(cb3.Text);
            }
            else if (linkType == LinkType.localVar)
            {
                findLogicNum(cb3.Text);
            }
            
        }

        /// <summary>
        /// 寻找该设备的端口数目
        /// </summary>
        /// <param name="id"></param>
        private void findPort(string id)
        {
            if (devName == null)
            {
                return;
            }
            for (int i = 0; i < devName.Count; i++)
            {
                if (devName[i].Split(' ')[0] == id)
                {
                    string filepath = IniHelper.findDevicesDisplay(devName[i].Split(' ')[1]);
                    //获取全部Section下的Key
                    List<string> list = IniConfig.ReadKeys("ports", filepath);
                    cb4.Items.Clear();
                    //循环添加行信息
                    for (int j = 0; j < list.Count; j++)
                    {

                        cb4.Items.Add(list[j]);
                    }
                    break;
                }
            }

        }

        /// <summary>
        /// 查找当前ip的存在场景 定时 面板 号
        /// </summary>
        private void findNum()
        {
            try
            {
                
                //////////////////////////////////后期还需要添加
                switch (cb2.Text)
                {
                    case "场景":
                        cb4.Items.Clear();
                        string tmpip = ip;
                        if (cb1.Text != "254")
                        {
                            string[] ips = ip.Split('.');
                            tmpip = string.Format("{0}.{1}.{2}.{3}", ips[0], ips[1], ips[2], cb1.Text);
                        }
                        DataJson.Scene scene = DataListHelper.getSceneList(tmpip);

                        foreach (DataJson.scenes scenes in scene.scenes)
                        {
                            cb4.Items.Add(scenes.id);
                        }
                        break;
                    case "定时":
                        cb4.Items.Clear();
                        DataJson.Timer timer = DataListHelper.getTimerList(ip);

                        foreach (DataJson.timers tms in timer.timers)
                        {
                            cb4.Items.Add(tms.id);
                        }
                        break;
                    case "面板":
                        cb4.Items.Clear();
                        DataJson.Panel panel = DataListHelper.getPanelList(ip);

                        foreach (DataJson.panels pls in panel.panels)
                        {
                            cb4.Items.Add(pls.id);
                        }
                        break;
                    case "感应编组":
                         cb4.Items.Clear();
                        DataJson.Sensor sensor = DataListHelper.getSensorList(ip);

                        foreach (DataJson.sensors srs in sensor.sensors)
                        {
                            cb4.Items.Add(srs.id);
                        }
                        break;
                    case "逻辑":
                        cb4.Items.Clear();
                        DataJson.Logic logic = DataListHelper.getLogicList(ip);

                        foreach (DataJson.logics lgs in logic.logics)
                        {
                            cb4.Items.Add(lgs.id);
                        }
                        break;
                    case "局部变量":
                        cb4.Items.Clear();
                        logic = DataListHelper.getLogicList(ip);

                        foreach (DataJson.logics lgs in logic.logics)
                        {
                            for (int i = (lgs.id - 1) * 16 + 1; i <= lgs.id * 16; i++)
                            {
                                cb4.Items.Add(i);
                            }

                        }
                        break;
                    default:
                        break;
                }
            }
            catch { }
        }

        /// <summary>
        /// 查找当前ip的存在虚拟端口
        /// </summary>
        private void findVarNum()
        {
            try
            {
                cb4.Items.Clear();
                foreach (DataJson.PointInfo point in FileMesege.PointList.virtualport)
                {
                    if (point.ip == ip)
                    {

                        cb4.Items.Add(Convert.ToInt32(point.address.Substring(6, 2), 16));
                    }
                }

            }
            catch { }
        }

        /// <summary>
        /// 局部变量 加载逻辑号
        /// </summary>
        /// <param name="logicID"></param>
        private void findLogicNum(string logicID)
        {
            int id = Validator.GetNumber(logicID);
            if (id == -1)
            {
                return;
            }
            cb4.Items.Clear();
            for (int i = (id - 1) * 8 + 1; i <= id * 8; i++)
            {
                cb4.Items.Add(i);
            }
        }

        private void btnDecid_Click(object sender, EventArgs e)
        {
            try {
                string newobj = "";
                if (cb1.Text != "" && cb2.Text != "")
                {

                    for (int i = 0; i < TypeList.Count; i++)
                    {
                        if (TypeList[i].Split(',')[1] == cb2.Text)
                        {
                            newobj = ToolsUtil.strtohexstr(cb1.Text) + ToolsUtil.strtohexstr(TypeList[i].Split(',')[2]);
                            break;
                        }
                    }

                    if (cb3.Text != "" && cb4.Text != "")
                    {
                        if (cb2.Text != "局部变量")
                        {
                            //设备
                            newobj = newobj + ToolsUtil.strtohexstr(cb3.Text) + ToolsUtil.strtohexstr(cb4.Text);
                        }
                        else
                        {
                            //局部变量
                            string tmp = ToolsUtil.strtohexstr(cb4.Text);
                            while (tmp.Length < 4)
                            {
                                tmp = tmp.Insert(0, "0");
                            }
                            newobj = ToolsUtil.strtohexstr(cb1.Text) + ToolsUtil.strtohexstr("249") + tmp;
                        }
                        
                    }
                    else if (cb3.Text == "" && cb4.Text != "")
                    {
                        //非设备类
                        string tmp = ToolsUtil.strtohexstr(cb4.Text);
                        while (tmp.Length < 4)
                        {
                            tmp = tmp.Insert(0, "0");
                        }
                        newobj = newobj + tmp;
                    }
                }
                if (newobj.Length == 8)
                {
                    this.obj = newobj;
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

        private void LogicAddress_Paint(object sender, PaintEventArgs e)
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


        private void btnClose_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.No;
            this.Close();
        }

        private void plInfoTitle_MouseDown_1(object sender, MouseEventArgs e)
        {
            mPoint = new Point(e.X, e.Y);
        }

        private void plInfoTitle_MouseMove_1(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Location = new Point(this.Location.X + e.X - mPoint.X, this.Location.Y + e.Y - mPoint.Y);
            }
        }

        #endregion









    }
}
