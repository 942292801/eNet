﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace eNet编辑器
{
    /// <summary>
    /// 数据操作工具
    /// </summary>
    class DataChange
    {

        //判断是否为十六进制
        public static bool IsHexadecimal(string str)
        {
            Regex reg = new Regex(@"^[0-9A-Fa-f]+$");
            return reg.IsMatch(str);
        }

        /// <summary>
        /// 十进制字符串转 16进制byte
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static byte[] strToToHexByte(string hexString)
        {

            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += "";
            //hexString = hexString.Insert(hexString.Length, "0");

            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }

        /// <summary>
        /// byte转十六进制字符串
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="l"></param>
        /// <returns></returns>
        public static string byteToHexStr(byte[] bytes, int l)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < l; i++)
                {
                    returnStr += bytes[i].ToString("X2");
                }
            }
            return returnStr;
        }

        /// <summary>
        /// 十进制字符串转十六进制字符串
        /// </summary>
        /// <param name="s"></param>
        /// <param name="encode"></param>
        /// <returns></returns>
        public static string StringToHexString(string s, Encoding encode)
        {
            s = s.Replace(" ", "");
            byte[] b = encode.GetBytes(s);//按照指定编码将string编程字节数组
            string result = string.Empty;
            for (int i = 0; i < b.Length; i++)//逐字节变为16进制字符，以%隔开
            {
                result += " " + Convert.ToString(b[i], 16);
            }
            return result;
        }

        //十进制字符串转十六进制字符串
        public static string HexStringToString(string hexStr)
        {
            return Convert.ToInt64(hexStr, 16).ToString();
        }

        /// <summary>
        /// 十六进制字符串转二进制字符串
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static string HexString2BinString(string hexString)
        {
            try
            {
                string result = string.Empty;
                foreach (char c in hexString)
                {
                    int v = Convert.ToInt32(c.ToString(), 16);
                    int v2 = int.Parse(Convert.ToString(v, 2));
                    // 去掉格式串中的空格，即可去掉每个4位二进制数之间的空格，
                    result += string.Format("{0:d4} ", v2);
                }
                return result;
            }
            catch
            {
                return null;
            }
        }


        /// <summary>
        /// 截取二进制位数  binval为二进制数值即地址 inset为0 / 0-1 / 位置数
        /// </summary>
        /// <param name="binval"></param>
        /// <param name="inset">位置数</param>
        /// <returns>返回十进制值</returns>
        public static string getBinBit(string binval, string inset)
        {
            try
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
                return Convert.ToInt64(DataChange.Reversal(bin), 2).ToString();
            }
            catch
            {
                return "0";
            }
        }

        /// <summary>
        /// 字符串 反转排序  123 变为 321
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Reversal(string input)
        {
            string result = "";
            for (int i = input.Length - 1; i >= 0; i--)
            {
                result += input[i];
            }
            return result;
        }

        /// <summary>
        /// 根据位数替换数据
        /// </summary>
        /// <param name="content">16进制数据格式00 00 00 00</param>
        /// <param name="val">10进制替换数据</param>
        /// <param name="bitSite">替换的位置</param>
        /// <returns></returns>
        public static string replaceStr(string content, string val, string bitSite)
        {
            //十进制转换为十六进制字符串
            val = Convert.ToInt32(val).ToString("X");
            //二进制内容
            string contentBin = DataChange.HexString2BinString(content.Replace(" ", "")).Replace(" ", "");
            string valBin = DataChange.HexString2BinString(val.Replace(" ", "")).Replace(" ", "");
            //反转二进制数据
            contentBin = DataChange.Reversal(contentBin);
            valBin = DataChange.Reversal(valBin);
            if (bitSite.Contains("-"))
            {
                string[] infos = bitSite.Split('-');
                int end = Convert.ToInt32(infos[1]);
                int start = Convert.ToInt32(infos[0]);
                //移除需要替换的字符
                contentBin = contentBin.Remove(start, end - start + 1);
                //补齐32bit数据
                int j = 32 - valBin.Length - contentBin.Length;
                for (int i = j; i > 0; i--)
                {
                    valBin=  valBin + "0";
                }
                //插入新字符
                contentBin = contentBin.Insert(start, valBin);


            }
            else
            {
                //移除需要替换的字符
                contentBin = contentBin.Remove(Convert.ToInt32(bitSite), 1);
                //插入新字符
                contentBin = contentBin.Insert(Convert.ToInt32(bitSite), valBin);

            }
            //再次反转二进制数值 并转换为十进制
            return Convert.ToInt32(DataChange.Reversal(contentBin), 2).ToString("X8");

        }

        
        /// <summary>
        /// 产生随机9位数  PID唯一数字
        /// </summary>
        /// <returns></returns>
        public static int randomNum()
        {
            Random r = null;
            DataJson.PointInfo point = null;
            int num = 0;
            bool isExit = true;
            while (isExit)
            {
                ToolsUtil.DelayMilli(1);
                r = new Random(int.Parse(DateTime.Now.ToString("HHmmssfff")));
                num = r.Next(100000000, 999999999);//随机生成一个9位整数
                point = DataListHelper.findPointByPid(num);
                if (point == null)
                {
                    break;
                }

            }
            

            return num;
           
           
        }

        /// <summary>
        /// 计算表中ID序号 (用于dgv添加按钮 增加补缺ID)
        /// </summary>
        /// <param name="hasharry"></param>
        /// <returns></returns>
        public static int polishId(HashSet<int> hasharry)
        {
            try
            {
                /*
                List<int> arry = hasharry.ToList<int>();
                arry.Sort();
                return arry[arry.Count - 1] + 1;*/
                List<int> arry = hasharry.ToList<int>();
                arry.Sort();

                if (arry.Count == 0)
                {
                    //该区域节点前面数字不存在
                    return 1;
                }
                //哈希表 不存在序号 直接返回
                for (int i = 0; i < arry.Count; i++)
                {
                    if (arry[i] != i + 1)
                    {
                        return i + 1;
                    }
                }
                return arry[arry.Count - 1] + 1;
            }
            catch
            {
                return 1;
            }
        }


        /// <summary>
        /// cb信息内容的判断1-9 或 1,2,3  或数字（链路类型）
        /// </summary>
        /// <param name="cb"></param>
        /// <param name="info"></param>
        public static void dealInfoNum(ComboBox cb, string info)
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


    }
}
