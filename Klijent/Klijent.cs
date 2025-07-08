using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Klase; // ako si enum stavila u poseban folder
namespace KlijentApp
{
    class Klijent
    {
        static void Main()
        {
            Console.Title = "KLIJENT";

            // Izbor protokola
            Console.WriteLine("Izaberi protokol za komunikaciju:");
            Console.WriteLine("1 - TCP");
            Console.WriteLine("2 - UDP");
            Console.Write("Unos: ");
            string unos = Console.ReadLine();
            Protocol protokol = (unos == "1") ? Protocol.TCP : Protocol.UDP;

            // IP i port servera
            Console.Write("Unesi IP adresu servera (npr. 127.0.0.1): ");
            string ip = Console.ReadLine();
            Console.Write("Unesi broj porta servera: ");
            int port = int.Parse(Console.ReadLine());

            if (protokol == Protocol.TCP)
            {
                try
                {
                    TcpClient tcpKlijent = new TcpClient();
                    tcpKlijent.Connect(ip, port); // Uspostavljanje konekcije
                    Console.WriteLine("Uspostavljena TCP konekcija sa serverom.");
                    // Ovde možeš dodati komunikaciju ako želiš
                    tcpKlijent.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Greška prilikom povezivanja: " + ex.Message);
                }
            }
            else // UDP
            {
                try
                {
                    UdpClient udp = new UdpClient();
                    Console.WriteLine($"UDP klijent spreman za slanje poruka na {ip}:{port}");
                    // Ovde možeš dodati kod za slanje poruka
                    udp.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Greška pri kreiranju UDP klijenta: " + ex.Message);
                }
            }

            Console.WriteLine("Pritisni Enter za kraj...");
            Console.ReadLine();
        }
    }
}