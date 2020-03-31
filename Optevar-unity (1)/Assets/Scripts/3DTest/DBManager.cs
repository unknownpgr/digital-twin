using MySql.Data.MySqlClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;


[SerializeField]
class DBClass{
    public string ipAddress = "localhost";
    public string db_id = "root";
    public string db_pw = "uostest";

    public string db_name = "";
    public string sensor_table = "tb_sensornode";//'tb_sensornode'
    //public string sensor_value_table = "tb_sensingvalue";
    public string area_table = "mmwave_area";
    public string sensor_id_table = "tb_sensornode_sensor_ids";
}

public class DBManager : MonoBehaviour
{
    JsonParser jsonParser = new JsonParser();
    DBClass db;
    MySqlConnection conn;
    
    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    void Init()
    {
        
        db = jsonParser.Load<DBClass>("dbconf");//dbcon.jsonf를 통해 load한다(이 파일 수정하기)
        if (db!=null) {
            //Debug.Log("읽어옴");
        }
        string strConn = string.Format("server={0};uid={1};pwd={2};database={3};charset=utf8 ;",
                               db.ipAddress, db.db_id, db.db_pw, db.db_name);//연결형식
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
        /*
        string q = "" +
            "SELECT node_address,location" +
            " FROM " + db.sensor_table + ";";
            */

        //node_id, sensor_type
        string q = "" +
            "SELECT tb_sensornode_node_address, sensor_ids" +
            " FROM " + db.sensor_id_table + 
           " WHERE sensor_ids IN (21, 22, 23, 27);";

        
        //bool ret = false;
        Debug.Log("load쿼리문 : "+q);
        
        if (conn.Ping())//conn.State == System.Data.ConnectionState.Open)// == true
        {
            MySqlCommand command = new MySqlCommand(q, conn);
            MySqlDataReader rdr = command.ExecuteReader();
            //ret = false;
            temp = RdrToStr(rdr);
            //ret = (temp != string.Empty);

            Debug.Log("센서 data : "+temp);
            /*
            <string>
            13000001;(41.52,15.75,17.83)
            13000002;(63.42,15.75,17.74)
            13000003;(41.55,15.75,9.10)
            13000004;(63.38,15.75,9.10)
            13000005;(24.20,15.75,8.13)
            13000006;(29.53,15.75,30.66)
            13000007;(7.39,15.75,12.44)
            현재 : 
            sensor_node_ID;sensor_node_location
            
            최종 : 
            sensor_node_ID;sensor_node_type;sensor_node_location
             */

        }

        //conn.close();
        return temp;
    }
    

    public string AreaLoad()//area
    {
        string temp = "";
        string q = "" +
            "SELECT area_id" +
            " FROM " + db.area_table + ";";
        Debug.Log(q);
        if (conn.Ping())
        {
            MySqlCommand command = new MySqlCommand(q, conn);
            //Debug.Log("command : " + command);
            MySqlDataReader rdr = command.ExecuteReader();
            temp = RdrToStr(rdr);
            Debug.Log("area data : "+temp);
        }
        return temp;
    }


    string RdrToStr(MySqlDataReader rdr)
    {
        string temp = string.Empty;
        try
        {
            while (rdr.Read())
            {
                for (int i = 0; i < rdr.FieldCount; i++)
                {
                    if (i != rdr.FieldCount-1)
                        temp += rdr[i] + ";";    // parser 넣어주기
                    else if (i == rdr.FieldCount-1)
                        temp += rdr[i] + "\n";
                }
            }
        }
        catch(Exception e)
        {
            temp = "No return";
        }
        //}
        return temp;
    }
}
