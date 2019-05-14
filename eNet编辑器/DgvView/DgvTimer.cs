﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using System.Reflection;
using System.Text.RegularExpressions;
using eNet编辑器.AddForm;
using DevComponents.DotNetBar.Primitives;
using System.IO;
using System.Net.Sockets;
using Newtonsoft.Json;
using DevComponents.DotNetBar.Controls;

namespace eNet编辑器.DgvView
{
    public partial class DgvTimer : Form
    {
        public DgvTimer()
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
            X2Date_Initialize();
        }

        #region X2Date_Initialize

        /// <summary>
        /// Initializes our X2 "Date" environment
        /// </summary>
        private void X2Date_Initialize()
        {
            DataGridViewDateTimeInputColumn oc =
                dataGridView1.Columns["shortTime"] as DataGridViewDateTimeInputColumn;

            if (oc != null)
            {
                // Hook onto the following events so we can
                // demonstrate cell click processing

                oc.ButtonCustom2Click += X2Date_ButtonClearClick;
                oc.ButtonCustomClick += X2Date_ButtonCustomClick;
            }
        }


        #region X2Date_ButtonClearClick

        /// <summary>
        /// Handles X2 "Date" ButtonClear Clicks
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void X2Date_ButtonClearClick(object sender, EventArgs e)
        {
            DataGridViewDateTimeInputCell cell =
                    dataGridView1.CurrentCell as DataGridViewDateTimeInputCell;
            cell.Value = "日落时间";
            
        }

        #endregion

        #region X2Date_ButtonCustomClick

        /// <summary>
        /// Handles X2 "Date" ButtonCustom click events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void X2Date_ButtonCustomClick(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("Set Date to today?", "Set Date",
                                  MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                                  MessageBoxDefaultButton.Button1);

            if (dr == DialogResult.Yes)
            {
                DataGridViewDateTimeInputCell cell =
                    dataGridView1.CurrentCell as DataGridViewDateTimeInputCell;

                if (cell != null)
                {
                    DataGridViewDateTimeInputEditingControl ec =
                        cell.DataGridView.EditingControl as DataGridViewDateTimeInputEditingControl;

                    if (ec != null)
                        ec.Text = DateTime.Today.ToString();
                }
            }
        }

        #endregion

        #endregion

        /// <summary>
        /// 主Form信息显示
        /// </summary>
        public event Action<string> AppTxtShow;

        

        private void DgvTimer_Load(object sender, EventArgs e)
        {
            //Listbox的hot特效
            listbox.DataItemVisualCreated += new DataItemVisualEventHandler(ListBox3DataItemVisualCreated);
            listbox.ValueMember = "Id"; // Id property will be used as ValueMemeber so SelectedValue will return the Id
            listbox.DisplayMember = "Text"; // Text property will be used as the item text

            //新增对象列 加载
            this.dataGridView1.Rows.Clear();
            DataGridViewComboBoxColumn dgvc = new DataGridViewComboBoxColumn();
            DirectoryInfo folder = new DirectoryInfo(Application.StartupPath + "//types");
            foreach (FileInfo file in folder.GetFiles("*.ini"))
            {
                dgvc.Items.Add(IniConfig.GetValue(file.FullName, "define", "name"));
            }
            //设置列名
            dgvc.HeaderText = "类型";
            //设置下拉列表的默认值 
            dgvc.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;

            //或者这样设置 默认选择第一项
            dgvc.DefaultCellStyle.NullValue = dgvc.Items[0];

            dgvc.Name = "type";

            //插入
            this.dataGridView1.Columns.Insert(1, dgvc);
        }

        void ListBox3DataItemVisualCreated(object sender, DataItemVisualEventArgs e)
        {
            ListBoxItem item = (ListBoxItem)e.Visual;
            item.HotTracking = true;
            
        }

        #region 刷新dgv框相关操作 
        
        /// <summary>
        /// 刷新该节点的所有信息
        /// </summary>
        public void TimerAddItem()
        {
           
            try
            {
                clear1_7Check();
                clearVery_customCheck();
                cbPriorHoliday.Checked = false;
                listbox.Items.Clear();
                this.dataGridView1.Rows.Clear();
                //multipleList.Clear();
                //查看获取对象是否存在
                DataJson.timers tms = getTimersInfoList();
                if (tms == null)
                {
                    return;
                }
                dealtmsDates_prior(tms);
                List<DataJson.timersInfo> delTimer = new List<DataJson.timersInfo>();
                string ip4 = SocketUtil.strtohexstr(SocketUtil.getIP(FileMesege.timerSelectNode));//16进制
                //循环加载该场景号的所有信息
                foreach (DataJson.timersInfo tmInfo in tms.timersInfo)
                {
                        
                    int dex = dataGridView1.Rows.Add();

                    if (tmInfo.pid == 0)
                    {
                        //pid号为0则为空 按地址来找
                        if (tmInfo.address != "" && tmInfo.address != "FFFFFFFF")
                        {
                            DataJson.PointInfo point = DataListHelper.findPointByType_address(tmInfo.type, ip4 + tmInfo.address.Substring(2, 6));
                            if (point != null)
                            {
                                tmInfo.pid = point.pid;
                                tmInfo.address = point.address;
                                tmInfo.type = point.type;
                                dataGridView1.Rows[dex].Cells[3].Value = string.Format("{0} {1} {2} {3}", point.area1, point.area2, point.area3, point.area4).Trim();//改根据地址从信息里面获取
                                dataGridView1.Rows[dex].Cells[4].Value = point.name;
                            }
                        }

                    }
                    else
                    {
                        //pid号有效 需要更新address type
                        DataJson.PointInfo point = DataListHelper.findPointByPid(tmInfo.pid);
                        if (point == null)
                        {
                            //pid号有无效 删除该场景
                            delTimer.Add(tmInfo);
                            dataGridView1.Rows.Remove(dataGridView1.Rows[dex]);
                            continue;
                        }
                        else
                        {
                            //pid号有效
                            tmInfo.address = point.address;
                            //////////////////////////////////////////////////////争议地域
                            //类型不一致 在value寻找
                            if (tmInfo.type != point.type && !string.IsNullOrEmpty(point.value) && !string.IsNullOrEmpty(point.objType))
                            {
                                //根据value寻找type                        
                                point.type = IniHelper.findObjValueType_ByobjTypeValue(point.objType, point.value);
                            }
                            //////////////////////////////////////////////////////到这里
                            if (tmInfo.type != point.type || tmInfo.type == "")
                            {
                                //当类型为空时候清空操作
                                tmInfo.opt = "";
                                tmInfo.optName = "";
                            }
                            tmInfo.type = point.type;
                            dataGridView1.Rows[dex].Cells[3].Value = string.Format("{0} {1} {2} {3}", point.area1, point.area2, point.area3, point.area4).Trim();//改根据地址从信息里面获取
                            dataGridView1.Rows[dex].Cells[4].Value = point.name;
                        }

                    }
                    dataGridView1.Rows[dex].Cells[0].Value = tmInfo.id;
                    dataGridView1.Rows[dex].Cells[1].Value = IniHelper.findTypesIniNamebyType(tmInfo.type); 
                    dataGridView1.Rows[dex].Cells[2].Value = DgvMesege.addressTransform(tmInfo.address);
                    
                    dataGridView1.Rows[dex].Cells[5].Value = (tmInfo.optName + " " + tmInfo.opt).Trim();
                    dataGridView1.Rows[dex].Cells[6].Value = tmInfo.shortTime;
                    dataGridView1.Rows[dex].Cells[7].Value = "删除";


                }
                for (int i = 0; i < delTimer.Count; i++)
                {
                    tms.timersInfo.Remove(delTimer[i]);
                }

            }
            catch (Exception ex)
            {
                this.dataGridView1.Rows.Clear();
                MessageBox.Show(ex + "\r\n临时调试错误信息 后期删除屏蔽");
            }

           
           
        }

        /// <summary>
        /// 处理基本日期信息
        /// </summary>
        /// <param name="tms"></param>
        private void dealtmsDates_prior(DataJson.timers tms)
        {
            if (!string.IsNullOrEmpty(tms.dates))
            {
                if (tms.dates.Contains("/"))
                {
                    //为自定义日期
                    cbCustom.Checked = true;
                    string[] dates = tms.dates.Split(',');
                    for (int i = 0; i < dates.Length; i++)
                    {
                        listbox.Items.Add(dates[i]);
                    }
                }
                else
                {
                    //为普通周一到周五
                    string[] dates = tms.dates.Split(',');
                    if (dates.Length == 7)
                    {
                        cbEveryday.Checked = true;
                    }
                    else
                    {
                        for (int i = 0; i < dates.Length; i++)
                        {
                            switch (dates[i])
                            {
                                case "0":
                                    cbSun.Checked = true;
                                    break;
                                case "1":
                                    cbMon.Checked = true;
                                    break;
                                case "2":
                                    cbTue.Checked = true;
                                    break;
                                case "3":
                                    cbWed.Checked = true;
                                    break;
                                case "4":
                                    cbThur.Checked = true;
                                    break;
                                case "5":
                                    cbFri.Checked = true;
                                    break;
                                case "6":
                                    cbSat.Checked = true;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }

                }
            }
            
            if (!string.IsNullOrEmpty(tms.priorHoloday))
            {
                switch (tms.priorHoloday)
                {
                    case "00000001":
                        cbPriorHoliday.Checked = true;
                        rdbcheck = true;
                        break;
                    case "01000001":
                        cbPriorHoliday.Checked = false;
                        rdbcheck = false;
                        break;
                    default:
                        break;
                }
            }
            
        }

        #endregion

        #region 数据操作工具
        /// <summary>
        /// 在选中节点的基础上 按IP和定时号ID 寻找timerList表中是timers
        /// </summary>
        /// <returns></returns>
        private DataJson.timers getTimersInfoList()
        {
            if (FileMesege.timerSelectNode == null || FileMesege.timerSelectNode.Parent == null)
            {
                return null;
            }
          
            string ip = FileMesege.timerSelectNode.Parent.Text.Split(' ')[0];
            string[] timerNodetxt = FileMesege.timerSelectNode.Text.Split(' ');
            int timerNum = Convert.ToInt32(Regex.Replace(timerNodetxt[0], @"[^\d]*", ""));
            return DataListHelper.getTimersInfoList(ip, timerNum);
        }

        /// <summary>
        /// 获取某个Timers列表中对应ID号的TiInfo 否则返回空
        /// </summary>
        /// <param name="sc"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        private DataJson.timersInfo getTimerInfo(DataJson.timers tms, int id)
        {
            foreach (DataJson.timersInfo info in tms.timersInfo)
            {
                if (info.id == id)
                {
                    return info;
                }
            }
            return null;
        }

        /// <summary>
        /// 定时信息按id号排序
        /// </summary>
        /// <param name="tms"></param>
        private void timerInfoSort(DataJson.timers tms)
        {
            tms.timersInfo.Sort(delegate(DataJson.timersInfo x, DataJson.timersInfo y)
            {
                return (x.id).CompareTo(y.id);
            });
        }

        #endregion

        #region 基本日期选择处理

        private void cbMon_MouseUp(object sender, MouseEventArgs e)
        {
            clearVery_customCheck();
            datesUpdate();
        }

        private void cbTue_MouseUp(object sender, MouseEventArgs e)
        {
            clearVery_customCheck();
            datesUpdate();
        }

        private void cbWed_MouseUp(object sender, MouseEventArgs e)
        {
            clearVery_customCheck();
            datesUpdate();
        }

        private void cbThur_MouseUp(object sender, MouseEventArgs e)
        {
            clearVery_customCheck();
            datesUpdate();
        }

        private void cbFri_MouseUp(object sender, MouseEventArgs e)
        {
            clearVery_customCheck();
            datesUpdate();
        }

        private void cbSat_MouseUp(object sender, MouseEventArgs e)
        {
            clearVery_customCheck();
            datesUpdate();
        }

        private void cbSun_MouseUp(object sender, MouseEventArgs e)
        {
            clearVery_customCheck();
            datesUpdate();
        }

        private void cbEveryday_MouseUp(object sender, MouseEventArgs e)
        {
            clear1_7Check();
            cbCustom.Checked = false;
            datesUpdate();
        }

        //自定义假期
        private void cbCustom_MouseUp(object sender, MouseEventArgs e)
        {
            clear1_7Check();
            cbEveryday.Checked = false;
            datesUpdate();

        }

        bool rdbcheck = false;
        //跳过节假日
        private void cbPriorHoliday_MouseUp(object sender, MouseEventArgs e)
        {
            
            if (rdbcheck)
            {
                cbPriorHoliday.Checked = false;
                rdbcheck = false;
            }
            else
            {
                cbPriorHoliday.Checked = true;
                rdbcheck = true;
            }
            priorHolidayUpdate();
        }

     

        //清除每天和 自定义的选中
        private void clearVery_customCheck()
        {
            cbEveryday.Checked = false;
            cbCustom.Checked = false;
        }

        //清除星期一 至 星期日的选中
        private void clear1_7Check()
        {
            cbMon.Checked = false;
            cbTue.Checked = false;
            cbWed.Checked = false;
            cbThur.Checked = false;
            cbFri.Checked = false;
            cbSat.Checked = false;
            cbSun.Checked = false;
        }

        //更新timerList日期dates
        private void datesUpdate()
        {
            DataJson.timers tms = getTimersInfoList();
            if (tms == null)
            {
                return;
            }
            List<string> tmp = new List<string>();
            
            //全部选中
            if (cbEveryday.Checked)
            {
                tmp.Add("0");
                tmp.Add("1");
                tmp.Add("2");
                tmp.Add("3");
                tmp.Add("4");
                tmp.Add("5");
                tmp.Add("6");
            }
            else if (cbCustom.Checked)
            {
                //自定义日期
                for (int i = 0; i < listbox.Items.Count; i++)
                {

                    tmp.Add(listbox.Items[i].ToString());
                }
            }
            else
            {
                if (cbSun.Checked)
                {
                    tmp.Add("0");
                }
                if (cbMon.Checked)
                {
                    tmp.Add("1");
                }
                if (cbTue.Checked)
                {
                    tmp.Add("2");
                }
                if (cbWed.Checked)
                {
                    tmp.Add("3");
                }
                if (cbThur.Checked)
                {
                    tmp.Add("4");
                }
                if (cbFri.Checked)
                {
                    tmp.Add("5");
                }
                if (cbSat.Checked)
                {
                    tmp.Add("6");
                }
            }
            tms.dates = "";
            for (int i = 0; i < tmp.Count; i++)
            {
                tms.dates = string.Format("{0} {1}", tms.dates, tmp[i]);
            }
            tms.dates = tms.dates.Trim().Replace(" ", ",");
            //AppTxtShow(tms.dates);
        }

        //更新timerList是否为网关priorHoliday
        private void priorHolidayUpdate()
        {
            DataJson.timers tms = getTimersInfoList();
            if (tms == null)
            {
                return;
            }
            if (cbPriorHoliday.Checked)
            {
                tms.priorHoloday = "00000001";
            }
            else
            {
                tms.priorHoloday = "01000001";
            }
        }

        #endregion

        #region 自定义日期
        private void btnAddDay_Click(object sender, EventArgs e)
        {
            
            
            if (!cbCustom.Checked)
            {
                return;
            }
            if (FileMesege.timerSelectNode == null || FileMesege.timerSelectNode.Parent == null)
            {
                return;
            }
            timerYYHHDD tmy = new timerYYHHDD();
            tmy.AddCustomDate += new Action<string>(tmy_AddCustomDate);
            Point pt = MousePosition;
            //把窗口向屏幕中间刷新
            tmy.StartPosition = FormStartPosition.Manual;
            tmy.Left = pt.X + 10;
            tmy.Top = pt.Y + 10;
            //把窗口向屏幕中间刷新
            tmy.Show();
        }

        private void btnDelDay_Click(object sender, EventArgs e)
        {
            if (!cbCustom.Checked)
            {
                return;
            }
            if (FileMesege.timerSelectNode == null || FileMesege.timerSelectNode.Parent == null)
            {
                return ;
            }
            listbox.Items.Remove(listbox.SelectedItem);
            datesUpdate();


        }


        //添加日期回调 用哈希数表排序
        private void tmy_AddCustomDate(string date)
        {
            
            for (int i = 0; i < listbox.Items.Count; i++)
            {
                if (date == listbox.Items[i].ToString())
                {
                    //存在相同的
                    return;
                }
            }
            listbox.Items.Add(date);    
            //排序   
            lisboxSort();
            datesUpdate();
        }

        /// <summary>
        /// 排序列表日期
        /// </summary>
        private void lisboxSort()
        {
            List<string> tmp = new List<string>();
            for (int i = 0; i < listbox.Items.Count; i++)
            {
                tmp.Add(listbox.Items[i].ToString());
            }
            //排序
            tmp.Sort(delegate(string x, string y)
            {
                string[] xip = x.Split('/');
                string[] yip = y.Split('/');

                //月份为* 
                if (xip[1].Contains("*"))
                {
                    //比较项月份为星
                    if (yip[1].Contains("*"))
                    {
                        if (xip[0].Contains("*") && yip[0].Contains("*"))
                        {
                            return Convert.ToInt32(xip[2]).CompareTo(Convert.ToInt32(yip[2]));
                        }
                        if (xip[0].Contains("*") && !yip[0].Contains("*"))
                        {
                            return -1;
                        }
                        if (!xip[0].Contains("*") && yip[0].Contains("*"))
                        {
                            return 1;
                        }


                    }
                    else
                    {
                        //月份不为星
                        return -1;
                    }
                }

                //年为* 
                if (xip[0].Contains("*"))
                {
                    //比较项月份为*
                    if (yip[1].Contains("*"))
                    {
                        return 1;
                    }
                    //比较项年份为*
                    if (yip[0].Contains("*"))
                    {
                        //同月份  不同月份
                        if (xip[1] == yip[1])
                        {
                            return Convert.ToInt32(xip[2]).CompareTo(Convert.ToInt32(yip[2]));
                        }
                        else
                        {
                            return Convert.ToInt32(xip[1]).CompareTo(Convert.ToInt32(yip[1]));
                        }
                    }
                    return -1;
                }

                try
                {
                    if (xip[0] == yip[0])
                    {
                        if (xip[1] == yip[1])
                        {

                            return Convert.ToInt32(xip[2]).CompareTo(Convert.ToInt32(yip[2]));

                        }

                        return Convert.ToInt32(xip[1]).CompareTo(Convert.ToInt32(yip[1]));

                    }

                    return Convert.ToInt32(xip[0]).CompareTo(Convert.ToInt32(yip[0]));
                }
                catch
                {
                    return 1;
                }

            });
            listbox.Items.Clear();
            for (int i = 0; i < tmp.Count; i++)
            {
                listbox.Items.Add(tmp[i]);
            }
        }

        #endregion

        #region 增加 清空 下载 开启 关闭 删除选中行
        //增加
        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                DataJson.timers tms = getTimersInfoList();
                if (tms == null)
                {
                    return;
                }
                
                //新建表
                DataJson.timersInfo tmInfo = new DataJson.timersInfo();

                int id = 0;
                string type = "", opt = "", optname = "", add = "",shortTime = "";
                //撤销 
                DataJson.totalList OldList = FileMesege.cmds.getListInfos();
                HashSet<int> hasharry = new HashSet<int>();
                //寻找最大的ID值
                foreach (DataJson.timersInfo find in tms.timersInfo)
                {
                    if (find.id > id)
                    {
                        hasharry.Add(find.id);
                        type = find.type;
                        opt = find.opt;
                        optname = find.optName;
                        shortTime = find.shortTime;
                        add = find.address;
                    }
                }
                tmInfo.id = polishId(hasharry);
                tmInfo.pid = 0;
                tmInfo.type = type;

                //地址加一处理 并搜索PointList表获取地址 信息
                if (!string.IsNullOrEmpty(add) && add != "FFFFFFFF")
                {
                    switch (add.Substring(2, 2))
                    {
                        case "00":
                            //设备类地址
                            add = add.Substring(0, 6) + SocketUtil.strtohexstr((Convert.ToInt32(add.Substring(6, 2), 16) + 1).ToString());
                            break;
                        default:
                            string hexnum = SocketUtil.strtohexstr((Convert.ToInt32(add.Substring(4, 4), 16) + 1).ToString());
                            while (hexnum.Length < 4)
                            {
                                hexnum = hexnum.Insert(0, "0");
                            }
                            add = add.Substring(0, 4) + hexnum;
                            break;
                    }
                    string ip4 = SocketUtil.strtohexstr(SocketUtil.getIP(FileMesege.sceneSelectNode));//16进制
                    //添加地域和名称 在sceneInfo表中
                    DataJson.PointInfo point = DataListHelper.findPointByType_address(type, ip4 + add.Substring(2, 6));
                    if (point != null)
                    {
                        tmInfo.pid = point.pid;
                    }
                }

                tmInfo.address = add;
                tmInfo.opt = opt;
                tmInfo.optName = optname;
                tmInfo.shortTime = shortTime;
                tms.timersInfo.Add(tmInfo);
                //排序
                timerInfoSort(tms);
                DataJson.totalList NewList = FileMesege.cmds.getListInfos();
                FileMesege.cmds.DoNewCommand(NewList, OldList);
                //重新刷新
                TimerAddItem();
            }
            catch (Exception ex) { MessageBox.Show(ex + "临时调试错误信息"); }
        }

        /// <summary>
        /// 计算表中ID序号
        /// </summary>
        /// <param name="hasharry"></param>
        /// <returns></returns>
        private int polishId(HashSet<int> hasharry)
        {
            try
            {
                List<int> arry = hasharry.ToList<int>();
                arry.Sort();
                return arry[arry.Count - 1] + 1;
            }
            catch
            {
                return 1;
            }
        }

        //清空
        private void btnClear_Click(object sender, EventArgs e)
        {
            try
            {
                DataJson.timers tms = getTimersInfoList();
                if (tms == null)
                {
                    return;
                }
                //撤销 
                DataJson.totalList OldList = FileMesege.cmds.getListInfos();
                tms.timersInfo.Clear();
                DataJson.totalList NewList = FileMesege.cmds.getListInfos();
                FileMesege.cmds.DoNewCommand(NewList, OldList);
                this.dataGridView1.Rows.Clear();
            }
            catch (Exception ex) { MessageBox.Show(ex + "临时调试错误信息"); }
        }

        //下载
        private void btnDown_Click(object sender, EventArgs e)
        {
            Socket sock = null;
            try
            {
                DataJson.timers tms = getTimersInfoList();
                if (tms == null)
                {
                    return;
                }
                //场景信息不为空
                if (tms.timersInfo.Count > 0)
                {
                    DataJson.Tn tn = new DataJson.Tn();
                    tn.timer = new List<DataJson.Timernumber>();
                    if (string.IsNullOrEmpty(tms.dates))
                    {
                        //没有填写执行日期
                        AppTxtShow("载入失败,需要填写执行日期！");
                        return;
                    }
                    //把有效的对象操作 放到SN对象里面
                    foreach (DataJson.timersInfo tmInfo in tms.timersInfo)
                    {
                        //确保有信息
                        if (string.IsNullOrEmpty(tmInfo.opt) || string.IsNullOrEmpty(tmInfo.shortTime))
                        {
                            continue;
                        }
                        DataJson.Timernumber sb = new DataJson.Timernumber();

                        sb.num = tmInfo.id;
                        sb.obj = tmInfo.address;
                        sb.val = tmInfo.opt;
                        sb.hour =  Convert.ToInt32( tmInfo.shortTime.Split(':')[0]);
                        sb.min = Convert.ToInt32(tmInfo.shortTime.Split(':')[1]);

                        string[] dates = tms.dates.Split(',');
                        if (tms.dates.Contains("/"))
                        {
                            //自定义日期
                            for (int i = 0; i < dates.Length; i++)
                            {
                                string[] ymd = dates[i].Split('/');
                                if (ymd[0].Contains("*"))
                                {
                                    //年为255
                                    sb.year = 255;
                                }
                                else
                                {
                                    sb.year = Convert.ToInt32(ymd[0]);
                                }
                                if (ymd[1].Contains("*"))
                                {
                                    //月为255
                                    sb.mon = 255;
                                }
                                else
                                {
                                    sb.mon = Convert.ToInt32(ymd[1]);
                                }
                                sb.day = Convert.ToInt32(ymd[2]);
                                sb.week = 255;
                                tn.timer.Add(sb);
                            }
                        }
                        else
                        {
                            //星期一到日 0-7
                            for (int i = 0; i < dates.Length; i++)
                            {
                                sb.year = 255;
                                sb.mon = 255;
                                sb.day = 255;
                                sb.week = Convert.ToInt32(dates[i]);
                                tn.timer.Add(sb);
                            }
                        }
                        
                    }
                    if (tn.timer.Count > 0)
                    {
                        //序列化SN对象
                        string sjson = FileMesege.ConvertJsonString(JsonConvert.SerializeObject(tn));

                        //写入数据格式
                        string path = "down /json/s" + tms.id.ToString() + ".json$" + sjson;
                        //测试写出文档
                        //File.WriteAllText(FileMesege.filePath + "\\objs\\s" + sceneNum + ".json", path);
                        //string check = "exist /json/s" + sceneNum + ".json$";
                        TcpSocket ts = new TcpSocket();
                        int i = 0;

                        while (i < 10)
                        {
                            sock = ts.ConnectServer(FileMesege.timerSelectNode.Text.Split(' ')[0], 6001, 1);
                            if (sock == null)
                            {
                                i++;
                            }
                            else
                            {
                                i = 0;
                                break;
                            }
                        }

                        int flag = -1;
                        while (sock != null)
                        {
                            if (sock == null)
                            {
                                break;
                            }
                            if (i == 10)
                            {
                                break;
                            }
                            //重连
                            //0:发送数据成功；-1:超时；-2:发送数据出现错误；-3:发送数据时出现异常
                            flag = ts.SendData(sock, path, 1);
                            //flag = ts.SendData(sock, "exist /json/s" + sceneNum + ".json$", 1);
                            if (flag == 0)
                            {
                                AppTxtShow("加载成功！");
                                break;
                            }
                            i++;

                        }
                        if (sock != null)
                        {
                            sock.Close();
                        }

                        if (i == 10)
                        {
                            AppTxtShow("加载失败");
                        }

                    }//if有场景信息
                    else
                    {
                        AppTxtShow("无定时指令！");
                    }

                }
                else
                {
                    AppTxtShow("无定时指令！");
                }

            }
            catch
            {
                //Exception ex
                //TxtShow("加载失败！\r\n");
            }
        }

        private void btnOn_Click(object sender, EventArgs e)
        {
            Socket sock = null;
            //产生场景文件写进去
            if (FileMesege.timerSelectNode == null || FileMesege.timerSelectNode.Parent == null)
            {
                return;
            }
            try
            {
                string ip = FileMesege.timerSelectNode.Parent.Text.Split(' ')[0];
                string[] ids = FileMesege.timerSelectNode.Text.Split(' ');
                int sceneNum = Convert.ToInt32(Regex.Replace(ids[0], @"[^\d]*", ""));
                //发送调用指令
                string ip4 = SocketUtil.getIP(FileMesege.timerSelectNode);
                TcpSocket ts = new TcpSocket();

                sock = ts.ConnectServer(ip, 6003, 2);
                if (sock == null)
                {
                    //防止一连失败
                    sock = ts.ConnectServer(ip, 6003, 2);
                    if (sock == null)
                    {
                        AppTxtShow("连接失败！请检查网络");
                        //sock.Close();
                        return;
                    }

                }
                string number = "";
                if (sceneNum < 256)
                {
                    number = String.Format("0.{0}", sceneNum.ToString());
                }
                else
                {
                    //模除剩下的数
                    int num = sceneNum % 256;
                    //有多小个256
                    sceneNum = (sceneNum - num) / 256;
                    number = String.Format("{0}.{1}", sceneNum.ToString(), num.ToString());
                }


                string oder = String.Format("SET;00000001;{{{0}.32.{1}}};\r\n", ip4, number);  // "SET;00000001;{" + ip4 + ".16." + number + "};\r\n";
                int flag = ts.SendData(sock, oder, 2);
                if (flag == 0)
                {
                    AppTxtShow("发送指令成功！");
                    sock.Close();
                }
                else
                {
                    flag = ts.SendData(sock, oder, 2);
                    if (flag == 0)
                    {
                        AppTxtShow("发送指令成功！");
                        sock.Close();
                    }
                }
            }
            catch
            {
                //TxtShow("发送指令失败！\r\n");
            }
            
        }

        private void btnOff_Click(object sender, EventArgs e)
        {
            Socket sock = null;
            //产生场景文件写进去
            if (FileMesege.timerSelectNode == null || FileMesege.timerSelectNode.Parent == null)
            {
                return;
            }
            try
            {
                string ip = FileMesege.timerSelectNode.Parent.Text.Split(' ')[0];
                string[] ids = FileMesege.timerSelectNode.Text.Split(' ');
                int sceneNum = Convert.ToInt32(Regex.Replace(ids[0], @"[^\d]*", ""));
                //发送调用指令
                string ip4 = SocketUtil.getIP(FileMesege.timerSelectNode);
                TcpSocket ts = new TcpSocket();

                sock = ts.ConnectServer(ip, 6003, 2);
                if (sock == null)
                {
                    //防止一连失败
                    sock = ts.ConnectServer(ip, 6003, 2);
                    if (sock == null)
                    {
                        AppTxtShow("连接失败！请检查网络");
                        //sock.Close();
                        return;
                    }

                }
                string number = "";
                if (sceneNum < 256)
                {
                    number = String.Format("0.{0}", sceneNum.ToString());
                }
                else
                {
                    //模除剩下的数
                    int num = sceneNum % 256;
                    //有多小个256
                    sceneNum = (sceneNum - num) / 256;
                    number = String.Format("{0}.{1}", sceneNum.ToString(), num.ToString());
                }


                string oder = String.Format("SET;00000000;{{{0}.32.{1}}};\r\n", ip4, number);  // "SET;00000001;{" + ip4 + ".16." + number + "};\r\n";
                int flag = ts.SendData(sock, oder, 2);
                if (flag == 0)
                {
                    AppTxtShow("发送指令成功！");
                    sock.Close();
                }
                else
                {
                    flag = ts.SendData(sock, oder, 2);
                    if (flag == 0)
                    {
                        AppTxtShow("发送指令成功！");
                        sock.Close();
                    }
                }
            }
            catch
            {
                //TxtShow("发送指令失败！\r\n");
            }
        }

        //删除选中行
        private void btnDel_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                if ((bool)dataGridView1.Rows[i].Cells[8].EditedFormattedValue)
                {
                    Multiple(i);
                }
            }
            if (dataGridView1.RowCount < 1 || multipleList.Count == 0)
            {
                //没有选中数据
                return;
            }
            else
            {
                DataJson.timers tms = getTimersInfoList();
                if (tms == null)
                {
                    return;
                }
                //撤销 
                DataJson.totalList OldList = FileMesege.cmds.getListInfos();
                //获取该节点IP地址场景下的 场景信息对象
                foreach (DataJson.timersInfo info in multipleList)
                {
                    tms.timersInfo.Remove(info);
                }
                DataJson.totalList NewList = FileMesege.cmds.getListInfos();
                FileMesege.cmds.DoNewCommand(NewList, OldList);
                multipleList.Clear();
                TimerAddItem();
            }
        }


        //存储删除选中行信息点
        HashSet<DataJson.timersInfo> multipleList = new HashSet<DataJson.timersInfo>();

        //记录选中的项的timerInfo信息
        private void Multiple(int rowNumber)
        {

            if (!(bool)dataGridView1.Rows[rowNumber].Cells[8].EditedFormattedValue)
            {
                return;
            }
            DataJson.timers tms = getTimersInfoList();
            if (tms == null)
            {
                return;
            }
            //获取sceneInfo对象表中对应ID号info对象
            DataJson.timersInfo info = getTimerInfo(tms, Convert.ToInt32(dataGridView1.Rows[rowNumber].Cells[0].Value));
            if (info == null)
            {
                return;
            }
            multipleList.Add(info);
        }

        #endregion


        #region del按键 删除操作
        //del按键
        private void dataGridView1_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyData == Keys.Delete)
                {

                    DelKeyOpt();

                }
            }
            catch (Exception ex) { MessageBox.Show(ex + "临时调试错误信息"); }
        }

        //删除操作
        private void DelKeyOpt()
        {
            try
            {
                //获取当前选中单元格的列序号
                int colIndex = dataGridView1.CurrentRow.Cells.IndexOf(dataGridView1.CurrentCell);

                //当粘贴选中单元格为操作
                if (colIndex == 5)
                {

                    DataJson.timers tms = getTimersInfoList();
                    if (tms == null)
                    {
                        return;
                    }
                    //获取sceneInfo对象表中对应ID号info对象
                    DataJson.timersInfo tmInfo = getTimerInfo(tms, Convert.ToInt32(dataGridView1.CurrentRow.Cells[0].Value));
                    if (tmInfo == null)
                    {
                        return;
                    }
                    //撤销
                    DataJson.totalList OldList = FileMesege.cmds.getListInfos();
                    tmInfo.opt = "";
                    tmInfo.optName = "";
                    DataJson.totalList NewList = FileMesege.cmds.getListInfos();
                    FileMesege.cmds.DoNewCommand(NewList, OldList);
                    dataGridView1.Rows[dataGridView1.CurrentCell.RowIndex].Cells[5].Value = null;
                }//if
            }//try
            catch
            {

            }

        }
        #endregion

        #region 表格单击双击 操作 高亮显示
        private bool isFirstClick = true;
        private bool isDoubleClick = false;
        private int milliseconds = 0;
        /// <summary>
        /// 行号
        /// </summary>
        private int rowCount = 0;
        /// <summary>
        /// 列号
        /// </summary>
        private int columnCount = 0;
        private int oldrowCount = 0;
        private int oldcolumnCount = 0;

        bool isClick = false;
        //移动到删除的时候高亮一行
        private void dataGridView1_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (isClick == true)
            {
                return;

            }
            else
            {
                //选中行号
                int rowNum = e.RowIndex;
                //选中列号
                int columnNum = e.ColumnIndex;
                if (rowNum >= 0 && columnNum >= 0)
                {
                    dataGridView1.ClearSelection();
                    dataGridView1.Rows[rowNum].Selected = true;//选中行
                }
            }
        }

        private void dataGridView1_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            oldrowCount = rowCount;
            oldcolumnCount = columnCount;
            rowCount = e.RowIndex;
            columnCount = e.ColumnIndex;
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
            if (isClick == true)
            {
                isClick = false;
            }
            else
            {
                isClick = true;
            }
        }

        private void doubleClickTimer_Tick(object sender, EventArgs e)
        {
            milliseconds += 100;
            // 第二次鼠标点击超出双击事件间隔
            if (milliseconds >= SystemInformation.DoubleClickTime)
            {
                doubleClickTimer.Stop();


                if (isDoubleClick)
                {

                    if (rowCount >= 0 && columnCount >= 0)
                    {
                        int id = Convert.ToInt32(dataGridView1.Rows[rowCount].Cells[0].Value);
                        switch (dataGridView1.Columns[columnCount].Name)
                        {
                            case "address":
                                //改变地址
                                string add = "";
                                if (dataGridView1.Rows[rowCount].Cells[2].Value != null)
                                {
                                    //原地址
                                    add = dataGridView1.Rows[rowCount].Cells[2].Value.ToString();
                                }
                                string objType = dataGridView1.Rows[rowCount].Cells[1].EditedFormattedValue.ToString();
                                //赋值List 并添加地域 名字
                                dgvAddress(id, objType, add);

                                break;
                            case "operation":

                                //操作
                                string info = dgvOperation(Convert.ToInt32(dataGridView1.Rows[rowCount].Cells[0].Value), dataGridView1.Rows[rowCount].Cells[1].EditedFormattedValue.ToString());
                                if (info != null)
                                {
                                    dataGridView1.Rows[rowCount].Cells[5].Value = info;
                                }

                                break;

                            case "shortTime":
                                //改变延时
                                dataGridView1.Columns[columnCount].ReadOnly = false;
                                break;
                            default: break;
                        }
                    }
                }
                else
                {
                    //处理单击事件操作

                    if (rowCount >= 0 && columnCount >= 0)
                    {
                        //DGV的行号
                        int id = Convert.ToInt32(dataGridView1.Rows[rowCount].Cells[0].Value);
                        switch (dataGridView1.Columns[columnCount].Name)
                        {
                            case "del":
                                //删除表
                                dgvDle(id);
                                //移除该行信息
                                dataGridView1.Rows.Remove(dataGridView1.Rows[rowCount]);
                                break;
                            default: break;



                        }
                    }
                }
                isFirstClick = true;
                isDoubleClick = false;
                milliseconds = 0;
            }
        }

        private void dataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {

        }

      
        private void dataGridView1_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            //选中行号
            int rowNum = e.RowIndex;
            //选中列号
            int columnNum = e.ColumnIndex;
            if (rowNum >= 0 && columnNum >= 0)
            {
                switch (dataGridView1.Columns[columnNum].Name)
                {

                    case "checkDel":
                        dataGridView1.Rows[rowNum].Selected = true;//选中行

                        for (int i = dataGridView1.SelectedRows.Count; i > 0; i--)
                        {
                            dataGridView1.SelectedRows[i - 1].Cells[8].Value = true;

                        }
                        //提交编辑
                        dataGridView1.EndEdit();
                        break;


                    default: break;
                }
            }
        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                //选中行号
                int rowNum = e.RowIndex;
                //选中列号
                int columnNum = e.ColumnIndex;
                if (rowNum >= 0 && columnNum >= 0)
                {
                    switch (dataGridView1.Columns[columnNum].Name)
                    {
                        case "shortTime":
                            //改变延时
                            dgvShortTimer(Convert.ToInt32(dataGridView1.Rows[rowNum].Cells[0].Value), (DateTime)(dataGridView1.Rows[rowNum].Cells[6].Value));
                            dataGridView1.Columns[columnNum].ReadOnly = true;
                            break;
                        case "type":
                            //改变对象  
                            string isChange = dgvObjtype(Convert.ToInt32(dataGridView1.Rows[rowNum].Cells[0].Value), dataGridView1.Rows[rowNum].Cells[1].EditedFormattedValue.ToString());
                            if (!string.IsNullOrEmpty(isChange))
                            {
                                dataGridView1.Rows[rowNum].Cells[1].Value = IniHelper.findTypesIniNamebyType(isChange);
                            }
                            break;
                        
                        default: break;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show(ex + "临时调试错误信息"); }
        }

        /// <summary>
        /// 修改DGV表类型
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        private string dgvObjtype(int id, string type)
        {

            DataJson.timers tms = getTimersInfoList();
            if (tms == null)
            {
                return null;
            }
            //获取sceneInfo对象表中对应ID号info对象
            DataJson.timersInfo tmInfo = getTimerInfo(tms, id);
            if (tmInfo == null)
            {
                return null;
            }
            
            if (tmInfo.pid != 0)
            {
                DataJson.PointInfo point = DataListHelper.findPointByPid(tmInfo.pid);
                if (point.type != "")
                {

                    return point.type;
                }

            }
            //撤销 
            DataJson.totalList OldList = FileMesege.cmds.getListInfos();
            tmInfo.type = IniHelper.findTypesIniTypebyName(type);
            tmInfo.opt = "";
            tmInfo.optName = "";
            
            DataJson.totalList NewList = FileMesege.cmds.getListInfos();
            FileMesege.cmds.DoNewCommand(NewList, OldList);
            return null;
        }


        //ID= 选中行的序号
        private void dgvDle(int id)
        {

            DataJson.timers tms = getTimersInfoList();
            if (tms == null)
            {
                return ;
            }
            //获取sceneInfo对象表中对应ID号info对象
            DataJson.timersInfo tmInfo = getTimerInfo(tms,id);
            if (tmInfo == null)
            {
                return ;
            }
            //撤销 
            DataJson.totalList OldList = FileMesege.cmds.getListInfos();
            tms.timersInfo.Remove(tmInfo);
            DataJson.totalList NewList = FileMesege.cmds.getListInfos();
            FileMesege.cmds.DoNewCommand(NewList, OldList);
        }



        /// <summary>
        /// 获取新的地址 刷新地域 名字
        /// </summary>
        /// <param name="id"></param>
        /// <param name="objType">当前对象的类型</param>
        /// <param name="add">当前对象的地址</param>
        /// <returns></returns>
        private void dgvAddress(int id, string objType, string add)
        {
            sceneAddress dc = new sceneAddress();
            //把窗口向屏幕中间刷新
            dc.StartPosition = FormStartPosition.CenterParent;
            //把当前选仲树状图网关传递到info里面 给新建设备框网关使用  
            //dc.Obj = obj;
            dc.ObjType = objType;
            dc.Obj = add;
            dc.ShowDialog();
            if (dc.DialogResult == DialogResult.OK)
            {
                DataJson.timers tms = getTimersInfoList();
                if (tms == null)
                {
                    return;
                }
                //获取sceneInfo对象表中对应ID号info对象
                DataJson.timersInfo tmInfo = getTimerInfo(tms, id);
                if (tmInfo == null)
                {
                    return;
                }
                string ip = FileMesege.timerSelectNode.Parent.Text.Split(' ')[0];
                //地址
                tmInfo.address = dc.Obj;
                if (string.IsNullOrEmpty(tmInfo.address))
                {
                    //地址为空直接退出
                    return;
                }
                //按照地址查找type的类型 
                string type = IniHelper.findIniTypesByAddress(ip, tmInfo.address).Split(',')[0];
                if (string.IsNullOrEmpty(type))
                {
                    type = IniHelper.findTypesIniTypebyName(objType);
                }
                tmInfo.type = type;
                //获取树状图的IP第四位  + Address地址的 后六位
                string ad = SocketUtil.GetIPstyle(ip, 4) + tmInfo.address.Substring(2, 6);
                //区域加名称
                DataJson.PointInfo point = DataListHelper.findPointByType_address(type, ad);
                //撤销 
                DataJson.totalList OldList = FileMesege.cmds.getListInfos();
                if (point != null)
                {
                    tmInfo.pid = point.pid;
                    tmInfo.type = point.type;
                    if (tmInfo.type != point.type)
                    {
                        tmInfo.opt = "";
                        tmInfo.optName = "";
                    }
                }
                else
                {
                    tmInfo.pid = 0;
                    tmInfo.opt = "";
                    tmInfo.optName = "";
                }
                DataJson.totalList NewList = FileMesege.cmds.getListInfos();
                FileMesege.cmds.DoNewCommand(NewList, OldList);
            }
            TimerAddItem();

        }


        /// <summary>
        /// DGV表 操作栏
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type">类型</param>
        /// <returns></returns>
        private string dgvOperation(int id, string type)
        {

            
            sceneConcrol dc = new sceneConcrol();
            DataJson.timers tms = getTimersInfoList();
            if (tms == null)
            {
                return null;
            }
            //获取sceneInfo对象表中对应ID号info对象
            DataJson.timersInfo tmInfo = getTimerInfo(tms, id);
            if (tmInfo == null)
            {
                return null;
            }
            dc.Point = DataListHelper.findPointByPid(tmInfo.pid, FileMesege.PointList.equipment);
            //把窗口向屏幕中间刷新
            dc.StartPosition = FormStartPosition.CenterParent;
            dc.ObjType = type;

            dc.ShowDialog();
            if (dc.DialogResult == DialogResult.OK)
            {
                //撤销 
                DataJson.totalList OldList = FileMesege.cmds.getListInfos();
                tmInfo.opt = dc.Opt;
                tmInfo.optName = dc.Ver;
                DataJson.totalList NewList = FileMesege.cmds.getListInfos();
                FileMesege.cmds.DoNewCommand(NewList, OldList);
                return dc.Ver + " " + dc.Opt;
            }
            
            return null;

            
        }


        /// <summary>
        /// DGV表  定时时间
        /// </summary>
        /// <param name="id"></param>
        /// <param name="shortTime"></param>
        private void dgvShortTimer(int id, DateTime shortTime)
        { 
            DataJson.timers tms = getTimersInfoList();
            if (tms == null)
            {
                return ;
            }
            //获取sceneInfo对象表中对应ID号info对象
            DataJson.timersInfo tmInfo = getTimerInfo(tms,id);
            if (tmInfo == null)
            {
                return ;
            }
            //撤销 
            DataJson.totalList OldList = FileMesege.cmds.getListInfos();
            tmInfo.shortTime = shortTime.ToShortTimeString();
            DataJson.totalList NewList = FileMesege.cmds.getListInfos();
            FileMesege.cmds.DoNewCommand(NewList, OldList);
        }

        #endregion


        #region 复制 粘贴
        /// <summary>
        /// 复制点位的对象 与参数 
        /// </summary>
        public void copyData()
        {
            //获取当前选中单元格的列序号
            int colIndex = dataGridView1.CurrentRow.Cells.IndexOf(dataGridView1.CurrentCell);
            //当粘贴选中单元格为操作
            if (colIndex == 5 || colIndex == 6)
            {
                int id = Convert.ToInt32(dataGridView1.CurrentRow.Cells[0].Value);
                DataJson.timers tms = getTimersInfoList();
                if (tms == null)
                {
                    return;
                }
                //获取sceneInfo对象表中对应ID号info对象
                DataJson.timersInfo tmInfo = getTimerInfo(tms, id);
                if (tmInfo == null)
                {
                    return;
                }
                //获取sceneInfo对象表中对应ID号info对象
                FileMesege.copyTimer = tmInfo;

            }


        }

        /// <summary>
        /// 粘贴点位的对象与参数
        /// </summary>
        public void pasteData()
        {
            
            try
            {
                bool ischange = false;
                //撤销
                DataJson.totalList OldList = FileMesege.cmds.getListInfos();
                DataJson.timers tms = getTimersInfoList();
                if (tms == null)
                {
                    return;
                }
                for (int i = 0; i < dataGridView1.SelectedCells.Count; i++)
                {
                    int colIndex = dataGridView1.SelectedCells[i].ColumnIndex;
                    int id = Convert.ToInt32(dataGridView1.Rows[dataGridView1.SelectedCells[i].RowIndex].Cells[0].Value);
                    DataJson.timersInfo tmInfo = getTimerInfo(tms, id);
                    if (tmInfo == null)
                    {
                        continue;
                    }

                    if (FileMesege.copyTimer.type == "" || tmInfo.type == "" || tmInfo.type != FileMesege.copyTimer.type)
                    {
                        continue;
                    }
                    if (colIndex == 5)
                    {
                       
                        ischange = true;
                        tmInfo.opt = FileMesege.copyTimer.opt;
                        tmInfo.optName = FileMesege.copyTimer.optName;

                        dataGridView1.Rows[dataGridView1.SelectedCells[i].RowIndex].Cells[5].Value = (tmInfo.optName + " " + tmInfo.opt).Trim();
                    }//if
                    else if (colIndex == 6)
                    {
                        ischange = true;
                        tmInfo.shortTime = FileMesege.copyTimer.shortTime;

                        dataGridView1.Rows[dataGridView1.SelectedCells[i].RowIndex].Cells[6].Value = tmInfo.shortTime;
                    }
                }
                if (ischange)
                {
                    DataJson.totalList NewList = FileMesege.cmds.getListInfos();
                    FileMesege.cmds.DoNewCommand(NewList, OldList);
                }

            }//try
            catch
            {

            }


        }
        #endregion



    }
}
