using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Klase;

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
                int tcpPort = 5000;
                TcpListener tcpListener = new TcpListener(IPAddress.Any, tcpPort);
                tcpListener.Start();

                Console.WriteLine($"TCP server pokrenut na adresi: {GetLocalIPAddress()} port: {tcpPort}");
                Console.WriteLine("Čekam TCP konekcije...");
            }
            else // UDP
            {
                int udpPort = 6000;
                UdpClient udpClient = new UdpClient(udpPort);

                Console.WriteLine($"UDP server pokrenut na adresi: {GetLocalIPAddress()} port: {udpPort}");
                Console.WriteLine("Čekam UDP pakete...");
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
        }
    }
}