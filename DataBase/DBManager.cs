using UnityEngine;
using System.Data;
using System.Data.SqlClient;
using System;
using UnityEngine.Windows;

public class DBManager : MonoBehaviour
{
    #region �̱��� ����
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

    #region �α��� / ���� ����
    // �α��� �Լ�
    // ���̵� �������� �ʰų� �н����� ����ġ �� false ��ȯ(�˾��� ������ res�� ��ȯ)
    // �α��� ���� �� last_date ���� �� true ��ȯ
    public bool LogIn(string inputID, string pwd, out string res)
    {
        res = "";
        // DB ����
        connection.Open();

        SqlCommand cmd = new SqlCommand();
        cmd.Connection = connection;

        cmd.CommandText = $"SELECT * FROM players WHERE ID = '{inputID}'";
        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
        DataSet ds = new DataSet();
        adapter.Fill(ds, "players");

        DataTable table = ds.Tables[0];
        int count = table.Rows.Count;
        // �ش� ���̵� �������� �ʴ� ���
        if (count == 0)
        {
            res = "���̵� �������� �ʽ��ϴ�.";
            connection.Close();
            return false;
        }

        // �н����尡 ����ġ�ϴ� ���
        if (table.Rows[0]["password"].ToString() != pwd)
        {
            res = "��й�ȣ�� ��ġ���� �ʽ��ϴ�.";
            connection.Close();
            return false;
        }

        // �α��� ���� �� ������ ���� �ð� ����
        cmd.CommandText = "UPDATE players SET last_date = @last_date WHERE ID = @ID";
        string date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        cmd.Parameters.Add("@ID", SqlDbType.VarChar).Value = inputID;
        cmd.Parameters.Add("@last_date", SqlDbType.DateTime).Value = date;
        cmd.ExecuteNonQuery();

        // ��Ʈ��ũ �Ŵ����� ���̵� ����
        GameManager.Instance.SaveMyName(inputID);

        // DB ���� ����
        connection.Close();
        return true;
    }

    // ���� ����
    // �Էµ� ���̵� ���� �� false ��ȯ
    // ���� �����ϴٸ� DB�� ��� �� true ��ȯ
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
        if(count > 0) // ���̵� ���� ��
        {
            connection.Close();
            return false;
        }

        // ������ ����
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
    #endregion

    #region ���� ī��Ʈ
    public void WinCountUpdate(string myId, bool isWinner)
    {
        connection.Open();

        SqlCommand cmd = new SqlCommand();
        cmd.Connection = connection;

        cmd.CommandText = $"SELECT win, loose FROM players WHERE ID = '{myId}'";
        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
        DataSet ds = new DataSet();
        adapter.Fill(ds, "players");

        DataTable table = ds.Tables[0];
        int count = table.Rows.Count;
        if (count == 0) // �������� �ʴ� ���̵��� ���
        {
            Debug.Log("�������� �ʴ� ���̵��Դϴ�. from DBManager");
            connection.Close();
            return;
        }

        // ������ ��� win, ������ ��� loose ����
        if(isWinner)
        {
            cmd.CommandText = "UPDATE players SET win = @win WHERE ID = @ID";
            cmd.Parameters.Add("@ID", SqlDbType.VarChar).Value = myId;
            cmd.Parameters.Add("@win", SqlDbType.Int).Value = Convert.ToInt32(table.Rows[0]["win"]) + 1;
        }
        else
        {
            cmd.CommandText = "UPDATE players SET loose = @loose WHERE ID = @ID";
            cmd.Parameters.Add("@ID", SqlDbType.VarChar).Value = myId;
            cmd.Parameters.Add("@loose", SqlDbType.Int).Value = Convert.ToInt32(table.Rows[0]["loose"]) + 1;
        }
        cmd.ExecuteNonQuery();

        connection.Close();
    }

    // �¼�, �й��, �·� ������ �����ϴ� �Լ�
    // �·��� ����� �Ұ����� ��� -1(���ǵ� �� �� ���)
    // �ݵ�� 2ĭ �̻��� �迭 ������ ��
    public void GetWinRateData(string myId, int[] arr, out float winRate)
    {
        winRate = -1;

        connection.Open();

        SqlCommand cmd = new SqlCommand();
        cmd.Connection = connection;

        cmd.CommandText = $"SELECT win, loose FROM players WHERE ID = '{myId}'";
        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
        DataSet ds = new DataSet();
        adapter.Fill(ds, "players");

        DataTable table = ds.Tables[0];
        int count = table.Rows.Count;
        if (count == 0) // �������� �ʴ� ���̵��� ���
        {
            Debug.Log("�������� �ʴ� ���̵��Դϴ�. from DBManager");
            connection.Close();
            return;
        }

        arr[0] = Convert.ToInt32(table.Rows[0]["win"]);
        arr[1] = Convert.ToInt32(table.Rows[0]["loose"]);
        int total = arr[0] + arr[1];
        if (total > 0)
            winRate = (float)arr[0] / total * 100;

        connection.Close();
    }
    #endregion
}