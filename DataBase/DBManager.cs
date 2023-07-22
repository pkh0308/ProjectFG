using UnityEngine;
using System.Data;
using System.Data.SqlClient;
using System;

public class DBManager : MonoBehaviour
{
    #region 싱글톤 구현
    public static DBManager Instance;
    
    void Awake()
    {
        Instance = this;
    }
    #endregion

    string dbConn;
    SqlConnection connection;

    void Start()
    {
        dbConn = "Data Source=192.168.0.2,1433;Initial Catalog=GameDB;User ID=pkh0308;Password=18otpakstp";
        connection = new SqlConnection(dbConn);
    }

    // 로그인 함수
    // 아이디가 존재하지 않거나 패스워드 불일치 시 false 반환(팝업용 문구는 res로 반환)
    // 로그인 성공 시 last_date 갱신 후 true 반환
    public bool LogIn(string inputID, string pwd, out string res)
    {
        res = "";
        // DB 연결
        connection.Open();

        SqlCommand cmd = new SqlCommand();
        cmd.Connection = connection;

        cmd.CommandText = $"SELECT * FROM players WHERE ID = '{inputID}'";
        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
        DataSet ds = new DataSet();
        adapter.Fill(ds, "players");

        DataTable table = ds.Tables[0];
        int count = table.Rows.Count;
        // 해당 아이디가 존재하지 않는 경우
        if (count == 0)
        {
            res = "아이디가 존재하지 않습니다.";
            connection.Close();
            return false;
        }

        // 패스워드가 불일치하는 경우
        if (table.Rows[0]["password"].ToString() != pwd)
        {
            res = "비밀번호가 일치하지 않습니다.";
            connection.Close();
            return false;
        }

        // 로그인 성공 시 마지막 접속 시간 갱신
        cmd.CommandText = "UPDATE players SET last_date = @last_date WHERE ID = @ID";
        string date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        cmd.Parameters.Add("@ID", SqlDbType.VarChar).Value = inputID;
        cmd.Parameters.Add("@last_date", SqlDbType.DateTime).Value = date;
        cmd.ExecuteNonQuery();

        // 네트워크 매니저에 아이디 저장
        NetworkManager.Instance.SaveMyName(inputID);

        // DB 연결 해제
        connection.Close();
        return true;
    }

    // 계정 생성
    // 입력된 아이디 존재 시 false 반환
    // 생성 가능하다면 DB에 등록 후 true 반환
    public bool CreateAccount(string inputID, string pwd)
    {
        connection.Open();

        SqlCommand cmd = new SqlCommand();
        cmd.Connection = connection;

        cmd.CommandText = $"SELECT * FROM players WHERE ID = '{inputID}'";
        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
        DataSet ds = new DataSet();
        adapter.Fill(ds, "players");

        DataTable table = ds.Tables[0];
        int count = table.Rows.Count;
        if(count > 0) // 아이디 존재 시
        {
            connection.Close();
            return false;
        }

        // 데이터 저장
        cmd.CommandText = "INSERT INTO players Values(@ID, @pass, @win, @loose, @create_date, @last_date)";
        string date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

        cmd.Parameters.Add("@ID", SqlDbType.VarChar).Value = inputID;
        cmd.Parameters.Add("@pass", SqlDbType.VarChar).Value = pwd;
        cmd.Parameters.Add("@win", SqlDbType.Int).Value = 0;
        cmd.Parameters.Add("@loose", SqlDbType.Int).Value = 0;
        cmd.Parameters.Add("@create_date", SqlDbType.DateTime).Value = date;
        cmd.Parameters.Add("@last_date", SqlDbType.DateTime).Value = date;
        cmd.ExecuteNonQuery();

        connection.Close();
        return true;
    }
}