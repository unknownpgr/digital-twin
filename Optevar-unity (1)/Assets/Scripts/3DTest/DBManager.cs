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
        DockerManager DM = new DockerManager();
        DM.RunDocker();

        db = jsonParser.Load<DBClass>("dbconf_temp");//dbcon.jsonf를 통해 load한다(이 파일 수정하기)
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
    /*
    public void SensorValueSelect(Text _nodeId, Text _start, Text _end)
    {
        //select * from products where registdate between '2013-03-01' and '2013-03-04'
        string q = "" +
            "SELECT * " +
            " FROM '" + db.sensor_value_table +
            "' WHERE 'node_address' = '" + _nodeId.text +
            "' and 'created_date' BETWEEN '" + _start.text + "' and '" + _end.text + "';";
        Debug.Log(q);
        string temp = "DB connection Error.";
        if (conn.State == System.Data.ConnectionState.Open)
        {
            MySqlCommand command = new MySqlCommand(q, conn);
            MySqlDataReader rdr = command.ExecuteReader();
            bool ret = false;
            temp = RdrToStr(rdr);
            ret = (temp != string.Empty);

            string[] tmp = temp.Split(';');
        }
        Debug.Log(temp);
    }

    public void SensorValueSelect(GameObject _gameObject)
    {
        if (_gameObject != null)
        this.SensorValueSelect(
        _gameObject.transform.GetChild(1).GetChild(0).GetComponentInChildren<Text>(),
        _gameObject.transform.GetChild(2).GetChild(0).GetComponentInChildren<Text>(),
        _gameObject.transform.GetChild(2).GetChild(1).GetComponentInChildren<Text>());
        
    }*/
    /*
    public void SensorSave(SensorNodeJson sensor)//SensorNodeJson로??
    {

        if (conn.Ping())//conn.State == System.Data.ConnectionState.Open)//
        {
            string q = "" +
                "UPDATE " + db.sensor_table +
                " SET " +
                "location = '(" +
                sensor.positions.x.ToString("F2") + "," +
                sensor.positions.y.ToString("F2") + "," +
                sensor.positions.z.ToString("F2") + ")' " +
                "WHERE node_address = '" +
                sensor.nodeId + "';";
            Debug.Log("save 쿼리문 : " + q);
            MySqlCommand command = new MySqlCommand(q, conn);
            command.ExecuteNonQuery();
            
        }
        //conn.Close()
    }
    
    public void SensorSave(areasensor_attribute sensor)
    {
        // area에 관한 DB 테이블이 현재 존재하지 않으므로 개발 필요.
        if (conn.Ping())//conn.State == System.Data.ConnectionState.Open
        {

            //if (SensorLoad(sensor.one_sensor.areaId))
            //{
            //UPDATE
            string q = "" +
                "UPDATE " + db.sensor_table +
                " SET " +
                "location = '(" +
                sensor.one_sensor.position.x.ToString("F2") + "," +
                sensor.one_sensor.position.y.ToString("F2") + "," +
                sensor.one_sensor.position.z.ToString("F2") + ")' " +
                "WHERE node_address = '" +
                sensor.one_sensor.areaId + "';";
            Debug.Log("save 쿼리문 : " + q);
            MySqlCommand command = new MySqlCommand(q, conn);
            command.ExecuteNonQuery();
            
        }
    }*/
    public string SensorLoad()// 화재센서 & 방향지시등센서 
    {
        string temp = "";
        string q = "" +
            "SELECT tb_sensornode_node_address, sensor_ids" +
            " FROM " + db.sensor_id_table +
           " WHERE sensor_ids IN (21, 22, 23, 26, 27);";
        Debug.Log("load쿼리문 : " + q);

        if (conn.Ping())
        {
            MySqlCommand command = new MySqlCommand(q, conn);
            MySqlDataReader rdr = command.ExecuteReader();
            temp = RdrToStr(rdr);
            Debug.Log("센서 data : " + temp);
            rdr.Close();

        }

        return temp;
    }
    public string SensorLoad(bool test = false)// 화재센서 & 방향지시등센서 
    {
        // Test mode setting. if true, just return hardcoded string
        if (test)
        {
            return @"
            00010000;0x21
            00010000;0x22
            00010000;0x23
            00020000;0x21
            00020000;0x22
            00030000;0x23
            00030000;0x21
            00030000;0x22
            00020000;0x23
            00040000;0x27
            00050000;0x27
            00060000;0x27";
        }

        string temp = "";
        string q = "" +
            "SELECT tb_sensornode_node_address, sensor_ids" +
            " FROM " + db.sensor_id_table +
           " WHERE sensor_ids IN ("
           + Const.NODE_SENSOR_TEMP + ","
           + Const.NODE_SENSOR_FIRE + ","
           + Const.NODE_SENSOR_SMOKE + ","
           + Const.NODE_DIRECTION +
           ");";
        Debug.Log("load쿼리문 : " + q);

        if (conn.Ping())
        {
            MySqlCommand command = new MySqlCommand(q, conn);
            MySqlDataReader rdr = command.ExecuteReader();
            temp = RdrToStr(rdr);
            Debug.Log("센서 data : " + temp);
            rdr.Close();

        }

        return temp;
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
}
