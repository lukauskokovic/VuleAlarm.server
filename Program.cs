using System;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.IO;
namespace VuleAlarmService
{
    class Program
    {
        const int pin = 20;
        static string[] WhiteList;
        static Dictionary<Socket, string> ConnectedUsers = new Dictionary<Socket, string>();
        static void Main(string[] args)
        {
            RPIO.Export(pin);
            RPIO.SetDirection(pin, RPIO.Direction.IN);

            if(!File.Exists("whitelist.txt")){
                Console.WriteLine("Popuni whitelist.txt");
                File.Create("whitelist.txt");
                return;
            }
            WhiteList = File.ReadAllLines("whitelist.txt");
            Console.WriteLine("Whitelsited users: ");
            for(int i = 0; i < WhiteList.Length; i++){
                Console.WriteLine("> '" + WhiteList[i] + "'");
            }
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Parse("192.168.0.18"), 1300));
            socket.Listen(2);
            Console.WriteLine("Running");
            
            new Thread(() => 
            {
                Console.WriteLine("Starting thread");
                while(true){
                    bool pinValue = RPIO.ReadInput(pin);
                    Console.WriteLine(pinValue);
                    #region Sending message
                    Socket[] array = ConnectedUsers.Keys.ToArray();
                    for(int i = 0; i < ConnectedUsers.Count; i++){
                        Socket socket = array[i];
                        try{
                            socket.Send(new byte[]{(byte)(pinValue?0:1)});//all good
                        }catch{
                            Console.WriteLine("'{0}' disconnected", ConnectedUsers[socket]);
                            ConnectedUsers.Remove(socket);
                        }
                    }
                    #endregion
                    
                    Thread.Sleep(1000);
                }
            }).Start();
            //Accept loop
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
                    bool found = false;
                    for(int i = 0; i < WhiteList.Length; i++){
                        if(WhiteList[i] == name){
                            found = true;
                            break;
                        }
                    }
                    if(found){
                        ConnectedUsers.Add(s, name);
                        Console.WriteLine("Mobile '{0}' connected", name);
                    }
                    else{
                        Console.WriteLine("User " + name + " not whitelisted");
                        s.Dispose();
                        continue;
                    }
                }catch(Exception ex){
                    Console.WriteLine("Error " + ex.Message);
                    s.Dispose();
                }
            }
        }
    }
}
