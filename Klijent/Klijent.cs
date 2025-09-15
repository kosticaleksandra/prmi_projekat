using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Klase; 

namespace Klijent
{
    class Klijent
    {
        static void Main()
        {

            // [KT1 - Zadatak 3] Konfiguracija klijentske aplikacije:
            //  - Izbor protokola (TCP/UDP)
            Console.Title = "KLIJENT";

            Console.WriteLine("Izaberi protokol za komunikaciju:");
            Console.WriteLine("1 - TCP");
            Console.WriteLine("2 - UDP");
            Console.Write("Unos: ");
            string unos = Console.ReadLine();
            Protocol protokol = (unos == "1") ? Protocol.TCP : Protocol.UDP;
           
            // [KT1 - Zadatak 5] Unos server IP i porta na koje će se klijent povezati/ slati poruke
            Console.Write("Unesi IP adresu servera (npr. 127.0.0.1): ");
            string ip = Console.ReadLine();
            Console.Write("Unesi broj porta servera: ");
            int port = int.Parse(Console.ReadLine());

            if (protokol == Protocol.TCP)
            {
                try
                {
                    // [KT1 - Zadatak 3] Za TCP klijent uspostavlja vezu sa serverom
                    TcpClient tcpKlijent = new TcpClient();
                    tcpKlijent.Connect(ip, port); // Uspostavljanje konekcije
                    Console.WriteLine("Uspostavljena TCP konekcija sa serverom.");
                    // Ovde možeš dodati komunikaciju ako želiš

                    // [KT1 - Zadatak 5] Slanje poruke serveru i ispis odgovora (komunikacija klijent↔server)
                    NetworkStream stream = tcpKlijent.GetStream();

                    //5 zad
                    Console.Write("Unesi poruku za slanje serveru: ");
                    string porukaZaSlanje = Console.ReadLine();
                    byte[] podaciZaSlanje = Encoding.UTF8.GetBytes(porukaZaSlanje);
                    stream.Write(podaciZaSlanje, 0, podaciZaSlanje.Length);
                    Console.WriteLine("Poruka poslata.");

                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string odgovor = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine("Odgovor od servera: " + odgovor);

                    stream.Close();

                //kraj dodatka
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
                    // [KT1 - Zadatak 3/5] UDP klijent bez uspostavljene TCP konekcije – šalje datagram
                    UdpClient udp = new UdpClient();

                    Console.Write("Unesi poruku za slanje UDP serveru: ");
                    string porukaZaSlanje = Console.ReadLine();
                    byte[] podaciZaSlanje = Encoding.UTF8.GetBytes(porukaZaSlanje);

                    udp.Send(podaciZaSlanje, podaciZaSlanje.Length, ip, port);
                    Console.WriteLine("Poruka poslata UDP serveru.");

                    // [KT1 - Zadatak 5] Prijem odgovora od UDP servera
                    IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Any, 0);

                    byte[] primljeniPodaci = udp.Receive(ref serverEndPoint);
                    string odgovor = Encoding.UTF8.GetString(primljeniPodaci);
                    Console.WriteLine("Odgovor od servera: " + odgovor);

                    udp.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Greška pri radu sa UDP klijentom: " + ex.Message);
                }
            }


            Console.WriteLine("Pritisni Enter za kraj...");
            Console.ReadLine();
        }
    }

}