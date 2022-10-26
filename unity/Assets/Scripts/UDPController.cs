using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UDPController
{
    private string ip;
    private int port;

    public delegate void ReceivedHandler(string strMsg);
    private ReceivedHandler received;
    private Thread thread;
    private UdpClient client;

    public ReceivedHandler Received { get => received; set => received = value; }

    public UDPController(string ip, int port)
    {
        this.ip = ip;
        this.port = port;
    }
    ~UDPController()
    {
        Dispose();
    }

    public void ListenStart()
    {
        client = new UdpClient(port);
        thread = new Thread(new ThreadStart(Thread));
        thread.Start();
    }

    public void Dispose()
    {
        if (thread != null)
        {
            thread.Abort();
            thread = null;
        }
        if (client != null)
        {
            client.Close();
            client.Dispose();
            client = null;
        }
    }

    private void Thread()
    {
        while (true)
        {
            if (client != null)
            {
                try
                {
                    IPEndPoint ep = null;
                    byte[] rcvBytes = client.Receive(ref ep);
                    string rcvMsg = string.Empty;
                    rcvMsg = Encoding.UTF8.GetString(rcvBytes);
                    if (rcvMsg != string.Empty)
                    {
                        Received?.Invoke(rcvMsg);
                    }
                }
                catch (System.Exception)
                {
                }
            }
        }
    }

    public void Send(string strMsg)
    {
        byte[] dgram = Encoding.UTF8.GetBytes(strMsg);
        client.Send(dgram, dgram.Length);
    }
}