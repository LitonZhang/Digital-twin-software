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
/// �ͻ���
/// </summary>
public class SocketServer : MonoBehaviour
{
    private static Socket socket;
    //������Ϣ������
    private static byte[] buffer = new byte[1024];
    public static string[] EventArray;
    

    //���������
    private void Start()
    {
        //1�������׽��֣������ò���
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //2���󶨷�������IP�Ͷ˿�
        EndPoint ep = new IPEndPoint(IPAddress.Any, 9999);
        socket.Bind(ep);

        //3�������Ƿ��пͻ��˽�������
        //������ʾ��ͬһʱ���ڷ��������ԶԸ������µĿͻ��˽��в�������������������Ҫ�����Ŷ�
        socket.Listen(1);
        //4��Ӧ��ͻ���
        /*if (HDES.monitor)
        {
            StartAccept();
        }*/
        StartAccept();
        //Console.Read();
        Debug.Log("Create Socket Server");
    }

    /// <summary>
    /// ��ʼӦ��ͻ���
    /// </summary>
    static void StartAccept()
    {
        //�첽Ӧ��ͻ���
        socket.BeginAccept(AcceptCallback, null);
    }

    /// <summary>
    /// ��ʼ������Ϣ
    /// </summary>
    /// <param name="client">Ӧ��Ŀͻ����׽���</param>
    static void StartReceive(Socket client)
    {
        //��Ӧ��Ŀͻ�����Ϊ���������ص�����
        client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveCallback, client);
    }

    /// <summary>
    /// Ӧ��Ļص�����
    /// </summary>
    /// <param name="iar"></param>
    static void AcceptCallback(IAsyncResult iar)
    {
        //1������Ӧ�𣬻��Ӧ��Ŀͻ��˵��׽��֣��������˽����ж��Socket��ÿһ��Socket��Ӧ��������Ӧ�Ŀͻ��ˣ�
        //һ��һͨ�ţ�clientָ����Ӧ��Socket�����ǿͻ��˱�����Socket���ڷ������ˣ��ɷ��������������������Ӧ�Ŀͻ���
        Socket client = socket.EndAccept(iar);
        //2��Ӧ��Socket������Ϣ
        StartReceive(client);
        //3��������һ����������������Ӧ��
        StartAccept();
    }

    static void ReceiveCallback(IAsyncResult iar)
    {
        //1���ӻص���������л�ȡ���ݹ����Ĳ���������ȡ���ݹ�����Ӧ��ͻ��˶���
        Socket client = iar.AsyncState as Socket;
        //2����ȡ���յ���Ϣ�ĳ���
        int len = client.EndReceive(iar);
        //3���ж���Ϣ�Ƿ�ɹ�����
        if (len == 0)
        {
            return;
        }
        //4��������Ϣ������
        string str = Encoding.UTF8.GetString(buffer, 0, len);
        //Debug.Log(str);
        EvnetSplit(str);
   
        //5�������������Կͻ��˵���Ϣ
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
                    string newitem1 = item.Substring(0,item.Length-2);//ȥβ"/n"
                    string newitem2= newitem1.Replace("EVENT", "");//��ͷ "EVENT"
                    EventArray = newitem2.Split(",");// �ָ�
                    foreach(string ww in EventArray[1..5])
                    {
                        //Debug.Log(ww);//��ӡ��������
                    }                
                    
                    try
                    {
                        Monitor.EventList.Add(EventArray[1..5]); //���¼������������¼�
                        //GameObject.Find("GameManager").GetComponent<HDES>().EventTransfer("1");
                        //HDES.EventTransfer(EventArray[1..5]);//ִ�����¼�
                    }
                    catch
                    {
                        Debug.Log("Sokcet���ݷ���ʧ��");
                    }                                            
                }
                else
                {
                    Debug.Log("���ݲ�����");
                    break;
                }
            }
        }
    }

}
