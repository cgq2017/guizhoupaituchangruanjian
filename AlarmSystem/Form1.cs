using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using DataAccess;
using System.Data.SqlClient;
using AlarmDataProcess;


namespace AlarmSystem
{
    public partial class Form1 : Form
    {
        AlarmComputing AlarmComp = new AlarmComputing();
        DataOperation dbo = new DataOperation();
        NoticeSend notice = new NoticeSend();

        //[DllImport("Ux64_dllc.dll")]
        //public static extern void Usb_Qu_Open();
        //[DllImport("Ux64_dllc.dll")]
        //public static extern void Usb_Qu_Close();
        //[DllImport("Ux64_dllc.dll")]
        //public static unsafe extern bool Usb_Qu_write(byte Q_index, byte Q_type, byte* pQ_data);
        //[DllImport("Ux64_dllc.dll")]
        //public static extern int Usb_Qu_Getstate();
        
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //数据库连接
            DataOperation.InitSqlConf();
            DataOperation.InitDbcon();
            
            ////获得数据库名称,20171006，供发送告警信息使用
            //dbName = DataOperation.dsServer;

            //预警方式选择有效
            chkOneAlarm.Enabled = true;
            chkMoreAlarm.Enabled = true;
            chkEmailAlarm.Enabled = true;
            chkMsgAlarm.Enabled = true;

            chkOneAlarm.Checked = false;
            chkMoreAlarm.Checked = false;
            chkEmailAlarm.Checked = false;
            chkMsgAlarm.Checked = false;

            numericUpDown2.Enabled = false;
            numericUpDown3.Enabled = false;
            btnAlarmStart.Enabled = false;


            //20171007
            //获得管理员邮箱和手机号
            getUserEmailMobileInfo();
            txtSubject.Text = "排土场灾害预警";
            //设置短信参数
            //1波特率
            cbRate.Items.Add("57600");
            cbRate.Items.Add("28800");
            cbRate.Items.Add("14400");
            cbRate.SelectedIndex = 0;
            //2串口
            foreach (string s in SerialPort.GetPortNames())
            {
                cbPort.Items.Add(s);
            }
            cbPort.SelectedIndex = 0;
                

            //定时设置
            int alarmTime = Convert.ToInt32(numericUpDown1.Value)*60*1000;  //转换为毫秒
            timer1.Interval = alarmTime; //单一指标报警时间间隔
            timer1.Enabled = false;

            //cbSensor1填充
            cbSensor1.Items.Add("所有数据");
            DataSet dsSensorType = new DataSet(); //通知数据集
            dsSensorType = dbo.GetDataSetWithOutParams("Sensor_SelectSensorTypeInfo");
            int tmpNum = dsSensorType.Tables[0].Rows.Count;
            if(tmpNum>0)
            {
                for(int i=0;i<tmpNum;i++)
                {
                    cbSensor1.Items.Add(dsSensorType.Tables[0].Rows[i][1].ToString());
                }
            }
            cbSensor1.SelectedIndex = 0;

            //cbSensor2填充，只有4个分组         
            cbSensor2.Items.Add("所有分组");
            for (int i = 1; i <= 4; i++)
            {
                cbSensor2.Items.Add(i.ToString());
            }
            cbSensor2.SelectedIndex = 0;
        }


   
        //private void showOneEventInfo()
        //{
        //    string tmpId = "01";  // 企业编号;
        //    SqlParameter[] para = new SqlParameter[] { new SqlParameter("@t1", tmpId) };
        //    DataSet dsEvent = dbo.GetDataSetWithParams("Alarm_SelectOneAlarmInfoById", para);

        //    int tmpNum = dsEvent.Tables[0].Rows.Count;
        //    if (tmpNum > 0)
        //    {
        //        dataView1.DataSource = dsEvent.Tables[0];

        //       // checkQueryStatus();  //检查每条预警等级的颜色
        //    }
        //}


        private void checkQueryStatus1()
        {
            string tmpId;
            for (int i = 0; i < dataView1.Rows.Count; i++)
            {
                dataView1[4, i].Value = Convert.ToDouble(dataView1[4, i].Value).ToString("f3");
                
                tmpId = dataView1.Rows[i].Cells[5].FormattedValue.ToString();
                if (tmpId == "1")
                {
                    dataView1[0,i].Value = Image.FromFile("Images/Alarm/红色.png");
                    //((Image)(gvDataView.Rows[i].Cells[5].FindControl("AlarmImage"))).BackColor = System.Drawing.Color.Red;
                    //dataView1.Rows[i].Cells[6].Style.BackColor  = Color.Red;  //"~/Manager/images/Alarm/红色.png";
                    //((Image)(dataView1.Rows[i].Cells[6].FindControl("AlarmImage"))).Visible = true;
                }
                else if (tmpId == "2")
                {
                    dataView1[0, i].Value = Image.FromFile("Images/Alarm/橙色.png");
                    //dataView1.Rows[i].Cells[6].Style.BackColor = Color.Brown; 
                }
                else if (tmpId == "3")
                {
                    dataView1[0, i].Value = Image.FromFile("Images/Alarm/黄色.png");
                    //dataView1.Rows[i].Cells[6].Style.BackColor = Color.Yellow;
                }
                else if (tmpId == "4")
                {
                    dataView1[0, i].Value = Image.FromFile("Images/Alarm/蓝色.png");
                    //dataView1.Rows[i].Cells[6].Style.BackColor = Color.Blue;
                }
                else
                {
                    //dataView1[0, i].Value = Image.FromFile("Images/Alarm/正常.png");
                    dataView1[0, i].Value = Image.FromFile("");
                    //dataView1.Rows[i].Cells[0].Style.BackColor = Color.Red;
                }
            }
        }



        private void checkQueryStatus2()
        {
            string tmpId;
            for (int i = 0; i < dataView2.Rows.Count; i++)
            {
                dataView2[4, i].Value = Convert.ToDouble(dataView2[4, i].Value).ToString("f3");

                tmpId = dataView2.Rows[i].Cells[5].FormattedValue.ToString();
                if (tmpId == "1")
                {
                    dataView2[0, i].Value = Image.FromFile("Images/Alarm/红色.png");
                    //((Image)(gvDataView.Rows[i].Cells[5].FindControl("AlarmImage"))).BackColor = System.Drawing.Color.Red;
                    //dataView1.Rows[i].Cells[6].Style.BackColor  = Color.Red;  //"~/Manager/images/Alarm/红色.png";
                    //((Image)(dataView1.Rows[i].Cells[6].FindControl("AlarmImage"))).Visible = true;
                }
                else if (tmpId == "2")
                {
                    dataView2[0, i].Value = Image.FromFile("Images/Alarm/橙色.png");
                    //dataView1.Rows[i].Cells[6].Style.BackColor = Color.Brown; 
                }
                else if (tmpId == "3")
                {
                    dataView2[0, i].Value = Image.FromFile("Images/Alarm/黄色.png");
                    //dataView1.Rows[i].Cells[6].Style.BackColor = Color.Yellow;
                }
                else if (tmpId == "4")
                {
                    dataView2[0, i].Value = Image.FromFile("Images/Alarm/蓝色.png");
                    //dataView1.Rows[i].Cells[6].Style.BackColor = Color.Blue;
                }
                else
                {
                    //dataView2[0, i].Value = Image.FromFile("Images/Alarm/正常.png");
                    dataView2[0, i].Value = Image.FromFile("");                   
                    //dataView1.Rows[i].Cells[0].Style.BackColor = Color.Red;
                }
            }
        }


   
        private void chkMoreAlarm_CheckedChanged(object sender, EventArgs e)
        {
            if (chkMoreAlarm.Checked == true)
            {
                numericUpDown2.Enabled = true;
                numericUpDown3.Enabled = true;
                btnAlarmStart.Enabled = true;
            }
            else
            {
                numericUpDown2.Enabled = false;
                numericUpDown3.Enabled = false;
                btnAlarmStart.Enabled = false;
            }

        }

  
  
        private void btnAlarmStart_Click(object sender, EventArgs e)
        {
            //首先检查短信告警参数
            if(chkMsgAlarm.Checked)
            {
                if(cbPort.SelectedItem == null)
                {
                    MessageBox.Show("短信告警","短信串口错误，请检查");
                    return;
                }
                if (txtUserMobile.Text == "")
                {
                    MessageBox.Show("短信告警", "缺少用户手机号，请修改！");
                    return;
                }
            }

            //第2，检查邮件告警参数
            if(chkEmailAlarm.Checked)
            {
                if((txtServerName.Text =="")||(txtUserName.Text == "")||(txtUserPwd.Text =="")||(txtSubject.Text ==""))
                {
                    MessageBox.Show("邮件告警", "邮箱登录信息不全，请检查！");
                    return;
                }
                if (txtReceiverEmail.Text == "")
                {
                    MessageBox.Show("邮件告警", "缺少用户邮箱，请修改！");
                    return;
                }
            }


            if (btnAlarmStart.Text == "启动预警")
            {
                chkOneAlarm.Enabled = false;
                chkMoreAlarm.Enabled = false;
                numericUpDown2.Enabled = false;
                numericUpDown3.Enabled = false;
                btnAlarmStart.Text = "停止预警";

                //定时设置
                int alarmTime = Convert.ToInt32(numericUpDown1.Value) * 60 * 1000;  //转换为毫秒
                timer1.Interval = alarmTime; //综合指标报警时间间隔
                timer1.Enabled = true;

                //先立即计算一次。随后的等定时到了再计算。
                ComputingAlarmResult();
            }
            else
            {
                chkOneAlarm.Enabled = true;
                chkMoreAlarm.Enabled = true;
                numericUpDown2.Enabled = true;
                numericUpDown3.Enabled = true;
                timer1.Enabled = false;
                btnAlarmStart.Text = "启动预警";
            }
        }

 
        private void timer1_Tick(object sender, EventArgs e)
        {
            ComputingAlarmResult();
        }


 
        //计算预警
        //增加短信和邮件告警，20171006
       private void ComputingAlarmResult()
        {
            string str1="", str2="", str3="", str4 = "";

            if (chkOneAlarm.Checked)
            {
                str1 = AlarmComp.CreateOneAlarmInfobySurfaceData();
                str2 = AlarmComp.CreateOneAlarmInfobyInDispData();
                str3 = AlarmComp.CreateOneAlarmInfobyRainfallData();
            }
            
            if(chkMoreAlarm.Checked)
            {
                AlarmComp.deadNum = Convert.ToInt32(numericUpDown2.Value);
                AlarmComp.LostValue = Convert.ToDecimal(numericUpDown3.Value);
                str4 = AlarmComp.compFusionAlarm();
            }



            string serverName = "";//获得数据库名称,20171006，供发送告警信息使用
            string dbServer = DataOperation.dsServer;
            if(dbServer =="JF")
            {
                serverName = "锦丰排土场";
            }
            else if (dbServer == "YL")
            {
                serverName = "燕垅排土场";
            }
            else if (dbServer == "WF")
            {
                serverName = "瓮福排土场";
            }
            else if (dbServer == "SHS")
            {
                serverName = "石灰石排土场";
            }

            //组合告警信息
            string oneNotice = "";
            if (str1 != "")
            {
                oneNotice = "地表位移" + str1 + "级";
            }
            else if (str2 != "")
            {
                oneNotice += ";" + "内部位移" + str2 + "级";
            }
            else if (str3 != "")
            {
                oneNotice += ";" + "降雨量" + str3 + "级";
            }

            string FusionNotice = "";
            if (str4 != "")
            {
                FusionNotice = "综合指标预警" + str4 + "级";
            }
            //最终信息组合
            //string finalNotice = serverName + "预警结果：" + oneNotice + "/" + FusionNotice;
            string finalNotice = serverName + "预警结果：";
            if (oneNotice != "")
            {
                finalNotice += oneNotice;
            }
            else if (FusionNotice != "")
            {
                finalNotice += "/" + FusionNotice;
            }

        if(chkLightAlarm.Checked)
        {
            
            if (str1 != "" || str2 != "" || str3 != "" || str4 != "")
            {
                notice.SendLight("1");
            }
           // notice.SendLight("1");
            

         }

            bool IsOk = false;
            //短信告警，需要发送单一指标和综合指标结果
            if(chkMsgAlarm.Checked)
            {
                if (oneNotice != "" || FusionNotice != "")
                {
                    int msgRate = Convert.ToInt32(cbRate.SelectedItem.ToString());
                    IsOk = notice.SendMsg(cbPort.SelectedItem.ToString(), msgRate, txtUserMobile.Text.Trim(), finalNotice);
                }
                //int msgRate = Convert.ToInt32(cbRate.SelectedItem.ToString());
                //IsOk = notice.SendMsg(cbPort.SelectedItem.ToString(), msgRate, txtUserMobile.Text.Trim(), finalNotice);
            }

            //邮件告警，需要发送单一指标和综合指标结果
            if(chkEmailAlarm.Checked)
            {
                string sendResult = notice.sendMail(txtServerName.Text.Trim(), txtUserName.Text.Trim(), txtUserPwd.Text.Trim(), txtFromEmail.Text.Trim(),txtReceiverEmail.Text.Trim(),txtSubject.Text.Trim(), finalNotice);
                if(sendResult !="1")
                {
                    MessageBox.Show(sendResult,"邮件发送错误！");
                }
            }
        }


        private void getUserEmailMobileInfo()
        {
           //从数据库中提取用户信息
           //string connString = "Data Source=127.0.0.1" + ";Initial Catalog=FrameWork;User ID=sa;Password=admin";
           SqlConnection conn = new SqlConnection();
           //conn.ConnectionString = connString;
           conn.ConnectionString = DataOperation.dataConFrame;  //获得FrameWork数据库连接
           SqlCommand cmd = new SqlCommand("User_GetUserEmailMobileInfo", conn);
           cmd.CommandType = CommandType.StoredProcedure;
           if (cmd.Connection.State == ConnectionState.Open)
           {
               cmd.Connection.Close();
           }
           cmd.Connection.Open();

           SqlDataAdapter sda = new SqlDataAdapter(cmd);
           DataSet dsUser = new DataSet();
           sda.Fill(dsUser);
           sda.Dispose();
           cmd.Connection.Close();
           int tmpNum = dsUser.Tables[0].Rows.Count;

           string userEmail = "", userMobile = "";

           if (tmpNum > 0)
           {
               for (int i = 0; i < tmpNum; i++)
               {
                   userEmail += dsUser.Tables[0].Rows[i][1].ToString() + ";";
                   userMobile += dsUser.Tables[0].Rows[i][2].ToString() + ";";
               }
           }
           txtReceiverEmail.Text = userEmail;
           txtUserMobile.Text = userMobile;
       }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int tmpPage = tabControl1.SelectedIndex;
            if(tmpPage == 1)
            {
                initState1();
            }
            else if(tmpPage == 2)
            {
                initState2();
            }
            else
            {
                toolBtnDelete.Visible = false;
            }
        }


   
        private void toolBtnDelete_Click(object sender, EventArgs e)
        {
            int pageIndex,lineIndex=-1;
            pageIndex = tabControl1.SelectedIndex;
            
            try
            {

                //获取当前行的信息
                if (pageIndex == 1)
                {
                    lineIndex = dataView1.CurrentCell.RowIndex;
                }
                else if(pageIndex == 2)
                {                  
                    lineIndex = dataView2.CurrentCell.RowIndex;
                }

                if (lineIndex < 0)
                {
                    MessageBox.Show(DataOperation.DeleteItem, "数据操作", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
                else
                {
                    DialogResult tmpACK = MessageBox.Show(DataOperation.DeleteOperationgConfirm, "数据操作", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                    if (tmpACK == DialogResult.OK)
                    {
                        if (pageIndex == 1)
                        {
                            string tmpLine = dataView1.Rows[lineIndex].Cells[1].FormattedValue.ToString();
                            //MySqlParameter[] para = new MySqlParameter[] { new MySqlParameter("@t1", tmpLine) };
                            //dbo1.ExecuteProcWithParams("f_DeleteEqmJltbInfoByNo", para);
                            SqlParameter[] para = new SqlParameter[] { new SqlParameter("@t1", tmpLine) };
                            dbo.ExecuteProcWithParams("Alarm_DeleteOneAlarmInfoById", para);

                            initState1();
                            MessageBox.Show(DataOperation.DeleteOperationSuccess, "数据操作", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else if(pageIndex == 2)
                        {
                            string tmpLine = dataView2.Rows[lineIndex].Cells[0].FormattedValue.ToString();
                            SqlParameter[] para = new SqlParameter[] { new SqlParameter("@t1", tmpLine) };
                            dbo.ExecuteProcWithParams("Alarm_DeleteFusionAlarmInfoById", para);

                            initState2();
                            MessageBox.Show(DataOperation.DeleteOperationSuccess, "数据操作", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        }
                    }
                }
            }
            catch
            {
                MessageBox.Show(DataOperation.DeleteOperationFailure, "数据操作", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }



        private void initState1()
        {
            try
            {
                toolBtnDelete.Visible = true;
                cbSensor1.SelectedIndex = 0;

                //string tmpId = "01";  // 企业编号;
                //SqlParameter[] para = new SqlParameter[] { new SqlParameter("@t1", tmpId) };
                DataSet dsEvent = dbo.GetDataSetWithOutParams("Alarm_SelectOneAlarmInfo");

                int tmpNum = dsEvent.Tables[0].Rows.Count;
                if (tmpNum > 0)
                {
                    dataView1.DataSource = dsEvent.Tables[0];

                    //checkQueryStatus1();  //检查每条预警等级的颜色

                    showGridViewHeader1();
                    toolBtnDelete.Enabled = true;
                    gbFilter1.Enabled = true;
                }
                else
                {
                    dataView1.DataSource = null;
                    toolBtnDelete.Enabled = false;
                    gbFilter1.Enabled = false;
                }
            }
            catch
            {
                MessageBox.Show(DataOperation.OperationError, "数据操作", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }



        private void initState2()
        {
            try
            {
                toolBtnDelete.Visible = true;
                cbSensor2.SelectedIndex = 0;

                //string tmpId = "01";  // 企业编号;
                //SqlParameter[] para = new SqlParameter[] { new SqlParameter("@t1", tmpId) };
                DataSet dsEvent = dbo.GetDataSetWithOutParams("Alarm_SelectFusionAlarmInfo");

                int tmpNum = dsEvent.Tables[0].Rows.Count;
                if (tmpNum > 0)
                {
                    dataView2.DataSource = dsEvent.Tables[0];

                    //checkQueryStatus2();  //检查每条预警等级的颜色

                    showGridViewHeader2();
                    toolBtnDelete.Enabled = true;
                    gbFilter2.Enabled = true;
                }
                else
                {
                    dataView2.DataSource = null;
                    toolBtnDelete.Enabled = false;
                    gbFilter2.Enabled = false;
                }
            }
            catch
            {
                MessageBox.Show(DataOperation.OperationError, "数据操作", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }



        private void showGridViewHeader1()
        {
            dataView1.Columns[0].HeaderText = "预警颜色";
            dataView1.Columns[1].HeaderText = "事件序号";
            dataView1.Columns[2].HeaderText = "传感器Id";
            dataView1.Columns[3].HeaderText = "传感器类型";
            dataView1.Columns[4].HeaderText = "测量值";
            dataView1.Columns[5].HeaderText = "预警等级";
            dataView1.Columns[6].HeaderText = "事件发生时刻";
        }



        private void showGridViewHeader2()
        {
            dataView2.Columns[0].HeaderText = "事件序号";
            dataView2.Columns[1].HeaderText = "分组Id";
            dataView2.Columns[2].HeaderText = "事件发生时刻";
            dataView2.Columns[3].HeaderText = "计算结果";
            dataView2.Columns[4].HeaderText = "预警等级";
            dataView2.Columns[5].HeaderText = "地表位移";
            dataView2.Columns[6].HeaderText = "内部位移";
            dataView2.Columns[7].HeaderText = "降雨量";
            dataView2.Columns[8].HeaderText = "土压力";
            dataView2.Columns[9].HeaderText = "孔隙水压力";
            dataView2.Columns[10].HeaderText = "土壤含水率";
            dataView2.Columns[11].HeaderText = "死亡人数";
            dataView2.Columns[12].HeaderText = "财产损失";
            dataView2.Columns[13].HeaderText = "1级关联度";
            dataView2.Columns[14].HeaderText = "2级关联度";
            dataView2.Columns[15].HeaderText = "3级关联度";
            dataView2.Columns[16].HeaderText = "4级关联度";
            dataView2.Columns[17].HeaderText = "5级关联度";
        }

        private void btnFilter2_Click(object sender, EventArgs e)
        {
            try
            {
                string t1 = cbSensor2.Text;
                DataTable table1 = (DataTable)dataView2.DataSource;

                if (t1 == "所有数据")
                {
                    table1.DefaultView.RowFilter = "";
                }
                else
                {
                    table1.DefaultView.RowFilter = "c2 like '*" + t1 + "'";
                }

                dataView2.DataSource = table1;
                //checkQueryStatus2();
            }
            catch
            {
                MessageBox.Show("查询失败！", "数据操作", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }
        }

        private void btnFilter1_Click(object sender, EventArgs e)
        {
            try
            {
                string t1 = cbSensor1.Text;
                DataTable table1 = (DataTable)dataView1.DataSource;

                if (t1 == "所有数据")
                {
                    table1.DefaultView.RowFilter = "";
                }
                else
                {
                    table1.DefaultView.RowFilter = "c3 like '*" + t1 + "'";
                }

                dataView1.DataSource = table1;
                checkQueryStatus1();
            }
            catch
            {
                MessageBox.Show("查询失败！", "数据操作", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }
        }

        private void toolBtnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        private void toolBtnRefresh_Click(object sender, EventArgs e)
        {
            int pageIndex = tabControl1.SelectedIndex;
            if(pageIndex == 1)
            {
                initState1();
            }
            else if(pageIndex == 2)
            {
                initState2();
            }
        }


        private void timer2_Tick(object sender, EventArgs e)
        {
            AlarmComp.compFusionAlarm();
        }

  
        //20171004
        //增加帮助和软件版权说明
        private void toolBtnHelp_Click(object sender, EventArgs e)
        {
            AboutBox1 ok = new AboutBox1();
            ok.ShowDialog();
        }

        private void StopLight_Click(object sender, EventArgs e)
        {
            notice.SendLight("0");
        }

    }
}
