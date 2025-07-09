using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using PcapDotNet.Core;  
using PcapDotNet.Packets;
using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.IpV4;
using PcapDotNet.Packets.Transport;

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
            // ********* DODAT ZADATAK 6 - ispis detalja paketa *********

            // MAC adrese
            EthernetDatagram eth = packet.Ethernet;
            if (eth != null)
            {
                Console.WriteLine($"MAC pošiljalac: {eth.Source}");
                Console.WriteLine($"MAC primalac: {eth.Destination}");
            }
            else
            {
                Console.WriteLine("Nema Ethernet zaglavlja.");
            }

            // IP adrese i portovi
            IpV4Datagram ip = packet.Ethernet?.IpV4;
            if (ip != null)
            {
                Console.WriteLine($"IP pošiljalac: {ip.Source}");
                Console.WriteLine($"IP primalac: {ip.Destination}");
                Console.WriteLine($"Transportni protokol: {ip.Protocol}");

                // TCP ili UDP portovi
                if (ip.Protocol == PcapDotNet.Packets.IpV4.IpV4Protocol.Tcp)
                {
                    TcpDatagram tcp = ip.Tcp;
                    if (tcp != null)
                    {
                        Console.WriteLine($"TCP port pošiljaoca: {tcp.SourcePort}");
                        Console.WriteLine($"TCP port primaoca: {tcp.DestinationPort}");
                    }
                }
                else if (ip.Protocol == PcapDotNet.Packets.IpV4.IpV4Protocol.Udp)
                {
                    UdpDatagram udp = ip.Udp;
                    if (udp != null)
                    {
                        Console.WriteLine($"UDP port pošiljaoca: {udp.SourcePort}");
                        Console.WriteLine($"UDP port primaoca: {udp.DestinationPort}");
                    }
                }
                else
                {
                    Console.WriteLine("Nije TCP ni UDP protokol.");
                }
            }
            else
            {
                Console.WriteLine("Nema IPv4 zaglavlja.");
            }

            Console.WriteLine(new string('-', 50));
            // *************************************************************
        }
    }
}

