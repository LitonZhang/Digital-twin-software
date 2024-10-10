using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using System.Text;
/*using System.Threading.Tasks;
using System.Linq;*/


/// <summary>
/// 客户端
/// </summary>
public class SocketServer : MonoBehaviour
{
    private static Socket socket;
    //接收消息的载体
    private static byte[] buffer = new byte[1024];
    public static string[] EventArray;
    

    //服务器入口
    private void Start()
    {
        //1，创建套接字，并设置参数
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //2，绑定服务器的IP和端口
        EndPoint ep = new IPEndPoint(IPAddress.Any, 9999);
        socket.Bind(ep);

        //3，监听是否有客户端进行连接
        //参数表示在同一时间内服务器可以对该数量下的客户端进行操作，超出数量，则需要进行排队
        socket.Listen(1);
        //4，应答客户端
        /*if (HDES.monitor)
        {
            StartAccept();
        }*/
        StartAccept();
        //Console.Read();
        Debug.Log("Create Socket Server");
    }

    /// <summary>
    /// 开始应答客户端
    /// </summary>
    static void StartAccept()
    {
        //异步应答客户端
        socket.BeginAccept(AcceptCallback, null);
    }

    /// <summary>
    /// 开始接收消息
    /// </summary>
    /// <param name="client">应答的客户端套接字</param>
    static void StartReceive(Socket client)
    {
        //将应答的客户端作为参数传给回调函数
        client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveCallback, client);
    }

    /// <summary>
    /// 应答的回调函数
    /// </summary>
    /// <param name="iar"></param>
    static void AcceptCallback(IAsyncResult iar)
    {
        //1，结束应答，获得应答的客户端的套接字（服务器端将会有多个Socket，每一个Socket对应的是其响应的客户端）
        //一对一通信，client指的是应答Socket，而非客户端本身，该Socket属于服务器端，由服务器产生，针对请求响应的客户端
        Socket client = socket.EndAccept(iar);
        //2，应答Socket接收消息
        StartReceive(client);
        //3，处理完一个，继续处理，继续应答
        StartAccept();
    }

    static void ReceiveCallback(IAsyncResult iar)
    {
        //1，从回调函数结果中获取传递过来的参数，即获取传递过来的应答客户端对象
        Socket client = iar.AsyncState as Socket;
        //2，获取接收到信息的长度
        int len = client.EndReceive(iar);
        //3，判断信息是否成功接收
        if (len == 0)
        {
            return;
        }
        //4，解析信息并处理
        string str = Encoding.UTF8.GetString(buffer, 0, len);
        //Debug.Log(str);
        EvnetSplit(str);
   
        //5，继续接收来自客户端的信息
        StartReceive(client);
    }
    static void EvnetSplit(string evnet) 
    {
        string[] words = evnet.Split(';');
        foreach (string item in words)
        {
            if (item != "")
            {
                //Debug.Log("1"+item+"2");

                //item.Remove(1, 6);           
                bool containResult = item.Contains("/n");
                if (containResult)
                {
                    string newitem1 = item.Substring(0,item.Length-2);//去尾"/n"
                    string newitem2= newitem1.Replace("EVENT", "");//掐头 "EVENT"
                    EventArray = newitem2.Split(",");// 分割
                    foreach(string ww in EventArray[1..5])
                    {
                        //Debug.Log(ww);//打印出来看看
                    }                
                    
                    try
                    {
                        Monitor.EventList.Add(EventArray[1..5]); //在事件表中增加新事件
                        //GameObject.Find("GameManager").GetComponent<HDES>().EventTransfer("1");
                        //HDES.EventTransfer(EventArray[1..5]);//执行新事件
                    }
                    catch
                    {
                        Debug.Log("Sokcet数据发送失败");
                    }                                            
                }
                else
                {
                    Debug.Log("数据不完整");
                    break;
                }
            }
        }
    }

}
