﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using eNet编辑器;
using System.IO;
using eNet编辑器.ThreeView;
using eNet编辑器.AddForm;
using System.Net;
using System.Text.RegularExpressions;
using System.Reflection;

namespace eNet编辑器.DgvView
{
    //public delegate void DgvDeviceCursorDefault();
    public partial class DgvName : Form
    {
       
        public DgvName()
        {
            //设置窗体双缓存
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();
            InitializeComponent();
            //利用反射设置DataGridView的双缓冲
            Type dgvType = this.dataGridView1.GetType();
            PropertyInfo pi = dgvType.GetProperty("DoubleBuffered",
            BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(this.dataGridView1, true, null);

        }

        public event Action<string> txtAppShow;
        public event Action updateSectionTitleNode;
        //public event DgvDeviceCursorDefault dgvDeviceCursorDefault;
        /// <summary>
        /// 解决窗体闪烁问题
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;
                return cp;
            }
        }
        //客户端
        ClientAsync client;

        private void DgvName_Load(object sender, EventArgs e)
        {

            
        }

       

        #region 刷新窗体事件
        /// <summary>
        /// DGV添加没一行的数值 重新加载数据
        /// </summary>
        /// <param name="filepath">ini文件路径</param>
        /// <param name="num">树状图上下级索引号</param>
        public void dgvNameAddItem()
        {
            try{
            //封装一个添加DGV信息的函数 打开配置文件自己读
            this.dataGridView1.Rows.Clear();
            if (FileMesege.tnselectNode == null || FileMesege.tnselectNode.Parent == null )
            {
                return;
            }
            string[] arr = FileMesege.tnselectNode.Text.Split(' ');
            string filename = arr[1] + ".ini";
            string filepath = Application.StartupPath + "\\devices\\" + filename;
            TreeMesege tm = new TreeMesege();
            string[] num = tm.GetNodeNum(FileMesege.tnselectNode).Split(' ');

            //关闭刷新按键 所有信息释放对象
            if (btnNew.Style == DevComponents.DotNetBar.eDotNetBarStyle.Office2003 )
            {
                timer1.Enabled = false;
                client.Close();
                btnNew.Style = DevComponents.DotNetBar.eDotNetBarStyle.VS2005;
               
            }
            if (filepath == "" || num == null)
            {
                return;
            }
            
            //ini添加port type   DeviceList添加 section name 需要用/分割开来
            int master = Convert.ToInt32(num[0]);
            int nub = Convert.ToInt32(num[1]);//树状图的索引号
            //选中节点的设备号
            int device = Convert.ToInt32(FileMesege.DeviceList[master].module[nub].id);
            string value = "";
            int index = 0;
            
            //获取全部Section下的Key
            List<string> list = IniConfig.ReadKeys("ports", filepath);
            //循环添加行信息
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == "0")
                {
                    continue;
                }
                //获取类型版本类型版本
                value = IniConfig.GetValue(filepath, "ports",list[i]);
                if(value == "")
                {
                    break;
                }
                //添加新的一行 端口 类型 区域
                index = this.dataGridView1.Rows.Add();
                //端口号
                this.dataGridView1.Rows[index].Cells[0].Value = list[i].ToString();
                //类型
                this.dataGridView1.Rows[index].Cells[1].Value = IniHelper.findTypesIniNamebyType(value);
                this.dataGridView1.Rows[index].Cells[4].Value = "离线";
                this.dataGridView1.Rows[index].Cells[5].Value = "操作";
                //this.dataGridView1.Rows[index].Cells[3].Value =arr[arr.Length-1];
            }
            //分割的DeviceList 里面ip地址
            string iplast = FileMesege.DeviceList[master].ip;
            string str = null;
            int count = 0;
            
           
            //添加区域和名称
            foreach (DataJson.PointInfo e in FileMesege.PointList.equipment)
            {
                //需要更改IP不是唯一的
                //判断与选中网关节点IP是否相同
                if (e.ip == iplast )
                {

                    str = e.address;
                    //为0直接退出
                    if (str.Substring(6, 2) == "00")
                    {
                        continue;
                    }
                    //判断ID是否为当前选中的ID 
                    if (str.Substring(4, 2) == SocketUtil.strtohexstr((device).ToString()))
                    {

                        count = Convert.ToInt32(str.Substring(6, 2), 16);
                        //判断当前行是否超出 已存在的行数
                        if (count <= this.dataGridView1.Rows.Count)
                        {
                            if (count == 0)
                            {
                                count++;
                            }
                            //解析ID获取行数
                            this.dataGridView1.Rows[count - 1].Cells[2].Value = string.Format("{0} {1} {2} {3}", e.area1, e.area2, e.area3, e.area4).Trim(); 
                            //添加DGV名称
                            this.dataGridView1.Rows[count -1].Cells[3].Value = e.name;

                        }
                    }


                }
                
            }
            shuaxinBtn();
            }//try
            catch (Exception ex) { MessageBox.Show(ex + "临时调试错误信息"); }

        }


        #endregion


        #region 单双击事件

        /// <summary>
        /// 增添添加dgv表格中地域的信息  
        /// </summary>
        /// <param name="count">行号</param>
        private void sectionAdd()
        {
           
            try
            {
                //区域信息 选中节点非空
                if (FileMesege.sectionNode != null)
                {
                    string[] parents = FileMesege.tnselectNode.Parent.Text.Split(' ');
                    string[] devs = FileMesege.tnselectNode.Text.Split(' ');
                    
                    //获取IP最后一位
                     string IP = SocketUtil.GetIPstyle(parents[0], 4);
                    //获取10进制的设备ID号
                    string idstr = Regex.Replace(devs[0], @"[^\d]*", "");
                    //十六进制的ID号
                    string ID = SocketUtil.strtohexstr(idstr);
                    //10进制的ID端口号 
                    string idportstr = dataGridView1.Rows[count].Cells[0].Value.ToString();
                    //十六进制的ID端口号
                    string IDports = SocketUtil.strtohexstr(idportstr);
                    string address = IP + "00" + ID + IDports;
                    string name = "";

                    //暂时存储名称 去掉数字
                    TreeMesege tm = new TreeMesege();
                    string[] tmID = tm.GetSectionId(FileMesege.sectionNode.FullPath.Split('\\'));
                    string[] sections = tm.GetSection(tmID[0],tmID[1],tmID[2],tmID[3]).Split('\\');
                    string type = dataGridView1.Rows[count].Cells[1].Value.ToString();
                    string section = FileMesege.sectionNode.FullPath.Replace("\\", " ");
                    if (section.Contains("全部"))
                    {
                        return;
                    }
                    //新建point点
                    DataListHelper.newPoint(address, parents[0], sections, type);

                    try
                    {
                        //同一个区域和同一个名称 直接返回
                        if (section == dataGridView1.Rows[count].Cells[2].Value.ToString() && dataGridView1.Rows[count].Cells[3].Value.ToString().Contains(FileMesege.titleinfo))
                        {
                            return;
                        }
                    }
                    catch { 
                    }
                    
                    dataGridView1.Rows[count].Cells[2].Value = section;
                    
                    //正常的修改名称操作
                    name = sectionChangeName();

                    dataGridView1.Rows[count].Cells[3].Value = name;

                    foreach (DataJson.PointInfo e in FileMesege.PointList.equipment)
                    {
                        //循环判断 NameList中是否存在该节点
                        if (address == e.address && e.ip == parents[0])
                        {
                            e.name = name;
                            e.type = IniHelper.findTypesIniTypebyName(dataGridView1.Rows[count].Cells[1].Value.ToString());
                            return;
                        }
                    }
                }
            }//try
            catch (Exception ex) {
                //MessageBox.Show(ex + "临时调试错误信息"); 
            }
         
        }


        /// <summary>
        /// 添加区域后 添加或修改 名称序号
        /// </summary>
        /// <returns></returns>
        private string sectionChangeName()
        {

            string title = FileMesege.titleinfo;
            //名称栏为空 或者区域栏 返回空值 
            if (title == "")
            {
                title = "未定义";
            }
            //纯文字title
            string strTitle = Regex.Replace(title, @"\d", "");
            HashSet<int> hasharry = new HashSet<int>();
            string num = "";
            //判断当前是否有匹配
            TreeMesege tm = new TreeMesege();
            //获取id1 - id4
            string[] tmID = tm.GetSectionId(FileMesege.sectionNode.FullPath.Split('\\'));
            string[] section = tm.GetSection(tmID[0], tmID[1], tmID[2], tmID[3]).Split('\\');
            //循环所有信息
            foreach (DataJson.PointInfo eq in FileMesege.PointList.equipment)
            {
                //当地域信息相同
                if (eq.area1 == section[0] && eq.area2 == section[1] && eq.area3 == section[2] && eq.area4 == section[3])
                {
                    if (eq.name != null && eq.name.Contains(strTitle))
                    {
                        //获取序号
                        num = Regex.Replace(eq.name, @"[^\d]*", "");
                        if (num != "")
                        {
                            hasharry.Add(Convert.ToInt32(num));

                        }

                    }
                }

            }
            //哈希表 同一个区域的所有序号都在里面
            List<int> arry = hasharry.ToList<int>();
            arry.Sort();
            if (arry.Count == 0)
            {
                //该区域节点前面数字不存在
                return strTitle + "1";
            }
            //哈希表 不存在序号 直接返回
            for (int i = 0; i < arry.Count; i++)
            {
                if (arry[i] != i + 1)
                {
                    return strTitle + (i + 1).ToString();
                }
            }
            return strTitle + (arry[arry.Count - 1] + 1).ToString();


        }

        /// <summary>
        /// 设备操作弹框
        /// </summary>
        /// <param name="count">行号</param>
        private void concrolAdd(int rowcount)
        {

            DGVconcrol dc = new DGVconcrol();
            //把窗口向屏幕中间刷新
            dc.StartPosition = FormStartPosition.CenterParent;
            string[] parents = FileMesege.tnselectNode.Parent.Text.Split(' ');
            string[] devs = FileMesege.tnselectNode.Text.Split(' ');
            string IP = SocketUtil.GetIPstyle(parents[0], 4);
            string ID = SocketUtil.strtohexstr(Regex.Replace(devs[0], @"[^\d]*", ""));
            string IDports = SocketUtil.strtohexstr((rowcount + 1).ToString());
            string address = IP + "00" + ID + IDports;
            dc.Point = null;
            foreach (DataJson.PointInfo ep in FileMesege.PointList.equipment)
            {
                //循环判断 NameList中是否存在该节点
                if (address == ep.address && ep.ip == parents[0])
                {
                    dc.Point = ep;
                    break;
                }
            }

            dc.Rowindex = rowcount;
            dc.ShowDialog();
            if (dc.DialogResult == DialogResult.OK)
            {

            }
        }

        /// <summary>
        /// 状态框界面显示
        /// </summary>
        private void DgvStateShow() 
        {
            DgvNameState dns = new DgvNameState();
            dns.RowNum = count;
            if (dataGridView1.Rows[count].Cells[4].Value != null)
            {
                //当前状态显示值
                dns.StateValue = getStateValue(dataGridView1.Rows[count].Cells[4].Value.ToString());
            }
            else
            {
                return;
            }
            //居中显示
            dns.StartPosition = FormStartPosition.CenterParent;
            dns.ShowDialog();
        }

        /// <summary>
        /// 把状态栏数值 提取出来 rgb
        /// </summary>
        /// <param name="stateString"></param>
        /// <returns></returns>
        private List<int> getStateValue(string stateString)
        {
            if (string.IsNullOrEmpty(stateString))
            {
                //状态栏为空 直接返回null
                return null;
            }
            else
            {
                //提出来存放的数组List
                List<int> stateList = new List<int>();
                string stateValue = null;
                string[] strArry =  stateString.Replace(" ","").Split(':');
                for (int i = 0; i < strArry.Length; i++)
                {
                    
                     stateValue = Regex.Replace(strArry[i], @"[^\d]*", "");
                     if (!string.IsNullOrEmpty(stateValue))
                     {
                         //获取到的值非空 且为数字
                         stateList.Add(Convert.ToInt32(stateValue));
                     }
                }
               
                return stateList;
            }
        }

        //单元格结束编辑
        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            try{
            //行数
            int count = e.RowIndex;
            dataGridView1.Columns[3].ReadOnly = true;
            if (FileMesege.tnselectNode == null)
            {
                return;
            }
            string[] parents = FileMesege.tnselectNode.Parent.Text.Split(' ');
            string[] devs = FileMesege.tnselectNode.Text.Split(' ');
            string IP = SocketUtil.GetIPstyle(parents[0], 4);
            string ID = SocketUtil.strtohexstr(Regex.Replace(devs[0], @"[^\d]*", ""));
            string IDports = SocketUtil.strtohexstr((count + 1).ToString());
            string address = IP + "00" + ID + IDports;
            //撤销
            DataJson.totalList OldList = FileMesege.cmds.getListInfos();
            foreach (DataJson.PointInfo ep in FileMesege.PointList.equipment)
            {
                //循环判断 NameList中是否存在该节点
                if (address == ep.address && ep.ip == parents[0])
                {
                    
                    if (dataGridView1.Rows[count].Cells[3].Value != null)
                    {
                        string EditName = dataGridView1.Rows[count].Cells[3].Value.ToString();
                        //剔除名字相同 
                        foreach (DataJson.PointInfo et in FileMesege.PointList.equipment)
                        {
                            if (et.area1 == ep.area1 && et.area2 == ep.area2 && et.area3 == ep.area3 && et.area4 == ep.area4 && et.address != ep.address)
                            {
                                if (et.name == EditName)
                                {
                                    
                                    dataGridView1.Rows[count].Cells[3].Value = ep.name;
                                    return;
                                }
                            }
                        }
                        ep.name = EditName;
                    }
                    else
                    {
                        ep.name = "";
                    }
                    DataJson.totalList NewList = FileMesege.cmds.getListInfos();
                    FileMesege.cmds.DoNewCommand(NewList, OldList);
                    
                    return;
                }
            }
            DataJson.PointInfo eq = new DataJson.PointInfo();
            if (dataGridView1.Rows[count].Cells[3].Value != null)
            {
                eq.name = dataGridView1.Rows[count].Cells[3].Value.ToString();
            }
            else
            {
                eq.name = "";
            }           
            eq.ip = parents[0];
            eq.address = address;
            FileMesege.PointList.equipment.Add(eq);
            DataJson.totalList NewList2 = FileMesege.cmds.getListInfos();
            FileMesege.cmds.DoNewCommand(NewList2, OldList);
            }//try
            catch (Exception ex) { MessageBox.Show(ex + "临时调试错误信息"); }
        }

        private bool isFirstClick = true;
        private bool isDoubleClick = false;
        private int milliseconds = 0; 
        /// <summary>
        /// 行号
        /// </summary>
        private int count = 0;
        private int ind = 0;
        private int oldcount = 0;
        private int oldind = 0;

        //鼠标单击开启 判断单双击时钟
        private void dataGridView1_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            oldcount = count;
            oldind = ind;
            count = e.RowIndex;
            ind =e.ColumnIndex;
            // 鼠标单击.
            if (isFirstClick)
            {
                isFirstClick = false;
                doubleClickTimer.Start();
            }
            // 鼠标双击
            else
            {

                isDoubleClick = true;
            }
 
        }

        //时钟判断函数
        private void doubleClickTimer_Tick_1(object sender, EventArgs e)
        {
            milliseconds += 100;
            // 第二次鼠标点击超出双击事件间隔
            if (milliseconds >= SystemInformation.DoubleClickTime)
            {
                doubleClickTimer.Stop();


                if (isDoubleClick)
                {
                    //处理双击事件操作
                    dataGridView1.Columns[3].ReadOnly = false;
                }
                else
                {
                    //处理单击事件操作
                    //DGV的行号

                    if (count >= 0 && ind>=0)
                    {
                        try{
                        bool isConcrol = false;
                        switch (dataGridView1.Columns[ind].Name)
                        {
                            case "NameOperation":
                                //弹出操作对话框
                                //dataGridView1.Rows[count].Cells[4].Style.BackColor = Color.Red;
                                isConcrol = true;
                                concrolAdd(count);
                                break;
                            case "NameSection":
                                //添加地域信息
                                if (FileMesege.sectionNode != null)
                                {
                                    if (FileMesege.titleinfo == "")
                                    {
                                        FileMesege.titleinfo = "未定义";
                                    }
                                    //撤销
                                    DataJson.totalList OldList = FileMesege.cmds.getListInfos();
                                    sectionAdd();
                                    DataJson.totalList NewList = FileMesege.cmds.getListInfos();
                                    FileMesege.cmds.DoNewCommand(NewList, OldList);
                                }                             
                                break;
                            case "NameName":
                                //不能单独赋值name
                                break;
                                   
                            case "NameState":
                                isConcrol = true;
                                //展示状态窗口
                                DgvStateShow();
                                break;
                            default: break;
                        }
                        // 点位地址赋值
                        if (!isConcrol && !string.IsNullOrEmpty( FileMesege.titlePointSection ))
                        { 
                            //添加地址区域 名称 point添加地址  device添加                            
                            pointAddAddress();
                           
                        }
                        }//try
                        catch (Exception ex) { MessageBox.Show(ex + "临时调试错误信息"); }
                    }
                }
                isFirstClick = true;
                isDoubleClick = false;
                milliseconds = 0;
            }
        }

        /// <summary>
        /// 点位绑定地址
        /// </summary>
        private void pointAddAddress()
        { 
            //修改IP和addres  type待定是否需要修改
            string[] parents = FileMesege.tnselectNode.Parent.Text.Split(' ');
            string[] devs = FileMesege.tnselectNode.Text.Split(' ');

            //获取IP最后一位
            string IP = SocketUtil.GetIPstyle(parents[0], 4);
            //获取10进制的设备ID号
            string idstr = Regex.Replace(devs[0], @"[^\d]*", "");
            //十六进制的ID号
            string ID = SocketUtil.strtohexstr(idstr);
            //10进制的ID端口号 
            string idportstr = dataGridView1.Rows[count].Cells[0].Value.ToString();
            //十六进制的ID端口号
            string IDports = SocketUtil.strtohexstr(idportstr);
            string address = IP + "00" + ID + IDports;
            
            //area1-4 + name
            List<string> section_name = DataListHelper.dealPointInfo(FileMesege.titlePointSection);
            if (section_name.Count < 5)
            {
                return;
            }

            foreach (DataJson.PointInfo eq in FileMesege.PointList.equipment)
            {
                if (eq.address == address && eq.ip == parents[0])
                {
                    eq.address = "FFFFFFFF";
                    eq.ip = "";
                    //类型删除 不能删除
                    //eq.type = "";
                    break;
                }
            }
            foreach (DataJson.PointInfo eq in FileMesege.PointList.equipment)
            {
                if (eq.area1 == section_name[0] && eq.area2 == section_name[1] && eq.area3 == section_name[2] && eq.area4 == section_name[3] && eq.name == section_name[4])
                {
                    //获取当前选中类型类型
                    string type = IniHelper.findTypesIniTypebyName(dataGridView1.Rows[count].Cells[1].Value.ToString());
                    if(!string.IsNullOrEmpty(eq.type) && eq.type != type)
                    {

                        txtAppShow(string.Format("该点位类型为（{0}）与端口类型不一致！", IniHelper.findTypesIniNamebyType(eq.type)));
                        return ;
                    }
                    //撤销
                    DataJson.totalList OldList = FileMesege.cmds.getListInfos();
                    eq.ip = parents[0];
                    eq.address = address;
                    eq.type = type;
                    DataJson.totalList NewList = FileMesege.cmds.getListInfos();
                    FileMesege.cmds.DoNewCommand(NewList, OldList);
                    //刷新dgv
                    dgvNameAddItem();
                    updateSectionTitleNode();
                    break;
                }
            }
            
        }


        #endregion


        #region  刷新按键
        /// <summary>
        /// 刷新按键 按时获取1  
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnNew_Click_1(object sender, EventArgs e)
        {
            shuaxinBtn();
        }

        /// <summary>
        /// 刷新按钮 把当前设备状态刷回来
        /// </summary>
        public void shuaxinBtn()
        {

            try
            {
                if (client != null && client.Connected())
                {
                    client.Dispoes();
                }
                //实例化客户端
                client = new ClientAsync();
                newClient();
                string[] strip = FileMesege.tnselectNode.Parent.Text.Split(' ');
                //异步连接
                client.ConnectAsync(strip[0], 6003);
                if (client != null && client.Connected())
                {
                    timer1_Tick(this, EventArgs.Empty);
                    timer1.Enabled = true;
                }
                
            }
            catch
            {
                timer1.Enabled = false;
                client = null;
                return;
            }
        }

        /// <summary>
        /// 5秒自动发码 一次获取设备在线状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (client != null && client.Connected())
                {
                    string msg = "GET;{" + SocketUtil.getIP(FileMesege.tnselectNode) + ".0." + SocketUtil.getID(FileMesege.tnselectNode) + ".255};\r\n";

                    //客户端发送数据
                    client.SendAsync(msg);
                }
            }
            catch
            {
                timer1.Enabled = false;
                client = null;
                return;
            }
            
        }

        /// <summary>
        /// 新建连接socket
        /// </summary>
        private void newClient()
        {
            
            //实例化事件 传值到封装函数  c为函数类处理返回的client
            client.Completed += new Action<System.Net.Sockets.TcpClient, ClientAsync.EnSocketAction>((c, enAction) =>
            {
                string key = "";

                try
                {
                    if (c.Client.Connected)
                    {
                        //强转类型
                        IPEndPoint iep = c.Client.RemoteEndPoint as IPEndPoint;
                        //返回的IP 和 端口号
                        key = string.Format("{0}:{1}", iep.Address.ToString(), iep.Port);
                    }
                }
                catch { }

                switch (enAction)
                {
                    case ClientAsync.EnSocketAction.Connect:
                        //MessageBox.Show("已经与" + key + "建立连接");
                        btnNew.Style = DevComponents.DotNetBar.eDotNetBarStyle.Office2003;
                        //timer1.Start();

                        break;
                    case ClientAsync.EnSocketAction.SendMsg:

                        //MessageBox.Show(DateTime.Now + "：向" + key + "发送了一条消息");
                        break;
                    case ClientAsync.EnSocketAction.Close:
                        //client.Close();
                        //btnNew.Style = DevComponents.DotNetBar.eDotNetBarStyle.VS2005;
                        //MessageBox.Show("服务端连接关闭");
                        break;
                    case ClientAsync.EnSocketAction.Error:
                        //btnNew.Style = DevComponents.DotNetBar.eDotNetBarStyle.VS2005;
                        //MessageBox.Show("连接发生错误,请检查网络连接");

                        break;
                    default:
                        break;
                }
            });
            //信息接收处理
            client.Received += new Action<string, string>((key, msg) =>
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
                        int j = Convert.ToInt32(match.Groups[4].Value) - 1;
                        //判断ini类型
                        dataGridView1.Rows[j].Cells[4].Value = getDataType(j + 1, strs[1]);
                    }
                }
                catch
                {
                    //报错不处理
                    //MessageBox.Show("DgvName处理信息出错869行");
                }
            });
        }

        /// <summary>
        /// 获取FB:0000 0000 :{IP Linid ID Port } 截取不同类型的数据 
        /// </summary>
        /// <param name="type">第几行 行号</param>
        /// <param name="val">数值</param>
        /// <returns>正常返回截取后的值 否则全值返回</returns>
        private string getDataType(int rowNum , string val)
        {
            
            if (FileMesege.tnselectNode == null || FileMesege.tnselectNode.Parent == null)
            {
                return val;
            }
            //转换为二进制的返回值
            string binVal = DataChange.HexString2BinString(val).Replace(" ","");
            //Convert.ToString(0xval,2);
            string type = "";
            
            string[] arr = FileMesege.tnselectNode.Text.Split(' ');
            string filename = string.Format("{0}.ini", arr[1]);
            string filepath =string.Format("{0}\\devices\\{1}",Application.StartupPath,filename);
            //获取types下 ini类型名称
            type = IniConfig.GetValue(filepath, "ports", rowNum.ToString()).Split(',')[0];
            filepath = string.Format("{0}\\types\\{1}.ini",Application.StartupPath,type) ;
            //获取全部Section下的Key
            List<string> list = IniConfig.ReadKeys("data", filepath);
            
            string[]  infos = null;
            string nowState = "";
            //最后返回的信息值
            string sataename = "";
            for (int i = 0; i < list.Count; i++)
            {
                //获取类型下data的数据  rw,uint8,0-1,0-1,亮度(%)
                infos = IniConfig.GetValue(filepath, "data", list[i]).Split(',');
                //读取的value的格式不规范 直接退出不作处理 排除format
                if (infos.Length != 5)
                {
                    continue; 
                }
                nowState = getBinBit(binVal, infos[2]);
                
                sataename = sataename + string.Format("{0}:{1} ", infos[4], nowState);
                
            }

            return sataename;
                          
        }

        /// <summary>
        /// 截取二进制位数  binval为二进制数值 inset为0 / 0-1 / 位置数
        /// </summary>
        /// <param name="binval"></param>
        /// <param name="inset">位置数</param>
        /// <returns>返回十进制值</returns>
        private string getBinBit(string binval ,string inset)
        {
            string bin = "";
            //截取位数 组成一个新值
            if (inset.Contains("-"))
            {
                string[] infos = inset.Split('-');
                int end = Convert.ToInt32(infos[1]);
                int start = Convert.ToInt32(infos[0]);
                //反转二进制数据
                bin = DataChange.Reversal(binval).Substring(start, end - start + 1);
                
            }
            else
            {
                //反转二进制数据
                bin = DataChange.Reversal(binval).Substring(Convert.ToInt32(inset), 1);
                
            }
            //再反转复原二进制数据
            return Convert.ToInt32(DataChange.Reversal(bin), 2).ToString();
        }

        #endregion

        #region 鼠标右击  鼠标图标更改
        /// <summary>
        /// 右击鼠标清位置信息与名字信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                //treesection临时存放数据处
                FileMesege.sectionNode = null;
                //treetitle名字临时存放
                FileMesege.titleinfo = "";
                updateSectionTitleNode();

                //鼠标图标改变
                //cursor_default();
                //dgvDeviceCursorDefault();
            }
        }

        /// <summary>
        /// 鼠标图标更改为正常图标
        /// </summary>
        public void cursor_default()
        {

            dataGridView1.Cursor = Cursors.Default;
        }

        /// <summary>
        /// 鼠标图标更改为复制图标
        /// </summary>
        public void cursor_copy()
        {
           
            //定义图片
            Bitmap a = (Bitmap)Bitmap.FromFile(Application.StartupPath + "\\cursor32.png");
            //定义加载到那个控件
            DgvMesege.SetCursor(a, new Point(0, 0), dataGridView1);
            //dataGridView1.Cursor = Cursors.PanWest;
            
        }

       
        #endregion

        #region Del按键处理
        //删除按键
        private void dataGridView1_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyData == Keys.Delete)
                {

                    if (FileMesege.tnselectNode == null)
                    {
                        return;
                    }

                    DelKeySection(dataGridView1.CurrentCell.RowIndex);
                    dataGridView1.Rows[dataGridView1.CurrentCell.RowIndex].Cells[2].Value = null;
                    dataGridView1.Rows[dataGridView1.CurrentCell.RowIndex].Cells[3].Value = null;
                }
            }
            catch (Exception ex) { MessageBox.Show(ex+ "临时调试错误信息"); }
        }


        private void DelKeySection(int rowIndex)
        {
            string[] parents = FileMesege.tnselectNode.Parent.Text.Split(' ');
            string[] devs = FileMesege.tnselectNode.Text.Split(' ');
            string IP = SocketUtil.GetIPstyle(parents[0], 4);
            string ID = SocketUtil.strtohexstr(Regex.Replace(devs[0], @"[^\d]*", ""));
            string IDports = SocketUtil.strtohexstr((rowIndex + 1).ToString());
            string address = IP + "00" + ID + IDports;

            foreach (DataJson.PointInfo ep in FileMesege.PointList.equipment)
            {
                //循环判断 NameList中是否存在该节点
                if (address == ep.address && ep.ip == parents[0])
                {
                    //撤销
                    DataJson.totalList OldList = FileMesege.cmds.getListInfos();
                    //清除信息
                    //FileMesege.PointList.equipment.Remove(ep);
                    ep.address = "FFFFFFFF";
                    ep.ip = "";
                    ep.type = "";
                    DataJson.totalList NewList = FileMesege.cmds.getListInfos();
                    FileMesege.cmds.DoNewCommand(NewList, OldList);
                    updateSectionTitleNode();
                    return;
                }
            }
          
        }

        #endregion

     

    
   


        //end
    }
}
