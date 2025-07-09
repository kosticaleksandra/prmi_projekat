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

                    //dodato za treci zadatak
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
                    UdpClient udp = new UdpClient();

                    Console.Write("Unesi poruku za slanje UDP serveru: ");
                    string porukaZaSlanje = Console.ReadLine();
                    byte[] podaciZaSlanje = Encoding.UTF8.GetBytes(porukaZaSlanje);

                    udp.Send(podaciZaSlanje, podaciZaSlanje.Length, ip, port);
                    Console.WriteLine("Poruka poslata UDP serveru.");

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