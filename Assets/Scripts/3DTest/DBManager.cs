using MySql.Data.MySqlClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;


[SerializeField]
class DBClass
{
	public string ipAddress = "";
	public string db_id = "";
	public string db_pw = "";
	public string db_name = "";
	public string sensor_table = "a";//'tb_sensornode'
									 //public string sensor_value_table = "tb_sensingvalue";
	public string area_table = "a";
	public string sensor_id_table = "a";
	public string port = "";
}

public class DBManager : MonoBehaviour
{
	JsonParser jsonParser = new JsonParser();
	DBClass db;
	MySqlConnection conn;

	// Start is called before the first frame update
	void Start()
	{
		//Init();
	}

	// Update is called once per frame
	void Update()
	{

	}

	public void Init()
	{
		//DockerManager DM = new DockerManager();
		//DM.RunDocker();

		db = jsonParser.Load<DBClass>("dbconf");//dbcon.jsonf를 통해 load한다(이 파일 수정하기)
		if (db == null)
		{

		}
		string strConn = string.Format("server={0};port={1};uid={2};pwd={3};database={4};charset=utf8;",
							   db.ipAddress, db.port, db.db_id, db.db_pw, db.db_name);//연결형식
		conn = new MySqlConnection(strConn);
		try
		{
			conn.Open();
		}
		catch (MySqlException e)
		{
			Debug.Log(e);
			return;
		}

		Debug.Log("DB Loaded.");
	}

	public string SensorLoad(bool test = false)// 화재센서 & 방향지시등센서 
	{
		// Test mode setting. if true, just return hardcoded string
		string result = "";

		// DB에 저장 / 로드할 때에는 10진수로 생각함.
		string q = "" +
			"SELECT tb_sensornode_node_address, sensor_ids" +
			" FROM " + db.sensor_id_table +
		   " WHERE sensor_ids IN ("
		   + Remove0x(Constants.NODE_SENSOR_TEMP) + ","
		   + Remove0x(Constants.NODE_SENSOR_FIRE) + ","
		   + Remove0x(Constants.NODE_SENSOR_SMOKE) + ","
		   + Remove0x(Constants.NODE_SIREN) + ","
		   + Remove0x(Constants.NODE_DIRECTION) +
		   ");";
		Debug.Log("load쿼리문 : " + q);

		if (conn.Ping())
		{
			MySqlCommand command = new MySqlCommand(q, conn);
			MySqlDataReader rdr = command.ExecuteReader();

			while (rdr.Read())
			{
				result += rdr[0] + ";" + Convert.ToInt32("" + rdr[1], 16).ToString() + "\n";
			}

			rdr.Close();
		}
		        /*
				result = @"
				 13000001;80
				 13000002;80
				 13000003;81
				 13000004;81
                 13000005;33
				 13000005;33
				 13000005;33
                 13000006;33
				 13000006;33
				 13000006;33
                 13000007;39
                 13000008;39
                 13000009;40
				 ";*/
		        

		Debug.Log("센서 data : " + result);
		return result;
	}


	public string AreaLoad(bool test = false)
	{
		// Test mode setting. if true, just return hardcoded string
		if (test)
		{
			return @"
         room1
         room2
         room3
         ";
		}

		string temp = "";
		string q = "" +
			"SELECT area_id" +
			" FROM " + db.area_table + ";";

		Debug.Log(q);
		if (conn.Ping())
		{
			MySqlCommand command = new MySqlCommand(q, conn);
			MySqlDataReader rdr = command.ExecuteReader();
			temp = RdrToStr(rdr);
			Debug.Log("area data : " + temp);
			rdr.Close();
		}
        /*
        temp = @"
         room1
         room2
         room3
         ";*/
        return temp;
	}


	private string RdrToStr(MySqlDataReader rdr)
	{
		string temp = string.Empty;
		try
		{
			while (rdr.Read())
			{
				for (int i = 0; i < rdr.FieldCount; i++)
				{
					if (i != rdr.FieldCount - 1)
						temp += rdr[i] + ";";    // parser 넣어주기
					else if (i == rdr.FieldCount - 1)
						temp += rdr[i] + "\n";
				}
			}
		}
		catch (Exception e)
		{
			Debug.Log(e);
			temp = "No return";
		}
		return temp;
	}

	// e.g. 0x30 => 30
	private int Remove0x(int hex)
	{
		return Int32.Parse(hex.ToString("X").Replace("0x", ""));
	}
}
