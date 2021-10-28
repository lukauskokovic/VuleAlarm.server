using System;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace VuleAlarmService
{
    class Program
    {
        static Dictionary<Socket, string> ConnectedUsers = new Dictionary<Socket, string>();
        static void Main(string[] args)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Parse("192.168.0.17"), 1421));
            socket.Listen(2);
            Console.WriteLine("Running");
            
            new Thread(() => 
            {
                Console.WriteLine("Starting thread");
                while(true){
                    Socket[] array = ConnectedUsers.Keys.ToArray();
                    for(int i = 0; i < ConnectedUsers.Count; i++){
                        Socket socket = array[i];
                        try{
                            socket.Send(new byte[]{0});//all good
                        }catch{
                            Console.WriteLine("'{0}' disconnected", ConnectedUsers[socket]);
                            ConnectedUsers.Remove(socket);
                        }
                    }
                    Thread.Sleep(1000);
                }
            }).Start();
            while(true){
                Socket s = socket.Accept();
                try{
                    s.ReceiveTimeout = 500;
                    byte[] buffer = new byte[32];
                    int read = s.Receive(buffer);
                    if(read > 32){
                        Console.WriteLine("Huge buffer discarding user...");
                        s.Dispose();
                        continue;
                    }
                    string name = System.Text.Encoding.UTF8.GetString(buffer, 0, read);
                    ConnectedUsers.Add(s, name);
                    Console.WriteLine("Mobile '{0}' connected", name);
                }catch(Exception ex){
                    Console.WriteLine("Error " + ex.Message);
                    s.Dispose();
                }
            }
        }
    }
}
