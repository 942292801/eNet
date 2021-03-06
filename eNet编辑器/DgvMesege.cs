﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;

namespace eNet编辑器
{
    class DgvMesege
    {
       

        /// <summary>
        /// 地址转换  FFFFFFFF转换为255.255.255.255
        /// </summary>
        /// <returns></returns>
        public static string addressTransform(string address,string masterIP)
        {
            if (string.IsNullOrEmpty(address) )
            {
                return "";
            }

            string ip = Convert.ToInt32(address.Substring(0, 2),16).ToString();
            if (ip == "254" && !string.IsNullOrEmpty(masterIP))
            {
                try
                {
                    //如果等于本机IP
                    ip = masterIP.Split('.')[3];
                }
                catch
                {

                }
            }
            string link = Convert.ToInt32(address.Substring(2, 2), 16).ToString();
            string ID = Convert.ToInt32(address.Substring(4, 2), 16).ToString();
            string Port = Convert.ToInt32(address.Substring(6, 2), 16).ToString();
        
            return string.Format("{0}.{1}.{2}.{3}",ip,link,ID,Port);
        }


        /*public static string addressTransform(string address)
        {
            if (string.IsNullOrEmpty(address))
            {
                return "";
            }

            string ip = Convert.ToInt32(address.Substring(0, 2), 16).ToString();

            string link = Convert.ToInt32(address.Substring(2, 2), 16).ToString();
            string ID = Convert.ToInt32(address.Substring(4, 2), 16).ToString();
            string Port = Convert.ToInt32(address.Substring(6, 2), 16).ToString();

            return string.Format("{0}.{1}.{2}.{3}", ip, link, ID, Port);
        }*/



        /// <summary>
        /// 地址加j  例如 230.0.5.6 + 2  = 230.0.5.8
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static string addressAdd(string address,int j)
        {
            if (string.IsNullOrEmpty(address) || address == "FFFFFFFF" || address.Length != 8)//测试临时屏蔽 || address.Substring(0, 2) != "FE")
            {
                return string.Empty;
            }
            else
            {
                string addressType = address.Substring(2, 2);
                if (addressType == "00" || addressType == "F9" || addressType == "FA" || addressType == "FB" || addressType == "F8")
                {
                    if (addressType == "FB")
                    {
                        if (address.Substring(6, 2) == "02" || address.Substring(6, 2) == "03")
                        {

                            //日期 和时间 
                            return address;
                        }
                    }
                   
                    address = address.Substring(0, 6) + ToolsUtil.strtohexstr((Convert.ToInt32(address.Substring(6, 2), 16) + j).ToString());
                
                }
                else if (addressType == "10" || addressType == "20" || addressType == "30" || addressType == "40" || addressType == "F9")
                {
                    string hexnum = ToolsUtil.strtohexstr((Convert.ToInt32(address.Substring(4, 4), 16) + j).ToString());
                    while (hexnum.Length < 4)
                    {
                        hexnum = hexnum.Insert(0, "0");
                    }
                    address = address.Substring(0, 4) + hexnum;
                }
              
                return address;
      
            }
           
        }

        /// <summary>
        /// 地址减j  例如 230.0.5.6 + 2  = 230.0.5.8
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static string addressReduce(string address, int j)
        {
            if (string.IsNullOrEmpty(address) || address == "FFFFFFFF" || address.Length != 8 )//测试临时屏蔽 || address.Substring(0, 2) != "FE")
            {
                return string.Empty;
            }
            else
            {
                string addressType = address.Substring(2, 2);
                if (addressType == "00" || addressType == "F9" || addressType == "FA" || addressType == "FB" || addressType == "F8")
                {
                    if (addressType == "FB")
                    {
                        if (address.Substring(6, 2) == "02" || address.Substring(6, 2) == "03")
                        {

                            //日期 和时间 
                            return address;
                        }
                    }
                    int num = Convert.ToInt32(address.Substring(6, 2), 16) - j;
                    if(num > 0)
                    {
                        address = address.Substring(0, 6) + ToolsUtil.strtohexstr(num.ToString());
                    }
                    else
                    {
                        return string.Empty;
                    }
  
                }
                else if (addressType == "10" || addressType == "20" || addressType == "30" || addressType == "40" || addressType == "F9")
                {
                    int num = Convert.ToInt32(address.Substring(4, 4), 16) - j;
                    if (num > 0)
                    {
                        string hexnum = ToolsUtil.strtohexstr(num.ToString());
                        while (hexnum.Length < 4)
                        {
                            hexnum = hexnum.Insert(0, "0");
                        }
                        address = address.Substring(0, 4) + hexnum;
                    }
                    else
                    {
                        return string.Empty;
                    }
                   
                    
                }
                return address;

            }

        }

        /// <summary>
        /// 按键地址递增
        /// </summary>
        /// <param name="address"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        public static string KeyAddressAdd(string address, int j)
        {
            if (string.IsNullOrEmpty(address) || address == "FFFFFFFF" || address.Length != 8 )
            {
                return string.Empty;
            }
            else
            {
          
                string hexnum = ToolsUtil.strtohexstr((Convert.ToInt32(address.Substring(4, 4), 16) + j).ToString());
                while (hexnum.Length < 4)
                {
                    hexnum = hexnum.Insert(0, "0");
                }
                address = address.Substring(0, 4) + hexnum;

                return address;

            }
        }

        /// <summary>
        /// 按键地址递减
        /// </summary>
        /// <param name="address"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        public static string KeyAddressReduce(string address, int j)
        {
            if (string.IsNullOrEmpty(address) || address == "FFFFFFFF" || address.Length != 8 )
            {
                return string.Empty;
            }
            else
            {

                int num = Convert.ToInt32(address.Substring(4, 4), 16) - j;
                if (num > 0)
                {
                    string hexnum = ToolsUtil.strtohexstr(num.ToString());
                    while (hexnum.Length < 4)
                    {
                        hexnum = hexnum.Insert(0, "0");
                    }
                    address = address.Substring(0, 4) + hexnum;
                }
                else
                {
                    return string.Empty;
                }
                return address;

            }

        }

        /// <summary>
        /// 实现复制功能，将DataGridView中选定单元格的值复制到剪贴板中
        /// </summary>
        /// <param name="dgv_Test"></param>
        public static void CopyData(DataGridView dgv_Test)
        {
            Clipboard.SetDataObject(dgv_Test.GetClipboardContent());
        }

        /// <summary>
        /// 实现粘贴功能，将剪贴板中的内容粘贴到DataGridView中
        /// </summary>
        /// <param name="dgv_Test"></param>  
        public static void PasteData(DataGridView dgv_Test)
        {
            try
            {
                string clipboardText = Clipboard.GetText(); //获取剪贴板中的内容
                if (string.IsNullOrEmpty(clipboardText))
                {
                    return;
                }
                int colnum = 0;
                int rownum = 0;
                for (int i = 0; i < clipboardText.Length; i++)
                {
                    if (clipboardText.Substring(i, 1) == "\t")
                    {
                        colnum++;
                    }
                    if (clipboardText.Substring(i, 1) == "\n")
                    {
                        rownum++;
                    }
                }
                //粘贴板上的数据来源于EXCEL时，每行末尾都有\n，来源于DataGridView是，最后一行末尾没有\n
                if (clipboardText.Substring(clipboardText.Length - 1, 1) == "\n")
                {
                    rownum--;
                }
                colnum = colnum / (rownum + 1);
                object[,] data; //定义object类型的二维数组
                data = new object[rownum + 1, colnum + 1];  //根据剪贴板的行列数实例化数组
                string rowStr = "";
                //对数组各元素赋值
                for (int i = 0; i <= rownum; i++)
                {
                    for (int j = 0; j <= colnum; j++)
                    {
                        //一行中的其它列
                        if (j != colnum)
                        {
                            rowStr = clipboardText.Substring(0, clipboardText.IndexOf("\t"));
                            clipboardText = clipboardText.Substring(clipboardText.IndexOf("\t") + 1);
                        }
                        //一行中的最后一列
                        if (j == colnum && clipboardText.IndexOf("\r") != -1)
                        {
                            rowStr = clipboardText.Substring(0, clipboardText.IndexOf("\r"));
                        }
                        //最后一行的最后一列
                        if (j == colnum && clipboardText.IndexOf("\r") == -1)
                        {
                            rowStr = clipboardText.Substring(0);
                        }
                        data[i, j] = rowStr;
                    }
                    //截取下一行及以后的数据
                    clipboardText = clipboardText.Substring(clipboardText.IndexOf("\n") + 1);
                }
                //获取当前选中单元格的列序号
                int colIndex = dgv_Test.CurrentRow.Cells.IndexOf(dgv_Test.CurrentCell);
                //获取当前选中单元格的行序号
                int rowIndex = dgv_Test.CurrentRow.Index;
                for (int i = 0; i <= rownum; i++)
                {
                    for (int j = 0; j <= colnum; j++)
                    {
                        dgv_Test.Rows[i + rowIndex].Cells[j + colIndex].Value = data[i, j];
                    }
                }
            }
            catch
            {
                MessageBox.Show("粘贴区域大小不一致");
                return;
            }
        }


        #region DGV选中最后一行
        public static void selectLastCount(DataGridView dataGridView)
        {
            if (dataGridView.Rows.Count > 0)
            {
                dataGridView.CurrentCell = dataGridView.Rows[dataGridView.Rows.Count - 1].Cells[0];
            }
        }

        #endregion


        #region  判断DGV是否点击空白

        /// <summary>
        /// 点击空白处取消dataGridView1的选中  y为e.Y坐标
        /// </summary>
        /// <param name="dataGridView1"></param>
        /// <param name="y"></param>
        public static bool endDataViewCurrent(DataGridView dataGridView1,int y,int x)
        {
            if (GetRowIndexAt(dataGridView1,y,x) == -1)
            {
                dataGridView1.CurrentCell = null;
                return true;
            }
            return false;
        }
        public static int GetRowIndexAt(DataGridView dataGridView1, int mouseLocation_Y, int mouseLocation_X)
        {
            if (dataGridView1.FirstDisplayedScrollingRowIndex < 0)
            {
                return -1;
            }
            if (dataGridView1.ColumnHeadersVisible == true && mouseLocation_Y <= dataGridView1.ColumnHeadersHeight)
            {
                return -1;
            }
            int index = dataGridView1.FirstDisplayedScrollingRowIndex;
            int displayedCount = dataGridView1.DisplayedRowCount(true);
            for (int k = 1; k <= displayedCount; )
            {
                if (dataGridView1.Rows[index].Visible == true)
                {
                    Rectangle rect = dataGridView1.GetRowDisplayRectangle(index, true);  // 取该区域的显示部分区域   
                    if (rect.Top <= mouseLocation_Y && mouseLocation_Y < rect.Bottom)
                    {
                        int width = 1;
                        for (int i = 0; i < dataGridView1.ColumnCount; i++)
                        {
                            width += dataGridView1.Columns[i].Width;
                        }
                        if (width> mouseLocation_X && mouseLocation_X > 1)
                        {
                            return index;
                        }
                        
                    }
                    k++;
                }
                index++;
            }
            return -1;
        }
        #endregion


        #region DGV表滑动条恢复 并选中原来选中格
        /// <summary>
        /// 横坐标距离 X_Value, 垂直标距离 Y_Value, 行号 rowCount, 列号 columnCount
        /// </summary>
        /// <param name="dataGridView"></param>
        /// <param name="X_Value"></param>
        /// <param name="Y_Value"></param>
        /// <param name="rowCount"></param>
        /// <param name="columnCount"></param>
        public static void RecoverDgvForm(DataGridView dataGridView, int X_Value, int Y_Value, int rowCount, int columnCount)
        {
            try
            {
                if (dataGridView.Rows.Count < 1)
                {
                    return;
                }
                dataGridView.FirstDisplayedScrollingRowIndex = Y_Value;//设置垂直滚动条位置
                dataGridView.HorizontalScrollingOffset = X_Value;
                dataGridView.CurrentCell = dataGridView.Rows[rowCount].Cells[columnCount];//设置单元格焦点
            }catch{//(Exception ex){
                //throw ex;
                ///MessageBox.Show(ex.ToString());
            }
        }

        #endregion


    }//class
}
