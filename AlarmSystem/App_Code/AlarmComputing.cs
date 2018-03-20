using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Data.SqlClient;
using DataAccess;

namespace AlarmDataProcess
{
    public class AlarmComputing
    {
        DataOperation dbo = new DataOperation();
        //private DateTime dt = DateTime.Now;
        static DataSet dsEvent = new DataSet(); //单一预警事件数据集
        static DataSet dsAlarmData = new DataSet(); //参数集
        static DataSet dsFusionEvent = new DataSet(); //综合预警事件数据集
        static DataSet dsPlat = new DataSet(); //排土场平台信息，最多有4个分组

        public int deadNum; //死亡人数，由主界面传入
        public decimal LostValue;    //财产损失（万元），由主界面传入

        public void getOneEventInfo()
        {
            //string tmpId = "01";  // 企业编号;
            //SqlParameter[] para = new SqlParameter[] { new SqlParameter("@t1", tmpId) };
            //dsEvent = dbo.GetDataSetWithParams("Alarm_SelectOneAlarmInfoById", para);
            dsEvent = dbo.GetDataSetWithOutParams("Alarm_SelectOneAlarmInfo");
        }


        //获得最新综合预警事件信息
        //Created by zxm on May 28,2017
        public void getFusionEventInfo()
        {
            dsEvent = dbo.GetDataSetWithOutParams("Alarm_SelectLatestFusionAlarmData");
        }



        //获得预警指标限值
        public void getAlarmSetData()
        {
            //string tmpId = "01";  // 企业编号;
            //SqlParameter[] para = new SqlParameter[] { new SqlParameter("@t1", tmpId) };
            //dsAlarmData = dbo.GetDataSetWithParams("Alarm_SelectAlarmSetData", para);
            dsAlarmData = dbo.GetDataSetWithOutParams("Alarm_SelectAlarmSetData");
        }


        //获得预警指标限值
        public void getPlatInfo()
        {
            dsPlat = dbo.GetDataSetWithOutParams("Ptc_SelectAllPlatInfo");
        }


        //20170528 by zxm
        //获得最新所有传感器数据，并计算综合预警指标。
        //20171006修改，返回预警等级和结果，以便短信和邮件告警
        //private void compFusionAlarm(SqlConnection dboConn)
        //public void compFusionAlarm()
        public string compFusionAlarm()        
        {
            string strNull = "";    //返回空串，表示未正常执行

            //预警包括2步计算：主指标10个取关联度最大值，然后按3个修正指标进阶。
            //1 先按照主指标进行计算
            //定义数组
            //各指标对应报警的界限,5表示5种预警，10表示10个主指标
            //经典域
            int[,] JA = new int[5, 10];
            int[,] JB = new int[5, 10];
            //两者差值
            decimal[,] JBA = new decimal[5, 10];
            //
            decimal[,] RV = new decimal[5, 10];
            //单一指标关联度
            decimal[,] KF = new decimal[5, 10];
            //带权重的关联度
            decimal[] KW = new decimal[5];
            //最大关联度
            decimal maxKW;

            //节域
            int[] PA = new int[10];
            int[] PB = new int[10];
            //单一关联度
            decimal[] RP = new decimal[10];

            //权重
            decimal[] W = new decimal[10];

            //4组数据，每组10个
            decimal[,] V = new decimal[11, 4];
            //初始化为0
            for (int i = 0; i < 11;i++)
            {
                for(int j=0;j<4;j++)
                {
                    V[i, j] = 0;
                }
            }
            ////对KF初始化为0
            //for(int i=0;i<5;i++)
            //{
            //    for(int j=0;j<10;j++)
            //    {
            //        KF[i, j] = 0;
            //    }
            //}



            //应该与已有事件记录中的日期时间做比较，如果重复了，则不检测
            bool mark = false;   //false表示不重复。
            string sensorTime = "";
            decimal[] XZLimit = new decimal[3];

            getFusionEventInfo();   //获取已有最新综合预警事件
            getAlarmSetData();      //获取预警限值
            getPlatInfo();           //获取排土场平台数据
            if (dsAlarmData.Tables[0].Rows.Count <= 0)
            {
                return strNull;
            }
            if (dsPlat.Tables[0].Rows.Count <= 0)
            {
                return strNull;
            }


            //****************************************************************************************
            //20170528端午节
            //下面，重点是要分组，对排土场4个平台进行分别计算。
            //其中：4个组内必定有内部位移、孔隙水压力和土压力，每组中的3个内部位移值需要计算其平均值；
            //公用且只有1个测量点的是：降雨量、温度、湿度；
            //公用的还有：土壤含水率，4个值要计算其平均值；
            //瓮福排土场还有GPS要分清：4个组的GPS数量分别是2、3、1、1，各组的gps要计算其平均值。
            //
            //分组涉及到数据表有：
            //1 排土场平台数据表PlatInfoTable,用于得到平台信息；
            //2-4 传感器信息表SensorInfo，传感器类型表SensorType,传感器组表SensorGroup，用于得到各组内的传感器编号和组信息；
            //5-12,采集的实时数据表8个
            //****************************************************************************************


            //获得预警参数信息
            //string corpId = "01";
            //SqlParameter[] para = new SqlParameter[] { new SqlParameter("@t1", corpId) };
            DataSet dsTmp = dbo.GetDataSetWithOutParams("Alarm_SelectAlarmSetData");
            int DataNum = dsTmp.Tables[0].Rows.Count;
            if (DataNum > 0)
            {
                //先获得10个主指标的预警限值
                for (int i = 0; i < 10; i++)
                {
                    //主指标权重
                    W[i] = Convert.ToDecimal(dsTmp.Tables[0].Rows[i][6].ToString());

                    //求经典域
                    if ((i == 3) || (i == 4) || (i == 6))  //物料粘聚力,物料内摩擦角,平台宽度
                    {
                        JA[0, i] = 0;
                        JB[0, i] = Convert.ToInt32(dsTmp.Tables[0].Rows[i][2].ToString());
                        JA[1, i] = Convert.ToInt32(dsTmp.Tables[0].Rows[i][2].ToString());
                        JB[1, i] = Convert.ToInt32(dsTmp.Tables[0].Rows[i][3].ToString());
                        JA[2, i] = Convert.ToInt32(dsTmp.Tables[0].Rows[i][3].ToString());
                        JB[2, i] = Convert.ToInt32(dsTmp.Tables[0].Rows[i][4].ToString());
                        JA[3, i] = Convert.ToInt32(dsTmp.Tables[0].Rows[i][4].ToString());
                        JB[3, i] = Convert.ToInt32(dsTmp.Tables[0].Rows[i][5].ToString());
                        JA[4, i] = Convert.ToInt32(dsTmp.Tables[0].Rows[i][5].ToString());
                        JB[4, i] = JA[4, i] * 5;

                        //求节域
                        PA[i] = 0;
                        PB[i] = JB[4, i];
                    }
                    else
                    {
                        //求经典域
                        JA[0, i] = Convert.ToInt32(dsTmp.Tables[0].Rows[i][2].ToString());
                        JB[0, i] = JA[0, i] * 5;
                        JA[1, i] = Convert.ToInt32(dsTmp.Tables[0].Rows[i][3].ToString());
                        JB[1, i] = Convert.ToInt32(dsTmp.Tables[0].Rows[i][2].ToString());
                        JA[2, i] = Convert.ToInt32(dsTmp.Tables[0].Rows[i][4].ToString());
                        JB[2, i] = Convert.ToInt32(dsTmp.Tables[0].Rows[i][3].ToString());
                        JA[3, i] = Convert.ToInt32(dsTmp.Tables[0].Rows[i][5].ToString());
                        JB[3, i] = Convert.ToInt32(dsTmp.Tables[0].Rows[i][4].ToString());
                        JA[4, i] = 0;
                        JB[4, i] = Convert.ToInt32(dsTmp.Tables[0].Rows[i][5].ToString());

                        //求节域
                        PA[i] = 0;
                        PB[i] = JB[0, i];
                    }
                }

                //获取土壤含水量、伤亡和财产损失的阈值
                XZLimit[0] = Convert.ToDecimal(dsTmp.Tables[0].Rows[10][2].ToString());
                XZLimit[1] = Convert.ToDecimal(dsTmp.Tables[0].Rows[11][2].ToString());
                XZLimit[2] = Convert.ToDecimal(dsTmp.Tables[0].Rows[11][3].ToString());

            }


            DataSet dsSensor = new DataSet();
            DataSet dsGroup = new DataSet();
            string s1;  //传感器编号
            string s2;  //监测时刻
            double d1, d2, d3, d4;
            double dataSum; //某个组内的实际采集数据的平均值;
            int dataNm; //某个组的实际采集有数据的传感器数量，不是组内传感器数量。
            string[] tmpStr1;
            string[] tmpStr2;
            char[] tmpChar1 = { ';' };
            char[] tmpChar2 = { ',' };


            //2.1 地表位移，需要对x,y,h计算矢量
            //string tmpName = "gps";
            string tmpName = "地表位移";//20170711晚上修改

            SqlParameter[] para = new SqlParameter[] { new SqlParameter("@t1", tmpName) };
            dsGroup = dbo.GetDataSetWithParams("Sensor_SelectSensorGroupOrderByName", para);
            int groupNum = dsGroup.Tables[0].Rows.Count;
            if (groupNum <= 0)
            {
                return strNull;
            }

            tmpStr1 = (dsGroup.Tables[0].Rows[0][0].ToString()).Split(tmpChar1);
            dsSensor = dbo.GetDataSetWithOutParams("Ptc_SelectLatestMpData");
            int tmpNum = dsSensor.Tables[0].Rows.Count;
            if (tmpNum <= 0)
            {
                //Response.Write("<script>window.alert('缺少地表位移数据！')</script>");
                return strNull;
            }

            //检查地表位移分组，最多4组，最小1个
            for (int k = 0; k < tmpStr1.Length; k++)
            {
                dataSum = 0;    //清空
                dataNm = 0;
                tmpStr2 = tmpStr1[k].Split(tmpChar2);
                if (tmpStr2.Length > 0)
                {
                    for (int i = 0; i < tmpStr2.Length; i++)
                    {
                        for (int j = 0; j < tmpNum; j++)
                        {
                            s1 = dsSensor.Tables[0].Rows[j][0].ToString();  //编号
                            s2 = dsSensor.Tables[0].Rows[j][1].ToString();  //时间
                            d1 = Convert.ToDouble(dsSensor.Tables[0].Rows[j][2].ToString());    //x
                            d2 = Convert.ToDouble(dsSensor.Tables[0].Rows[j][3].ToString());    //y
                            d3 = Convert.ToDouble(dsSensor.Tables[0].Rows[j][4].ToString());    //h
                            d4 = Math.Sqrt(Math.Pow(d1, 2) + Math.Pow(d2, 2) + Math.Pow(d3,2));    //矢量值

                            //乘以1000，将数据单位调整为mm
                            d4 = d4 * 1000;
                            d4 = Math.Round(d4, 3); //保留3位小数

                            if (s1 == tmpStr2[i])
                            {
                                dataSum += d4;
                                dataNm +=1;
                            }


                            sensorTime = s2; 

                            //判别是否已计算过综合预警事件
                            //应该与已有事件记录中的日期时间做比较，如果重复了，则不检测
                            if (mark ==false)
                            {
                                s2 = DateTime.Parse(s2).ToString("yyyy-MM-dd HH:mm");
                                string eventGroupId, eventTime;
                                int tmpNum2 = dsEvent.Tables[0].Rows.Count;
                                if (tmpNum2 > 0)
                                {
                                    for (int m = 0; m < tmpNum2; m++)
                                    {
                                        eventGroupId = dsEvent.Tables[0].Rows[m][0].ToString();
                                        eventTime = dsEvent.Tables[0].Rows[m][1].ToString();
                                        eventTime = DateTime.Parse(eventTime).ToString("yyyy-MM-dd HH:mm");
                                        if (s2 == eventTime)
                                        {
                                            mark = true;
                                            break;
                                        }
                                    }
                                }
                            }

                        }
                    }

                    if (dataNm > 0)
                    {
                        dataSum = dataSum / dataNm;  //求得平均值
                        V[0, k] = Convert.ToDecimal(dataSum);
                    }
                }
            }
            
            //如果只有一个GPS，则共用
            if(tmpStr1.Length ==1)
            {
                V[0, 1] = V[0, 0];
                V[0, 2] = V[0, 0];
                V[0, 3] = V[0, 0];
            }

     

            //判断是否已有预警过。如果已经有数据，则不再往下计算，直接返回。
            //采用mark变量测试
            if(mark ==true)
            {
                return strNull;
            }



            //2.2 内部位移，需要对a,b方向计算矢量
            tmpName = "内部位移";
            para = new SqlParameter[] { new SqlParameter("@t1", tmpName) };
            dsGroup = dbo.GetDataSetWithParams("Sensor_SelectSensorGroupOrderByName", para);
            groupNum = dsGroup.Tables[0].Rows.Count;
            if (groupNum <= 0)
            {
                return strNull;
            }

            tmpStr1 = (dsGroup.Tables[0].Rows[0][0].ToString()).Split(tmpChar1);
            dsSensor = dbo.GetDataSetWithOutParams("Ptc_SelectLatestDmData");
            tmpNum = dsSensor.Tables[0].Rows.Count;
            if (tmpNum <= 0)
            {
                return strNull;
            }

            //检查4个内部位移分组，保持不变
            for (int k = 0; k < tmpStr1.Length; k++)
            {
                dataSum = 0;    //清空
                dataNm = 0;
                tmpStr2 = tmpStr1[k].Split(tmpChar2);
                if (tmpStr2.Length > 0)
                {
                    for (int i = 0; i < tmpStr2.Length; i++)
                    {
                        for (int j = 0; j < tmpNum; j++)
                        {
                            s1 = dsSensor.Tables[0].Rows[j][0].ToString();
                            s2 = dsSensor.Tables[0].Rows[j][1].ToString();
                            d1 = Convert.ToDouble(dsSensor.Tables[0].Rows[j][2].ToString());    //a
                            d2 = Convert.ToDouble(dsSensor.Tables[0].Rows[j][3].ToString());    //b
                            d3 = Math.Sqrt(Math.Pow(d1, 2) + Math.Pow(d2, 2));    //矢量值

                            //20170528，贵州反馈结果是：采集值单位就是mm，不需要转换。
                            ////乘以1000，将数据单位调整为mm
                            //d3 = d3 * 1000;
                            d3 = Math.Round(d3, 3); //保留3位小数

                            if (s1 == tmpStr2[i])
                            {
                                //sData.Time = s2;
                                //sData.Value = d3;
                                //break;
                                dataSum += d3;
                                dataNm += 1;
                            }
                        }
                    }

                    if (dataNm > 0)
                    {
                        dataSum = dataSum / dataNm;  //求得平均值
                        V[1, k] = Convert.ToDecimal(dataSum);
                    }
                }
            }



            //2.3 降雨量
            //tmpName = "雨量";
            tmpName = "降雨量";//20170711晚上修改
            para = new SqlParameter[] { new SqlParameter("@t1", tmpName) };
            dsGroup = dbo.GetDataSetWithParams("Sensor_SelectSensorGroupOrderByName", para);
            groupNum = dsGroup.Tables[0].Rows.Count;
            if (groupNum <= 0)
            {
                return strNull;
            }

            tmpStr1 = (dsGroup.Tables[0].Rows[0][0].ToString()).Split(tmpChar1);
            dsSensor = dbo.GetDataSetWithOutParams("Ptc_SelectLatestRainfallData");
            tmpNum = dsSensor.Tables[0].Rows.Count;
            if (tmpNum <= 0)
            {
                return strNull;
            }

            // 检查降雨量分组，1个
            for (int k = 0; k < tmpStr1.Length; k++)
            {
                dataSum = 0;    //清空
                dataNm = 0;
                tmpStr2 = tmpStr1[k].Split(tmpChar2);
                if (tmpStr2.Length > 0)
                {
                    for (int i = 0; i < tmpStr2.Length; i++)
                    {
                        for (int j = 0; j < tmpNum; j++)
                        {
                            s1 = dsSensor.Tables[0].Rows[j][0].ToString();
                            s2 = dsSensor.Tables[0].Rows[j][1].ToString();
                            d1 = Convert.ToDouble(dsSensor.Tables[0].Rows[j][2].ToString()); 

                            ////乘以1000，将数据单位调整为mm
                            //d1 = d1 * 1000;
                            d1 = Math.Round(d1, 3); //保留3位小数

                            if (s1 == tmpStr2[i])
                            {
                                dataSum += d1;
                                dataNm += 1;
                            }
                        }
                    }

                    if (dataNm > 0)
                    {
                        dataSum = dataSum / dataNm;  //求得平均值
                        V[2, k] = Convert.ToDecimal(dataSum);
                    }
                }
            }

            //如果只有一个降雨量，则共用
            if (tmpStr1.Length == 1)
            {
                V[2, 1] = V[2, 0];
                V[2, 2] = V[2, 0];
                V[2, 3] = V[2, 0];
            }


            //2.8 孔隙水压力
            tmpName = "孔隙水压力";
            para = new SqlParameter[] { new SqlParameter("@t1", tmpName) };
            dsGroup = dbo.GetDataSetWithParams("Sensor_SelectSensorGroupOrderByName", para);
            groupNum = dsGroup.Tables[0].Rows.Count;
            if (groupNum <= 0)
            {
                return strNull;
            }

            tmpStr1 = (dsGroup.Tables[0].Rows[0][0].ToString()).Split(tmpChar1);
            dsSensor = dbo.GetDataSetWithOutParams("Ptc_SelectLatestStData");
            tmpNum = dsSensor.Tables[0].Rows.Count;
            if (tmpNum <= 0)
            {
                return strNull;
            }

            // 检查孔隙水压力分组，4组，每组1个
            for (int k = 0; k < tmpStr1.Length; k++)
            {
                dataSum = 0;    //清空
                dataNm = 0;
                tmpStr2 = tmpStr1[k].Split(tmpChar2);
                if (tmpStr2.Length > 0)
                {
                    for (int i = 0; i < tmpStr2.Length; i++)
                    {
                        for (int j = 0; j < tmpNum; j++)
                        {
                            s1 = dsSensor.Tables[0].Rows[j][0].ToString();
                            s2 = dsSensor.Tables[0].Rows[j][1].ToString();
                            d1 = Convert.ToDouble(dsSensor.Tables[0].Rows[j][2].ToString());

                            //除以1000，将数据单位调整为kPa
                            d1 = d1 / 1000;
                            d1 = Math.Round(d1, 3); //保留3位小数

                            if (s1 == tmpStr2[i])
                            {
                                dataSum += d1;
                                dataNm += 1;
                            }
                        }
                    }

                    if (dataNm > 0)
                    {
                        dataSum = dataSum / dataNm;  //求得平均值
                        V[7, k] = Convert.ToDecimal(dataSum);
                    }
                }
            }


            //20170530，为了处理无穷大的问题，改为调整预警限值
            //对孔隙水压力的单独判断，因为锦丰排土场其值大大超过了最大值6的5倍，达到了300kPa.
            //所以，需要单独对锦丰排土场的调整节域，再乘以12如何？
            for (int k = 0; k < 4; k++)
            {
                if (V[7, k] > JB[0, 7])
                {
                    JB[0, 7] = 12 * JB[0, 7];
                    PB[7] = 12 * PB[7];
                    break;
                }
            }


            //2.9 土压力
            tmpName = "土压力计";
            para = new SqlParameter[] { new SqlParameter("@t1", tmpName) };
            dsGroup = dbo.GetDataSetWithParams("Sensor_SelectSensorGroupOrderByName", para);
            groupNum = dsGroup.Tables[0].Rows.Count;
            if (groupNum <= 0)
            {
                return strNull;
            }

            tmpStr1 = (dsGroup.Tables[0].Rows[0][0].ToString()).Split(tmpChar1);
            dsSensor = dbo.GetDataSetWithOutParams("Ptc_SelectLatestEpData");
            tmpNum = dsSensor.Tables[0].Rows.Count;
            if (tmpNum <= 0)
            {
                return strNull;
            }

            // 检查土压力分组，4组，每组1个
            for (int k = 0; k < tmpStr1.Length; k++)
            {
                dataSum = 0;    //清空
                dataNm = 0;
                tmpStr2 = tmpStr1[k].Split(tmpChar2);
                if (tmpStr2.Length > 0)
                {
                    for (int i = 0; i < tmpStr2.Length; i++)
                    {
                        for (int j = 0; j < tmpNum; j++)
                        {
                            s1 = dsSensor.Tables[0].Rows[j][0].ToString();
                            s2 = dsSensor.Tables[0].Rows[j][1].ToString();
                            d1 = Convert.ToDouble(dsSensor.Tables[0].Rows[j][2].ToString());

                            ////乘以1000，将数据单位调整为mm
                            //d1 = d1 * 1000;
                            d1 = Math.Round(d1, 3); //保留3位小数

                            if (s1 == tmpStr2[i])
                            {
                                dataSum += d1;
                                dataNm += 1;
                            }
                        }
                    }

                    if (dataNm > 0)
                    {
                        dataSum = dataSum / dataNm;  //求得平均值
                        V[8, k] = Convert.ToDecimal(dataSum);
                    }
                }
            }


            //2.11 土壤含水率
            tmpName = "土壤含水率";
            para = new SqlParameter[] { new SqlParameter("@t1", tmpName) };
            dsGroup = dbo.GetDataSetWithParams("Sensor_SelectSensorGroupOrderByName", para);
            groupNum = dsGroup.Tables[0].Rows.Count;
            if (groupNum <= 0)
            {
                return strNull;
            }

            tmpStr1 = (dsGroup.Tables[0].Rows[0][0].ToString()).Split(tmpChar1);
            dsSensor = dbo.GetDataSetWithOutParams("Ptc_SelectLatestSmcData");
            tmpNum = dsSensor.Tables[0].Rows.Count;
            if (tmpNum <= 0)
            {
                return strNull;
            }

            // 检查土壤含水率分组，1组，该组4个
            for (int k = 0; k < tmpStr1.Length; k++)
            {
                dataSum = 0;    //清空
                dataNm = 0;
                tmpStr2 = tmpStr1[k].Split(tmpChar2);
                if (tmpStr2.Length > 0)
                {
                    for (int i = 0; i < tmpStr2.Length; i++)
                    {
                        for (int j = 0; j < tmpNum; j++)
                        {
                            s1 = dsSensor.Tables[0].Rows[j][0].ToString();
                            s2 = dsSensor.Tables[0].Rows[j][1].ToString();
                            d1 = Convert.ToDouble(dsSensor.Tables[0].Rows[j][2].ToString());

                            d1 = Math.Round(d1, 3); //保留3位小数

                            if (s1 == tmpStr2[i])
                            {
                                dataSum += d1;
                                dataNm += 1;
                            }
                        }
                    }

                    if (dataNm > 0)
                    {
                        dataSum = dataSum / dataNm;  //求得平均值
                        V[10, k] = Convert.ToDecimal(dataSum);
                    }
                }
            }

            //4个组都设置相同含水率
            if (tmpStr1.Length == 1)
            {
                V[10, 1] = V[10, 0];
                V[10, 2] = V[10, 0];
                V[10, 3] = V[10, 0];
            }


            //2.4--2.7,2.10,得到排土场平台的数据，4组，每组5个
            tmpNum = dsPlat.Tables[0].Rows.Count;
            if(tmpNum >1)   //比如翁福有多个平台
            {
                for(int i=0;i<tmpNum;i++)
                {
                    V[5, i] = Convert.ToDecimal(dsPlat.Tables[0].Rows[i][1].ToString()); //高度
                    V[6, i] = Convert.ToDecimal(dsPlat.Tables[0].Rows[i][2].ToString()); //宽度
                    V[9, i] = Convert.ToDecimal(dsPlat.Tables[0].Rows[i][3].ToString()); //地震烈度
                    V[3, i] = Convert.ToDecimal(dsPlat.Tables[0].Rows[i][4].ToString()); //粘聚力
                    V[4, i] = Convert.ToDecimal(dsPlat.Tables[0].Rows[i][5].ToString()); //摩擦角
                }
            }
            else     //平台共用
            {
                V[5, 0] = Convert.ToDecimal(dsPlat.Tables[0].Rows[0][1].ToString()); //高度
                V[6, 0] = Convert.ToDecimal(dsPlat.Tables[0].Rows[0][2].ToString()); //宽度
                V[9, 0] = Convert.ToDecimal(dsPlat.Tables[0].Rows[0][3].ToString()); //地震烈度
                V[3, 0] = Convert.ToDecimal(dsPlat.Tables[0].Rows[0][4].ToString()); //粘聚力
                V[4, 0] = Convert.ToDecimal(dsPlat.Tables[0].Rows[0][5].ToString()); //摩擦角
 
                for(int i=1;i<4;i++)
                {
                    V[5, i] = V[5, 0];
                    V[6, i] = V[6, 0];
                    V[9, i] = V[9, 0];
                    V[3, i] = V[3, 0];
                    V[4, i] = V[4, 0];
                }
            }


            //3. 开始计算10个主指标对应5种预警等级的关联度
            //20170528，要求4个组分别计算
            for (int k = 0; k < 4; k++)
            {
                //3.1 先计算单指标关联度
                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        JBA[i, j] = JB[i, j] - JA[i, j];

                        //计算RV
                        RV[i, j] = Math.Abs(V[j,k] - (JA[i, j] + JB[i, j]) / 2) - (JB[i, j] - JA[i, j]) / 2;
                    }
                }
                for (int i = 0; i < 10; i++)
                {
                    RP[i] = Math.Abs(V[i,k] - (PA[i] + PB[i]) / 2) - (PB[i] - PA[i]) / 2;
                }


                //开始计算单一指标关联度
                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        if ((j == 3) || (j == 4) || (j == 6))
                        {
                            if ((V[j,k] >= JA[i, j]) && (V[j,k] < JB[i, j]))
                            {
                                KF[i, j] = -RV[i, j] / JBA[i, j];
                            }
                            else
                            {
                                KF[i, j] = RV[i, j] / (RP[j] - RV[i, j]);
                            }
                        }
                        else
                        {
                            //20170530
                            //考虑到采集的数据，比如降雨量为0时，计算成为0/0。所以，
                            //将判别式改为V[j, k] >= JA[i, j])
                            //if ((V[j,k] > JA[i, j]) && (V[j,k] <= JB[i, j]))
                            if ((V[j, k] >= JA[i, j]) && (V[j, k] <= JB[i, j]))
                            {
                                KF[i, j] = -RV[i, j] / JBA[i, j];
                            }
                            else
                            {
                                //分母为0时，怎么调整计算方案？
                                //如石灰石矿：i=4,j=8时出现分母为0
                                decimal tmpR = RP[j] - RV[i, j];
                                if(tmpR==0)
                                {
                                    tmpR = (decimal)0.001;
                                }
                                KF[i, j] = RV[i, j] / tmpR;
                                //KF[i, j] = RV[i, j] / (RP[j] - RV[i, j]);
                            }
                        }
                    }
                }


                //3.2 再与权重相关
                for (int i = 0; i < 5; i++)
                {
                    KW[i] = 0;
                    for (int j = 0; j < 10; j++)
                    {
                        KW[i] = KW[i] + W[j] * KF[i, j];
                    }
                }

                //3.3 求最大值
                maxKW = 0;
                int finalAlarm = 4; //默认为无警

                for (int i = 0; i < 5; i++)
                {
                    if (KW[i] > maxKW)
                    {
                        maxKW = KW[i];
                        finalAlarm = i;
                    }
                }

                //输出计算结果
                //先输出初步关联度
                //string result1 = KW[0].ToString("F3") + "," + KW[1].ToString("F3") + "," + KW[2].ToString("F3") + "," + KW[3].ToString("F3") + "," + KW[4].ToString("F3");
                //LblAlarmResult1.Text = "关联度计算分别是：" + result1;
                //LblAlarmResult2.Text = "预警等级为：" + (finalAlarm + 1).ToString() + ", 关联度是：" + maxKW.ToString("F3");
                decimal dg1 = Convert.ToDecimal(KW[0]);
                decimal dg2 = Convert.ToDecimal(KW[1]);
                decimal dg3 = Convert.ToDecimal(KW[2]);
                decimal dg4 = Convert.ToDecimal(KW[3]);
                decimal dg5 = Convert.ToDecimal(KW[4]);


                //3. 按修正指标进一步判别
                decimal[] XZ = new decimal[3];
                XZ[0] = Convert.ToDecimal(V[10,k]); //得到土壤含水量%
                XZ[1] = Convert.ToDecimal(deadNum);  //死亡人数
                XZ[2] = LostValue;                  //财产损失


                if (XZ[0] >= XZLimit[0])
                {
                    finalAlarm -= 1;
                }
                else if (XZ[1] >= XZLimit[1])
                {
                    finalAlarm -= 1;
                }
                else if (XZ[2] >= XZLimit[2])
                {
                    finalAlarm -= 1;
                }

                //如果严重超标了，则定为I级。
                if (finalAlarm < 0)
                {
                    finalAlarm = 0;
                }
                //输出最终的预警结果
                //LblAlarmResult3.Text = "最终预警等级为：" + (finalAlarm + 1).ToString();
                //if (finalAlarm == 0)
                //{
                //    //ImageResult.ForeColor = System.Drawing.Color.Red;
                //    ImageResult.BackColor = System.Drawing.Color.Red;
                //}
                //else if (finalAlarm == 1)
                //{
                //    ImageResult.BackColor = System.Drawing.Color.Orange;
                //}
                //else if (finalAlarm == 1)
                //{
                //    ImageResult.BackColor = System.Drawing.Color.Yellow;
                //}
                //else if (finalAlarm == 1)
                //{
                //    ImageResult.BackColor = System.Drawing.Color.Blue;
                //}
                //else
                //{
                //    ImageResult.BackColor = System.Drawing.Color.White;
                //}

                ////判别是否超过限值，依次红、橙、黄、蓝进行
                //int alarmLevel = 0;//默认是不超限

                //如果超过限值，则增加事件记录
                //修改20170609，无论是哪种状态（红橙黄蓝无），都保存记录
                //修改20170729。根据20170727的贵阳交流，无警不再保存和显示。
                //if (finalAlarm <4)
                if (finalAlarm < 4)
                {
                    //保存该限值到数据表OneAlarmInfoTable中
                    string t1 = (k+1).ToString();   //分组号
                    string t2 = DateTime.Parse(sensorTime).ToString("yyyy-MM-dd HH:mm");    //时间
                    decimal t3 = maxKW;
                    int t4 = finalAlarm+1;

                    //如果是无警状态，则设置输出为0
                    //Updated by zxm on 20170715
                    if(t4==5)
                    {
                        t4 = 0;
                    }


                    para = new SqlParameter[] { new SqlParameter("@t1", t1),new SqlParameter("@t2", t2),new SqlParameter("@t3", t3),new SqlParameter("@t4", t4),
                        new SqlParameter("@t5", V[0,k]),new SqlParameter("@t6", V[1,k]), new SqlParameter("@t7", V[2,k]),new SqlParameter("@t8", V[8,k]),
                        new SqlParameter("@t9", V[7,k]),new SqlParameter("@t10", XZ[0]), new SqlParameter("@t11", XZ[1]),new SqlParameter("@t12", XZ[2]),
                        new SqlParameter("@t13", dg1),new SqlParameter("@t14", dg2), new SqlParameter("@t15", dg3),new SqlParameter("@t16", dg4),
                        new SqlParameter("@t17", dg5)};
                    dbo.ExecuteProcWithParams("Alarm_AddFusionAlarmInfo", para);

                    strNull  = t4.ToString();
                }
            }
            return strNull;
        }




        //20170514 by zxm
        //获得最新地表位移数据，并比较其预警指标。
        //20171006修改，返回预警等级，以便短信和邮件告警
        //
        //public void CreateOneAlarmInfobySurfaceData()
        public string CreateOneAlarmInfobySurfaceData()
        {
            string strNull = "";
            
            //如何识别最新到来的表面位移传感器数据？通过定时器！
            //1 读取传感器最新数据；
            //2 检查日期是否在定时范围内；
            //3 若是，则预警检测
            //4 若超过预警限，则插入该信息到数据库中。
            //5 待全部传感器信息检查完成后，再重新获取预警数据表记录，显示到界面上。

            getOneEventInfo();
            getAlarmSetData();
            if (dsAlarmData.Tables[0].Rows.Count <= 0)
            {
                return strNull;
            }


            //1 读取传感器最新数据；
            DataSet dsSensor = new DataSet(); //通知数据集
            //dsSensor = dbo.GetDataSetWithOutParams("Data_SelectMpDataByCorpId");
            dsSensor = dbo.GetDataSetWithOutParams("Ptc_SelectLatestMpData");

            int tmpNum = dsSensor.Tables[0].Rows.Count;
            if (tmpNum > 0)
            {
                for (int k = 0; k < tmpNum; k++)   //针对每个地表位移传感器
                {
                    string sensorId;
                    string sensorTime;
                    double d1, d2, d3, dtotal;


                    sensorId = dsSensor.Tables[0].Rows[k][0].ToString();
                    sensorTime = dsSensor.Tables[0].Rows[k][1].ToString();
                    sensorTime = DateTime.Parse(sensorTime).ToString("yyyy-MM-dd HH:mm");

                    //dtEventTime = Convert.ToDateTime(eventTime);
                    ////摘取年月日:时分
                    //newEventTime = dtEventTime.Year.ToString() + dtEventTime.Month.ToString() + dtEventTime.Day.ToString() + ":" + dtEventTime.Hour.ToString() + dtEventTime.Minute.ToString();

                    //2 检查日期是否在定时范围内；
                    //应该与已有事件记录中的日期时间做比较，如果重复了，则不检测
                    bool mark = false;//重复为true
                    string eventSensorId, eventTime;
                    int tmpNum2 = dsEvent.Tables[0].Rows.Count;
                    if (tmpNum2 > 0)
                    {
                        for (int i = 0; i < tmpNum2; i++)
                        {
                            eventSensorId = dsEvent.Tables[0].Rows[i][1].ToString();
                            eventTime = dsEvent.Tables[0].Rows[i][5].ToString();
                            eventTime = DateTime.Parse(eventTime).ToString("yyyy-MM-dd HH:mm");

                            //dtSensorTime = Convert.ToDateTime(sensorTime);
                            //newSensorTime = dtSensorTime.Year.ToString() + dtSensorTime.Month.ToString() + dtSensorTime.Day.ToString() + ":" + dtSensorTime.Hour.ToString() + dtSensorTime.Minute.ToString();
                            if ((sensorId == eventSensorId) && (sensorTime == eventTime))
                            {
                                mark = true;
                                break;
                            }
                        }
                    }


                    //如果不重复，则比较预警限值
                    if (mark == false)
                    {
                        //先计算矢量值
                        d1 = Convert.ToDouble(dsSensor.Tables[0].Rows[k][2].ToString());    //X
                        d2 = Convert.ToDouble(dsSensor.Tables[0].Rows[k][3].ToString());    //Y
                        d3 = Convert.ToDouble(dsSensor.Tables[0].Rows[k][4].ToString());    //H
                        dtotal = Math.Sqrt(Math.Pow(d1, 2) + Math.Pow(d2, 2) + Math.Pow(d3, 2));    //矢量值
                        //乘以1000，将数据单位调整为mm
                        dtotal = dtotal * 1000;
                        //20171004统一预警数据精度
                        dtotal = Math.Round(dtotal, 3); //保留3位小数

                        //读取数据库中的预警限值
                        int alarmD1 = Int32.Parse(dsAlarmData.Tables[0].Rows[0][2].ToString());
                        int alarmD2 = Int32.Parse(dsAlarmData.Tables[0].Rows[0][3].ToString());
                        int alarmD3 = Int32.Parse(dsAlarmData.Tables[0].Rows[0][4].ToString());
                        int alarmD4 = Int32.Parse(dsAlarmData.Tables[0].Rows[0][5].ToString());

                        //判别是否超过限值，依次红、橙、黄、蓝进行
                        int alarmLevel = 0;//默认是不超限
                        if (dtotal > alarmD1)
                        {
                            alarmLevel = 1;
                        }
                        else if ((dtotal > alarmD2) && (dtotal <= alarmD1))
                        {
                            alarmLevel = 2;
                        }
                        else if ((dtotal > alarmD3) && (dtotal <= alarmD2))
                        {
                            alarmLevel = 3;
                        }
                        else if ((dtotal > alarmD4) && (dtotal <= alarmD3))
                        {
                            alarmLevel = 4;
                        }


                        //如果超过限值，则增加事件记录
                        //修改20170609，无论是哪种状态（红橙黄蓝无），都保存记录
                        //修改20170729。根据20170727的贵阳交流，无警不再保存和显示。
                        //if (alarmLevel > 0)
                        if (alarmLevel > 0)
                        {
                            //保存该限值到数据表OneAlarmInfoTable中
                            string t1 = sensorId;
                            double t2 = dtotal;
                            int t3 = alarmLevel;
                            string t4 = DateTime.Parse(sensorTime).ToString("yyyy-MM-dd HH:mm");

                            SqlParameter[] para = new SqlParameter[] { new SqlParameter("@t1", t1),new SqlParameter("@t2", t2),
                        new SqlParameter("@t3", t3),new SqlParameter("@t4", t4)};
                            dbo.ExecuteProcWithParams("Alarm_AddOneAlarmInfo", para);

                            strNull = t3.ToString();
                        }
                    }
                }
            }
            return strNull;
        }



        //20170514 by zxm
        //获得最新内部位移数据，并比较其预警指标。
        //20171006修改，返回预警等级，以便短信和邮件告警
        //public void CreateOneAlarmInfobyInDispData()
        public string CreateOneAlarmInfobyInDispData()
        {
            string strNull = "";

            //如何识别最新到来的内部位移传感器数据？通过定时器！
            //1 读取传感器最新数据；
            //2 检查日期是否在定时范围内；
            //3 若是，则预警检测
            //4 若超过预警限，则插入该信息到数据库中。
            //5 待全部传感器信息检查完成后，再重新获取预警数据表记录，显示到界面上。

            //1 读取传感器最新数据；
            DataSet dsSensor = new DataSet(); 
            dsSensor = dbo.GetDataSetWithOutParams("Ptc_SelectLatestDmData");

            int tmpNum = dsSensor.Tables[0].Rows.Count;
            if (tmpNum <= 0)
            {
                return strNull;
            }

            for (int k = 0; k < tmpNum; k++)
            {
                string sensorId;
                string sensorTime;
                double d1, d2, d3, dtotal;

                sensorId = dsSensor.Tables[0].Rows[k][0].ToString();
                sensorTime = dsSensor.Tables[0].Rows[k][1].ToString();//数据采集时分"yyyy-MM-dd HH:mm"
                sensorTime = DateTime.Parse(sensorTime).ToString("yyyy-MM-dd HH:mm");
                //DateTime dt = Convert.ToDateTime(eventTime);

                //2 检查日期是否在定时范围内；
                //应该与已有事件记录中的日期时间做比较，如果重复了，则不检测
                bool mark = false;//重复为true
                string eventSensorId, eventTime;
                int tmpNum2 = dsEvent.Tables[0].Rows.Count;
                if (tmpNum2 > 0)
                {
                    for (int i = 0; i < tmpNum2; i++)
                    {
                        eventSensorId = dsEvent.Tables[0].Rows[i][1].ToString();
                        eventTime = dsEvent.Tables[0].Rows[i][5].ToString();
                        eventTime = DateTime.Parse(eventTime).ToString("yyyy-MM-dd HH:mm");
                        if ((sensorId == eventSensorId) && (sensorTime == eventTime))
                        {
                            mark = true;
                            break;
                        }
                    }
                }

                //如果不重复，则比较预警限值
                if (mark == false)
                {
                    //先计算矢量值
                    d1 = Convert.ToDouble(dsSensor.Tables[0].Rows[k][2].ToString());    //a
                    d2 = Convert.ToDouble(dsSensor.Tables[0].Rows[k][3].ToString());    //b
                    dtotal = Math.Sqrt(Math.Pow(d1, 2) + Math.Pow(d2, 2));              //矢量值
                    //乘以1000，将数据单位调整为mm
                    //20170528，贵州反馈结果是：采集值单位就是mm，不需要转换。
                    //dtotal = dtotal * 1000;
                    //20171004统一预警数据精度
                    dtotal = Math.Round(dtotal, 3); //保留3位小数

                    //读取数据库中的内部位移预警限值
                    int alarmD1 = Int32.Parse(dsAlarmData.Tables[0].Rows[1][2].ToString());
                    int alarmD2 = Int32.Parse(dsAlarmData.Tables[0].Rows[1][3].ToString());
                    int alarmD3 = Int32.Parse(dsAlarmData.Tables[0].Rows[1][4].ToString());
                    int alarmD4 = Int32.Parse(dsAlarmData.Tables[0].Rows[1][5].ToString());

                    //判别是否超过限值，依次红、橙、黄、蓝进行
                    int alarmLevel = 0;//默认是不超限
                    if (dtotal > alarmD1)
                    {
                        alarmLevel = 1;
                    }
                    else if ((dtotal > alarmD2) && (dtotal <= alarmD1))
                    {
                        alarmLevel = 2;
                    }
                    else if ((dtotal > alarmD3) && (dtotal <= alarmD2))
                    {
                        alarmLevel = 3;
                    }
                    else if ((dtotal > alarmD4) && (dtotal <= alarmD3))
                    {
                        alarmLevel = 4;
                    }


                    //如果超过限值，则增加事件记录
                    //修改20170609，无论是哪种状态（红橙黄蓝无），都保存记录
                    //修改20170729。根据20170727的贵阳交流，无警不再保存和显示。
                    //if (alarmLevel > 0)
                    if (alarmLevel > 0)
                    {

                        //保存该限值到数据表OneAlarmInfoTable中
                        string t1 = sensorId;
                        double t2 = dtotal;
                        int t3 = alarmLevel;
                        string t4 = DateTime.Parse(sensorTime).ToString("yyyy-MM-dd HH:mm");

                        SqlParameter[] para = new SqlParameter[] { new SqlParameter("@t1", t1),new SqlParameter("@t2", t2),
                        new SqlParameter("@t3", t3),new SqlParameter("@t4", t4)};
                        dbo.ExecuteProcWithParams("Alarm_AddOneAlarmInfo", para);

                        strNull = t3.ToString();
                    }
                }
            }
            return strNull;
        }



        //20170514 by zxm
        //获得最新降雨量数据，并比较其预警指标。
        //20171006修改，返回预警等级，以便短信和邮件告警
        //
        //public void CreateOneAlarmInfobyRainfallData()
        public string CreateOneAlarmInfobyRainfallData()
        {
            string strNull = "";
            
            //如何识别最新到来的降雨量传感器数据？通过定时器！
            //1 读取传感器最新数据；
            //2 检查日期是否在定时范围内；
            //3 若是，则预警检测
            //4 若超过预警限，则插入该信息到数据库中。
            //5 待全部传感器信息检查完成后，再重新获取预警数据表记录，显示到界面上。

            //1 读取传感器最新数据；
            DataSet dsSensor = new DataSet(); 
            //dsSensor = dbo.GetDataSetWithOutParams("Data_SelectMpDataByCorpId");
            dsSensor = dbo.GetDataSetWithOutParams("Ptc_SelectLatestRainfallData");

            int tmpNum = dsSensor.Tables[0].Rows.Count;
            if (tmpNum <= 0)
            {
                return strNull;
            }
            
            for (int k = 0; k < tmpNum; k++)
            {
                string sensorId;
                string sensorTime;
                double dtotal;

                sensorId = dsSensor.Tables[0].Rows[k][0].ToString();
                sensorTime = dsSensor.Tables[0].Rows[k][1].ToString();//数据采集时分"yyyy-MM-dd HH:mm"
                sensorTime = DateTime.Parse(sensorTime).ToString("yyyy-MM-dd HH:mm");
                //DateTime dt = Convert.ToDateTime(eventTime);

                //2 检查日期是否在定时范围内；
                //应该与已有事件记录中的日期时间做比较，如果重复了，则不检测
                bool mark = false;//重复为true
                string eventSensorId, eventTime;
                int tmpNum2 = dsEvent.Tables[0].Rows.Count;
                if (tmpNum2 > 0)
                {
                    for (int i = 0; i < tmpNum2; i++)
                    {
                        eventSensorId = dsEvent.Tables[0].Rows[i][1].ToString();
                        eventTime = dsEvent.Tables[0].Rows[i][5].ToString();
                        eventTime = DateTime.Parse(eventTime).ToString("yyyy-MM-dd HH:mm");
                        if ((sensorId == eventSensorId) && (sensorTime == eventTime))
                        {
                            mark = true;
                            break;
                        }
                    }

                    //如果不重复，则比较预警限值
                    if (mark == false)
                    {
                        //先计算矢量值
                        dtotal = Convert.ToDouble(dsSensor.Tables[0].Rows[k][2].ToString());    //降雨量
                        //20171004统一预警数据精度
                        dtotal = Math.Round(dtotal, 3); //保留3位小数

                        //读取数据库中的预警限值
                        int alarmD1 = Int32.Parse(dsAlarmData.Tables[0].Rows[2][2].ToString());
                        int alarmD2 = Int32.Parse(dsAlarmData.Tables[0].Rows[2][3].ToString());
                        int alarmD3 = Int32.Parse(dsAlarmData.Tables[0].Rows[2][4].ToString());
                        int alarmD4 = Int32.Parse(dsAlarmData.Tables[0].Rows[2][5].ToString());

                        //判别是否超过限值，依次红、橙、黄、蓝进行
                        int alarmLevel = 0;//默认是不超限
                        if (dtotal > alarmD1)
                        {
                            alarmLevel = 1;
                        }
                        else if ((dtotal > alarmD2) && (dtotal <= alarmD1))
                        {
                            alarmLevel = 2;
                        }
                        else if ((dtotal > alarmD3) && (dtotal <= alarmD2))
                        {
                            alarmLevel = 3;
                        }
                        else if ((dtotal > alarmD4) && (dtotal <= alarmD3))
                        {
                            alarmLevel = 4;
                        }


                        //如果超过限值，则增加事件记录
                        //修改20170609，无论是哪种状态（红橙黄蓝无），都保存记录
                        //修改20170729。根据20170727的贵阳交流，无警不再保存和显示。
                        //if (alarmLevel > 0)
                        if (alarmLevel > 0)
                        {
                            //保存该限值到数据表OneAlarmInfoTable中
                            string t1 = sensorId;
                            double t2 = dtotal;
                            int t3 = alarmLevel;
                            string t4 = DateTime.Parse(sensorTime).ToString("yyyy-MM-dd HH:mm");

                            SqlParameter[] para = new SqlParameter[] { new SqlParameter("@t1", t1),new SqlParameter("@t2", t2),
                        new SqlParameter("@t3", t3),new SqlParameter("@t4", t4)};
                            dbo.ExecuteProcWithParams("Alarm_AddOneAlarmInfo", para);
            
                            strNull = t3.ToString();
                        }
                    }
                }
            }
            return strNull;
        }

    }
}
