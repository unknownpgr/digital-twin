using MySql.Data.MySqlClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



[SerializeField]
class DBClass{
    public string ipAddress = "localhost";
    public string db_id = "root";
    public string db_pw = "uostest";
    public string db_name = "test";
    public string sensor_table = "SensorTable";//table이름
    public string sensor_value_table = "SensorValueTable";//table이름
    public string area_table = "AreaTable";//table이름
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
            Debug.Log("읽어옴");
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
        
    }
    public void SensorSave(SensorNodeJson sensor)//SensorNodeJson로??
    {

        if (conn.Ping())
        {
            if (SensorLoad(sensor.nodeId))//존재하고있는지 확인
            {
                //UPDATE
                string q = "" +
                    "UPDATE '" + db.sensor_table +
                    "' SET " +
                    "'location' = '(" +
                    sensor.positions.x.ToString("F2") + "," +
                    sensor.positions.y.ToString("F2") + "," +
                    sensor.positions.z.ToString("F2") + "') " +
                    "WHERE 'node_address' = " +
                    sensor.nodeId + ";";
                Debug.Log("save 쿼리문 : " + q);
                MySqlCommand command = new MySqlCommand(q, conn);
                command.ExecuteNonQuery();
            }

            else// 새로운 센서추가할때<-- 안쓰임
            {
                //INSERT
                // 클라이언트에서 새로 만드는 경우는 없는걸로 가정함.
                /*
                string q = "" +
                    "INSERT INTO " + db.sensor_table +
                    " VALUES (" +
                    sensor.one_sensor.nodeId + ", " +
                    //sensor.one_sensor.nodeType + "," +
                    sensor.one_sensor.positions.x.ToString("F2") + "," +
                    sensor.one_sensor.positions.y.ToString("F2") + "," +
                    sensor.one_sensor.positions.z.ToString("F2") + "');";
                MySqlCommand command = new MySqlCommand(q, conn);
                command.ExecuteNonQuery();
                */
            }
        }
    }
    /*
     public void SensorSave(sensor_attribute sensor)
    {
        Debug.Log("동작시작");
        if (conn.Ping())
        {
            Debug.Log("연결됨");
            if (SensorLoad(sensor.one_sensor.nodeId))
            {
                //UPDATE
                string q = "" +
                    "UPDATE '" + db.sensor_table +
                    "' SET " +
                    "'location' = '(" +
                    sensor.one_sensor.positions.x.ToString("F2") + "," +
                    sensor.one_sensor.positions.y.ToString("F2") + "," +
                    sensor.one_sensor.positions.z.ToString("F2") + "') " +
                    "WHERE 'node_address' = " +
                    sensor.one_sensor.nodeId + ";";
                Debug.Log("save 쿼리문 : " + q);
                MySqlCommand command = new MySqlCommand(q, conn);
                command.ExecuteNonQuery();
            }
            else
            {
                //INSERT
                // 클라이언트에서 새로 만드는 경우는 없는걸로 가정함.
                
                //string q = "" +
                //    "INSERT INTO " + db.sensor_table +
                //    " VALUES (" +
                //    sensor.one_sensor.nodeId + ", " +
                //    //sensor.one_sensor.nodeType + "," +
                //    sensor.one_sensor.positions.x.ToString("F2") + "," +
                //    sensor.one_sensor.positions.y.ToString("F2") + "," +
                //    sensor.one_sensor.positions.z.ToString("F2") + "');";
                //MySqlCommand command = new MySqlCommand(q, conn);
                //command.ExecuteNonQuery();
                
}
        }
    }
         */
    public void SensorSave(areasensor_attribute sensor)//쿼리문으로 바꾸는과정
    {
        // area에 관한 DB 테이블이 현재 존재하지 않으므로 개발 필요.
        if (conn.Ping())
        {
            if (SensorLoad(sensor.one_sensor.areaId))
            {
                //UPDATE
                string q = "" +
                    "UPDATE '" + db.area_table +
                    "' SET " +
                    "'location' = '(" +
                    sensor.one_sensor.position.x.ToString("F2") + "," +
                    sensor.one_sensor.position.y.ToString("F2") + "," +
                    sensor.one_sensor.position.z.ToString("F2") + ")' " +
                    "WHERE 'node_address' = " +
                    sensor.one_sensor.areaId + ";";
                MySqlCommand command = new MySqlCommand(q, conn);
                command.ExecuteNonQuery();
            }
            else
            {
                //INSERT
                // 클라이언트에서 새로 만드는 경우는 없는걸로 가정함.
                /*
                string q = "" +
                    "INSERT INTO " + db.sensor_table +
                    " VALUES (" +
                    sensor.one_sensor.areaId + ", " +
                    sensor.one_sensor.position.x + "," +
                    sensor.one_sensor.position.y + "," +
                    sensor.one_sensor.position.z + ");";
                MySqlCommand command = new MySqlCommand(q, conn);
                command.ExecuteNonQuery();
                */
            }
        }
    }

    public bool SensorLoad(string id, sensor_attribute sensor = null)
    {

        string q = "" +
            "SELECT 'node_address','location'" +
            " FROM '" + db.sensor_table +
            "' WHERE nodeId = " + id + ";" ;
        bool ret = false;
        Debug.Log("load쿼리문 : "+q);
        if (conn.Ping())
        {
            MySqlCommand command = new MySqlCommand(q, conn);
            MySqlDataReader rdr = command.ExecuteReader();
            ret = false;
            string temp = RdrToStr(rdr);
            ret = (temp != string.Empty);

            string[] tmp = temp.Split(';');
            if (sensor != null)
            {
                sensor.one_sensor.nodeId = (tmp[0]);
                //sensor.one_sensor.nodeType = int.Parse(tmp[1]);
                string[] pos = tmp[1].Split(',');
                sensor.one_sensor.positions = new Vector3(
                    float.Parse(pos[0]), float.Parse(pos[1]), float.Parse(tmp[2]));
            }
            Debug.Log("node_addr, type, loc : " + temp); // "2,33,(2.4,5.22,8)"  // node_addr, type, location
            // 이 값을 parsing해서 location에 따라 센서 설치하는거 구현해야함
        }
        return ret;
    }

    public bool SensorLoad(int id, areasensor_attribute sensor)
    {
        // area에 관한 DB 테이블이 현재 존재하지 않으므로 개발 필요.
        string q = "" +
            "SELECT 'node_address','location'" +
            " FROM '" + db.area_table +
            "' WHERE nodeId = " + id.ToString() + ";";
        bool ret = false;
        Debug.Log(q);
        if (conn.Ping())
        {
            MySqlCommand command = new MySqlCommand(q, conn);
            Debug.Log("command : " + command);
            MySqlDataReader rdr = command.ExecuteReader();
            ret = false;
            string temp = RdrToStr(rdr);
            ret = (temp != string.Empty);

            string[] tmp = temp.Split(';');
            if (sensor != null)
            {
                sensor.one_sensor.areaId = (tmp[0]);
                string[] pos = tmp[1].Split(',');
                sensor.one_sensor.position = new Vector3(
                    float.Parse(pos[0]), float.Parse(pos[1]), float.Parse(tmp[2]));
            }
            Debug.Log(temp);
        }
        return ret;
    }
    /*
    public bool SensorLoad(int id, areasensor_attribute sensor)
    {
        // area에 관한 DB 테이블이 현재 존재하지 않으므로 개발 필요.
        string q = "" +
            "SELECT 'node_address','location'" +
            " FROM '" + db.area_table +
            "' WHERE nodeId = " + id.ToString() + ";";
        bool ret = false;
        Debug.Log(q);
        if (conn.Ping())
        {
            MySqlCommand command = new MySqlCommand(q, conn);
            MySqlDataReader rdr = command.ExecuteReader();
            ret = false;
            string temp = RdrToStr(rdr);
            ret = (temp != string.Empty);

            string[] tmp = temp.Split(';');
            if (sensor != null)
            {
                sensor.one_sensor.areaId = (tmp[0]);
                string[] pos = tmp[1].Split(',');
                sensor.one_sensor.position = new Vector3(
                    float.Parse(pos[0]), float.Parse(pos[1]), float.Parse(tmp[2]));
            }
            Debug.Log(temp);
        }
        return ret;
    }
     */

    string RdrToStr(MySqlDataReader rdr)
    {
        string temp = string.Empty;
        if (!rdr.Read())
        {
            temp = "No return";
        }
        else
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
        return temp;
    }
}
