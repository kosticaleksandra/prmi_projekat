using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Klase;
using System.Collections.Generic;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            // [KT1 - Zadatak 2] Konfiguracija serverske aplikacije:
            //  - Izbor protokola (TCP/UDP)
            //  - Otvaranje odgovarajuće utičnice
            //  - Ispis IP adrese i porta na kojima server čeka konekcije/pakete
            Console.Title = "SERVER";
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

                // [KT2 - Zadatak 7] Istovremeni, neblokirajući rad sa više klijenata (Select)
                int tcpPort = 5000;
                Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(new IPEndPoint(IPAddress.Any, tcpPort));
                listener.Listen(10);

                Console.WriteLine($"[TCP SERVER] Pokrenut na {GetLocalIPAddress()}:{tcpPort}");

                List<Socket> aktivniKlijenti = new List<Socket>();

                while (true)
                {
                    List<Socket> zaCitanje = new List<Socket>(aktivniKlijenti);
                    zaCitanje.Add(listener); // dodaj i glavni listener

                    Socket.Select(zaCitanje, null, null, 1000 * 1000); // timeout 1s

                    foreach (Socket s in zaCitanje)
                    {
                        if (s == listener)
                        {
                            // Novi klijent
                            Socket noviKlijent = listener.Accept();
                            aktivniKlijenti.Add(noviKlijent);
                            Console.WriteLine($"[TCP SERVER] Novi klijent povezan: {noviKlijent.RemoteEndPoint}");
                        }
                        else
                        {
                            // Postojeći klijent šalje podatke
                            byte[] buffer = new byte[1024];
                            int bajtova = 0;

                            try
                            {
                                bajtova = s.Receive(buffer);
                            }
                            catch
                            {
                                bajtova = 0;
                            }

                            if (bajtova <= 0)
                            {
                                Console.WriteLine($"[TCP SERVER] Klijent se odjavio: {s.RemoteEndPoint}");
                                aktivniKlijenti.Remove(s);
                                s.Close();
                                continue;
                            }

                            // [KT1 - Zadatak 10] Prijem poruke i slanje odgovora klijentu
                            string poruka = Encoding.UTF8.GetString(buffer, 0, bajtova);
                            Console.WriteLine($"[TCP SERVER] Poruka od {s.RemoteEndPoint}: {poruka}");

                            // Odgovor klijentu
                            string odgovor = "Server je primio poruku: " + poruka;
                            byte[] odgovorBytes = Encoding.UTF8.GetBytes(odgovor);
                            s.Send(odgovorBytes);
                        }
                    }
                }

            }
            else // UDP
            {
                // [KT1 - Zadatak 2 i 10] UDP server: prijem datagrama i slanje odgovora pošiljaocu
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
            }
        }

        // [Pomoćna funkcija] Ispis lokalne IPv4 adrese (koristi se u Zad. 2)
        private static string GetLocalIPAddress()
        {
            foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip.ToString();

            return "127.0.0.1";
         }

    }
}

