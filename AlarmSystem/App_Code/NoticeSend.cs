using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.IO.Ports;
using GSMMODEM;
using System.Data.SqlClient;
using System.Data;
using DataAccess;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;



/// <summary>
/// NoticeSend 的摘要说明
/// </summary>
public class NoticeSend
{
    //private GSMModem gm = new GSMModem();
    //委托 收到短信的回调函数委托
    delegate void UpdataDelegate();         //可以有参数，本处不需要
    UpdataDelegate UpdateHandle = null;

    DataOperation dbo = new DataOperation();

    TcpClient smtpSrv;
    NetworkStream netStrm;
    string CRLF = "\r\n";

    [DllImport("Ux64_dllc.dll")]
    public static extern void Usb_Qu_Open();
    [DllImport("Ux64_dllc.dll")]
    public static extern void Usb_Qu_Close();
    [DllImport("Ux64_dllc.dll")]
    public static unsafe extern bool Usb_Qu_write(byte Q_index, byte Q_type, byte* pQ_data);
    [DllImport("Ux64_dllc.dll")]
    public static extern int Usb_Qu_Getstate();
    unsafe public void SendLight(string a)
    {
        


      if(a=="1")
        {
            

            const byte C_lampoff = 0;
            const byte C_lampon = 1;
            const byte C_lampblink = 2;
            const byte C_D_not = 100;  //  // Don't care  // Do not change before state
            bool bchk = false;
            byte* bbb = stackalloc byte[6];
            bbb[0] = C_lampblink;//红灯，0代表关，1代表开，2代表闪烁；
            bbb[1] = C_lampon;//黄灯
            bbb[2] = C_lampon;//绿灯
            bbb[3] = C_lampoff;//蓝灯
            bbb[4] = C_lampoff;//白灯
            bbb[5] = 4; // 声音
            bchk = Usb_Qu_write(0, 0, bbb);
        }
        if(a=="0")
        {
            const byte C_lampoff = 0;
            const byte C_lampon = 1;
            const byte C_lampblink = 2;
            const byte C_D_not = 100;  //  // Don't care  // Do not change before state
            bool bchk = false;
            byte* bbb = stackalloc byte[6];
            bbb[0] = C_lampoff;//红灯，0代表关，1代表开，2代表闪烁；
            bbb[1] = C_lampoff;//黄灯
            bbb[2] = C_lampoff;//绿灯
            bbb[3] = C_lampoff;//蓝灯4
            bbb[4] = C_lampoff;//白灯
            bbb[5] = 0; // 声音
            bchk = Usb_Qu_write(0, 0, bbb);
        }
        
    }

	public NoticeSend()
	{
		//
		// TODO: 在此处添加构造函数逻辑
		//
	}

    //public void SendMail(string tmpUser,string tmpInfo)
    //{
    //    string tmpSubject = "排土场灾害预警";
        
    //    //管理员邮箱信息提取
    //    char[] tmpChar = { ';' };
    //    string[] tmpStr = tmpUser.Split(tmpChar);
    //    if(tmpStr.Length>0)
    //    {
    //        for(int i=0;i<tmpStr.Length;i++)
    //        {
    //            sendMail1("",tmpStr[i],tmpSubject,tmpInfo);
    //        }
    //    }
    //}
 

    //Created by ZXM on Oct 10,2008
    //can be for three progress of start,middle check and end stages.
//    protected void EmailNotice(string stageName)
//    {
        
//        DataSet dsItem = new DataSet();
//        DataSet dsEmail = new DataSet();
//        string tmpItem = "";
//        string tmpProgress = "";
//        string tmpStudentEmail = "";
//        string tmpTeacherEmail = "";
//        string tmpStudentName = "";
//        string tmpTeacherName = "";
//        string tmpNoFinish = "";

//        string strSrvName = "mail.bipt.edu.cn";
//        string tmpSubject = "URT" + stageName + "报告问题重要通知";
//        string tmpBody = "您好，到目前为止，您的URT" ;

//                //Start to send the emails
//                jmail.MessageClass reportMsg = new jmail.MessageClass();
//                reportMsg.Charset = "GB2312";
//                reportMsg.Encoding = "BASE64";
//                reportMsg.ContentType = "text/html";
//                reportMsg.ISOEncodeHeaders = false;
//                reportMsg.Priority = Convert.ToByte(1);

//                reportMsg.From = "urt@bipt.edu.cn";
//                reportMsg.FromName = "URT管理员";
//                reportMsg.Subject = tmpSubject;
//                reportMsg.MailServerUserName = "urt";
//                reportMsg.MailServerPassWord = "111111";
//                //reportMsg.ReplyTo 
//                //reportMsg.AddRecipient(smtpMailTo, "", "");
//                reportMsg.Body = tmpBody;

//                for (int i = 0; i < tmpNum; i++)
//                {
//                    tmpItem = dsItem.Tables[0].Rows[i][0].ToString();
//                    dsEmail = dbo.GetNoReportEmailInfo(tmpItem);  //get the emails 

//of students and teachers for the itmes of no reports submission
//                    tmpStudentEmail = dsEmail.Tables[0].Rows[0][0].ToString();
//                    tmpStudentName = dsEmail.Tables[0].Rows[0][1].ToString();
//                    tmpTeacherEmail = dsEmail.Tables[0].Rows[0][2].ToString();
//                    tmpTeacherName = dsEmail.Tables[0].Rows[0][3].ToString();

//                    if (tmpStudentEmail != "")
//                    {
//                      //reportMsg.AddRecipient("urt@bipt.edu.cn", "", "");
//                        reportMsg.AddRecipient(tmpStudentEmail, "", "");
//                    }
//                    if (reportMsg.Send(strSrvName, false))
//                    {
//                        reportMsg.Recipients.Clear(); //clear
//                    }
                    
//                    //test the teacher email
//                    if (tmpTeacherEmail != "")
//                    {
//                        //reportMsg.AddRecipient("zhangxiaoming@bipt.edu.cn", "", 

//"");
//                        reportMsg.AddRecipient(tmpTeacherEmail, "", "");
//                    }
//                    if (reportMsg.Send(strSrvName, false))
//                    {
//                        reportMsg.Recipients.Clear(); //clear
//                    }
                    

//                //sendMail("urt", "111111", "urt@bipt.edu.cn", "urt@bipt.edu.cn", stageName, "URT管理员");
//                //sendMail2("urt", "111111", "urt@bipt.edu.cn", "urt@bipt.edu.cn", stageName, "URT管理员");
//                //sendMail3("urt", "111111", "urt@bipt.edu.cn", zhangxiaoming@bipt.edu.cn", stageName, "URT管理员");
//                //sendMail4("urt", "111111", "urt@bipt.edu.cn", "zhangxiaoming@bipt.edu.cn", stageName, "URT管理员");//successfully
//                Response.Write("<script type='text/javascript'>alert('邮件发送成功！')</script>");
//            }
//        }
//        catch (Exception ex)
//        {
//            Response.Write("<script type='text/javascript'>alert('邮件通知错误！')</script>");
//        }

//    }



    //Created by ZXM on Sept 18,2008
    public string sendMail(string smtpServer,string smtpUserName, string smtpPwd, string tmpFromEmail,string tmpToEmail, string tmpSubject,string tmpBody)
    {
        try
        {
            string data;

            //smtpSrv = new TcpClient(smtpServer, 25);
            //QQ邮箱SMTP服务器地址为“smtp.qq.com”,SMTP服务器需要身份验证。如果是设置SMTP的SSL加密方式，则SMTP服务器端口为465或587。
            smtpSrv = new TcpClient(smtpServer,587);
            netStrm = smtpSrv.GetStream();
            StreamReader rdStrm = new StreamReader(smtpSrv.GetStream());
            WriteStream("EHLO Local");
            WriteStream("AUTH LOGIN");
            data = smtpUserName;
            data = AuthStream(data);
            WriteStream(data);
            data = smtpPwd;
            data = AuthStream(data);
            WriteStream(data);


            //管理员邮箱信息提取
            char[] tmpChar = { ';' };
            string[] tmpStr = tmpToEmail.Split(tmpChar);
            if (tmpStr.Length > 0)
            {
                for (int i = 0; i < tmpStr.Length; i++)
                {
                    data = "MAIL FROM: <" + tmpFromEmail + ">";
                    WriteStream(data);
                    data = "RCPT TO:<" + tmpStr[i] + ">";
                    WriteStream(data);
                    WriteStream("DATA");
                    data = "Date:" + DateTime.Now;
                    WriteStream(data);
                    data = "From:" + tmpFromEmail;
                    WriteStream(data);
                    data = "TO:" + tmpStr[i];
                    WriteStream(data);
                    data = "SUBJECT:" + tmpSubject;
                    WriteStream(data);
                    WriteStream("");//The end of the letter header
                    data = tmpStr[i] + ",您好。";
                    WriteStream(data);
                    data = tmpBody;
                    WriteStream(data);
                    WriteStream("");
                    //WriteStream("此邮件为管理系统通知，勿需回复。");
                    WriteStream("");
                    WriteStream(".");
                    WriteStream("QUIT");

                    //emailNotice.Send(smtpMailFrom, tmpStr[i], tmpSubject, tmpBody);
                }
            }

            netStrm.Close();
            rdStrm.Close();
            return "1";
        }
        catch (Exception ex)
        {
            return ex.ToString();
        }
    }


    //采用MailMesage方法
    public string sendMail2(string smtpServer, string smtpUserName, string smtpPwd, string tmpFromEmail, string tmpToEmail, string tmpSubject, string tmpBody)
　　{
        string strNull = "0";
　　　　try 
　　　　{ 
           //管理员邮箱信息提取
            char[] tmpChar = { ';' };
            string[] tmpStr = tmpToEmail.Split(tmpChar);
            if (tmpStr.Length > 0)
            {
                for (int i = 0; i < tmpStr.Length; i++)
                {
                    int nContain = 0;
                    ///添加发件人地址 
                    string from = tmpFromEmail;
                    MailMessage mailMsg = new MailMessage();
                    mailMsg.From = new MailAddress(from);
                    nContain += mailMsg.From.Address.Length;
                    ///添加收件人地址 
                    mailMsg.To.Add(tmpStr[i]);
                    nContain += mailMsg.To.ToString().Length;
                    ///添加邮件主题 
                    mailMsg.Subject = tmpSubject;
                    mailMsg.SubjectEncoding = Encoding.UTF8;
                    nContain += mailMsg.Subject.Length;
                    ///添加邮件内容 
                    mailMsg.Body = tmpBody;
                    mailMsg.BodyEncoding = Encoding.UTF8;
                    mailMsg.IsBodyHtml = true;
                    nContain += mailMsg.Body.Length;
                    if (mailMsg.IsBodyHtml == true)
                    {
                        nContain += 100;
                    }
 
                    ///发送邮件 
                    //定义发送邮件的Client 
                    SmtpClient client = new SmtpClient();
                    //表示以当前登录用户的默认凭据进行身份验证　 
                    client.UseDefaultCredentials = true;
                    //包含用户名和密码　 
                    //client.Credentials　=　new　System.Net.NetworkCredential(application.GetapplicationSendmail(),　application.GetapplicationSendpass()); 
                    client.Credentials = new System.Net.NetworkCredential(smtpUserName, smtpPwd);
                    ///设置邮件服务器主机的IP地址 
                    client.Host = smtpServer;
                    ///设置邮件服务器的端口 
                    client.Port = 25;
                    ///配置发送邮件的属性 
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    //System.Net.Mail.MailMessage　message　=　new　System.Net.Mail.MailMessage(strFrom,　strto,　strSubject,　strBody);　 
                    mailMsg.Priority = System.Net.Mail.MailPriority.Normal;
                    //client.UseDefaultCredentials　=　false; 
                    ///发送邮件 
                    client.Send(mailMsg);
                    strNull = "1";
                }
            }
            return strNull;
　　　　} 
　　　　catch　(Exception　ex)　{　return　ex.ToString();　} 
　　}


    public string sendMail126(string smtpServer, string smtpUserName, string smtpPwd, string tmpFromEmail, string tmpToEmail, string tmpSubject, string tmpBody)
    {
        string strNull = "";

        try
        {
            //管理员邮箱信息提取
            char[] tmpChar = { ';' };
            string[] tmpStr = tmpToEmail.Split(tmpChar);
            if (tmpStr.Length > 0)
            {
                for (int i = 0; i < tmpStr.Length; i++)
                {
                    SmtpClient client = new SmtpClient(smtpServer, 25)
                    {
                        Credentials = new NetworkCredential(smtpUserName, smtpPwd),
                        EnableSsl = true
                    };

                    MailAddress from = new MailAddress(tmpFromEmail);
                    MailAddress to = new MailAddress(tmpStr[i]);
                    MailMessage myMail = new System.Net.Mail.MailMessage(from, to);

                    myMail.Subject = tmpSubject;
                    myMail.SubjectEncoding = System.Text.Encoding.UTF8;
                    myMail.Body = tmpBody;

                    myMail.BodyEncoding = System.Text.Encoding.UTF8;
                    client.Send(myMail);

                    strNull = "1";
                }
            }
            return strNull;
        }
        catch (SmtpException ex)
        {
            return ex.ToString();
        }
    }  // end of function 


 
    private void WriteStream(string strCmd)
    {
        strCmd += CRLF;
        byte[] bw = System.Text.Encoding.Default.GetBytes(strCmd.ToCharArray());
        netStrm.Write(bw, 0, bw.Length);
    }


    private string AuthStream(string strCmd)
    {
        try
        {
            byte[] by = System.Text.Encoding.Default.GetBytes(strCmd.ToCharArray());
            strCmd = Convert.ToBase64String(by);
        }
        catch (Exception ex)
        {
            return ex.ToString();
        }
        return strCmd;
    }


 
    public bool SendMsg(string tmpPort,int tmpRate,string tmpUser,string tmpInfo)
    {
        GSMModem gm = new GSMModem();
        int b = 1;
        char[] tmpChar = { ';' };
        string[] tmpStr = tmpUser.Split(tmpChar);
        try
        {
            gm.ComPort = tmpPort;
            gm.BaudRate = tmpRate;
            //gm.OnRecieved += new GSMModem.OnRecievedHandler(gm_OnRecieved);
            //UpdateHandle = new UpdataDelegate(UpdateLabel8);        //实例化委托

            if (gm.IsOpen == false)
            {
                gm.Open();
            }

            //管理员手机信息提取
            //char[] tmpChar = { ';' };
            //string[] tmpStr = tmpUser.Split(tmpChar);

           
            
                for (int i = 0; i < tmpStr.Length; i++)
                {
                    
                    try {
                        gm.Close();
                        Thread.Sleep(1000);
                        gm.Open();
                        Thread.Sleep(1000);
                    gm.SendMsg(tmpStr[i], tmpInfo);
                    
                        }
                    catch
                    {
                        try { 
                        gm.Close();
                        Thread.Sleep(1000);
                        gm.Open();
                        Thread.Sleep(1000);
                        gm.SendMsg(tmpStr[i+1], tmpInfo);
                            }
                        catch
                        {
                            try
                            {
                                gm.Close();
                                Thread.Sleep(1000);
                                gm.Open();
                                Thread.Sleep(1000);
                                gm.SendMsg(tmpStr[i + 2], tmpInfo);
                                b = b / i;
                            }
                            catch
                            {
                                try
                                {
                                    gm.Close();
                                    Thread.Sleep(1000);
                                    gm.Open();
                                    Thread.Sleep(1000);
                                    gm.SendMsg(tmpStr[i + 3], tmpInfo);
                                    b = b / i;
                                }
                                catch
                                {
                                    try 
                                    {
                                        gm.Close();
                                        Thread.Sleep(1000);
                                        gm.Open();
                                        Thread.Sleep(1000);
                                        gm.SendMsg(tmpStr[i + 4], tmpInfo);
                                        b=b / i;
                                    }
                                    catch
                                    {
                                        try 
                                        {
                                            gm.Close();
                                            Thread.Sleep(1000);
                                            gm.Open();
                                            Thread.Sleep(1000);
                                            gm.SendMsg(tmpStr[i + 5], tmpInfo);
                                            b = b / i;
                                        }
                                        catch
                                        {
                                            try
                                            {
                                                gm.Close();
                                                Thread.Sleep(1000);
                                                gm.Open();
                                                Thread.Sleep(1000);
                                                gm.SendMsg(tmpStr[i + 6], tmpInfo);
                                                b = b / i;
                                            }
                                            catch
                                            {
                                                try
                                                {
                                                    gm.Close();
                                                    Thread.Sleep(2000);
                                                    gm.Open();
                                                    Thread.Sleep(2000);
                                                    gm.SendMsg(tmpStr[i + 7], tmpInfo);
                                                    b = b / i;
                                                }
                                                catch
                                                {
                                                    gm.Close();
                                                    Thread.Sleep(2000);
                                                    gm.Open();
                                                    Thread.Sleep(1000);
                                                    gm.SendMsg(tmpStr[i + 8], tmpInfo);
                                                    b = b / i;
                                                }
                                               
                                            }
                                            
                                        }
                                       

                                    }
                                    
                                }
                                
                            }
                            
                        }
                    }
                   
                    
                }
            
            if (gm.IsOpen == true)
            {
                gm.Close();
            }
            return true;
        }
        catch
        {
            if (gm.IsOpen == true)
            {
                gm.Close();
            }
            gm.Open();
            
            return true;
        }
    }

    void gm_OnRecieved(object sender, EventArgs e)
    {
        //Invoke(UpdateHandle, null);
    }

    void UpdateLabel8()
    {
        //label8.Text = "有新消息";
        //label8.ForeColor = Color.Green;
    }

}