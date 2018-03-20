//using System;
//using System.Data;
//using System.Configuration;
//using System.Web;
//using System.Web.Security;
//using System.Web.UI;
//using System.Web.UI.WebControls;
//using System.Web.UI.WebControls.WebParts;
//using System.Web.UI.HtmlControls;
//using System.Data.SqlClient;
////using System.Web.Mail;
//using System.IO;

using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Data.SqlClient;
using System.Xml;

/// <summary>
/// Data 的摘要说明
/// </summary>
/// 
namespace DataAccess
{
    public class DataOperation
    {
        public static SqlConnection dbConn = null;
        //public static MySqlConnection dbConn = null;
        public static string dataConStr;
        //private static string dsServer;
        public static string dsServer;
        public static string dataConFrame;

        #region 初始化数据库配置
        public static void InitSqlConf()
        {
            string s = "";
            string connString = "";
            string connFrame = "";  //保存Framework连接

            string serName = "";
            XmlDocument xmlDoc = new XmlDocument();
            //xmlDoc.Load(System.Windows.Forms.Application.StartupPath + "/ServerConf.xml");
            xmlDoc.Load(System.Windows.Forms.Application.StartupPath + "/SQLServerConf.xml");
            XmlNodeReader reader = new XmlNodeReader(xmlDoc);
            try
            {
                while (reader.Read())
                {
                    //判断当前读取得节点类型
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            s = reader.Name;
                            break;
                        case XmlNodeType.Text:
                            if (s.Equals("Server"))
                            {
                                serName = "";
                                connString += "Data Source=" + reader.Value;
                                connFrame += "Data Source=" + reader.Value;
                            }
                            else if (s.Equals("dataDS"))
                            {
                                serName = "dataDS";
                                connString += ";Database=" + reader.Value;
                                connFrame += ";Database=" + "FrameWork";
                            }
                            else if (s.Equals("dbName"))
                            {
                                dsServer = reader.Value;
                            }
                            else if (s.Equals("User"))
                            {
                                connString += ";Uid=" + reader.Value;
                                connFrame += ";Uid=" + reader.Value;
                            }
                            else if (s.Equals("Passwd"))
                            {
                                connString += ";PWD=" + reader.Value;
                                connFrame += ";PWD=" + reader.Value;
                                if (serName == "dataDS")
                                {
                                    dataConStr = connString;    //获得排土场数据库的连接
                                    dataConFrame = connFrame;   //获得FrameWork数据库的连接
                                    connString = "";
                                    connFrame = "";
                                    //MessageBox.Show(msmConStr);
                                }
                            }
                            break;
                    }
                }
            }
            finally
            {
                //清除打开的数据流
                if (reader != null)
                    reader.Close();
            }
        }

        public static void InitDbcon()
        {
            if (dbConn == null)
            {
                dbConn = new SqlConnection(dataConStr);
                dbConn.Open();
            }
        }
        #endregion

        #region 数据库连接变量区

        //MySQL的连接

        //MySqlConnection dbConn;
        //MySqlConnection dbConn = new MySqlConnection(DbAccess.InitConf.dataConStr);
        
        //SQLServer的连接
        //SqlConnection dbCon = new SqlConnection(DbAccess.InitConf.dataConStr);
        //DateTime dtStartTime, dtEndTime;//日期信息
    
        #endregion

   
   
        #region 建立数据库连接
        public static SqlConnection CreateConnect(string constr)
        {
            try
            {
                dbConn = new SqlConnection();
                dbConn.ConnectionString = constr;
                dbConn.Open();
            }
            catch (Exception ex)
            {
                dbConn = null;
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
            return dbConn;
        }
        #endregion

        #region 关闭数据库连接
        public static void CloseConnect()
        {
            dbConn.Close();
            dbConn.Dispose();
        }
        #endregion
        #region 用户操作提示信息

        public static string AddOperationSuccess = "添加数据成功！！！";
        public static string AddOperationFailure = "添加数据失败！！！";

        public static string ChangeOperationSuccess = "修改数据项成功！";
        public static string ChangeOperationFailure = "修改数据项失败！";

        public static string DeleteOperationSuccess = "删除数据项成功！";
        public static string DeleteOperationFailure = "删除数据项失败！";

        public static string SelectOperation = "请选择操作的数据项！";
        public static string DeleteOperationgConfirm = "你确定要删除所选择的数据项吗？";
        public static string UpdateOperationgConfirm = "你确定要更改所选择的数据项吗？";

        public static string InputIsNull = "输入项不能为空！";
        public static string InputIsNotIntegerPositive = "输入项不是正整数！";
        public static string InputYear = "年份不能为空！";
        public static string InputItemName = "名称不能为空！";
        public static string InputSameItemNameError = "项目名称已存在！";
        public static string InputError = "输入错误！";


        public static string InputPwd = "密码不能为空！";
        public static string InputLoginPwdError = "密码错误！";
        public static string InputLoginNameNull = "该用户不存在";
        public static string LoginSuccess = "用户登录成功！";

        public static string FileUploadPath = "请选择您要上传的文件！";
        public static string InputSizeValidate = "描述大小超出限制！";
        public static string PromptInfo = "您无权查看。请与系统管理员联系，为您分配部门！";
        public static string QueryFailure = "查询操作失败！";
        public static string QueryIsNull = "查询结果为空！";
        public static string QuerySuccess = "查询操作成功！";

        public static string SelectItem = "请选择您要修改的数据项！";
        public static string DeleteItem = "请选择您要删除的数据项！";
        public static string SelectLine = "请选择您要修改的数据行！";
        public static string DeleteLine = "请选择您要删除的数据行！";

        public static string ChangeUserInfoSuccess = "用户信息修改成功！";
        public static string ChangeUserInfoFailure = "用户信息修改失败！";

        public static string InputDictCodeError = "缺少码表数据项！";
        public static string InputDictError = "缺少数据字典！";

        public static string CheckDataConsistenceFailure = "数据重复性检查失败！";

        public static string OperationError = "操作失败！";


        public static string SearchResultNull = "查询结果为空！";
        public static string SearchConNull = "查询条件为空，请重新设置！";
        public static string ChangeAnotherRole = "所选角色与原角色一致，请重新选择角色！";
        public static string UserExisted = "该用户已存在，请重新选择需要添加的用户！";
        public static string RoleIsConfiged = "该角色已配置功能，不能删除！";
        public static string ConfigOperationSuccess = "角色功能配置成功！！！";
        public static string ConfigOperationFailure = "角色功能配置失败！！！";
        public static string RoleExisted = "该角色名称已存在，请重新填写！";
        public static string RoleNotExisted = "该角色名称可用！";
        public static string SelectEmployee = "请选择员工！";
        public static string SelectRole = "请选择角色！";

        public static string ImageTypeError = "图像格式错误，应选择JPEG格式！";


        #endregion

        //SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["IRMConnection"].ConnectionString);
        public DataOperation()
        {
            //
            // TODO: 在此处添加构造函数逻辑
            //
        }



        //#region 打开和关闭数据库连接 (2008/3/23启用)
        //private void open(SqlCommand cmd)
        //{
        //    if (cmd.Connection.State == ConnectionState.Open)
        //    {
        //        cmd.Connection.Close();
        //    }
        //    cmd.Connection.Open();
        //}
        //private void close(SqlCommand cmd)
        //{
        //    if (cmd.Connection.State == ConnectionState.Open)
        //    {
        //        cmd.Connection.Close();
        //    }
        //}
        //#endregion


  

        //#region sys_User表的操作
        //public int GetGroupId(string strUserLoginName)//获取管理员的所属部门
        //{
        //    int Id;
        //    SqlCommand cmd_Get = new SqlCommand("GetUserGroupId",conn);
        //    cmd_Get.CommandType = CommandType.StoredProcedure;
        //    cmd_Get.Parameters.Add(new SqlParameter("strUserLoginName", strUserLoginName));
        //    if (cmd_Get.Connection.State == ConnectionState.Open)
        //    {
        //        cmd_Get.Connection.Close();
        //    }
        //    cmd_Get.Connection.Open();
        //    SqlDataReader cmd_readId = cmd_Get.ExecuteReader();
            
        //    if (cmd_readId.Read())
        //    {
        //        Id = Convert.ToUInt16(cmd_readId[0].ToString());
        //    }
        //    else
        //    {
        //        Id=0;
        //    }            
        //    cmd_Get.Connection.Close();
        //    cmd_readId.Close();
        //    return Id;
        //}

        //public string GetUserLoginName(DateTime dtMaxTime, DateTime dtMinTime,string strHostIP)//获取管理员的登录名称
        //{
        //    string strLoginName;
        //    SqlCommand cmd_Get = new SqlCommand("GetUserInfo",conn);
        //    cmd_Get.CommandType = CommandType.StoredProcedure;
        //    cmd_Get.Parameters.Add(new SqlParameter("dtMaxTime",dtMaxTime));
        //    cmd_Get.Parameters.Add(new SqlParameter("dtMinTime", dtMinTime));
        //    cmd_Get.Parameters.Add(new SqlParameter("strHostIP", strHostIP));

        //    if (cmd_Get.Connection.State == ConnectionState.Open)
        //    {
        //        cmd_Get.Connection.Close();
        //    }
        //    cmd_Get.Connection.Open();

        //    SqlDataReader cmd_readInfo = cmd_Get.ExecuteReader();
        //    if (cmd_readInfo.Read())
        //    {
        //        strLoginName = cmd_readInfo[0].ToString();
        //    }
        //    else
        //    {
        //        strLoginName = "";
        //    }
        //    cmd_Get.Connection.Close();
        //    cmd_readInfo.Close();
        //    return strLoginName;
        //}

        //public string GetClientIP() //获取客户端的IP地址
        //{ 
        //    string result = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"]; 
        //    if (null == result || result == String.Empty) 
        //    {
        //        result = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"]; 
        //    } 
        //    if (null == result || result == String.Empty) 
        //    { 
        //        result = HttpContext.Current.Request.UserHostAddress;
        //    } 
        //    return result; 
        //} 
        //#endregion


        #region 通用数据接口函数

        #region 执行返回数据集的存储过程
        public DataSet GetDataSetWithParams(string sprocName, SqlParameter[] param)
        {
            SqlCommand cmd = new SqlCommand(sprocName, dbConn);
            cmd.CommandType = CommandType.StoredProcedure;
            if (cmd.Connection.State == ConnectionState.Open)
            {
                cmd.Connection.Close();
            }
            cmd.Connection.Open();
            if (param != null)
            {
                foreach (SqlParameter para in param)
                    cmd.Parameters.Add(para);
            }
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            sda.Fill(ds);
            sda.Dispose();
            cmd.Connection.Close();
            return ds;
        }
        #endregion


        #region 执行无参数有返回数据集的存储过程
        public DataSet GetDataSetWithOutParams(string sprocName)
        {
            SqlCommand cmd = new SqlCommand(sprocName, dbConn);
            cmd.CommandType = CommandType.StoredProcedure;
            if (cmd.Connection.State == ConnectionState.Open)
            {
                cmd.Connection.Close();
            }
            cmd.Connection.Open();

            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            sda.Fill(ds);
            sda.Dispose();
            cmd.Connection.Close();
            return ds;
        }
        #endregion

        #region 执行带参数无返回值的存储过程
        public void ExecuteProcWithParams(string sprocName, SqlParameter[] param)
        {
            SqlCommand cmd = new SqlCommand(sprocName, dbConn);
            cmd.CommandType = CommandType.StoredProcedure;
            if (cmd.Connection.State == ConnectionState.Open)
            {
                cmd.Connection.Close();
            }
            cmd.Connection.Open();

            if (param != null)
            {
                foreach (SqlParameter para in param)
                    cmd.Parameters.Add(para);
            }
            cmd.ExecuteNonQuery();
            cmd.Connection.Close();
        }
        #endregion
        #endregion

    }
}
