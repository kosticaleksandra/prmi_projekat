//SERVER
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Klase;
//using Server.Enums;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Izaberi protokol za komunikaciju (TCP/UDP):");
            string input = Console.ReadLine()?.Trim().ToUpper();

            Protocol protocol;
            if (input == "TCP")
                protocol = Protocol.TCP;
            else if (input == "UDP")
                protocol = Protocol.UDP;
            else
            {
                Console.WriteLine("Nepoznat protokol, podrazumevano se koristi TCP.");
                protocol = Protocol.TCP;
            }

            if (protocol == Protocol.TCP)
            {
                /* int tcpPort = 5000;
                 TcpListener tcpListener = new TcpListener(IPAddress.Any, tcpPort);
                 tcpListener.Start();

                 Console.WriteLine($"TCP server pokrenut na adresi: {GetLocalIPAddress()} port: {tcpPort}");
                 Console.WriteLine("Čekam TCP konekcije...");
                */


                //dodala novo 

                int tcpPort = 5000;
                TcpListener tcpListener = new TcpListener(IPAddress.Any, tcpPort);
                tcpListener.Start();

                Console.WriteLine($"TCP server pokrenut na adresi: {GetLocalIPAddress()} port: {tcpPort}");
                Console.WriteLine("Čekam TCP konekcije...");

                while (true)  // beskonačna petlja koja prima konekcije i komunicira
                {
                    TcpClient klijent = tcpListener.AcceptTcpClient(); // čeka novu konekciju
                    Console.WriteLine("Klijent povezan.");

                    NetworkStream stream = klijent.GetStream();

                    // Čitanje poruke od klijenta
                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string poruka = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine("Primljena poruka: " + poruka);

                    // Slanje odgovora nazad klijentu
                    string odgovor = "Server je primio poruku: " + poruka;
                    byte[] odgovorBytes = Encoding.UTF8.GetBytes(odgovor);
                    stream.Write(odgovorBytes, 0, odgovorBytes.Length);

                    // Zatvaranje konekcije sa klijentom
                    stream.Close();
                    klijent.Close();
                }



            }
            else // UDP
            {
                /*int udpPort = 6000;
                UdpClient udpClient = new UdpClient(udpPort);

                Console.WriteLine($"UDP server pokrenut na adresi: {GetLocalIPAddress()} port: {udpPort}");
                */

                //moja izmenaaaaaaaaaaaaaaaaaaaaaaaa
                int udpPort = 6000;
                UdpClient udpClient = new UdpClient(udpPort);

                Console.WriteLine($"UDP server pokrenut na adresi: {GetLocalIPAddress()} port: {udpPort}");
                Console.WriteLine("Čekam UDP pakete...");

                while (true)
                {
                    IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                    byte[] primljeniPodaci = udpClient.Receive(ref remoteEP);
                    string poruka = Encoding.UTF8.GetString(primljeniPodaci);
                    Console.WriteLine($"Primljena poruka od {remoteEP}: {poruka}");

                    string odgovor = "Server je primio poruku: " + poruka;
                    byte[] odgovorBytes = Encoding.UTF8.GetBytes(odgovor);

                    udpClient.Send(odgovorBytes, odgovorBytes.Length, remoteEP);
                }

                //zavrsen dodatak novi
            }

            Console.WriteLine("Pritisni Enter za izlaz...");
            Console.ReadLine();
        }

        private static string GetLocalIPAddress()
        {
            foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip.ToString();

            return "127.0.0.1";
         }*/

        }
    }
}
