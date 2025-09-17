using System;
using System.Text;
using System.Threading;
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

        static int _tcpCount = 0, _udpCount = 0;
        static int _maxPayloadLen = 0;
        static string _maxPayloadAscii = "";
        static readonly object _maxLock = new object();

        static void Main(string[] args)
        {
            
            Console.Title = "PACKET SNIFFER";
            Console.OutputEncoding = Encoding.UTF8;

            Console.Write("Želite li da pokrenete presretanje paketa? (da/ne): ");
            string odgovor = Console.ReadLine()?.Trim().ToLower();
            if (odgovor != "da")
            {
                Console.WriteLine("Presretanje nije pokrenuto.");
                Console.WriteLine("Pritisni Enter za izlaz...");
                Console.ReadLine();
                return;
            }

           
            Console.WriteLine("Unesite filter (npr. tcp port 5000) ili pritisnite Enter za bez filtera:");
            string filter = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(filter))
                filter = filter.Replace(",", " and "); 

           
            var devices = LivePacketDevice.AllLocalMachine;
            if (devices.Count == 0)
            {
                Console.WriteLine("Nema dostupnih mrežnih uređaja.");
                Console.WriteLine("Pritisni Enter za izlaz...");
                Console.ReadLine();
                return;
            }

            Console.WriteLine("Izaberite mrežni uređaj:");
            for (int i = 0; i < devices.Count; i++)
                Console.WriteLine($"{i}: {devices[i].Name} - {devices[i].Description}");
            int izbor = int.Parse(Console.ReadLine() ?? "0");
            var selectedDevice = devices[izbor];

            
            using (var communicator = selectedDevice.Open(65536,
                                                          PacketDeviceOpenAttributes.Promiscuous,
                                                          1000))
            {
                // Primena filtera
                if (!string.IsNullOrWhiteSpace(filter))
                {
                    try { communicator.SetFilter(filter); }
                    catch (ArgumentException ex)
                    {
                        Console.WriteLine("Nevažeći filter – nastavljam bez filtera.");
                        Console.WriteLine($"Detalji: {ex.Message}");
                    }
                }

                Console.WriteLine("Presretanje paketa pokrenuto. Pritisnite Enter za prekid.");


                var captureTask = Task.Run(() =>
                {
                    try { communicator.ReceivePackets(0, PacketHandler); }
                    catch (Exception ex) { Console.WriteLine($"Greška tokom presretanja: {ex.Message}"); }
                });

                Console.ReadLine();   // čeka Enter
                communicator.Break(); // uredan prekid
                try { captureTask.Wait(); } catch { /* ignore */ }






                
                Console.WriteLine("\n=== STATISTIKA (posle prekida) ===");
                Console.WriteLine($"TCP paketa:       {_tcpCount}");
                Console.WriteLine($"UDP paketa:       {_udpCount}");
                if (_tcpCount > _udpCount) Console.WriteLine("➜ Više je bilo TCP paketa.");
                else if (_udpCount > _tcpCount) Console.WriteLine("➜ Više je bilo UDP paketa.");
                else Console.WriteLine("➜ Jednak broj TCP i UDP paketa.");

                if (_maxPayloadLen > 0)
                {
                    Console.WriteLine($"\nNajduži aplikativni podatak ({_maxPayloadLen} B):");
                    Console.WriteLine(_maxPayloadAscii);
                }
                else
                {
                    Console.WriteLine("\nNije detektovan aplikativni payload.");
                }
            }

            
            Console.WriteLine("\nGotovo. Pritisni Enter za izlaz...");
            Console.ReadLine();
        }






        
        private static void PacketHandler(Packet packet)
        {
            Console.WriteLine($"Presretnut paket: {packet.Timestamp}, dužina: {packet.Length}");

           
            EthernetDatagram eth = packet.Ethernet;
            if (eth != null)
            {
                Console.WriteLine($"MAC pošiljalac: {eth.Source}");
                Console.WriteLine($"MAC primalac: {eth.Destination}");
            }
            else Console.WriteLine("Nema Ethernet zaglavlja.");

            IpV4Datagram ip = packet.Ethernet?.IpV4;
            if (ip != null)
            {
                Console.WriteLine($"IP pošiljalac: {ip.Source}");
                Console.WriteLine($"IP primalac: {ip.Destination}");
                Console.WriteLine($"Transportni protokol: {ip.Protocol}");

                if (ip.Protocol == IpV4Protocol.Tcp)
                {
                    Interlocked.Increment(ref _tcpCount);
                    var tcp = ip.Tcp;
                    if (tcp != null)
                    {
                        Console.WriteLine($"TCP port pošiljaoca: {tcp.SourcePort}");
                        Console.WriteLine($"TCP port primaoca: {tcp.DestinationPort}");

                       
                        Console.Write("TCP kontrolni bitovi: ");
                        if (tcp.ControlBits.HasFlag(TcpControlBits.Synchronize)) Console.Write("SYN ");
                        if (tcp.ControlBits.HasFlag(TcpControlBits.Acknowledgment)) Console.Write("ACK ");
                        if (tcp.ControlBits.HasFlag(TcpControlBits.Fin)) Console.Write("FIN ");
                        if (tcp.ControlBits.HasFlag(TcpControlBits.Reset)) Console.Write("RST ");
                        if (tcp.ControlBits.HasFlag(TcpControlBits.Push)) Console.Write("PSH ");
                        if (tcp.ControlBits.HasFlag(TcpControlBits.Urgent)) Console.Write("URG ");
                        Console.WriteLine();
                        Console.WriteLine($"Veličina TCP zaglavlja: {tcp.HeaderLength} bajtova");





                        var payload = tcp.Payload;
                        if (payload != null && payload.Length > 0)
                        {
                            var bytes = payload.ToMemoryStream().ToArray();
                            var ascii = Encoding.ASCII.GetString(bytes);
                            Console.WriteLine($"Aplikativni podaci (ASCII): {ascii}");
                            Console.WriteLine($"Dužina aplikativnih podataka: {bytes.Length} bajta");

                            lock (_maxLock)
                            {
                                if (bytes.Length > _maxPayloadLen)
                                {
                                    _maxPayloadLen = bytes.Length;
                                    _maxPayloadAscii = ascii;
                                }
                            }
                        }
                        else Console.WriteLine("Nema aplikativnih podataka.");
                    }
                }
                else if (ip.Protocol == IpV4Protocol.Udp)
                {
                    Interlocked.Increment(ref _udpCount);
                    var udp = ip.Udp;
                    if (udp != null)
                    {
                        Console.WriteLine($"UDP port pošiljaoca: {udp.SourcePort}");
                        Console.WriteLine($"UDP port primaoca: {udp.DestinationPort}");

                        var payload = udp.Payload;
                        if (payload != null && payload.Length > 0)
                        {
                            var bytes = payload.ToMemoryStream().ToArray();
                            var ascii = Encoding.ASCII.GetString(bytes);
                            Console.WriteLine($"Aplikativni podaci (ASCII): {ascii}");
                            Console.WriteLine($"Dužina aplikativnih podataka: {bytes.Length} bajta");

                            lock (_maxLock)
                            {
                                if (bytes.Length > _maxPayloadLen)
                                {
                                    _maxPayloadLen = bytes.Length;
                                    _maxPayloadAscii = ascii;
                                }
                            }
                        }
                        else Console.WriteLine("Nema aplikativnih podataka.");
                    }
                }
                else
                {
                    Console.WriteLine("Nije TCP ni UDP protokol.");
                }

                
                if (ip.TotalLength > 50)
                    Console.WriteLine($"IPv4 kontrolna suma: 0x{ip.HeaderChecksum:X}");
                else
                    Console.WriteLine($"IPv4 TTL: {ip.Ttl}");
            }
            else
            {
                Console.WriteLine("Nema IPv4 zaglavlja.");
            }

            Console.WriteLine(new string('-', 50));
        }
    }
}
