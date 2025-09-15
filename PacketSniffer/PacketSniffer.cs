using System;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using PcapDotNet.Core;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.IpV4;
using PcapDotNet.Packets.Transport;

namespace PacketSniffer
{
    internal class PacketSniffer
    {
        // [KT2 - Zadatak 9] STATISTIKA: broj TCP/UDP paketa + najduži aplikativni payload
        static int _tcpCount = 0, _udpCount = 0;
        static int _maxPayloadLen = 0;
        static string _maxPayloadAscii = "";
        static readonly object _maxLock = new object();

        static void Main(string[] args)
        {
            // [KT1 - Zadatak 4] Pokretanje/prekid presretanja (start/stop)
            Console.Title = "PACKET SNIFFER";
            Console.WriteLine("Želite li da pokrenete presretanje paketa? (da/ne)");
            string odgovor = Console.ReadLine()?.Trim().ToLower();
            if (odgovor != "da")
            {
                Console.WriteLine("Presretanje nije pokrenuto.");
                return;
            }

            // [KT1 - Zadatak 4] Opciono filtriranje (pcap filter sintaksa)
            Console.WriteLine("Unesite filter (npr. ip src 192.168.1.1, port 80) ili pritisnite Enter za bez filtera:");
            string filter = Console.ReadLine();

            // [KT1 - Zadatak 4] Izbor mrežnog interfejsa za presretanje
            var devices = LivePacketDevice.AllLocalMachine;
            if (devices.Count == 0)
            {
                Console.WriteLine("Nema dostupnih mrežnih uređaja.");
                return;
            }

            Console.WriteLine("Izaberite mrežni uređaj:");
            for (int i = 0; i < devices.Count; i++)
                Console.WriteLine($"{i}: {devices[i].Name} - {devices[i].Description}");

            int izbor = int.Parse(Console.ReadLine() ?? "0");
            var selectedDevice = devices[izbor];

            // [KT1 - Zadatak 4] Otvaranje interfejsa u promiscuous režimu + timeout
            using (var communicator = selectedDevice.Open(
                       65536,
                       PacketDeviceOpenAttributes.Promiscuous,
                       1000)) // timeout
            {
                // [KT1 - Zadatak 4] Primena filtera (ako je naveden)
                if (!string.IsNullOrWhiteSpace(filter))
                    communicator.SetFilter(filter);

                Console.WriteLine("Presretanje paketa pokrenuto. Pritisnite Enter za prekid.");

                // [KT1 - Zadatak 4] ENTER prekid:
                // - ReceivePackets radi u pozadinskom Task-u
                // - Enter -> communicator.Break() uredno prekida capture
                var captureTask = Task.Run(() =>
                {
                    try
                    {
                        communicator.ReceivePackets(0, PacketHandler);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Greška tokom presretanja: {ex.Message}");
                    }
                });

                Console.ReadLine();  // čekanje Enter
                communicator.Break(); // signal za prekid ReceivePackets-a

                try { captureTask.Wait(); }
                catch (AggregateException ae)
                {
                    foreach (var e in ae.InnerExceptions)
                        Console.WriteLine($"Greška u niti: {e.Message}");
                }

                // [KT2 - Zadatak 9] Ispis statistike nakon prekida presretanja
                Console.WriteLine("=== STATISTIKA (posle prekida) ===");
                Console.WriteLine($"TCP paketa:       {_tcpCount}");
                Console.WriteLine($"UDP paketa:       {_udpCount}");
                if (_maxPayloadLen > 0)
                {
                    Console.WriteLine($"Najduži aplikativni podatak ({_maxPayloadLen} B):");
                    Console.WriteLine(_maxPayloadAscii);
                }
                else
                {
                    Console.WriteLine("Nije detektovan aplikativni payload.");
                }
            }
        }

        // [KT1 - Zadatak 6] Osnovna interpretacija paketa (MAC, IP, protokol, portovi)
        // [KT2 - Zadatak 8] Dodatna polja (TCP kontrolni bitovi, veličina zaglavlja, payload + dužina, IPv4 checksum/TTL)
        private static void PacketHandler(Packet packet)
        {
            Console.WriteLine($"Presretnut paket: {packet.Timestamp}, dužina: {packet.Length}");

            // [KT1 - Z6] MAC adrese (Ethernet zaglavlje)
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

            // [KT1 - Z6] IPv4: izvor/odredište + identifikacija transportnog protokola
            IpV4Datagram ip = packet.Ethernet?.IpV4;
            if (ip != null)
            {
                Console.WriteLine($"IP pošiljalac: {ip.Source}");
                Console.WriteLine($"IP primalac: {ip.Destination}");
                Console.WriteLine($"Transportni protokol: {ip.Protocol}");

                if (ip.Protocol == IpV4Protocol.Tcp)
                {
                    TcpDatagram tcp = ip.Tcp;
                    if (tcp != null)
                    {
                        // [KT2 - Z9] Statistika: broj TCP paketa
                        Interlocked.Increment(ref _tcpCount);

                        // [KT1 - Z6] TCP portovi
                        Console.WriteLine($"TCP port pošiljaoca: {tcp.SourcePort}");
                        Console.WriteLine($"TCP port primaoca: {tcp.DestinationPort}");

                        // [KT2 - Z8] TCP kontrolni bitovi (lep, poimenice ispis)
                        Console.Write("TCP kontrolni bitovi: ");
                        if (tcp.ControlBits.HasFlag(TcpControlBits.Synchronize)) Console.Write("SYN ");
                        if (tcp.ControlBits.HasFlag(TcpControlBits.Acknowledgment)) Console.Write("ACK ");
                        if (tcp.ControlBits.HasFlag(TcpControlBits.Fin)) Console.Write("FIN ");
                        if (tcp.ControlBits.HasFlag(TcpControlBits.Reset)) Console.Write("RST ");
                        if (tcp.ControlBits.HasFlag(TcpControlBits.Push)) Console.Write("PSH ");
                        if (tcp.ControlBits.HasFlag(TcpControlBits.Urgent)) Console.Write("URG ");
                        Console.WriteLine();

                        // [KT2 - Z8] Veličina TCP zaglavlja (u bajtovima)
                        Console.WriteLine($"Veličina TCP zaglavlja: {tcp.HeaderLength} bajtova");

                        // [KT2 - Z8] Aplikativni podaci (TCP payload): ASCII + dužina
                        var tcpPayload = tcp.Payload;
                        if (tcpPayload != null && tcpPayload.Length > 0)
                        {
                            var bytes = tcpPayload.ToMemoryStream().ToArray();
                            var ascii = Encoding.ASCII.GetString(bytes);
                            Console.WriteLine($"Aplikativni podaci (ASCII): {ascii}");
                            Console.WriteLine($"Dužina aplikativnih podataka: {bytes.Length} bajta");

                            // [KT2 - Z9] Najduži aplikativni payload (globalna statistika)
                            lock (_maxLock)
                            {
                                if (bytes.Length > _maxPayloadLen)
                                {
                                    _maxPayloadLen = bytes.Length;
                                    _maxPayloadAscii = ascii;
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("Nema aplikativnih podataka.");
                        }
                    }
                }
                else if (ip.Protocol == IpV4Protocol.Udp)
                {
                    UdpDatagram udp = ip.Udp;
                    if (udp != null)
                    {
                        // [KT2 - Z9] Statistika: broj UDP paketa
                        Interlocked.Increment(ref _udpCount);

                        // [KT1 - Z6] UDP portovi
                        Console.WriteLine($"UDP port pošiljaoca: {udp.SourcePort}");
                        Console.WriteLine($"UDP port primaoca: {udp.DestinationPort}");

                        // [KT2 - Z8] Aplikativni podaci (UDP payload): ASCII + dužina
                        var udpPayload = udp.Payload;
                        if (udpPayload != null && udpPayload.Length > 0)
                        {
                            var bytes = udpPayload.ToMemoryStream().ToArray();
                            var ascii = Encoding.ASCII.GetString(bytes);
                            Console.WriteLine($"Aplikativni podaci (ASCII): {ascii}");
                            Console.WriteLine($"Dužina aplikativnih podataka: {bytes.Length} bajta");

                            // [KT2 - Z9] Najduži aplikativni payload (globalna statistika)
                            lock (_maxLock)
                            {
                                if (bytes.Length > _maxPayloadLen)
                                {
                                    _maxPayloadLen = bytes.Length;
                                    _maxPayloadAscii = ascii;
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("Nema aplikativnih podataka.");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Nije TCP ni UDP protokol.");
                }

                // [KT2 - Z8] IPv4: za "duže" pakete ispiši checksum; za kraće TTL
                if (ip.TotalLength > 50)
                {
                    Console.WriteLine($"IPv4 kontrolna suma: 0x{ip.HeaderChecksum:X}");
                }
                else
                {
                    Console.WriteLine($"IPv4 TTL: {ip.Ttl}");
                }
            }
            else
            {
                Console.WriteLine("Nema IPv4 zaglavlja.");
            }

            Console.WriteLine(new string('-', 50));
        }
    }
}
