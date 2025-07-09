using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using PcapDotNet.Core;  
using PcapDotNet.Packets;

namespace PacketSniffer
{
    internal class PacketSniffer
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Želite li da pokrenete presretanje paketa? (da/ne)");
            string odgovor = Console.ReadLine().Trim().ToLower();

            if (odgovor != "da")
            {
                Console.WriteLine("Presretanje nije pokrenuto.");
                return;
            }

            Console.WriteLine("Unesite filter (npr. ip src 192.168.1.1, port 80) ili pritisnite Enter za bez filtera:");
            string filter = Console.ReadLine();

            // Izlistaj dostupne mrežne interfejse
            var devices = LivePacketDevice.AllLocalMachine;
            if (devices.Count == 0)
            {
                Console.WriteLine("Nema dostupnih mrežnih uređaja.");
                return;
            }

            Console.WriteLine("Izaberite mrežni uređaj:");
            for (int i = 0; i < devices.Count; i++)
                Console.WriteLine($"{i}: {devices[i].Name} - {devices[i].Description}");

            int izbor = int.Parse(Console.ReadLine());

            var selectedDevice = devices[izbor];

            // Otvori uređaj za presretanje
            using (var communicator = selectedDevice.Open(65536, // snaplen
                                                          PacketDeviceOpenAttributes.Promiscuous, // promiscuous mode
                                                          1000)) // timeout
            {
                // Postavi filter ako postoji
                if (!string.IsNullOrWhiteSpace(filter))
                {
                    communicator.SetFilter(filter);
                }

                Console.WriteLine("Presretanje paketa pokrenuto. Pritisnite Enter za prekid.");

                // Startuj presretanje u drugoj niti ili asinhrono
                communicator.ReceivePackets(0, PacketHandler);

                Console.ReadLine();
            }
        }

        private static void PacketHandler(Packet packet)
        {
            Console.WriteLine($"Presretnut paket: {packet.Timestamp}, dužina: {packet.Length}");
            // Ovde možeš dodatno parsirati paket i ispisati MAC, IP adrese, portove itd.
        }
    }
}

