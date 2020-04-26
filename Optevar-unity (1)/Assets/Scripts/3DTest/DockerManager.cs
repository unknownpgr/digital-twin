using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Diagnostics;//docker test
using System.ComponentModel;

public class DockerManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void RunDocker() {
        //이거하기전에 yml파일을 현재 폴더에 넣어야함*****************
        //var ProcessInfo = new ProcessStartInfo("docker-compose", $"up");
        //var DockerProcessInfo = new ProcessStartInfo("docker-compose", $"up");//, $""); //command, arguments
        string path = Application.dataPath + "/Resources/scenario_jsons/";
        //var DockerProcessInfo = new ProcessStartInfo("cmd.exe", "cd "+path+" && docker-compose up");//, $""); //command, arguments
        ProcessStartInfo DockerProcessInfo = new ProcessStartInfo("cmd.exe");//, "docker-compose up");//, $""); //command, arguments
        Process process = new Process();
        //processInfo.FileName = 
        //processInfo.Arguments =
        //DockerProcessInfo.FileName = "cmd.exe";
        DockerProcessInfo.WorkingDirectory = @"" + path;
        //DockerProcessInfo.Arguments = @"docker-compose up";
        //UnityEngine.Debug.Log(@"" + path);
        
        DockerProcessInfo.CreateNoWindow = true;                               // cmd창을 띄우지 안도록 하기
        DockerProcessInfo.UseShellExecute = false;
        DockerProcessInfo.RedirectStandardOutput = true;        // cmd창에서 데이터를 가져오기
        DockerProcessInfo.RedirectStandardInput = true;          // cmd창으로 데이터 보내기
        DockerProcessInfo.RedirectStandardError = true;
        try
        {
            //process = Process.Start(DockerProcessInfo);
            process.StartInfo = DockerProcessInfo;
            process.Start();
            //UnityEngine.Debug.Log("ㅇㅇ1");
            process.StandardInput.Write(@"docker-compose start"+ Environment.NewLine);
            //process.StandardInput.Close();
            
            
            
            UnityEngine.Debug.Log("docker start");
            //string ret = process.StandardOutput.ReadToEnd();
            //UnityEngine.Debug.Log("output : "+ret);
            
            //string ret_buf = ret.Substring(ret.IndexOf(cmd_string) + cmd_string.Length);
            //Console.Write(ret_buf);

            //UnityEngine.Debug.Log("cmd : "+process.StandardOutput.ReadToEnd());
            //UnityEngine.
        }
        catch(Exception e) {
            UnityEngine.Debug.Log("연결실패 : "+ e);
        }
        //var output = process.StandardOutput.ReadToEnd();
        //UnityEngine.Debug.Log("docker connection output : "+ output);
        //process.StartInfo = processInfo;
        //process.Start();
        /*
        if (!process.HasExited) {
            process.Kill();// run_docker함수가 실행되는 곳이 update문이라면 start, kill같이 넣어도되지만/ 일회성이라면 따로 함수 구현

        }
        
        */
        //return을 연결됐다는 표시
        process.Close();
    }
    public void DockerTest(){
        string path = Application.dataPath + "/Resources/scenario_jsons/";
        ProcessStartInfo info = new ProcessStartInfo();
        //프로세스 생성 및 초기화
        Process process = new Process();
        /*
        info.FileName = @"cmd";
        info.WindowStyle = ProcessWindowStyle.Hidden;
        info.CreateNoWindow = false;
        info.UseShellExecute = false;

        info.RedirectStandardOutput = true;
        info.RedirectStandardInput = true;
        info.RedirectStandardError = true;

        process.EnableRaisingEvents = false;
        //명령 실행
        process.StartInfo = info;
        process.Start();
        UnityEngine.Debug.Log();
        */
        //process.Start("explorer.exe", "http://www.naver.com");
        //process.StandardOutput.
        //process.StandardInput.Write(@"path" + Environment.NewLine);
        //process.StandardInput.Write(@"cd "+ path + Environment.NewLine);
        //process.StandardInput.Write(@"docker-compose up" + Environment.NewLine);

        process.StandardInput.Close();

        process.WaitForExit();
        process.Close();

    }
    public void EndDocker() {

    }
}
