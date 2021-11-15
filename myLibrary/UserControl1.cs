using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Data.SqlClient;

namespace myLibrary
{
    //아래 클래스 삭제하지 말 것
    public partial class formInput: Form //여기에 Form을 넣으면 바뀜
    {
        public formInput()
        {
            InitializeComponent();
            //return 존재X 왜냐면 return type을 지정하지 않았으니까..
        }

        public formInput(String str)
        {
            InitializeComponent();
            label1.Text = str;
            label1.Visible = true;
            //Key, 입력을 받고 처리 CR ESC 이용..
        }

        public string textReturn = "";

        public void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                textReturn = textBox1.Text;
                DialogResult = DialogResult.OK;
                Close();
            }
            if (e.KeyCode == Keys.Escape)
            {
                textReturn = "";
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }
    }

    //static class에는 static 메서드가 필요함
    public static class mylib //생성자 없으면 기본 생성자로 대체됨
    {
        /// GetToken(int n, string str, char c);
        /// int n: n번째 아이템
        ///      string str: 문자열
        ///      char c: 구분자
        ///  설명) 문자열 str에 있는 데이터 중 구분자 d를 통해
        ///        필드를 구분하여 그 중 n번째 데이터을 반환
        ///  예시) GetToken(1, "a,b,c,d" ',')  returns 'b'
        public static string GetToken(int n, string str, char c)
        {
            string[] sArr = str.Split(c);
            if (n < sArr.Length) return sArr[n];
            return "";
        }

        
        // My Non-Split Version of GetToken(n, str, d)
        public static string GetTokenEx(int n, string str, char d)
        {

            if (str.Contains(d))
            {
                var items = new List<string>();
                string item = "";
                for (int i = 0; i < str.Length; i++)
                {
                    if (str[i] != d)
                        item += str[i];
                    if (str[i] == d || i == str.Length - 1)
                    {
                        items.Add(item);
                        item = "";
                    }

                }
                string[] ITEMS = items.ToArray();
                if (n <= ITEMS.Length && n >= 0) // n > 0으로 되어 있었음..
                    return ITEMS[n];
                else return "index out of bounds";
            }
            else return $"does not contain \'{d}\'";
        }

        // 강사님 Ver
        // str의 적합한 인덱스 start~end를 체크해서
        //   str의 substring을 반환
        public static string GetTokenEx2(int n, string str, char d)
        {

            if (str.Contains(d))
            {
                int i, j, k, start, end;
                for (i = j = k = start = end = 0; i < str.Length; i++)
                {
                    if (str[i] == d) k++;
                    if (k == n) start = i;
                    if (k == n + 1) end = i;
                }
                if (start == 0) return "";
                if (end == 0) end = str.Length;
                return str.Substring(start, end - (start + 1));
            }
            else return "";
        }

        public static string GetInput(string strPrompt) // static: 메모리에 고정
        {
            formInput dlg = new formInput(strPrompt);
            if (dlg.ShowDialog() == DialogResult.OK)
                return dlg.textReturn;
            return "";
        }

    }

    /// <summary>
    /// 
    /// Class 명: iniClass
    /// 클래스 기능: GetPrivateProfileString /  WritePrivateProfileString 함수를 
    ///     쉽게 사용할 수 있도록 중간 변환 해주는 클래스
    /// 주요 메서드: GetString(string sec, string key)
    ///              WriteString(string sec, string key, string val)
    /// 생성자: iniClass("iniFile 경로")
    ///      
    /// </summary>
    /// 
    public class iniClass
    {
        public string iniPath = "";
        public iniClass(string fp)
        {
            iniPath = fp;
        }


        [DllImport("kernel32.dll")] //WIN32 SDK
        static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder buf, int bSize, string path);
        [DllImport("kernel32.dll")] //이거 한개당 함수 한개씩 Import 가능 >> 왜 이런식이지?
        static extern bool WritePrivateProfileString(string section, string key, string val, string path);


        public string GetPString(string sec, string key) 
        {
            //buf(512): value의 최대 사이즈
            StringBuilder buf = new StringBuilder(512);
            GetPrivateProfileString(sec, key, "", buf, 512, iniPath);
            return buf.ToString();
        }

        //오버로드된 함수(default 처리)
        public string GetPString(string sec, string key, string def) 
        {
            //buf(512): value의 최대 사이즈
            StringBuilder buf = new StringBuilder(512);
            GetPrivateProfileString(sec, key, def, buf, 512, iniPath);
            return buf.ToString();
        }

        public bool WritePString(string sec, string key, string val)
        {
            return WritePrivateProfileString(sec, key, val, iniPath);
        }


    }

    //iniClass와 동일한 클래스
    public class iniFile : iniClass
    {
        public iniFile(string fp) : base(fp) { } // base 키워드 주목
        //public iniFile(string fp) : iniClass.iniClass(fp) {} //C++ 랑 다르게 오류 발생
    }

    public class SqlDB 
    {
        SqlConnection conn = null;
        SqlCommand cmd = null;
        string ConnString = null;
        char[] ws = { ' ', '\t', '\r', '\n' };

        public SqlDB(string str)
        {
            ConnString = str;
            conn = new SqlConnection(ConnString);
            conn.Open();
            cmd = new SqlCommand("", conn);
        }

        //return DataTable if 'select'
        //else return number of rows affected (int)
        public object Run(string sql) //다댱한 return type 존재함
        {
            
            try
            {
                cmd.CommandText = sql.Trim();
                //string.TrimEnd(), TrimStart()
                if (sql.Trim().Split(ws)[0].ToLower() == "select") //select문이라면..
                {
                    SqlDataReader sdr = cmd.ExecuteReader();
                    DataTable dt = new DataTable();
                    dt.Load(sdr); sdr.Close(); //sdr 안닫으면 dt 오류 발생 가능
                    return dt;
                }
                else
                {
                    return cmd.ExecuteNonQuery();
                }
            }
            catch(Exception e)
            {
                return null; //return type 일정하지 않음
            }            
        }

        


        // return 1st record/field data
        public string GetString(string sql)
        {
            try
            {
                cmd.CommandText = sql;
                object result = cmd.ExecuteScalar();
                if (result != null)
                    return result.ToString(); // 여기서 null 오류 발생
                else
                    return null; //
            }
            catch(Exception e)
            {   //라이브러리에서 오류나면 골치아픔
                return e.ToString(); 
            }
        }

        //select문의 결과를 팝업창으로 반환
        public void Render(string sql)
        {
            try
            {
                cmd.CommandText = sql.Trim();
                //string.TrimEnd(), TrimStart()
                if (sql.Trim().Split(ws)[0].ToLower() == "select") //select문이라면..
                {
                    SqlDataReader sdr = cmd.ExecuteReader();
                    DataTable dt = new DataTable();
                    dt.Load(sdr); sdr.Close(); //sdr 안닫으면 dt 오류 발생 가능
                    FormDB dlg = new FormDB(dt);  //여기서 formDB 호출
                    dlg.Show();
                    return;
                }
                else return;
            }
            catch (Exception e)
            {
                return;
                //MessageBox.Show(e.Message); //오류 뱉기
            }
        }

        public void Render(DataTable dt)
        {
            try
            {                
                FormDB dlg = new FormDB(dt);  //여기서 formDB 호출
                dlg.Show();
                return;                
            }
            catch (Exception e)
            {
                return;
                //MessageBox.Show(e.Message); //오류 뱉기
            }
        }
    }


}
