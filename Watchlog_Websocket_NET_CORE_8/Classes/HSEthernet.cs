using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Net.NetworkInformation;

namespace HSEthernet
{
    public class HSEClient
    {
        public string IPAdresi { get; private set; }
        //public bool IsConnected { get; set; }

        public HSEClient(string ipaddresi, out bool a)
        {
            // Varsayılan olarak false değerini atayalım
            a = false;
            Ping ping = new Ping();
            try
            {
                PingReply reply = ping.Send(ipaddresi, 100); // 100 ms timeout süresi

                if (reply.Status == IPStatus.Success)
                {
                    IPAdresi = ipaddresi;
                    a = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Hata: " + ex.Message);
            }
        }

        public class SystemInformation
        {
            public string SystemSoftwareVersion;

            public string ModelName;

            public string ParameterVersion;
        }
        public class ManagementTime
        {
            public string OperationStartTime;

            public string ElapsedTime;
        }
        public class RobotPositionData
        {
            public uint DataType;

            public uint ToolNumber;

            public uint UserCoordinateNumber;

            public int[] AxisData = new int[8];

            public int FrontBack;
            public int Arm;
            public int Flip;
            public int R180;
            public int T180;
            public int S180;
            public int L180;
            public int U180;
            public int B180;
            public int E180;
            public int W180;
            public int Conversion;
            public int Redundant;
        }
        public class BasePosition
        {
            public uint DataType;

            public int[] CoordinateData = new int[8];
        }
        public class Alarm_Data
        {
            public uint AlarmCode;

            public uint AlarmData;

            public uint AlarmType;

            public string AlarmTime;

            public string AlarmName;

            public override bool Equals(object obj)
            {
                if (obj is Alarm_Data other)
                {
                    return AlarmCode == other.AlarmCode &&
                           AlarmType == other.AlarmType &&
                           AlarmTime == other.AlarmTime &&
                           AlarmName == other.AlarmName;
                }
                return false;
            }
        }
        public class Alarm_Data_Sub : Alarm_Data
        {
            public string SubCodeDataAdditionalInfo;

            public string SubCodeDataCharacterStrings;

            public string SubCodeDataCharacterStringsRev;
        }
        public class Status_Information
        {
            public bool Step;

            public bool Cycle1;

            public bool AutomaticAndContinous;

            public bool Running;

            public bool InGuardSafeOperation;

            public bool Teach;

            public bool Play;

            public bool CommandRemote;

            public bool InHoldStatusByPP;

            public bool InHoldStatusExt;

            public bool InHoldStatusCmd;

            public bool Alarming;

            public bool ErrorOccuring;

            public bool ServoON;
            public override bool Equals(object obj)
            {
                if (obj is Status_Information other)
                {
                    return Step == other.Step &&
                           Cycle1 == other.Cycle1 &&
                           AutomaticAndContinous == other.AutomaticAndContinous &&
                           Running == other.Running &&
                           InGuardSafeOperation == other.InGuardSafeOperation &&
                           Teach == other.Teach &&
                           Play == other.Play &&
                           CommandRemote == other.CommandRemote &&
                           InHoldStatusByPP == other.InHoldStatusByPP &&
                           InHoldStatusExt == other.InHoldStatusExt &&
                           InHoldStatusCmd == other.InHoldStatusCmd &&
                           Alarming == other.Alarming &&
                           ErrorOccuring == other.ErrorOccuring &&
                           ServoON == other.ServoON;
                }
                return false;
            }
        }
        public class ExecutingJobInfo
        {
            public string JobName;

            public uint LineNo;

            public uint StepNo;

            public uint SpeedOverrideValue;
        }
        public class MoveDataCartesian
        {
            public int RobotNo;

            public int StationNo;

            public int Classification;

            public int Speed;

            public int Coordinate;

            public int X;

            public int Y;

            public int Z;

            public int Tx;

            public int Ty;

            public int Tz;

            public int ToolNo;

            public int UserCoordinateNo;

            public int[] BaseAxisPosition = new int[3];

            public int[] StationAxisPosition = new int[6];

            public int FrontBack;
            public int Arm;
            public int Flip;
            public int R180;
            public int T180;
            public int S180;
            public int L180;
            public int U180;
            public int B180;
            public int E180;
            public int W180;
        }
        public class MoveDataPulse
        {
            public int RobotNo;

            public int StationNo;

            public int Classification;

            public int Speed;

            public int[] RobotAxisPulseValue = new int[8];

            public int ToolNo;

            public int[] BaseAxisPosition = new int[3];

            public int[] StationAxisPosition = new int[6];
        }
        public class ExternalAxis
        {
            public uint DataType;

            public int[] CoordinateData = new int[8];
        }



        public string Version()
        {
            string Version = "Motocom YTR, Version = 1.0";
            return Version;
        }
        public void Hold(int OnOff)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            byte[] sendPackage = new byte[(36)];
            byte[] receivedPackage = new byte[750];

            _sendHeader.DataPartSize1 = 0x04;
            _sendHeader.DataPartSize2 = 0x00;
            _sendHeader.RequestID = 0x01;
            _sendHeader.CommandNumber1 = 0x83;
            _sendHeader.CommandNumber2 = 0x00;
            _sendHeader.Instance1 = 1;
            _sendHeader.Attribute = 1;
            _sendHeader.Service = 0x10;

            if (OnOff == 1 || OnOff == 2)
            {
                _sendHeader.DataByte1 = Convert.ToUInt16(OnOff);
            }
            else
            {
                Console.WriteLine("Hold komutuna yanlış girdi");
            }

            _sendHeader.DataByte2 = 0x00;
            _sendHeader.DataByte3 = 0x00;
            _sendHeader.DataByte4 = 0x00;

            #region sendingHeaderArray...

            byte[] sendingheaderPartArray = new byte[36] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                            (byte)_sendHeader.HeaderPartSize1,
                                                            (byte)_sendHeader.HeaderPartSize2,
                                                            (byte)_sendHeader.DataPartSize1,
                                                            (byte)_sendHeader.DataPartSize2,

                                                            (byte)_sendHeader.ReserveOne,
                                                            (byte)_sendHeader.ProcessingDivision,
                                                            (byte)_sendHeader.ACK,
                                                            (byte)_sendHeader.RequestID,

                                                            (byte)_sendHeader.BlockNumber1,
                                                            (byte)_sendHeader.BlockNumber2,
                                                            (byte)_sendHeader.BlockNumber3,
                                                            (byte)_sendHeader.BlockNumber4,

                                                            (byte)_sendHeader.ReserveTwo1,
                                                            (byte)_sendHeader.ReserveTwo2,
                                                            (byte)_sendHeader.ReserveTwo3,
                                                            (byte)_sendHeader.ReserveTwo4,

                                                            (byte)_sendHeader.ReserveTwo5,
                                                            (byte)_sendHeader.ReserveTwo6,
                                                            (byte)_sendHeader.ReserveTwo7,
                                                            (byte)_sendHeader.ReserveTwo8,

                                                            (byte)_sendHeader.CommandNumber1,
                                                            (byte)_sendHeader.CommandNumber2,
                                                            (byte)_sendHeader.Instance1,
                                                            (byte)_sendHeader.Instance2,

                                                            (byte)_sendHeader.Attribute,
                                                            (byte)_sendHeader.Service,
                                                            (byte)_sendHeader.Padding1,
                                                            (byte)_sendHeader.Padding2,

                                                            (byte)_sendHeader.DataByte1,
                                                            (byte)_sendHeader.DataByte2,
                                                            (byte)_sendHeader.DataByte3,
                                                            (byte)_sendHeader.DataByte4 };
            #endregion

            for (int j = 0; j < 36; j++)
            {
                //Header Part Filling.
                sendPackage[j] = sendingheaderPartArray[j];
            }

            try
            {
                int bytesTransferred = client.Send(sendPackage);
                int bytesReceived = client.Receive(receivedPackage);
            }
            catch (Exception)
            {
                Console.WriteLine("Hold komutu paket gönderim hatası");
            }

            fillReceivedDataHeader(_receiveHeader, receivedPackage);

            if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
            {
                Console.WriteLine("Hold komutu iletişim hatası");
                client.Close();
            }

            client.Close();
        }
        public void Servo(int OnOff)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            byte[] sendPackage = new byte[(36)];
            byte[] receivedPackage = new byte[750];

            _sendHeader.DataPartSize1 = 0x04;
            _sendHeader.DataPartSize2 = 0x00;
            _sendHeader.RequestID = 0x01;
            _sendHeader.CommandNumber1 = 0x83;
            _sendHeader.CommandNumber2 = 0x00;
            _sendHeader.Instance1 = 2;
            _sendHeader.Attribute = 1;
            _sendHeader.Service = 0x10;

            if (OnOff == 1 || OnOff == 2)
            {
                _sendHeader.DataByte1 = Convert.ToUInt16(OnOff);
            }
            else
            {
                Console.WriteLine("Servo komutuna yanlış girdi");
            }

            _sendHeader.DataByte2 = 0x00;
            _sendHeader.DataByte3 = 0x00;
            _sendHeader.DataByte4 = 0x00;

            #region sendingHeaderArray...

            byte[] sendingheaderPartArray = new byte[36] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                            (byte)_sendHeader.HeaderPartSize1,
                                                            (byte)_sendHeader.HeaderPartSize2,
                                                            (byte)_sendHeader.DataPartSize1,
                                                            (byte)_sendHeader.DataPartSize2,

                                                            (byte)_sendHeader.ReserveOne,
                                                            (byte)_sendHeader.ProcessingDivision,
                                                            (byte)_sendHeader.ACK,
                                                            (byte)_sendHeader.RequestID,

                                                            (byte)_sendHeader.BlockNumber1,
                                                            (byte)_sendHeader.BlockNumber2,
                                                            (byte)_sendHeader.BlockNumber3,
                                                            (byte)_sendHeader.BlockNumber4,

                                                            (byte)_sendHeader.ReserveTwo1,
                                                            (byte)_sendHeader.ReserveTwo2,
                                                            (byte)_sendHeader.ReserveTwo3,
                                                            (byte)_sendHeader.ReserveTwo4,

                                                            (byte)_sendHeader.ReserveTwo5,
                                                            (byte)_sendHeader.ReserveTwo6,
                                                            (byte)_sendHeader.ReserveTwo7,
                                                            (byte)_sendHeader.ReserveTwo8,

                                                            (byte)_sendHeader.CommandNumber1,
                                                            (byte)_sendHeader.CommandNumber2,
                                                            (byte)_sendHeader.Instance1,
                                                            (byte)_sendHeader.Instance2,

                                                            (byte)_sendHeader.Attribute,
                                                            (byte)_sendHeader.Service,
                                                            (byte)_sendHeader.Padding1,
                                                            (byte)_sendHeader.Padding2,

                                                            (byte)_sendHeader.DataByte1,
                                                            (byte)_sendHeader.DataByte2,
                                                            (byte)_sendHeader.DataByte3,
                                                            (byte)_sendHeader.DataByte4 };
            #endregion

            for (int j = 0; j < 36; j++)
            {
                //Header Part Filling.
                sendPackage[j] = sendingheaderPartArray[j];
            }

            try
            {
                int bytesTransferred = client.Send(sendPackage);
                int bytesReceived = client.Receive(receivedPackage);
            }
            catch (Exception)
            {
                Console.WriteLine("Servo komutu paket gönderim hatası");
            }

            fillReceivedDataHeader(_receiveHeader, receivedPackage);

            if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
            {
                Console.WriteLine("Servo komutu iletişim hatası (Remote mod durumunda olduğundan emin olun)");
                client.Close();
                return;
            }

            client.Close();
        }
        public void HLock(int OnOff)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            byte[] sendPackage = new byte[(36)];
            byte[] receivedPackage = new byte[750];

            _sendHeader.DataPartSize1 = 0x04;
            _sendHeader.DataPartSize2 = 0x00;
            _sendHeader.RequestID = 0x01;
            _sendHeader.CommandNumber1 = 0x83;
            _sendHeader.CommandNumber2 = 0x00;
            _sendHeader.Instance1 = 3;
            _sendHeader.Attribute = 1;
            _sendHeader.Service = 0x10;

            if (OnOff == 1 || OnOff == 2)
            {
                _sendHeader.DataByte1 = Convert.ToUInt16(OnOff);
            }
            else
            {
                Console.WriteLine("HLock komutuna yanlış girdi");
            }

            _sendHeader.DataByte2 = 0x00;
            _sendHeader.DataByte3 = 0x00;
            _sendHeader.DataByte4 = 0x00;

            #region sendingHeaderArray...

            byte[] sendingheaderPartArray = new byte[36] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                            (byte)_sendHeader.HeaderPartSize1,
                                                            (byte)_sendHeader.HeaderPartSize2,
                                                            (byte)_sendHeader.DataPartSize1,
                                                            (byte)_sendHeader.DataPartSize2,

                                                            (byte)_sendHeader.ReserveOne,
                                                            (byte)_sendHeader.ProcessingDivision,
                                                            (byte)_sendHeader.ACK,
                                                            (byte)_sendHeader.RequestID,

                                                            (byte)_sendHeader.BlockNumber1,
                                                            (byte)_sendHeader.BlockNumber2,
                                                            (byte)_sendHeader.BlockNumber3,
                                                            (byte)_sendHeader.BlockNumber4,

                                                            (byte)_sendHeader.ReserveTwo1,
                                                            (byte)_sendHeader.ReserveTwo2,
                                                            (byte)_sendHeader.ReserveTwo3,
                                                            (byte)_sendHeader.ReserveTwo4,

                                                            (byte)_sendHeader.ReserveTwo5,
                                                            (byte)_sendHeader.ReserveTwo6,
                                                            (byte)_sendHeader.ReserveTwo7,
                                                            (byte)_sendHeader.ReserveTwo8,

                                                            (byte)_sendHeader.CommandNumber1,
                                                            (byte)_sendHeader.CommandNumber2,
                                                            (byte)_sendHeader.Instance1,
                                                            (byte)_sendHeader.Instance2,

                                                            (byte)_sendHeader.Attribute,
                                                            (byte)_sendHeader.Service,
                                                            (byte)_sendHeader.Padding1,
                                                            (byte)_sendHeader.Padding2,

                                                            (byte)_sendHeader.DataByte1,
                                                            (byte)_sendHeader.DataByte2,
                                                            (byte)_sendHeader.DataByte3,
                                                            (byte)_sendHeader.DataByte4 };
            #endregion

            for (int j = 0; j < 36; j++)
            {
                //Header Part Filling.
                sendPackage[j] = sendingheaderPartArray[j];
            }

            try
            {
                int bytesTransferred = client.Send(sendPackage);
                int bytesReceived = client.Receive(receivedPackage);
            }
            catch (Exception)
            {
                Console.WriteLine("HLock komutu paket gönderim hatası");
            }

            fillReceivedDataHeader(_receiveHeader, receivedPackage);

            if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
            {
                Console.WriteLine("HLock komutu iletişim hatası (Remote mod durumunda olduğundan emin olun)");
                client.Close();
            }

            client.Close();
        }

        public void D_Read(int VariableNumber, out uint Data)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            byte[] sendPackage = new byte[32];
            byte[] receivedPackage = new byte[750];

            _sendHeader.DataPartSize1 = 0x00;
            _sendHeader.DataPartSize2 = 0x00;
            _sendHeader.RequestID = 0x00;
            _sendHeader.CommandNumber1 = 0x7C;
            _sendHeader.CommandNumber2 = 0x00;
            _sendHeader.Instance1 = Convert.ToUInt16(VariableNumber);
            _sendHeader.Attribute = 1;
            _sendHeader.Service = 0x0E;

            #region sendingHeaderArray...

            byte[] sendingheaderPartArray = new byte[32] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                            (byte)_sendHeader.HeaderPartSize1,
                                                            (byte)_sendHeader.HeaderPartSize2,
                                                            (byte)_sendHeader.DataPartSize1,
                                                            (byte)_sendHeader.DataPartSize2,

                                                            (byte)_sendHeader.ReserveOne,
                                                            (byte)_sendHeader.ProcessingDivision,
                                                            (byte)_sendHeader.ACK,
                                                            (byte)_sendHeader.RequestID,

                                                            (byte)_sendHeader.BlockNumber1,
                                                            (byte)_sendHeader.BlockNumber2,
                                                            (byte)_sendHeader.BlockNumber3,
                                                            (byte)_sendHeader.BlockNumber4,

                                                            (byte)_sendHeader.ReserveTwo1,
                                                            (byte)_sendHeader.ReserveTwo2,
                                                            (byte)_sendHeader.ReserveTwo3,
                                                            (byte)_sendHeader.ReserveTwo4,

                                                            (byte)_sendHeader.ReserveTwo5,
                                                            (byte)_sendHeader.ReserveTwo6,
                                                            (byte)_sendHeader.ReserveTwo7,
                                                            (byte)_sendHeader.ReserveTwo8,

                                                            (byte)_sendHeader.CommandNumber1,
                                                            (byte)_sendHeader.CommandNumber2,
                                                            (byte)_sendHeader.Instance1,
                                                            (byte)_sendHeader.Instance2,

                                                            (byte)_sendHeader.Attribute,
                                                            (byte)_sendHeader.Service,
                                                            (byte)_sendHeader.Padding1,
                                                            (byte)_sendHeader.Padding2 };
            #endregion


            for (int j = 0; j < 32; j++)
            {
                //Header Part Filling.
                sendPackage[j] = sendingheaderPartArray[j];
            }

            try
            {
                int bytesTransferred = client.Send(sendPackage);
                int bytesReceived = client.Receive(receivedPackage);
            }
            catch (Exception)
            {
                Console.WriteLine($"D_Read komutu paket gönderim hatası. IP: {IPAdresi}, {DateTime.Now}");
            }

            fillReceivedDataHeader(_receiveHeader, receivedPackage);

            if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
            {
                client.Close();

                Data = 0;
                Console.WriteLine($"D_Read komutu iletişim hatası. IP: {IPAdresi}, {DateTime.Now}");
            }
            else
            {
                string HexDegeri = $"{receivedPackage[35].ToString("X2")}{receivedPackage[34].ToString("X2")}{receivedPackage[33].ToString("X2")}{receivedPackage[32].ToString("X2")}";

                int decValue = int.Parse(HexDegeri, System.Globalization.NumberStyles.HexNumber);
                Data = Convert.ToUInt32(decValue);
                client.Close();
            }
        }
        public void D_Read(int VariableNumber, int Count, out int?[] Data)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            byte[] sendPackage = new byte[36];
            byte[] receivedPackage = new byte[750];

            _sendHeader.DataPartSize1 = 0x04;
            _sendHeader.DataPartSize2 = 0x00;
            _sendHeader.RequestID = 0x00;
            _sendHeader.CommandNumber1 = 0x04;
            _sendHeader.CommandNumber2 = 0x03;
            _sendHeader.Instance1 = Convert.ToUInt16(VariableNumber);
            _sendHeader.Attribute = 0;
            _sendHeader.Service = 0x33;


            // Decimal değerini byte dizisine dönüştürelim
            byte[] bytes = Decimal.GetBits(Count).SelectMany(BitConverter.GetBytes).ToArray();

            // Byte dizisini hexadecimal olarak çevirelim
            string hexValue = BitConverter.ToString(bytes).Replace("-", "");

            // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
            string[] hexArray = new string[hexValue.Length / 2];
            for (int i = 0; i < hexArray.Length; i++)
            {
                hexArray[i] = hexValue.Substring(i * 2, 2);
            }

            // Convert hex string to UInt16
            _sendHeader.DataByte1 = Convert.ToUInt16(hexArray[0], 16);
            _sendHeader.DataByte2 = Convert.ToUInt16(hexArray[1], 16);
            _sendHeader.DataByte3 = Convert.ToUInt16(hexArray[2], 16);
            _sendHeader.DataByte4 = Convert.ToUInt16(hexArray[3], 16);

            #region sendingHeaderArray...

            byte[] sendingheaderPartArray = new byte[36] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                            (byte)_sendHeader.HeaderPartSize1,
                                                            (byte)_sendHeader.HeaderPartSize2,
                                                            (byte)_sendHeader.DataPartSize1,
                                                            (byte)_sendHeader.DataPartSize2,

                                                            (byte)_sendHeader.ReserveOne,
                                                            (byte)_sendHeader.ProcessingDivision,
                                                            (byte)_sendHeader.ACK,
                                                            (byte)_sendHeader.RequestID,

                                                            (byte)_sendHeader.BlockNumber1,
                                                            (byte)_sendHeader.BlockNumber2,
                                                            (byte)_sendHeader.BlockNumber3,
                                                            (byte)_sendHeader.BlockNumber4,

                                                            (byte)_sendHeader.ReserveTwo1,
                                                            (byte)_sendHeader.ReserveTwo2,
                                                            (byte)_sendHeader.ReserveTwo3,
                                                            (byte)_sendHeader.ReserveTwo4,

                                                            (byte)_sendHeader.ReserveTwo5,
                                                            (byte)_sendHeader.ReserveTwo6,
                                                            (byte)_sendHeader.ReserveTwo7,
                                                            (byte)_sendHeader.ReserveTwo8,

                                                            (byte)_sendHeader.CommandNumber1,
                                                            (byte)_sendHeader.CommandNumber2,
                                                            (byte)_sendHeader.Instance1,
                                                            (byte)_sendHeader.Instance2,

                                                            (byte)_sendHeader.Attribute,
                                                            (byte)_sendHeader.Service,
                                                            (byte)_sendHeader.Padding1,
                                                            (byte)_sendHeader.Padding2,

                                                            (byte)_sendHeader.DataByte1,
                                                            (byte)_sendHeader.DataByte2,
                                                            (byte)_sendHeader.DataByte3,
                                                            (byte)_sendHeader.DataByte4 };
            #endregion


            for (int j = 0; j < 36; j++)
            {
                //Header Part Filling.
                sendPackage[j] = sendingheaderPartArray[j];
            }

            try
            {
                int bytesTransferred = client.Send(sendPackage);
                int bytesReceived = client.Receive(receivedPackage);
            }
            catch (Exception)
            {
                Console.WriteLine($"D_Read komutu paket gönderim hatası. IP: {IPAdresi}, {DateTime.Now}");
            }

            fillReceivedDataHeader(_receiveHeader, receivedPackage);

            if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
            {
                client.Close();
                Console.WriteLine($"D_Read komutu iletişim hatası. IP: {IPAdresi}, {DateTime.Now}");


                int?[] a = { 0 };

                Data = a;
            }
            else
            {
                int?[] newArray = new int?[Count];

                for (int i = 0; i < Count; i++)
                {
                    string Hex = $"{receivedPackage[39 + (4 * i)].ToString("X2")}{receivedPackage[38 + (4 * i)].ToString("X2")}{receivedPackage[37 + (4 * i)].ToString("X2")}{receivedPackage[36 + (4 * i)].ToString("X2")}";

                    int decValue = int.Parse(Hex, System.Globalization.NumberStyles.HexNumber);

                    newArray[i] = decValue;
                }

                Data = newArray;
            }

            client.Close();
        }
        public void D_Write(int VariableNumber, uint Data)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            byte[] sendPackage = new byte[36];
            byte[] receivedPackage = new byte[750];

            _sendHeader.DataPartSize1 = 0x04;
            _sendHeader.DataPartSize2 = 0x00;
            _sendHeader.RequestID = 0x01;
            _sendHeader.CommandNumber1 = 0x7C;
            _sendHeader.CommandNumber2 = 0x00;
            _sendHeader.Instance1 = Convert.ToUInt16(VariableNumber);
            _sendHeader.Attribute = 1;
            _sendHeader.Service = 0x02;


            // Decimal değerini byte dizisine dönüştürelim
            byte[] bytes = Decimal.GetBits(Data).SelectMany(BitConverter.GetBytes).ToArray();

            // Byte dizisini hexadecimal olarak çevirelim
            string hexValue = BitConverter.ToString(bytes).Replace("-", "");

            // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
            string[] hexArray = new string[hexValue.Length / 2];
            for (int i = 0; i < hexArray.Length; i++)
            {
                hexArray[i] = hexValue.Substring(i * 2, 2);
            }

            // Convert hex string to UInt16
            _sendHeader.DataByte1 = Convert.ToUInt16(hexArray[0], 16);
            _sendHeader.DataByte2 = Convert.ToUInt16(hexArray[1], 16);
            _sendHeader.DataByte3 = Convert.ToUInt16(hexArray[2], 16);
            _sendHeader.DataByte4 = Convert.ToUInt16(hexArray[3], 16);


            #region sendingHeaderArray...

            byte[] sendingheaderPartArray = new byte[36] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                            (byte)_sendHeader.HeaderPartSize1,
                                                            (byte)_sendHeader.HeaderPartSize2,
                                                            (byte)_sendHeader.DataPartSize1,
                                                            (byte)_sendHeader.DataPartSize2,

                                                            (byte)_sendHeader.ReserveOne,
                                                            (byte)_sendHeader.ProcessingDivision,
                                                            (byte)_sendHeader.ACK,
                                                            (byte)_sendHeader.RequestID,

                                                            (byte)_sendHeader.BlockNumber1,
                                                            (byte)_sendHeader.BlockNumber2,
                                                            (byte)_sendHeader.BlockNumber3,
                                                            (byte)_sendHeader.BlockNumber4,

                                                            (byte)_sendHeader.ReserveTwo1,
                                                            (byte)_sendHeader.ReserveTwo2,
                                                            (byte)_sendHeader.ReserveTwo3,
                                                            (byte)_sendHeader.ReserveTwo4,

                                                            (byte)_sendHeader.ReserveTwo5,
                                                            (byte)_sendHeader.ReserveTwo6,
                                                            (byte)_sendHeader.ReserveTwo7,
                                                            (byte)_sendHeader.ReserveTwo8,

                                                            (byte)_sendHeader.CommandNumber1,
                                                            (byte)_sendHeader.CommandNumber2,
                                                            (byte)_sendHeader.Instance1,
                                                            (byte)_sendHeader.Instance2,

                                                            (byte)_sendHeader.Attribute,
                                                            (byte)_sendHeader.Service,
                                                            (byte)_sendHeader.Padding1,
                                                            (byte)_sendHeader.Padding2,

                                                            (byte)_sendHeader.DataByte1,
                                                            (byte)_sendHeader.DataByte2,
                                                            (byte)_sendHeader.DataByte3,
                                                            (byte)_sendHeader.DataByte4 };
            #endregion


            for (int j = 0; j < 36; j++)
            {
                //Header Part Filling.
                sendPackage[j] = sendingheaderPartArray[j];
            }

            try
            {
                int bytesTransferred = client.Send(sendPackage);
                int bytesReceived = client.Receive(receivedPackage);
            }
            catch (Exception)
            {
                Console.WriteLine("D_Write komutu paket gönderim hatası");
            }

            fillReceivedDataHeader(_receiveHeader, receivedPackage);

            if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
            {
                client.Close();

                Console.WriteLine("D_Write komutu iletişim hatası");
            }
            else
            {
                client.Close();
            }
        }
        public void D_Write(int VariableNumber, int Count, int[] Data)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();

            MyData myData = new MyData(500);

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            byte[] sendPackage = new byte[36 + (Count * 4)];
            byte[] receivedPackage = new byte[750];


            // Decimal değerini byte dizisine dönüştürelim
            byte[] bytes2 = Decimal.GetBits((Count + 1) * 4).SelectMany(BitConverter.GetBytes).ToArray();

            // Byte dizisini hexadecimal olarak çevirelim
            string hexValue2 = BitConverter.ToString(bytes2).Replace("-", "");

            // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
            string[] hexArray2 = new string[hexValue2.Length / 2];
            for (int i = 0; i < hexArray2.Length; i++)
            {
                hexArray2[i] = hexValue2.Substring(i * 2, 2);
            }

            // Convert hex string to UInt16
            _sendHeader.DataPartSize1 = Convert.ToUInt16(hexArray2[0], 16);
            _sendHeader.DataPartSize2 = Convert.ToUInt16(hexArray2[1], 16);
            _sendHeader.RequestID = 0x01;
            _sendHeader.CommandNumber1 = 0x04;
            _sendHeader.CommandNumber2 = 0x03;
            _sendHeader.Instance1 = Convert.ToUInt16(VariableNumber);
            _sendHeader.Attribute = 0;
            _sendHeader.Service = 0x34;

            #region KAÇ ADET VERİYE YAZILACAK

            // Decimal değerini byte dizisine dönüştürelim
            byte[] bytes = Decimal.GetBits(Count).SelectMany(BitConverter.GetBytes).ToArray();

            // Byte dizisini hexadecimal olarak çevirelim
            string hexValue = BitConverter.ToString(bytes).Replace("-", "");

            // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
            string[] hexArray = new string[hexValue.Length / 2];
            for (int i = 0; i < hexArray.Length; i++)
            {
                hexArray[i] = hexValue.Substring(i * 2, 2);
            }

            // Convert hex string to UInt16
            _sendHeader.DataByte1 = Convert.ToUInt16(hexArray[0], 16);
            _sendHeader.DataByte2 = Convert.ToUInt16(hexArray[1], 16);
            _sendHeader.DataByte3 = Convert.ToUInt16(hexArray[2], 16);
            _sendHeader.DataByte4 = Convert.ToUInt16(hexArray[3], 16);

            #endregion

            for (int a = 0; a < Data.Length; a++)
            {
                // Decimal değerini byte dizisine dönüştürelim
                byte[] bytes1 = Decimal.GetBits(Data[a]).SelectMany(BitConverter.GetBytes).ToArray();

                // Byte dizisini hexadecimal olarak çevirelim
                string hexValue1 = BitConverter.ToString(bytes1).Replace("-", "");

                // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
                string[] hexArray1 = new string[hexValue1.Length / 2];

                for (int i = 0; i < hexArray1.Length; i++)
                {
                    hexArray1[i] = hexValue1.Substring(i * 2, 2);
                }

                for (int i = 0; i < 4; i++)
                {
                    myData[(a * 4) + i] = Convert.ToUInt16(hexArray1[i], 16);
                }
            }

            #region sendingHeaderArray...

            byte[] sendingheaderPartArray = new byte[36 + (Data.Length * 4)];

            sendingheaderPartArray[0] = Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0];
            sendingheaderPartArray[1] = Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0];
            sendingheaderPartArray[2] = Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0];
            sendingheaderPartArray[3] = Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0];

            sendingheaderPartArray[4] = (byte)_sendHeader.HeaderPartSize1;
            sendingheaderPartArray[5] = (byte)_sendHeader.HeaderPartSize2;
            sendingheaderPartArray[6] = (byte)_sendHeader.DataPartSize1;
            sendingheaderPartArray[7] = (byte)_sendHeader.DataPartSize2;

            sendingheaderPartArray[8] = (byte)_sendHeader.ReserveOne;
            sendingheaderPartArray[9] = (byte)_sendHeader.ProcessingDivision;
            sendingheaderPartArray[10] = (byte)_sendHeader.ACK;
            sendingheaderPartArray[11] = (byte)_sendHeader.RequestID;

            sendingheaderPartArray[12] = (byte)_sendHeader.BlockNumber1;
            sendingheaderPartArray[13] = (byte)_sendHeader.BlockNumber2;
            sendingheaderPartArray[14] = (byte)_sendHeader.BlockNumber3;
            sendingheaderPartArray[15] = (byte)_sendHeader.BlockNumber4;

            sendingheaderPartArray[16] = (byte)_sendHeader.ReserveTwo1;
            sendingheaderPartArray[17] = (byte)_sendHeader.ReserveTwo2;
            sendingheaderPartArray[18] = (byte)_sendHeader.ReserveTwo3;
            sendingheaderPartArray[19] = (byte)_sendHeader.ReserveTwo4;

            sendingheaderPartArray[20] = (byte)_sendHeader.ReserveTwo5;
            sendingheaderPartArray[21] = (byte)_sendHeader.ReserveTwo6;
            sendingheaderPartArray[22] = (byte)_sendHeader.ReserveTwo7;
            sendingheaderPartArray[23] = (byte)_sendHeader.ReserveTwo8;

            sendingheaderPartArray[24] = (byte)_sendHeader.CommandNumber1;
            sendingheaderPartArray[25] = (byte)_sendHeader.CommandNumber2;
            sendingheaderPartArray[26] = (byte)_sendHeader.Instance1;
            sendingheaderPartArray[27] = (byte)_sendHeader.Instance2;

            sendingheaderPartArray[28] = (byte)_sendHeader.Attribute;
            sendingheaderPartArray[29] = (byte)_sendHeader.Service;
            sendingheaderPartArray[30] = (byte)_sendHeader.Padding1;
            sendingheaderPartArray[31] = (byte)_sendHeader.Padding2;

            sendingheaderPartArray[32] = (byte)_sendHeader.DataByte1;
            sendingheaderPartArray[33] = (byte)_sendHeader.DataByte2;
            sendingheaderPartArray[34] = (byte)_sendHeader.DataByte3;
            sendingheaderPartArray[35] = (byte)_sendHeader.DataByte4;

            #endregion


            for (int i = 0; i < (Data.Length * 4); i++)
            {
                sendingheaderPartArray[36 + i] = (byte)myData[i];
            }


            for (int j = 0; j < (36 + Count * 4); j++)
            {
                //Header Part Filling.
                sendPackage[j] = sendingheaderPartArray[j];
            }

            try
            {
                int bytesTransferred = client.Send(sendPackage);
                int bytesReceived = client.Receive(receivedPackage);
            }
            catch (Exception)
            {
                Console.WriteLine("D_Write komutu paket gönderim hatası");
            }

            fillReceivedDataHeader(_receiveHeader, receivedPackage);

            if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
            {
                client.Close();

                Console.WriteLine("D_Write komutu iletişim hatası");
            }
            else
            {
                client.Close();
            }
        }

        public void I_Read(int VariableNumber, out ushort Data)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            byte[] sendPackage = new byte[32];
            byte[] receivedPackage = new byte[750];

            _sendHeader.DataPartSize1 = 0x00;
            _sendHeader.DataPartSize2 = 0x00;
            _sendHeader.RequestID = 0x00;
            _sendHeader.CommandNumber1 = 0x7B;
            _sendHeader.CommandNumber2 = 0x00;
            _sendHeader.Instance1 = Convert.ToUInt16(VariableNumber);
            _sendHeader.Attribute = 1;
            _sendHeader.Service = 0x0E;

            #region sendingHeaderArray...

            byte[] sendingheaderPartArray = new byte[32] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                            (byte)_sendHeader.HeaderPartSize1,
                                                            (byte)_sendHeader.HeaderPartSize2,
                                                            (byte)_sendHeader.DataPartSize1,
                                                            (byte)_sendHeader.DataPartSize2,

                                                            (byte)_sendHeader.ReserveOne,
                                                            (byte)_sendHeader.ProcessingDivision,
                                                            (byte)_sendHeader.ACK,
                                                            (byte)_sendHeader.RequestID,

                                                            (byte)_sendHeader.BlockNumber1,
                                                            (byte)_sendHeader.BlockNumber2,
                                                            (byte)_sendHeader.BlockNumber3,
                                                            (byte)_sendHeader.BlockNumber4,

                                                            (byte)_sendHeader.ReserveTwo1,
                                                            (byte)_sendHeader.ReserveTwo2,
                                                            (byte)_sendHeader.ReserveTwo3,
                                                            (byte)_sendHeader.ReserveTwo4,

                                                            (byte)_sendHeader.ReserveTwo5,
                                                            (byte)_sendHeader.ReserveTwo6,
                                                            (byte)_sendHeader.ReserveTwo7,
                                                            (byte)_sendHeader.ReserveTwo8,

                                                            (byte)_sendHeader.CommandNumber1,
                                                            (byte)_sendHeader.CommandNumber2,
                                                            (byte)_sendHeader.Instance1,
                                                            (byte)_sendHeader.Instance2,

                                                            (byte)_sendHeader.Attribute,
                                                            (byte)_sendHeader.Service,
                                                            (byte)_sendHeader.Padding1,
                                                            (byte)_sendHeader.Padding2 };
            #endregion


            for (int j = 0; j < 32; j++)
            {
                //Header Part Filling.
                sendPackage[j] = sendingheaderPartArray[j];
            }

            try
            {
                int bytesTransferred = client.Send(sendPackage);
                int bytesReceived = client.Receive(receivedPackage);
            }
            catch (Exception)
            {
                Console.WriteLine("I_Read komutu paket gönderim hatası");
            }

            fillReceivedDataHeader(_receiveHeader, receivedPackage);

            if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
            {
                client.Close();

                Data = 0;
                Console.WriteLine("I_Read komutu iletişim hatası");
            }
            else
            {
                string HexDegeri = $"{receivedPackage[33].ToString("X2")}{receivedPackage[32].ToString("X2")}";

                int decValue = int.Parse(HexDegeri, System.Globalization.NumberStyles.HexNumber);
                Data = Convert.ToUInt16(decValue);
                client.Close();
            }
        }
        public void I_Read(int VariableNumber, int Count, out short?[] Data)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            byte[] sendPackage = new byte[36];
            byte[] receivedPackage = new byte[750];

            _sendHeader.DataPartSize1 = 0x04;
            _sendHeader.DataPartSize2 = 0x00;
            _sendHeader.RequestID = 0x00;
            _sendHeader.CommandNumber1 = 0x03;
            _sendHeader.CommandNumber2 = 0x03;
            _sendHeader.Instance1 = Convert.ToUInt16(VariableNumber);
            _sendHeader.Attribute = 0;
            _sendHeader.Service = 0x33;


            // Decimal değerini byte dizisine dönüştürelim
            byte[] bytes = Decimal.GetBits(Count).SelectMany(BitConverter.GetBytes).ToArray();

            // Byte dizisini hexadecimal olarak çevirelim
            string hexValue = BitConverter.ToString(bytes).Replace("-", "");

            // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
            string[] hexArray = new string[hexValue.Length / 2];
            for (int i = 0; i < hexArray.Length; i++)
            {
                hexArray[i] = hexValue.Substring(i * 2, 2);
            }

            // Convert hex string to UInt16
            _sendHeader.DataByte1 = Convert.ToUInt16(hexArray[0], 16);
            _sendHeader.DataByte2 = Convert.ToUInt16(hexArray[1], 16);
            _sendHeader.DataByte3 = Convert.ToUInt16(hexArray[2], 16);
            _sendHeader.DataByte4 = Convert.ToUInt16(hexArray[3], 16);

            #region sendingHeaderArray...

            byte[] sendingheaderPartArray = new byte[36] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                            (byte)_sendHeader.HeaderPartSize1,
                                                            (byte)_sendHeader.HeaderPartSize2,
                                                            (byte)_sendHeader.DataPartSize1,
                                                            (byte)_sendHeader.DataPartSize2,

                                                            (byte)_sendHeader.ReserveOne,
                                                            (byte)_sendHeader.ProcessingDivision,
                                                            (byte)_sendHeader.ACK,
                                                            (byte)_sendHeader.RequestID,

                                                            (byte)_sendHeader.BlockNumber1,
                                                            (byte)_sendHeader.BlockNumber2,
                                                            (byte)_sendHeader.BlockNumber3,
                                                            (byte)_sendHeader.BlockNumber4,

                                                            (byte)_sendHeader.ReserveTwo1,
                                                            (byte)_sendHeader.ReserveTwo2,
                                                            (byte)_sendHeader.ReserveTwo3,
                                                            (byte)_sendHeader.ReserveTwo4,

                                                            (byte)_sendHeader.ReserveTwo5,
                                                            (byte)_sendHeader.ReserveTwo6,
                                                            (byte)_sendHeader.ReserveTwo7,
                                                            (byte)_sendHeader.ReserveTwo8,

                                                            (byte)_sendHeader.CommandNumber1,
                                                            (byte)_sendHeader.CommandNumber2,
                                                            (byte)_sendHeader.Instance1,
                                                            (byte)_sendHeader.Instance2,

                                                            (byte)_sendHeader.Attribute,
                                                            (byte)_sendHeader.Service,
                                                            (byte)_sendHeader.Padding1,
                                                            (byte)_sendHeader.Padding2,

                                                            (byte)_sendHeader.DataByte1,
                                                            (byte)_sendHeader.DataByte2,
                                                            (byte)_sendHeader.DataByte3,
                                                            (byte)_sendHeader.DataByte4 };
            #endregion


            for (int j = 0; j < 36; j++)
            {
                //Header Part Filling.
                sendPackage[j] = sendingheaderPartArray[j];
            }

            try
            {
                int bytesTransferred = client.Send(sendPackage);
                int bytesReceived = client.Receive(receivedPackage);
            }
            catch (Exception)
            {
                Console.WriteLine("I_Read komutu paket gönderim hatası");
            }

            fillReceivedDataHeader(_receiveHeader, receivedPackage);

            if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
            {
                client.Close();
                Console.WriteLine("I_Read komutu iletişim hatası");

                short?[] a = { 0 };

                Data = a;
            }
            else
            {
                short?[] newArray = new short?[Count];

                for (int i = 0; i < Count; i++)
                {
                    string Hex = $"{receivedPackage[37 + (2 * i)].ToString("X2")}{receivedPackage[36 + (2 * i)].ToString("X2")}";

                    int decValue = int.Parse(Hex, System.Globalization.NumberStyles.HexNumber);

                    newArray[i] = Convert.ToInt16(decValue);
                }

                Data = newArray;
            }

            client.Close();
        }
        public void I_Write(int VariableNumber, ushort Data)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            byte[] sendPackage = new byte[36];
            byte[] receivedPackage = new byte[750];

            _sendHeader.DataPartSize1 = 0x04;
            _sendHeader.DataPartSize2 = 0x00;
            _sendHeader.RequestID = 0x01;
            _sendHeader.CommandNumber1 = 0x7B;
            _sendHeader.CommandNumber2 = 0x00;
            _sendHeader.Instance1 = Convert.ToUInt16(VariableNumber);
            _sendHeader.Attribute = 1;
            _sendHeader.Service = 0x02;


            // Decimal değerini byte dizisine dönüştürelim
            byte[] bytes = Decimal.GetBits(Data).SelectMany(BitConverter.GetBytes).ToArray();

            // Byte dizisini hexadecimal olarak çevirelim
            string hexValue = BitConverter.ToString(bytes).Replace("-", "");

            // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
            string[] hexArray = new string[hexValue.Length / 2];
            for (int i = 0; i < hexArray.Length; i++)
            {
                hexArray[i] = hexValue.Substring(i * 2, 2);
            }

            // Convert hex string to UInt16
            _sendHeader.DataByte1 = Convert.ToUInt16(hexArray[0], 16);
            _sendHeader.DataByte2 = Convert.ToUInt16(hexArray[1], 16);
            // _sendHeader.DataByte3 = Convert.ToUInt16(hexArray[2], 16);
            // _sendHeader.DataByte4 = Convert.ToUInt16(hexArray[3], 16);


            #region sendingHeaderArray...

            byte[] sendingheaderPartArray = new byte[36] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                            (byte)_sendHeader.HeaderPartSize1,
                                                            (byte)_sendHeader.HeaderPartSize2,
                                                            (byte)_sendHeader.DataPartSize1,
                                                            (byte)_sendHeader.DataPartSize2,

                                                            (byte)_sendHeader.ReserveOne,
                                                            (byte)_sendHeader.ProcessingDivision,
                                                            (byte)_sendHeader.ACK,
                                                            (byte)_sendHeader.RequestID,

                                                            (byte)_sendHeader.BlockNumber1,
                                                            (byte)_sendHeader.BlockNumber2,
                                                            (byte)_sendHeader.BlockNumber3,
                                                            (byte)_sendHeader.BlockNumber4,

                                                            (byte)_sendHeader.ReserveTwo1,
                                                            (byte)_sendHeader.ReserveTwo2,
                                                            (byte)_sendHeader.ReserveTwo3,
                                                            (byte)_sendHeader.ReserveTwo4,

                                                            (byte)_sendHeader.ReserveTwo5,
                                                            (byte)_sendHeader.ReserveTwo6,
                                                            (byte)_sendHeader.ReserveTwo7,
                                                            (byte)_sendHeader.ReserveTwo8,

                                                            (byte)_sendHeader.CommandNumber1,
                                                            (byte)_sendHeader.CommandNumber2,
                                                            (byte)_sendHeader.Instance1,
                                                            (byte)_sendHeader.Instance2,

                                                            (byte)_sendHeader.Attribute,
                                                            (byte)_sendHeader.Service,
                                                            (byte)_sendHeader.Padding1,
                                                            (byte)_sendHeader.Padding2,

                                                            (byte)_sendHeader.DataByte1,
                                                            (byte)_sendHeader.DataByte2,
                                                            (byte)_sendHeader.DataByte3,
                                                            (byte)_sendHeader.DataByte4 };
            #endregion


            for (int j = 0; j < 36; j++)
            {
                //Header Part Filling.
                sendPackage[j] = sendingheaderPartArray[j];
            }

            try
            {
                int bytesTransferred = client.Send(sendPackage);
                int bytesReceived = client.Receive(receivedPackage);
            }
            catch (Exception)
            {
                Console.WriteLine("I_Write komutu paket gönderim hatası");
            }

            fillReceivedDataHeader(_receiveHeader, receivedPackage);

            if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
            {
                client.Close();

                Console.WriteLine("I_Write komutu iletişim hatası");
            }
            else
            {
                client.Close();
            }
        }
        public void I_Write(int VariableNumber, int Count, short[] Data)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();

            MyData myData = new MyData(500);

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            byte[] sendPackage = new byte[36 + (Count * 2)];
            byte[] receivedPackage = new byte[750];



            // Decimal değerini byte dizisine dönüştürelim
            byte[] bytes2 = Decimal.GetBits((Count * 2) + 4).SelectMany(BitConverter.GetBytes).ToArray();

            // Byte dizisini hexadecimal olarak çevirelim
            string hexValue2 = BitConverter.ToString(bytes2).Replace("-", "");

            // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
            string[] hexArray2 = new string[hexValue2.Length / 2];
            for (int i = 0; i < hexArray2.Length; i++)
            {
                hexArray2[i] = hexValue2.Substring(i * 2, 2);
            }



            // Convert hex string to UInt16
            _sendHeader.DataPartSize1 = Convert.ToUInt16(hexArray2[0], 16);
            _sendHeader.DataPartSize2 = Convert.ToUInt16(hexArray2[1], 16);
            _sendHeader.RequestID = 0x01;
            _sendHeader.CommandNumber1 = 0x03;
            _sendHeader.CommandNumber2 = 0x03;
            _sendHeader.Instance1 = Convert.ToUInt16(VariableNumber);
            _sendHeader.Attribute = 0;
            _sendHeader.Service = 0x34;

            #region KAÇ ADET VERİYE YAZILACAK

            // Decimal değerini byte dizisine dönüştürelim
            byte[] bytes = Decimal.GetBits(Count).SelectMany(BitConverter.GetBytes).ToArray();

            // Byte dizisini hexadecimal olarak çevirelim
            string hexValue = BitConverter.ToString(bytes).Replace("-", "");

            // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
            string[] hexArray = new string[hexValue.Length / 2];
            for (int i = 0; i < hexArray.Length; i++)
            {
                hexArray[i] = hexValue.Substring(i * 2, 2);
            }

            // Convert hex string to UInt16
            _sendHeader.DataByte1 = Convert.ToUInt16(hexArray[0], 16);
            _sendHeader.DataByte2 = Convert.ToUInt16(hexArray[1], 16);
            _sendHeader.DataByte3 = Convert.ToUInt16(hexArray[2], 16);
            _sendHeader.DataByte4 = Convert.ToUInt16(hexArray[3], 16);

            #endregion

            for (int a = 0; a < Data.Length; a++)
            {
                // Decimal değerini byte dizisine dönüştürelim
                byte[] bytes1 = Decimal.GetBits(Data[a]).SelectMany(BitConverter.GetBytes).ToArray();

                // Byte dizisini hexadecimal olarak çevirelim
                string hexValue1 = BitConverter.ToString(bytes1).Replace("-", "");

                // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
                string[] hexArray1 = new string[hexValue1.Length / 2];

                for (int i = 0; i < hexArray1.Length; i++)
                {
                    hexArray1[i] = hexValue1.Substring(i * 2, 2);
                }

                for (int i = 0; i < 2; i++)
                {
                    myData[(a * 2) + i] = Convert.ToUInt16(hexArray1[i], 16);
                }
            }

            #region sendingHeaderArray...

            byte[] sendingheaderPartArray = new byte[36 + (Data.Length * 2)];

            sendingheaderPartArray[0] = Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0];
            sendingheaderPartArray[1] = Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0];
            sendingheaderPartArray[2] = Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0];
            sendingheaderPartArray[3] = Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0];

            sendingheaderPartArray[4] = (byte)_sendHeader.HeaderPartSize1;
            sendingheaderPartArray[5] = (byte)_sendHeader.HeaderPartSize2;
            sendingheaderPartArray[6] = (byte)_sendHeader.DataPartSize1;
            sendingheaderPartArray[7] = (byte)_sendHeader.DataPartSize2;

            sendingheaderPartArray[8] = (byte)_sendHeader.ReserveOne;
            sendingheaderPartArray[9] = (byte)_sendHeader.ProcessingDivision;
            sendingheaderPartArray[10] = (byte)_sendHeader.ACK;
            sendingheaderPartArray[11] = (byte)_sendHeader.RequestID;

            sendingheaderPartArray[12] = (byte)_sendHeader.BlockNumber1;
            sendingheaderPartArray[13] = (byte)_sendHeader.BlockNumber2;
            sendingheaderPartArray[14] = (byte)_sendHeader.BlockNumber3;
            sendingheaderPartArray[15] = (byte)_sendHeader.BlockNumber4;

            sendingheaderPartArray[16] = (byte)_sendHeader.ReserveTwo1;
            sendingheaderPartArray[17] = (byte)_sendHeader.ReserveTwo2;
            sendingheaderPartArray[18] = (byte)_sendHeader.ReserveTwo3;
            sendingheaderPartArray[19] = (byte)_sendHeader.ReserveTwo4;

            sendingheaderPartArray[20] = (byte)_sendHeader.ReserveTwo5;
            sendingheaderPartArray[21] = (byte)_sendHeader.ReserveTwo6;
            sendingheaderPartArray[22] = (byte)_sendHeader.ReserveTwo7;
            sendingheaderPartArray[23] = (byte)_sendHeader.ReserveTwo8;

            sendingheaderPartArray[24] = (byte)_sendHeader.CommandNumber1;
            sendingheaderPartArray[25] = (byte)_sendHeader.CommandNumber2;
            sendingheaderPartArray[26] = (byte)_sendHeader.Instance1;
            sendingheaderPartArray[27] = (byte)_sendHeader.Instance2;

            sendingheaderPartArray[28] = (byte)_sendHeader.Attribute;
            sendingheaderPartArray[29] = (byte)_sendHeader.Service;
            sendingheaderPartArray[30] = (byte)_sendHeader.Padding1;
            sendingheaderPartArray[31] = (byte)_sendHeader.Padding2;

            sendingheaderPartArray[32] = (byte)_sendHeader.DataByte1;
            sendingheaderPartArray[33] = (byte)_sendHeader.DataByte2;
            sendingheaderPartArray[34] = (byte)_sendHeader.DataByte3;
            sendingheaderPartArray[35] = (byte)_sendHeader.DataByte4;

            #endregion


            for (int i = 0; i < (Data.Length * 2); i++)
            {
                sendingheaderPartArray[36 + i] = (byte)myData[i];
            }


            for (int j = 0; j < (36 + Count * 2); j++)
            {
                //Header Part Filling.
                sendPackage[j] = sendingheaderPartArray[j];
            }

            try
            {
                int bytesTransferred = client.Send(sendPackage);
                int bytesReceived = client.Receive(receivedPackage);
            }
            catch (Exception)
            {
                Console.WriteLine("I_Write komutu paket gönderim hatası");
            }

            fillReceivedDataHeader(_receiveHeader, receivedPackage);

            if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
            {
                client.Close();

                Console.WriteLine("I_Write komutu iletişim hatası");
            }
            else
            {
                client.Close();
            }
        }

        public void R_Read(int VariableNumber, out float Data)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            byte[] sendPackage = new byte[32];
            byte[] receivedPackage = new byte[750];

            _sendHeader.DataPartSize1 = 0x00;
            _sendHeader.DataPartSize2 = 0x00;
            _sendHeader.RequestID = 0x00;
            _sendHeader.CommandNumber1 = 0x7D;
            _sendHeader.CommandNumber2 = 0x00;
            _sendHeader.Instance1 = Convert.ToUInt16(VariableNumber);
            _sendHeader.Attribute = 1;
            _sendHeader.Service = 0x0E;

            #region sendingHeaderArray...

            byte[] sendingheaderPartArray = new byte[32] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                            (byte)_sendHeader.HeaderPartSize1,
                                                            (byte)_sendHeader.HeaderPartSize2,
                                                            (byte)_sendHeader.DataPartSize1,
                                                            (byte)_sendHeader.DataPartSize2,

                                                            (byte)_sendHeader.ReserveOne,
                                                            (byte)_sendHeader.ProcessingDivision,
                                                            (byte)_sendHeader.ACK,
                                                            (byte)_sendHeader.RequestID,

                                                            (byte)_sendHeader.BlockNumber1,
                                                            (byte)_sendHeader.BlockNumber2,
                                                            (byte)_sendHeader.BlockNumber3,
                                                            (byte)_sendHeader.BlockNumber4,

                                                            (byte)_sendHeader.ReserveTwo1,
                                                            (byte)_sendHeader.ReserveTwo2,
                                                            (byte)_sendHeader.ReserveTwo3,
                                                            (byte)_sendHeader.ReserveTwo4,

                                                            (byte)_sendHeader.ReserveTwo5,
                                                            (byte)_sendHeader.ReserveTwo6,
                                                            (byte)_sendHeader.ReserveTwo7,
                                                            (byte)_sendHeader.ReserveTwo8,

                                                            (byte)_sendHeader.CommandNumber1,
                                                            (byte)_sendHeader.CommandNumber2,
                                                            (byte)_sendHeader.Instance1,
                                                            (byte)_sendHeader.Instance2,

                                                            (byte)_sendHeader.Attribute,
                                                            (byte)_sendHeader.Service,
                                                            (byte)_sendHeader.Padding1,
                                                            (byte)_sendHeader.Padding2 };
            #endregion


            for (int j = 0; j < 32; j++)
            {
                //Header Part Filling.
                sendPackage[j] = sendingheaderPartArray[j];
            }

            try
            {
                int bytesTransferred = client.Send(sendPackage);
                int bytesReceived = client.Receive(receivedPackage);
            }
            catch (Exception)
            {
                Console.WriteLine("R_Read komutu paket gönderim hatası");
            }

            fillReceivedDataHeader(_receiveHeader, receivedPackage);

            if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
            {
                client.Close();

                Data = 0;
                Console.WriteLine("R_Read komutu iletişim hatası");
            }
            else
            {
                string HexDegeri = $"{receivedPackage[35].ToString("X2")}{receivedPackage[34].ToString("X2")}{receivedPackage[33].ToString("X2")}{receivedPackage[32].ToString("X2")}";

                try
                {
                    string hexInput = HexDegeri.Trim();

                    // Hexadecimal girişini ondalık sayıya dönüştürelim
                    float result = HexToFloat(hexInput);

                    Data = result;
                }
                catch (Exception ex)
                {
                    Data = 0;
                    Console.WriteLine("An error occurred: " + ex.Message);
                }

                client.Close();
            }
        }
        public void R_Read(int VariableNumber, int Count, out float?[] Data)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            byte[] sendPackage = new byte[36];
            byte[] receivedPackage = new byte[750];

            _sendHeader.DataPartSize1 = 0x04;
            _sendHeader.DataPartSize2 = 0x00;
            _sendHeader.RequestID = 0x00;
            _sendHeader.CommandNumber1 = 0x05;
            _sendHeader.CommandNumber2 = 0x03;
            _sendHeader.Instance1 = Convert.ToUInt16(VariableNumber);
            _sendHeader.Attribute = 0;
            _sendHeader.Service = 0x33;


            // Decimal değerini byte dizisine dönüştürelim
            byte[] bytes = Decimal.GetBits(Count).SelectMany(BitConverter.GetBytes).ToArray();

            // Byte dizisini hexadecimal olarak çevirelim
            string hexValue = BitConverter.ToString(bytes).Replace("-", "");

            // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
            string[] hexArray = new string[hexValue.Length / 2];
            for (int i = 0; i < hexArray.Length; i++)
            {
                hexArray[i] = hexValue.Substring(i * 2, 2);
            }

            // Convert hex string to UInt16
            _sendHeader.DataByte1 = Convert.ToUInt16(hexArray[0], 16);
            _sendHeader.DataByte2 = Convert.ToUInt16(hexArray[1], 16);
            _sendHeader.DataByte3 = Convert.ToUInt16(hexArray[2], 16);
            _sendHeader.DataByte4 = Convert.ToUInt16(hexArray[3], 16);

            #region sendingHeaderArray...

            byte[] sendingheaderPartArray = new byte[36] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                            (byte)_sendHeader.HeaderPartSize1,
                                                            (byte)_sendHeader.HeaderPartSize2,
                                                            (byte)_sendHeader.DataPartSize1,
                                                            (byte)_sendHeader.DataPartSize2,

                                                            (byte)_sendHeader.ReserveOne,
                                                            (byte)_sendHeader.ProcessingDivision,
                                                            (byte)_sendHeader.ACK,
                                                            (byte)_sendHeader.RequestID,

                                                            (byte)_sendHeader.BlockNumber1,
                                                            (byte)_sendHeader.BlockNumber2,
                                                            (byte)_sendHeader.BlockNumber3,
                                                            (byte)_sendHeader.BlockNumber4,

                                                            (byte)_sendHeader.ReserveTwo1,
                                                            (byte)_sendHeader.ReserveTwo2,
                                                            (byte)_sendHeader.ReserveTwo3,
                                                            (byte)_sendHeader.ReserveTwo4,

                                                            (byte)_sendHeader.ReserveTwo5,
                                                            (byte)_sendHeader.ReserveTwo6,
                                                            (byte)_sendHeader.ReserveTwo7,
                                                            (byte)_sendHeader.ReserveTwo8,

                                                            (byte)_sendHeader.CommandNumber1,
                                                            (byte)_sendHeader.CommandNumber2,
                                                            (byte)_sendHeader.Instance1,
                                                            (byte)_sendHeader.Instance2,

                                                            (byte)_sendHeader.Attribute,
                                                            (byte)_sendHeader.Service,
                                                            (byte)_sendHeader.Padding1,
                                                            (byte)_sendHeader.Padding2,

                                                            (byte)_sendHeader.DataByte1,
                                                            (byte)_sendHeader.DataByte2,
                                                            (byte)_sendHeader.DataByte3,
                                                            (byte)_sendHeader.DataByte4 };
            #endregion


            for (int j = 0; j < 36; j++)
            {
                //Header Part Filling.
                sendPackage[j] = sendingheaderPartArray[j];
            }

            try
            {
                int bytesTransferred = client.Send(sendPackage);
                int bytesReceived = client.Receive(receivedPackage);
            }
            catch (Exception)
            {
                Console.WriteLine("R_Read komutu paket gönderim hatası");
            }

            fillReceivedDataHeader(_receiveHeader, receivedPackage);

            if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
            {
                client.Close();
                Console.WriteLine("R_Read komutu iletişim hatası");

                float?[] a = { 0 };

                Data = a;
            }
            else
            {
                float?[] newArray = new float?[Count];

                for (int i = 0; i < Count; i++)
                {
                    string Hex = $"{receivedPackage[39 + (4 * i)].ToString("X2")}{receivedPackage[38 + (4 * i)].ToString("X2")}{receivedPackage[37 + (4 * i)].ToString("X2")}{receivedPackage[36 + (4 * i)].ToString("X2")}";

                    string hexInput = Hex.Trim();

                    // Hexadecimal girişini ondalık sayıya dönüştürelim
                    float result = HexToFloat(hexInput);

                    newArray[i] = result;
                }

                Data = newArray;
            }

            client.Close();
        }
        public void R_Write(int VariableNumber, float Data)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            byte[] sendPackage = new byte[36];
            byte[] receivedPackage = new byte[750];

            _sendHeader.DataPartSize1 = 0x04;
            _sendHeader.DataPartSize2 = 0x00;
            _sendHeader.RequestID = 0x01;
            _sendHeader.CommandNumber1 = 0x7D;
            _sendHeader.CommandNumber2 = 0x00;
            _sendHeader.Instance1 = Convert.ToUInt16(VariableNumber);
            _sendHeader.Attribute = 1;
            _sendHeader.Service = 0x02;


            // Float değerini byte dizisine dönüştürelim
            byte[] bytes = BitConverter.GetBytes(Data);

            // Byte dizisini hexadecimal olarak çevirelim
            string hexValue = BitConverter.ToString(bytes).Replace("-", "");

            // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
            string[] hexArray = new string[hexValue.Length / 2];
            for (int i = 0; i < hexArray.Length; i++)
            {
                hexArray[i] = hexValue.Substring(i * 2, 2);
            }

            // Convert hex string to UInt16
            _sendHeader.DataByte1 = Convert.ToUInt16(hexArray[0], 16);
            _sendHeader.DataByte2 = Convert.ToUInt16(hexArray[1], 16);
            _sendHeader.DataByte3 = Convert.ToUInt16(hexArray[2], 16);
            _sendHeader.DataByte4 = Convert.ToUInt16(hexArray[3], 16);

            #region sendingHeaderArray...

            byte[] sendingheaderPartArray = new byte[36] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                            (byte)_sendHeader.HeaderPartSize1,
                                                            (byte)_sendHeader.HeaderPartSize2,
                                                            (byte)_sendHeader.DataPartSize1,
                                                            (byte)_sendHeader.DataPartSize2,

                                                            (byte)_sendHeader.ReserveOne,
                                                            (byte)_sendHeader.ProcessingDivision,
                                                            (byte)_sendHeader.ACK,
                                                            (byte)_sendHeader.RequestID,

                                                            (byte)_sendHeader.BlockNumber1,
                                                            (byte)_sendHeader.BlockNumber2,
                                                            (byte)_sendHeader.BlockNumber3,
                                                            (byte)_sendHeader.BlockNumber4,

                                                            (byte)_sendHeader.ReserveTwo1,
                                                            (byte)_sendHeader.ReserveTwo2,
                                                            (byte)_sendHeader.ReserveTwo3,
                                                            (byte)_sendHeader.ReserveTwo4,

                                                            (byte)_sendHeader.ReserveTwo5,
                                                            (byte)_sendHeader.ReserveTwo6,
                                                            (byte)_sendHeader.ReserveTwo7,
                                                            (byte)_sendHeader.ReserveTwo8,

                                                            (byte)_sendHeader.CommandNumber1,
                                                            (byte)_sendHeader.CommandNumber2,
                                                            (byte)_sendHeader.Instance1,
                                                            (byte)_sendHeader.Instance2,

                                                            (byte)_sendHeader.Attribute,
                                                            (byte)_sendHeader.Service,
                                                            (byte)_sendHeader.Padding1,
                                                            (byte)_sendHeader.Padding2,

                                                            (byte)_sendHeader.DataByte1,
                                                            (byte)_sendHeader.DataByte2,
                                                            (byte)_sendHeader.DataByte3,
                                                            (byte)_sendHeader.DataByte4 };
            #endregion

            for (int j = 0; j < 36; j++)
            {
                //Header Part Filling.
                sendPackage[j] = sendingheaderPartArray[j];
            }

            try
            {
                int bytesTransferred = client.Send(sendPackage);
                int bytesReceived = client.Receive(receivedPackage);
            }
            catch (Exception)
            {
                Console.WriteLine("R_Write komutu paket gönderim hatası");
            }

            fillReceivedDataHeader(_receiveHeader, receivedPackage);

            if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
            {
                client.Close();

                Console.WriteLine("R_Write komutu iletişim hatası");
            }
            else
            {
                client.Close();
            }
        }
        public void R_Write(int VariableNumber, int Count, float[] Data)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();

            MyData myData = new MyData(500);

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            byte[] sendPackage = new byte[36 + (Count * 4)];
            byte[] receivedPackage = new byte[750];


            // Decimal değerini byte dizisine dönüştürelim
            byte[] bytes2 = Decimal.GetBits((Count + 1) * 4).SelectMany(BitConverter.GetBytes).ToArray();

            // Byte dizisini hexadecimal olarak çevirelim
            string hexValue2 = BitConverter.ToString(bytes2).Replace("-", "");

            // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
            string[] hexArray2 = new string[hexValue2.Length / 2];
            for (int i = 0; i < hexArray2.Length; i++)
            {
                hexArray2[i] = hexValue2.Substring(i * 2, 2);
            }

            // Convert hex string to UInt16
            _sendHeader.DataPartSize1 = Convert.ToUInt16(hexArray2[0], 16);
            _sendHeader.DataPartSize2 = Convert.ToUInt16(hexArray2[1], 16);
            _sendHeader.RequestID = 0x01;
            _sendHeader.CommandNumber1 = 0x05;
            _sendHeader.CommandNumber2 = 0x03;
            _sendHeader.Instance1 = Convert.ToUInt16(VariableNumber);
            _sendHeader.Attribute = 0;
            _sendHeader.Service = 0x34;

            #region KAÇ ADET VERİYE YAZILACAK

            // Decimal değerini byte dizisine dönüştürelim
            byte[] bytes = Decimal.GetBits(Count).SelectMany(BitConverter.GetBytes).ToArray();

            // Byte dizisini hexadecimal olarak çevirelim
            string hexValue = BitConverter.ToString(bytes).Replace("-", "");

            // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
            string[] hexArray = new string[hexValue.Length / 2];
            for (int i = 0; i < hexArray.Length; i++)
            {
                hexArray[i] = hexValue.Substring(i * 2, 2);
            }

            // Convert hex string to UInt16
            _sendHeader.DataByte1 = Convert.ToUInt16(hexArray[0], 16);
            _sendHeader.DataByte2 = Convert.ToUInt16(hexArray[1], 16);
            _sendHeader.DataByte3 = Convert.ToUInt16(hexArray[2], 16);
            _sendHeader.DataByte4 = Convert.ToUInt16(hexArray[3], 16);

            #endregion

            for (int a = 0; a < Data.Length; a++)
            {
                // Float değerini byte dizisine dönüştürelim
                byte[] bytes1 = BitConverter.GetBytes(Data[a]);

                // Byte dizisini hexadecimal olarak çevirelim
                string hexValue1 = BitConverter.ToString(bytes1).Replace("-", "");

                // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
                string[] hexArray1 = new string[hexValue1.Length / 2];
                for (int i = 0; i < hexArray1.Length; i++)
                {
                    hexArray1[i] = hexValue1.Substring(i * 2, 2);
                }

                for (int i = 0; i < 4; i++)
                {
                    myData[(a * 4) + i] = Convert.ToUInt16(hexArray1[i], 16);
                }
            }

            #region sendingHeaderArray...

            byte[] sendingheaderPartArray = new byte[36 + (Data.Length * 4)];

            sendingheaderPartArray[0] = Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0];
            sendingheaderPartArray[1] = Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0];
            sendingheaderPartArray[2] = Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0];
            sendingheaderPartArray[3] = Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0];

            sendingheaderPartArray[4] = (byte)_sendHeader.HeaderPartSize1;
            sendingheaderPartArray[5] = (byte)_sendHeader.HeaderPartSize2;
            sendingheaderPartArray[6] = (byte)_sendHeader.DataPartSize1;
            sendingheaderPartArray[7] = (byte)_sendHeader.DataPartSize2;

            sendingheaderPartArray[8] = (byte)_sendHeader.ReserveOne;
            sendingheaderPartArray[9] = (byte)_sendHeader.ProcessingDivision;
            sendingheaderPartArray[10] = (byte)_sendHeader.ACK;
            sendingheaderPartArray[11] = (byte)_sendHeader.RequestID;

            sendingheaderPartArray[12] = (byte)_sendHeader.BlockNumber1;
            sendingheaderPartArray[13] = (byte)_sendHeader.BlockNumber2;
            sendingheaderPartArray[14] = (byte)_sendHeader.BlockNumber3;
            sendingheaderPartArray[15] = (byte)_sendHeader.BlockNumber4;

            sendingheaderPartArray[16] = (byte)_sendHeader.ReserveTwo1;
            sendingheaderPartArray[17] = (byte)_sendHeader.ReserveTwo2;
            sendingheaderPartArray[18] = (byte)_sendHeader.ReserveTwo3;
            sendingheaderPartArray[19] = (byte)_sendHeader.ReserveTwo4;

            sendingheaderPartArray[20] = (byte)_sendHeader.ReserveTwo5;
            sendingheaderPartArray[21] = (byte)_sendHeader.ReserveTwo6;
            sendingheaderPartArray[22] = (byte)_sendHeader.ReserveTwo7;
            sendingheaderPartArray[23] = (byte)_sendHeader.ReserveTwo8;

            sendingheaderPartArray[24] = (byte)_sendHeader.CommandNumber1;
            sendingheaderPartArray[25] = (byte)_sendHeader.CommandNumber2;
            sendingheaderPartArray[26] = (byte)_sendHeader.Instance1;
            sendingheaderPartArray[27] = (byte)_sendHeader.Instance2;

            sendingheaderPartArray[28] = (byte)_sendHeader.Attribute;
            sendingheaderPartArray[29] = (byte)_sendHeader.Service;
            sendingheaderPartArray[30] = (byte)_sendHeader.Padding1;
            sendingheaderPartArray[31] = (byte)_sendHeader.Padding2;

            sendingheaderPartArray[32] = (byte)_sendHeader.DataByte1;
            sendingheaderPartArray[33] = (byte)_sendHeader.DataByte2;
            sendingheaderPartArray[34] = (byte)_sendHeader.DataByte3;
            sendingheaderPartArray[35] = (byte)_sendHeader.DataByte4;

            #endregion


            for (int i = 0; i < (Data.Length * 4); i++)
            {
                sendingheaderPartArray[36 + i] = (byte)myData[i];
            }


            for (int j = 0; j < (36 + Count * 4); j++)
            {
                //Header Part Filling.
                sendPackage[j] = sendingheaderPartArray[j];
            }

            try
            {
                int bytesTransferred = client.Send(sendPackage);
                int bytesReceived = client.Receive(receivedPackage);
            }
            catch (Exception)
            {
                Console.WriteLine("R_Write komutu paket gönderim hatası");
            }

            fillReceivedDataHeader(_receiveHeader, receivedPackage);

            if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
            {
                client.Close();

                Console.WriteLine("R_Write komutu iletişim hatası");
            }
            else
            {
                client.Close();
            }
        }

        public void S_Read(int VariableNumber, out string Data)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            byte[] sendPackage = new byte[32];
            byte[] receivedPackage = new byte[750];

            _sendHeader.DataPartSize1 = 0x00;
            _sendHeader.DataPartSize2 = 0x00;
            _sendHeader.RequestID = 0x00;
            _sendHeader.CommandNumber1 = 0x7E;
            _sendHeader.CommandNumber2 = 0x00;
            _sendHeader.Instance1 = Convert.ToUInt16(VariableNumber);
            _sendHeader.Attribute = 1;
            _sendHeader.Service = 0x0E;

            #region sendingHeaderArray...

            byte[] sendingheaderPartArray = new byte[32] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                            (byte)_sendHeader.HeaderPartSize1,
                                                            (byte)_sendHeader.HeaderPartSize2,
                                                            (byte)_sendHeader.DataPartSize1,
                                                            (byte)_sendHeader.DataPartSize2,

                                                            (byte)_sendHeader.ReserveOne,
                                                            (byte)_sendHeader.ProcessingDivision,
                                                            (byte)_sendHeader.ACK,
                                                            (byte)_sendHeader.RequestID,

                                                            (byte)_sendHeader.BlockNumber1,
                                                            (byte)_sendHeader.BlockNumber2,
                                                            (byte)_sendHeader.BlockNumber3,
                                                            (byte)_sendHeader.BlockNumber4,

                                                            (byte)_sendHeader.ReserveTwo1,
                                                            (byte)_sendHeader.ReserveTwo2,
                                                            (byte)_sendHeader.ReserveTwo3,
                                                            (byte)_sendHeader.ReserveTwo4,

                                                            (byte)_sendHeader.ReserveTwo5,
                                                            (byte)_sendHeader.ReserveTwo6,
                                                            (byte)_sendHeader.ReserveTwo7,
                                                            (byte)_sendHeader.ReserveTwo8,

                                                            (byte)_sendHeader.CommandNumber1,
                                                            (byte)_sendHeader.CommandNumber2,
                                                            (byte)_sendHeader.Instance1,
                                                            (byte)_sendHeader.Instance2,

                                                            (byte)_sendHeader.Attribute,
                                                            (byte)_sendHeader.Service,
                                                            (byte)_sendHeader.Padding1,
                                                            (byte)_sendHeader.Padding2 };
            #endregion

            for (int j = 0; j < 32; j++)
            {
                //Header Part Filling.
                sendPackage[j] = sendingheaderPartArray[j];
            }

            try
            {
                int bytesTransferred = client.Send(sendPackage);
                int bytesReceived = client.Receive(receivedPackage);
            }
            catch (Exception)
            {
                Console.WriteLine("S_Read komutu paket gönderim hatası");
            }

            fillReceivedDataHeader(_receiveHeader, receivedPackage);

            if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
            {
                client.Close();

                Data = " ";
                Console.WriteLine("S_Read komutu iletişim hatası");
            }
            else
            {
                string hexifade = "";

                for (int j = 32; j < 48; j++)
                {
                    hexifade = hexifade + receivedPackage[j].ToString("X2");
                }

                //string hexString = hexifade; // Hexadecimal string
                //byte[] bytes = new byte[hexString.Length / 2];

                //for (int i = 0; i < hexString.Length; i += 2)
                //{
                //    bytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
                //}

                // Başlangıçtaki ve sondaki sıfırları kırpma


                hexifade = hexifade.TrimStart('0').TrimEnd('0');

                // Kırpılmış hexadecimal dizeyi byte dizisine dönüştürme
                int byteCount = hexifade.Length / 2;
                byte[] bytes = new byte[byteCount];

                for (int i = 0; i < hexifade.Length; i += 2)
                {
                    bytes[i / 2] = Convert.ToByte(hexifade.Substring(i, 2), 16);
                }


                string text = System.Text.Encoding.UTF8.GetString(bytes);

                Data = text;


                client.Close();
            }
        }
        public void S_Read(int VariableNumber, int Count, out string[] Data)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            byte[] sendPackage = new byte[36];
            byte[] receivedPackage = new byte[750];

            _sendHeader.DataPartSize1 = 0x04;
            _sendHeader.DataPartSize2 = 0x00;
            _sendHeader.RequestID = 0x00;
            _sendHeader.CommandNumber1 = 0x06;
            _sendHeader.CommandNumber2 = 0x03;
            _sendHeader.Instance1 = Convert.ToUInt16(VariableNumber);
            _sendHeader.Attribute = 0;
            _sendHeader.Service = 0x33;


            // Decimal değerini byte dizisine dönüştürelim
            byte[] bytes = Decimal.GetBits(Count).SelectMany(BitConverter.GetBytes).ToArray();

            // Byte dizisini hexadecimal olarak çevirelim
            string hexValue = BitConverter.ToString(bytes).Replace("-", "");

            // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
            string[] hexArray = new string[hexValue.Length / 2];
            for (int i = 0; i < hexArray.Length; i++)
            {
                hexArray[i] = hexValue.Substring(i * 2, 2);
            }

            // Convert hex string to UInt16
            _sendHeader.DataByte1 = Convert.ToUInt16(hexArray[0], 16);
            _sendHeader.DataByte2 = Convert.ToUInt16(hexArray[1], 16);
            _sendHeader.DataByte3 = Convert.ToUInt16(hexArray[2], 16);
            _sendHeader.DataByte4 = Convert.ToUInt16(hexArray[3], 16);

            #region sendingHeaderArray...

            byte[] sendingheaderPartArray = new byte[36] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                            (byte)_sendHeader.HeaderPartSize1,
                                                            (byte)_sendHeader.HeaderPartSize2,
                                                            (byte)_sendHeader.DataPartSize1,
                                                            (byte)_sendHeader.DataPartSize2,

                                                            (byte)_sendHeader.ReserveOne,
                                                            (byte)_sendHeader.ProcessingDivision,
                                                            (byte)_sendHeader.ACK,
                                                            (byte)_sendHeader.RequestID,

                                                            (byte)_sendHeader.BlockNumber1,
                                                            (byte)_sendHeader.BlockNumber2,
                                                            (byte)_sendHeader.BlockNumber3,
                                                            (byte)_sendHeader.BlockNumber4,

                                                            (byte)_sendHeader.ReserveTwo1,
                                                            (byte)_sendHeader.ReserveTwo2,
                                                            (byte)_sendHeader.ReserveTwo3,
                                                            (byte)_sendHeader.ReserveTwo4,

                                                            (byte)_sendHeader.ReserveTwo5,
                                                            (byte)_sendHeader.ReserveTwo6,
                                                            (byte)_sendHeader.ReserveTwo7,
                                                            (byte)_sendHeader.ReserveTwo8,

                                                            (byte)_sendHeader.CommandNumber1,
                                                            (byte)_sendHeader.CommandNumber2,
                                                            (byte)_sendHeader.Instance1,
                                                            (byte)_sendHeader.Instance2,

                                                            (byte)_sendHeader.Attribute,
                                                            (byte)_sendHeader.Service,
                                                            (byte)_sendHeader.Padding1,
                                                            (byte)_sendHeader.Padding2,

                                                            (byte)_sendHeader.DataByte1,
                                                            (byte)_sendHeader.DataByte2,
                                                            (byte)_sendHeader.DataByte3,
                                                            (byte)_sendHeader.DataByte4 };
            #endregion


            for (int j = 0; j < 36; j++)
            {
                //Header Part Filling.
                sendPackage[j] = sendingheaderPartArray[j];
            }

            try
            {
                int bytesTransferred = client.Send(sendPackage);
                int bytesReceived = client.Receive(receivedPackage);
            }
            catch (Exception)
            {
                Console.WriteLine("S_Read komutu paket gönderim hatası");
            }

            fillReceivedDataHeader(_receiveHeader, receivedPackage);

            if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
            {
                client.Close();
                Console.WriteLine("S_Read komutu iletişim hatası");

                string[] a = { " " };

                Data = a;
            }
            else
            {
                string[] Cevap = new string[Count];
                int Baslangic = 36;
                int Bitis = 52;

                for (int t = 0; t < Count; t++)
                {
                    string hexifade = "";

                    for (int j = Baslangic; j < Bitis; j++)
                    {
                        hexifade = hexifade + receivedPackage[j].ToString("X2");
                    }

                    // string hexString = hexifade; // Hexadecimal string
                    //byte[] bytes1 = new byte[hexString.Length / 2];

                    //for (int i = 0; i < hexString.Length; i += 2)
                    //{
                    //    bytes1[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
                    //}


                    hexifade = hexifade.TrimStart('0').TrimEnd('0');

                    // Kırpılmış hexadecimal dizeyi byte dizisine dönüştürme
                    int byteCount = hexifade.Length / 2;
                    byte[] bytes1 = new byte[byteCount];

                    for (int i = 0; i < hexifade.Length; i += 2)
                    {
                        bytes1[i / 2] = Convert.ToByte(hexifade.Substring(i, 2), 16);
                    }

                    string text1 = System.Text.Encoding.UTF8.GetString(bytes1);

                    Cevap[t] = text1;

                    Baslangic = Baslangic + 16;
                    Bitis = Bitis + 16;
                }

                Data = Cevap;
            }

            client.Close();
        }
        public void S_Write(int VariableNumber, string Data)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            byte[] sendPackage = new byte[48];
            byte[] receivedPackage = new byte[750];

            _sendHeader.DataPartSize1 = 0x10;
            _sendHeader.DataPartSize2 = 0x00;
            _sendHeader.RequestID = 0x01;
            _sendHeader.CommandNumber1 = 0x7E;
            _sendHeader.CommandNumber2 = 0x00;
            _sendHeader.Instance1 = Convert.ToUInt16(VariableNumber);
            _sendHeader.Attribute = 1;
            _sendHeader.Service = 0x02;


            char[] harfDizisi = new char[16]; // 16 elemanlı bir karakter dizisi oluşturuldu

            // String ifadeyi harflere bölmek için bir döngü kullanıyoruz
            for (int i = 0; i < Data.Length; i++)
            {
                // Her bir harfi diziye ekliyoruz
                harfDizisi[i] = Data[i];
            }

            // Geri kalan elemanlar '\0' karakteriyle dolduruldu
            for (int i = Data.Length; i < harfDizisi.Length; i++)
            {
                harfDizisi[i] = '\0';
            }

            _sendHeader.StringYazma1 = harfDizisi[0].ToString();
            _sendHeader.StringYazma2 = harfDizisi[1].ToString();
            _sendHeader.StringYazma3 = harfDizisi[2].ToString();
            _sendHeader.StringYazma4 = harfDizisi[3].ToString();

            _sendHeader.StringYazma5 = harfDizisi[4].ToString();
            _sendHeader.StringYazma6 = harfDizisi[5].ToString();
            _sendHeader.StringYazma7 = harfDizisi[6].ToString();
            _sendHeader.StringYazma8 = harfDizisi[7].ToString();

            _sendHeader.StringYazma9 = harfDizisi[8].ToString();
            _sendHeader.StringYazma10 = harfDizisi[9].ToString();
            _sendHeader.StringYazma11 = harfDizisi[10].ToString();
            _sendHeader.StringYazma12 = harfDizisi[11].ToString();

            _sendHeader.StringYazma13 = harfDizisi[12].ToString();
            _sendHeader.StringYazma14 = harfDizisi[13].ToString();
            _sendHeader.StringYazma15 = harfDizisi[14].ToString();
            _sendHeader.StringYazma16 = harfDizisi[15].ToString();


            #region sendingHeaderArray...

            byte[] sendingheaderPartArray = new byte[48] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                            (byte)_sendHeader.HeaderPartSize1,
                                                            (byte)_sendHeader.HeaderPartSize2,
                                                            (byte)_sendHeader.DataPartSize1,
                                                            (byte)_sendHeader.DataPartSize2,

                                                            (byte)_sendHeader.ReserveOne,
                                                            (byte)_sendHeader.ProcessingDivision,
                                                            (byte)_sendHeader.ACK,
                                                            (byte)_sendHeader.RequestID,

                                                            (byte)_sendHeader.BlockNumber1,
                                                            (byte)_sendHeader.BlockNumber2,
                                                            (byte)_sendHeader.BlockNumber3,
                                                            (byte)_sendHeader.BlockNumber4,

                                                            (byte)_sendHeader.ReserveTwo1,
                                                            (byte)_sendHeader.ReserveTwo2,
                                                            (byte)_sendHeader.ReserveTwo3,
                                                            (byte)_sendHeader.ReserveTwo4,

                                                            (byte)_sendHeader.ReserveTwo5,
                                                            (byte)_sendHeader.ReserveTwo6,
                                                            (byte)_sendHeader.ReserveTwo7,
                                                            (byte)_sendHeader.ReserveTwo8,

                                                            (byte)_sendHeader.CommandNumber1,
                                                            (byte)_sendHeader.CommandNumber2,
                                                            (byte)_sendHeader.Instance1,
                                                            (byte)_sendHeader.Instance2,

                                                            (byte)_sendHeader.Attribute,
                                                            (byte)_sendHeader.Service,
                                                            (byte)_sendHeader.Padding1,
                                                            (byte)_sendHeader.Padding2,

                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma1)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma2)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma3)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma4)[0],

                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma5)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma6)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma7)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma8)[0],

                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma9)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma10)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma11)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma12)[0],

                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma13)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma14)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma15)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma16)[0]

            };
            #endregion

            for (int j = 0; j < 48; j++)
            {
                //Header Part Filling.
                sendPackage[j] = sendingheaderPartArray[j];
            }

            try
            {
                int bytesTransferred = client.Send(sendPackage);
                int bytesReceived = client.Receive(receivedPackage);
            }
            catch (Exception)
            {
                Console.WriteLine("S_Write komutu paket gönderim hatası");
            }

            fillReceivedDataHeader(_receiveHeader, receivedPackage);

            if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
            {
                client.Close();

                Console.WriteLine("S_Write komutu iletişim hatası");
            }
            else
            {
                client.Close();
            }
        }
        public void S_Write(int VariableNumber, int Count, string[] Data)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();

            MyString myString = new MyString(500);

            for (int x = 0; x < Data.Length; x++)
            {
                if (Data[x].Length > 16)
                {
                    Console.WriteLine("16 Karakterden fazla girdi");
                    return;
                }
            }

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            byte[] sendPackage = new byte[36 + (Count * 16)];
            byte[] receivedPackage = new byte[750];


            // Decimal değerini byte dizisine dönüştürelim
            byte[] bytes2 = Decimal.GetBits((Count * 16) + 4).SelectMany(BitConverter.GetBytes).ToArray();

            // Byte dizisini hexadecimal olarak çevirelim
            string hexValue2 = BitConverter.ToString(bytes2).Replace("-", "");

            // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
            string[] hexArray2 = new string[hexValue2.Length / 2];
            for (int i = 0; i < hexArray2.Length; i++)
            {
                hexArray2[i] = hexValue2.Substring(i * 2, 2);
            }

            // Convert hex string to UInt16
            _sendHeader.DataPartSize1 = Convert.ToUInt16(hexArray2[0], 16);
            _sendHeader.DataPartSize2 = Convert.ToUInt16(hexArray2[1], 16);
            _sendHeader.RequestID = 0x01;
            _sendHeader.CommandNumber1 = 0x06;
            _sendHeader.CommandNumber2 = 0x03;
            _sendHeader.Instance1 = Convert.ToUInt16(VariableNumber);
            _sendHeader.Attribute = 0;
            _sendHeader.Service = 0x34;

            #region KAÇ ADET VERİYE YAZILACAK

            // Decimal değerini byte dizisine dönüştürelim
            byte[] bytes = Decimal.GetBits(Count).SelectMany(BitConverter.GetBytes).ToArray();

            // Byte dizisini hexadecimal olarak çevirelim
            string hexValue = BitConverter.ToString(bytes).Replace("-", "");

            // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
            string[] hexArray = new string[hexValue.Length / 2];
            for (int i = 0; i < hexArray.Length; i++)
            {
                hexArray[i] = hexValue.Substring(i * 2, 2);
            }

            // Convert hex string to UInt16
            _sendHeader.DataByte1 = Convert.ToUInt16(hexArray[0], 16);
            _sendHeader.DataByte2 = Convert.ToUInt16(hexArray[1], 16);
            _sendHeader.DataByte3 = Convert.ToUInt16(hexArray[2], 16);
            _sendHeader.DataByte4 = Convert.ToUInt16(hexArray[3], 16);

            #endregion

            char[] harfDizisi = new char[16]; // 16 elemanlı bir karakter dizisi oluşturuldu

            for (int a = 0; a < Data.Length; a++)
            {
                // String ifadeyi harflere bölmek için bir döngü kullanıyoruz
                for (int i = 0; i < Data[a].Length; i++)
                {
                    // Her bir harfi diziye ekliyoruz
                    harfDizisi[i] = Data[a][i];
                }

                // Geri kalan elemanlar '\0' karakteriyle dolduruldu
                for (int i = Data[a].Length; i < harfDizisi.Length; i++)
                {
                    harfDizisi[i] = '\0';
                }

                for (int i = 0; i < 16; i++)
                {
                    myString[(a * 16) + i] = harfDizisi[i].ToString();
                }
            }

            #region sendingHeaderArray...

            byte[] sendingheaderPartArray = new byte[36 + (Data.Length * 16)];

            sendingheaderPartArray[0] = Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0];
            sendingheaderPartArray[1] = Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0];
            sendingheaderPartArray[2] = Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0];
            sendingheaderPartArray[3] = Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0];

            sendingheaderPartArray[4] = (byte)_sendHeader.HeaderPartSize1;
            sendingheaderPartArray[5] = (byte)_sendHeader.HeaderPartSize2;
            sendingheaderPartArray[6] = (byte)_sendHeader.DataPartSize1;
            sendingheaderPartArray[7] = (byte)_sendHeader.DataPartSize2;

            sendingheaderPartArray[8] = (byte)_sendHeader.ReserveOne;
            sendingheaderPartArray[9] = (byte)_sendHeader.ProcessingDivision;
            sendingheaderPartArray[10] = (byte)_sendHeader.ACK;
            sendingheaderPartArray[11] = (byte)_sendHeader.RequestID;

            sendingheaderPartArray[12] = (byte)_sendHeader.BlockNumber1;
            sendingheaderPartArray[13] = (byte)_sendHeader.BlockNumber2;
            sendingheaderPartArray[14] = (byte)_sendHeader.BlockNumber3;
            sendingheaderPartArray[15] = (byte)_sendHeader.BlockNumber4;

            sendingheaderPartArray[16] = (byte)_sendHeader.ReserveTwo1;
            sendingheaderPartArray[17] = (byte)_sendHeader.ReserveTwo2;
            sendingheaderPartArray[18] = (byte)_sendHeader.ReserveTwo3;
            sendingheaderPartArray[19] = (byte)_sendHeader.ReserveTwo4;

            sendingheaderPartArray[20] = (byte)_sendHeader.ReserveTwo5;
            sendingheaderPartArray[21] = (byte)_sendHeader.ReserveTwo6;
            sendingheaderPartArray[22] = (byte)_sendHeader.ReserveTwo7;
            sendingheaderPartArray[23] = (byte)_sendHeader.ReserveTwo8;

            sendingheaderPartArray[24] = (byte)_sendHeader.CommandNumber1;
            sendingheaderPartArray[25] = (byte)_sendHeader.CommandNumber2;
            sendingheaderPartArray[26] = (byte)_sendHeader.Instance1;
            sendingheaderPartArray[27] = (byte)_sendHeader.Instance2;

            sendingheaderPartArray[28] = (byte)_sendHeader.Attribute;
            sendingheaderPartArray[29] = (byte)_sendHeader.Service;
            sendingheaderPartArray[30] = (byte)_sendHeader.Padding1;
            sendingheaderPartArray[31] = (byte)_sendHeader.Padding2;

            sendingheaderPartArray[32] = (byte)_sendHeader.DataByte1;
            sendingheaderPartArray[33] = (byte)_sendHeader.DataByte2;
            sendingheaderPartArray[34] = (byte)_sendHeader.DataByte3;
            sendingheaderPartArray[35] = (byte)_sendHeader.DataByte4;

            #endregion


            for (int i = 0; i < (Data.Length * 16); i++)
            {
                sendingheaderPartArray[36 + i] = Encoding.ASCII.GetBytes(myString[i])[0];
            }


            for (int j = 0; j < (36 + Count * 16); j++)
            {
                //Header Part Filling.
                sendPackage[j] = sendingheaderPartArray[j];
            }

            try
            {
                int bytesTransferred = client.Send(sendPackage);
                int bytesReceived = client.Receive(receivedPackage);
            }
            catch (Exception)
            {
                Console.WriteLine("S_Write komutu paket gönderim hatası");
            }

            fillReceivedDataHeader(_receiveHeader, receivedPackage);

            if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
            {
                client.Close();

                Console.WriteLine("S_Write komutu iletişim hatası");
            }
            else
            {
                client.Close();
            }
        }

        public void B_Read(int VariableNumber, out byte Data)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            byte[] sendPackage = new byte[32];
            byte[] receivedPackage = new byte[750];

            _sendHeader.DataPartSize1 = 0x00;
            _sendHeader.DataPartSize2 = 0x00;
            _sendHeader.RequestID = 0x00;
            _sendHeader.CommandNumber1 = 0x7A;
            _sendHeader.CommandNumber2 = 0x00;
            _sendHeader.Instance1 = Convert.ToUInt16(VariableNumber);
            _sendHeader.Attribute = 1;
            _sendHeader.Service = 0x0E;

            #region sendingHeaderArray...

            byte[] sendingheaderPartArray = new byte[32] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                            (byte)_sendHeader.HeaderPartSize1,
                                                            (byte)_sendHeader.HeaderPartSize2,
                                                            (byte)_sendHeader.DataPartSize1,
                                                            (byte)_sendHeader.DataPartSize2,

                                                            (byte)_sendHeader.ReserveOne,
                                                            (byte)_sendHeader.ProcessingDivision,
                                                            (byte)_sendHeader.ACK,
                                                            (byte)_sendHeader.RequestID,

                                                            (byte)_sendHeader.BlockNumber1,
                                                            (byte)_sendHeader.BlockNumber2,
                                                            (byte)_sendHeader.BlockNumber3,
                                                            (byte)_sendHeader.BlockNumber4,

                                                            (byte)_sendHeader.ReserveTwo1,
                                                            (byte)_sendHeader.ReserveTwo2,
                                                            (byte)_sendHeader.ReserveTwo3,
                                                            (byte)_sendHeader.ReserveTwo4,

                                                            (byte)_sendHeader.ReserveTwo5,
                                                            (byte)_sendHeader.ReserveTwo6,
                                                            (byte)_sendHeader.ReserveTwo7,
                                                            (byte)_sendHeader.ReserveTwo8,

                                                            (byte)_sendHeader.CommandNumber1,
                                                            (byte)_sendHeader.CommandNumber2,
                                                            (byte)_sendHeader.Instance1,
                                                            (byte)_sendHeader.Instance2,

                                                            (byte)_sendHeader.Attribute,
                                                            (byte)_sendHeader.Service,
                                                            (byte)_sendHeader.Padding1,
                                                            (byte)_sendHeader.Padding2 };
            #endregion


            for (int j = 0; j < 32; j++)
            {
                //Header Part Filling.
                sendPackage[j] = sendingheaderPartArray[j];
            }

            try
            {
                int bytesTransferred = client.Send(sendPackage);
                int bytesReceived = client.Receive(receivedPackage);
            }
            catch (Exception)
            {
                Console.WriteLine("B_Read komutu paket gönderim hatası");
            }

            fillReceivedDataHeader(_receiveHeader, receivedPackage);

            if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
            {
                client.Close();

                Data = 0;
                Console.WriteLine("B_Read komutu iletişim hatası");
            }
            else
            {
                string HexDegeri = $"{receivedPackage[32].ToString("X2")}";

                int decValue = int.Parse(HexDegeri, System.Globalization.NumberStyles.HexNumber);
                Data = Convert.ToByte(decValue);
                client.Close();
            }
        }
        public void B_Read(int VariableNumber, int Count, out byte?[] Data)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            byte[] sendPackage = new byte[36];
            byte[] receivedPackage = new byte[750];

            _sendHeader.DataPartSize1 = 0x04;
            _sendHeader.DataPartSize2 = 0x00;
            _sendHeader.RequestID = 0x00;
            _sendHeader.CommandNumber1 = 0x02;
            _sendHeader.CommandNumber2 = 0x03;
            _sendHeader.Instance1 = Convert.ToUInt16(VariableNumber);
            _sendHeader.Attribute = 0;
            _sendHeader.Service = 0x33;


            // Decimal değerini byte dizisine dönüştürelim
            byte[] bytes = Decimal.GetBits(Count).SelectMany(BitConverter.GetBytes).ToArray();

            // Byte dizisini hexadecimal olarak çevirelim
            string hexValue = BitConverter.ToString(bytes).Replace("-", "");

            // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
            string[] hexArray = new string[hexValue.Length / 2];
            for (int i = 0; i < hexArray.Length; i++)
            {
                hexArray[i] = hexValue.Substring(i * 2, 2);
            }

            // Convert hex string to UInt16
            _sendHeader.DataByte1 = Convert.ToUInt16(hexArray[0], 16);
            _sendHeader.DataByte2 = Convert.ToUInt16(hexArray[1], 16);
            _sendHeader.DataByte3 = Convert.ToUInt16(hexArray[2], 16);
            _sendHeader.DataByte4 = Convert.ToUInt16(hexArray[3], 16);

            #region sendingHeaderArray...

            byte[] sendingheaderPartArray = new byte[36] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                            (byte)_sendHeader.HeaderPartSize1,
                                                            (byte)_sendHeader.HeaderPartSize2,
                                                            (byte)_sendHeader.DataPartSize1,
                                                            (byte)_sendHeader.DataPartSize2,

                                                            (byte)_sendHeader.ReserveOne,
                                                            (byte)_sendHeader.ProcessingDivision,
                                                            (byte)_sendHeader.ACK,
                                                            (byte)_sendHeader.RequestID,

                                                            (byte)_sendHeader.BlockNumber1,
                                                            (byte)_sendHeader.BlockNumber2,
                                                            (byte)_sendHeader.BlockNumber3,
                                                            (byte)_sendHeader.BlockNumber4,

                                                            (byte)_sendHeader.ReserveTwo1,
                                                            (byte)_sendHeader.ReserveTwo2,
                                                            (byte)_sendHeader.ReserveTwo3,
                                                            (byte)_sendHeader.ReserveTwo4,

                                                            (byte)_sendHeader.ReserveTwo5,
                                                            (byte)_sendHeader.ReserveTwo6,
                                                            (byte)_sendHeader.ReserveTwo7,
                                                            (byte)_sendHeader.ReserveTwo8,

                                                            (byte)_sendHeader.CommandNumber1,
                                                            (byte)_sendHeader.CommandNumber2,
                                                            (byte)_sendHeader.Instance1,
                                                            (byte)_sendHeader.Instance2,

                                                            (byte)_sendHeader.Attribute,
                                                            (byte)_sendHeader.Service,
                                                            (byte)_sendHeader.Padding1,
                                                            (byte)_sendHeader.Padding2,

                                                            (byte)_sendHeader.DataByte1,
                                                            (byte)_sendHeader.DataByte2,
                                                            (byte)_sendHeader.DataByte3,
                                                            (byte)_sendHeader.DataByte4 };
            #endregion


            for (int j = 0; j < 36; j++)
            {
                //Header Part Filling.
                sendPackage[j] = sendingheaderPartArray[j];
            }

            try
            {
                int bytesTransferred = client.Send(sendPackage);
                int bytesReceived = client.Receive(receivedPackage);
            }
            catch (Exception)
            {
                Console.WriteLine("B_Read komutu paket gönderim hatası");
            }

            fillReceivedDataHeader(_receiveHeader, receivedPackage);

            if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
            {
                client.Close();
                Console.WriteLine("B_Read komutu iletişim hatası. Lütfen Count parametresine çift sayı giriniz.");

                byte?[] a = { 0 };

                Data = a;
            }
            else
            {
                byte sayi1;

                byte?[] Cevap = new byte?[Count];

                for (int i = 0; i < Count; i++)
                {
                    string HexDegeri = $"{receivedPackage[36 + i].ToString("X2")}";

                    int decValue = int.Parse(HexDegeri, System.Globalization.NumberStyles.HexNumber);

                    sayi1 = Convert.ToByte(decValue);

                    Cevap[i] = sayi1;
                }

                Data = Cevap;
            }

            client.Close();
        }
        public void B_Write(int VariableNumber, byte Data)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            byte[] sendPackage = new byte[36];
            byte[] receivedPackage = new byte[750];

            _sendHeader.DataPartSize1 = 0x04;
            _sendHeader.DataPartSize2 = 0x00;
            _sendHeader.RequestID = 0x01;
            _sendHeader.CommandNumber1 = 0x7A;
            _sendHeader.CommandNumber2 = 0x00;
            _sendHeader.Instance1 = Convert.ToUInt16(VariableNumber);
            _sendHeader.Attribute = 1;
            _sendHeader.Service = 0x02;


            // Decimal değerini byte dizisine dönüştürelim
            byte[] bytes = Decimal.GetBits(Data).SelectMany(BitConverter.GetBytes).ToArray();

            // Byte dizisini hexadecimal olarak çevirelim
            string hexValue = BitConverter.ToString(bytes).Replace("-", "");

            // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
            string[] hexArray = new string[hexValue.Length / 2];
            for (int i = 0; i < hexArray.Length; i++)
            {
                hexArray[i] = hexValue.Substring(i * 2, 2);
            }

            // Convert hex string to UInt16
            _sendHeader.DataByte1 = Convert.ToUInt16(hexArray[0], 16);

            #region sendingHeaderArray...

            byte[] sendingheaderPartArray = new byte[36] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                            (byte)_sendHeader.HeaderPartSize1,
                                                            (byte)_sendHeader.HeaderPartSize2,
                                                            (byte)_sendHeader.DataPartSize1,
                                                            (byte)_sendHeader.DataPartSize2,

                                                            (byte)_sendHeader.ReserveOne,
                                                            (byte)_sendHeader.ProcessingDivision,
                                                            (byte)_sendHeader.ACK,
                                                            (byte)_sendHeader.RequestID,

                                                            (byte)_sendHeader.BlockNumber1,
                                                            (byte)_sendHeader.BlockNumber2,
                                                            (byte)_sendHeader.BlockNumber3,
                                                            (byte)_sendHeader.BlockNumber4,

                                                            (byte)_sendHeader.ReserveTwo1,
                                                            (byte)_sendHeader.ReserveTwo2,
                                                            (byte)_sendHeader.ReserveTwo3,
                                                            (byte)_sendHeader.ReserveTwo4,

                                                            (byte)_sendHeader.ReserveTwo5,
                                                            (byte)_sendHeader.ReserveTwo6,
                                                            (byte)_sendHeader.ReserveTwo7,
                                                            (byte)_sendHeader.ReserveTwo8,

                                                            (byte)_sendHeader.CommandNumber1,
                                                            (byte)_sendHeader.CommandNumber2,
                                                            (byte)_sendHeader.Instance1,
                                                            (byte)_sendHeader.Instance2,

                                                            (byte)_sendHeader.Attribute,
                                                            (byte)_sendHeader.Service,
                                                            (byte)_sendHeader.Padding1,
                                                            (byte)_sendHeader.Padding2,

                                                            (byte)_sendHeader.DataByte1,
                                                            (byte)_sendHeader.DataByte2,
                                                            (byte)_sendHeader.DataByte3,
                                                            (byte)_sendHeader.DataByte4 };
            #endregion


            for (int j = 0; j < 36; j++)
            {
                //Header Part Filling.
                sendPackage[j] = sendingheaderPartArray[j];
            }

            try
            {
                int bytesTransferred = client.Send(sendPackage);
                int bytesReceived = client.Receive(receivedPackage);
            }
            catch (Exception)
            {
                Console.WriteLine("B_Write komutu paket gönderim hatası");
            }

            fillReceivedDataHeader(_receiveHeader, receivedPackage);

            if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
            {
                client.Close();

                Console.WriteLine("B_Write komutu iletişim hatası");
            }
            else
            {
                client.Close();
            }
        }
        public void B_Write(int VariableNumber, int Count, byte[] Data)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();

            MyData myData = new MyData(500);

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            byte[] sendPackage = new byte[36 + Count];
            byte[] receivedPackage = new byte[750];


            // Decimal değerini byte dizisine dönüştürelim
            byte[] bytes2 = Decimal.GetBits(Count + 4).SelectMany(BitConverter.GetBytes).ToArray();

            // Byte dizisini hexadecimal olarak çevirelim
            string hexValue2 = BitConverter.ToString(bytes2).Replace("-", "");

            // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
            string[] hexArray2 = new string[hexValue2.Length / 2];
            for (int i = 0; i < hexArray2.Length; i++)
            {
                hexArray2[i] = hexValue2.Substring(i * 2, 2);
            }

            // Convert hex string to UInt16
            _sendHeader.DataPartSize1 = Convert.ToUInt16(hexArray2[0], 16);
            _sendHeader.DataPartSize2 = Convert.ToUInt16(hexArray2[1], 16);
            _sendHeader.RequestID = 0x01;
            _sendHeader.CommandNumber1 = 0x02;
            _sendHeader.CommandNumber2 = 0x03;
            _sendHeader.Instance1 = Convert.ToUInt16(VariableNumber);
            _sendHeader.Attribute = 0;
            _sendHeader.Service = 0x34;


            #region KAÇ ADET VERİYE YAZILACAK

            // Decimal değerini byte dizisine dönüştürelim
            byte[] bytes = Decimal.GetBits(Count).SelectMany(BitConverter.GetBytes).ToArray();

            // Byte dizisini hexadecimal olarak çevirelim
            string hexValue = BitConverter.ToString(bytes).Replace("-", "");

            // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
            string[] hexArray = new string[hexValue.Length / 2];
            for (int i = 0; i < hexArray.Length; i++)
            {
                hexArray[i] = hexValue.Substring(i * 2, 2);
            }

            // Convert hex string to UInt16
            _sendHeader.DataByte1 = Convert.ToUInt16(hexArray[0], 16);
            _sendHeader.DataByte2 = Convert.ToUInt16(hexArray[1], 16);
            _sendHeader.DataByte3 = Convert.ToUInt16(hexArray[2], 16);
            _sendHeader.DataByte4 = Convert.ToUInt16(hexArray[3], 16);

            #endregion


            for (int a = 0; a < Data.Length; a++)
            {
                // Decimal değerini byte dizisine dönüştürelim
                byte[] bytes1 = Decimal.GetBits(Data[a]).SelectMany(BitConverter.GetBytes).ToArray();

                // Byte dizisini hexadecimal olarak çevirelim
                string hexValue1 = BitConverter.ToString(bytes1).Replace("-", "");

                // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
                string[] hexArray1 = new string[hexValue1.Length / 2];

                for (int i = 0; i < hexArray1.Length; i++)
                {
                    hexArray1[i] = hexValue1.Substring(i * 2, 2);
                }

                for (int i = 0; i < 1; i++)
                {
                    myData[a + i] = Convert.ToUInt16(hexArray1[i], 16);
                }
            }

            #region sendingHeaderArray...

            byte[] sendingheaderPartArray = new byte[36 + Data.Length];

            sendingheaderPartArray[0] = Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0];
            sendingheaderPartArray[1] = Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0];
            sendingheaderPartArray[2] = Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0];
            sendingheaderPartArray[3] = Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0];

            sendingheaderPartArray[4] = (byte)_sendHeader.HeaderPartSize1;
            sendingheaderPartArray[5] = (byte)_sendHeader.HeaderPartSize2;
            sendingheaderPartArray[6] = (byte)_sendHeader.DataPartSize1;
            sendingheaderPartArray[7] = (byte)_sendHeader.DataPartSize2;

            sendingheaderPartArray[8] = (byte)_sendHeader.ReserveOne;
            sendingheaderPartArray[9] = (byte)_sendHeader.ProcessingDivision;
            sendingheaderPartArray[10] = (byte)_sendHeader.ACK;
            sendingheaderPartArray[11] = (byte)_sendHeader.RequestID;

            sendingheaderPartArray[12] = (byte)_sendHeader.BlockNumber1;
            sendingheaderPartArray[13] = (byte)_sendHeader.BlockNumber2;
            sendingheaderPartArray[14] = (byte)_sendHeader.BlockNumber3;
            sendingheaderPartArray[15] = (byte)_sendHeader.BlockNumber4;

            sendingheaderPartArray[16] = (byte)_sendHeader.ReserveTwo1;
            sendingheaderPartArray[17] = (byte)_sendHeader.ReserveTwo2;
            sendingheaderPartArray[18] = (byte)_sendHeader.ReserveTwo3;
            sendingheaderPartArray[19] = (byte)_sendHeader.ReserveTwo4;

            sendingheaderPartArray[20] = (byte)_sendHeader.ReserveTwo5;
            sendingheaderPartArray[21] = (byte)_sendHeader.ReserveTwo6;
            sendingheaderPartArray[22] = (byte)_sendHeader.ReserveTwo7;
            sendingheaderPartArray[23] = (byte)_sendHeader.ReserveTwo8;

            sendingheaderPartArray[24] = (byte)_sendHeader.CommandNumber1;
            sendingheaderPartArray[25] = (byte)_sendHeader.CommandNumber2;
            sendingheaderPartArray[26] = (byte)_sendHeader.Instance1;
            sendingheaderPartArray[27] = (byte)_sendHeader.Instance2;

            sendingheaderPartArray[28] = (byte)_sendHeader.Attribute;
            sendingheaderPartArray[29] = (byte)_sendHeader.Service;
            sendingheaderPartArray[30] = (byte)_sendHeader.Padding1;
            sendingheaderPartArray[31] = (byte)_sendHeader.Padding2;

            sendingheaderPartArray[32] = (byte)_sendHeader.DataByte1;
            sendingheaderPartArray[33] = (byte)_sendHeader.DataByte2;
            sendingheaderPartArray[34] = (byte)_sendHeader.DataByte3;
            sendingheaderPartArray[35] = (byte)_sendHeader.DataByte4;

            #endregion

            for (int i = 0; i < Data.Length; i++)
            {
                sendingheaderPartArray[36 + i] = (byte)myData[i];
            }

            for (int j = 0; j < (36 + Count); j++)
            {
                //Header Part Filling.
                sendPackage[j] = sendingheaderPartArray[j];
            }

            try
            {
                int bytesTransferred = client.Send(sendPackage);
                int bytesReceived = client.Receive(receivedPackage);
            }
            catch (Exception)
            {
                Console.WriteLine("B_Write komutu paket gönderim hatası");
            }

            fillReceivedDataHeader(_receiveHeader, receivedPackage);

            if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
            {
                client.Close();

                Console.WriteLine("B_Write komutu iletişim hatası");
            }
            else
            {
                client.Close();
            }
        }

        public void RegRead(int VariableNumber, out ushort Data)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            byte[] sendPackage = new byte[32];
            byte[] receivedPackage = new byte[750];

            _sendHeader.DataPartSize1 = 0x00;
            _sendHeader.DataPartSize2 = 0x00;
            _sendHeader.RequestID = 0x00;
            _sendHeader.CommandNumber1 = 0x79;
            _sendHeader.CommandNumber2 = 0x00;
            _sendHeader.Instance1 = Convert.ToUInt16(VariableNumber);
            _sendHeader.Attribute = 1;
            _sendHeader.Service = 0x0E;

            #region sendingHeaderArray...

            byte[] sendingheaderPartArray = new byte[32] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                            (byte)_sendHeader.HeaderPartSize1,
                                                            (byte)_sendHeader.HeaderPartSize2,
                                                            (byte)_sendHeader.DataPartSize1,
                                                            (byte)_sendHeader.DataPartSize2,

                                                            (byte)_sendHeader.ReserveOne,
                                                            (byte)_sendHeader.ProcessingDivision,
                                                            (byte)_sendHeader.ACK,
                                                            (byte)_sendHeader.RequestID,

                                                            (byte)_sendHeader.BlockNumber1,
                                                            (byte)_sendHeader.BlockNumber2,
                                                            (byte)_sendHeader.BlockNumber3,
                                                            (byte)_sendHeader.BlockNumber4,

                                                            (byte)_sendHeader.ReserveTwo1,
                                                            (byte)_sendHeader.ReserveTwo2,
                                                            (byte)_sendHeader.ReserveTwo3,
                                                            (byte)_sendHeader.ReserveTwo4,

                                                            (byte)_sendHeader.ReserveTwo5,
                                                            (byte)_sendHeader.ReserveTwo6,
                                                            (byte)_sendHeader.ReserveTwo7,
                                                            (byte)_sendHeader.ReserveTwo8,

                                                            (byte)_sendHeader.CommandNumber1,
                                                            (byte)_sendHeader.CommandNumber2,
                                                            (byte)_sendHeader.Instance1,
                                                            (byte)_sendHeader.Instance2,

                                                            (byte)_sendHeader.Attribute,
                                                            (byte)_sendHeader.Service,
                                                            (byte)_sendHeader.Padding1,
                                                            (byte)_sendHeader.Padding2 };
            #endregion


            for (int j = 0; j < 32; j++)
            {
                //Header Part Filling.
                sendPackage[j] = sendingheaderPartArray[j];
            }

            try
            {
                int bytesTransferred = client.Send(sendPackage);
                int bytesReceived = client.Receive(receivedPackage);
            }
            catch (Exception)
            {
                Console.WriteLine("RegRead komutu paket gönderim hatası");
            }

            fillReceivedDataHeader(_receiveHeader, receivedPackage);

            if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
            {
                client.Close();

                Data = 0;
                Console.WriteLine("RegRead komutu iletişim hatası");
            }
            else
            {
                string HexDegeri = $"{receivedPackage[33].ToString("X2")}{receivedPackage[32].ToString("X2")}";

                int decValue = int.Parse(HexDegeri, System.Globalization.NumberStyles.HexNumber);
                Data = Convert.ToUInt16(decValue);
                client.Close();
            }
        }
        public void RegRead(int VariableNumber, int Count, out ushort?[] Data)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            byte[] sendPackage = new byte[36];
            byte[] receivedPackage = new byte[750];

            _sendHeader.DataPartSize1 = 0x04;
            _sendHeader.DataPartSize2 = 0x00;
            _sendHeader.RequestID = 0x00;
            _sendHeader.CommandNumber1 = 0x01;
            _sendHeader.CommandNumber2 = 0x03;



            // Decimal değerini byte dizisine dönüştürelim
            byte[] bytes1 = Decimal.GetBits(VariableNumber).SelectMany(BitConverter.GetBytes).ToArray();

            // Byte dizisini hexadecimal olarak çevirelim
            string hexValue1 = BitConverter.ToString(bytes1).Replace("-", "");

            // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
            string[] hexArray1 = new string[hexValue1.Length / 2];
            for (int i = 0; i < hexArray1.Length; i++)
            {
                hexArray1[i] = hexValue1.Substring(i * 2, 2);
            }

            // Convert hex string to UInt16
            _sendHeader.Instance1 = Convert.ToUInt16(hexArray1[0], 16);
            _sendHeader.Instance2 = Convert.ToUInt16(hexArray1[1], 16);



            _sendHeader.Attribute = 0;
            _sendHeader.Service = 0x33;


            // Decimal değerini byte dizisine dönüştürelim
            byte[] bytes = Decimal.GetBits(Count).SelectMany(BitConverter.GetBytes).ToArray();

            // Byte dizisini hexadecimal olarak çevirelim
            string hexValue = BitConverter.ToString(bytes).Replace("-", "");

            // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
            string[] hexArray = new string[hexValue.Length / 2];
            for (int i = 0; i < hexArray.Length; i++)
            {
                hexArray[i] = hexValue.Substring(i * 2, 2);
            }

            // Convert hex string to UInt16
            _sendHeader.DataByte1 = Convert.ToUInt16(hexArray[0], 16);
            _sendHeader.DataByte2 = Convert.ToUInt16(hexArray[1], 16);
            _sendHeader.DataByte3 = Convert.ToUInt16(hexArray[2], 16);
            _sendHeader.DataByte4 = Convert.ToUInt16(hexArray[3], 16);

            #region sendingHeaderArray...

            byte[] sendingheaderPartArray = new byte[36] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                            (byte)_sendHeader.HeaderPartSize1,
                                                            (byte)_sendHeader.HeaderPartSize2,
                                                            (byte)_sendHeader.DataPartSize1,
                                                            (byte)_sendHeader.DataPartSize2,

                                                            (byte)_sendHeader.ReserveOne,
                                                            (byte)_sendHeader.ProcessingDivision,
                                                            (byte)_sendHeader.ACK,
                                                            (byte)_sendHeader.RequestID,

                                                            (byte)_sendHeader.BlockNumber1,
                                                            (byte)_sendHeader.BlockNumber2,
                                                            (byte)_sendHeader.BlockNumber3,
                                                            (byte)_sendHeader.BlockNumber4,

                                                            (byte)_sendHeader.ReserveTwo1,
                                                            (byte)_sendHeader.ReserveTwo2,
                                                            (byte)_sendHeader.ReserveTwo3,
                                                            (byte)_sendHeader.ReserveTwo4,

                                                            (byte)_sendHeader.ReserveTwo5,
                                                            (byte)_sendHeader.ReserveTwo6,
                                                            (byte)_sendHeader.ReserveTwo7,
                                                            (byte)_sendHeader.ReserveTwo8,

                                                            (byte)_sendHeader.CommandNumber1,
                                                            (byte)_sendHeader.CommandNumber2,
                                                            (byte)_sendHeader.Instance1,
                                                            (byte)_sendHeader.Instance2,

                                                            (byte)_sendHeader.Attribute,
                                                            (byte)_sendHeader.Service,
                                                            (byte)_sendHeader.Padding1,
                                                            (byte)_sendHeader.Padding2,

                                                            (byte)_sendHeader.DataByte1,
                                                            (byte)_sendHeader.DataByte2,
                                                            (byte)_sendHeader.DataByte3,
                                                            (byte)_sendHeader.DataByte4 };
            #endregion


            for (int j = 0; j < 36; j++)
            {
                //Header Part Filling.
                sendPackage[j] = sendingheaderPartArray[j];
            }

            try
            {
                int bytesTransferred = client.Send(sendPackage);
                int bytesReceived = client.Receive(receivedPackage);
            }
            catch (Exception)
            {
                Console.WriteLine("RegRead komutu paket gönderim hatası");
            }

            fillReceivedDataHeader(_receiveHeader, receivedPackage);

            if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
            {
                client.Close();
                Console.WriteLine("RegRead komutu iletişim hatası");

                ushort?[] a = { 0 };

                Data = a;
            }
            else
            {
                ushort sayi1;
                ushort?[] Cevap = new ushort?[Count];


                for (int j = 0; j < Count; j++)
                {
                    string HexDegeri = $"{receivedPackage[37 + 2 * j].ToString("X2")}{receivedPackage[36 + 2 * j].ToString("X2")}";

                    int decValue = int.Parse(HexDegeri, System.Globalization.NumberStyles.HexNumber);
                    sayi1 = Convert.ToUInt16(decValue);

                    Cevap[j] = sayi1;
                }

                Data = Cevap;
            }

            client.Close();
        }
        public void RegWrite(int VariableNumber, ushort Data)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            byte[] sendPackage = new byte[36];
            byte[] receivedPackage = new byte[750];

            _sendHeader.DataPartSize1 = 0x04;
            _sendHeader.DataPartSize2 = 0x00;
            _sendHeader.RequestID = 0x01;
            _sendHeader.CommandNumber1 = 0x79;
            _sendHeader.CommandNumber2 = 0x00;
            _sendHeader.Instance1 = Convert.ToUInt16(VariableNumber);
            _sendHeader.Attribute = 1;
            _sendHeader.Service = 0x10;


            // Decimal değerini byte dizisine dönüştürelim
            byte[] bytes = Decimal.GetBits(Data).SelectMany(BitConverter.GetBytes).ToArray();

            // Byte dizisini hexadecimal olarak çevirelim
            string hexValue = BitConverter.ToString(bytes).Replace("-", "");

            // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
            string[] hexArray = new string[hexValue.Length / 2];
            for (int i = 0; i < hexArray.Length; i++)
            {
                hexArray[i] = hexValue.Substring(i * 2, 2);
            }

            // Convert hex string to UInt16
            _sendHeader.DataByte1 = Convert.ToUInt16(hexArray[0], 16);
            _sendHeader.DataByte2 = Convert.ToUInt16(hexArray[1], 16);


            #region sendingHeaderArray...

            byte[] sendingheaderPartArray = new byte[36] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                            (byte)_sendHeader.HeaderPartSize1,
                                                            (byte)_sendHeader.HeaderPartSize2,
                                                            (byte)_sendHeader.DataPartSize1,
                                                            (byte)_sendHeader.DataPartSize2,

                                                            (byte)_sendHeader.ReserveOne,
                                                            (byte)_sendHeader.ProcessingDivision,
                                                            (byte)_sendHeader.ACK,
                                                            (byte)_sendHeader.RequestID,

                                                            (byte)_sendHeader.BlockNumber1,
                                                            (byte)_sendHeader.BlockNumber2,
                                                            (byte)_sendHeader.BlockNumber3,
                                                            (byte)_sendHeader.BlockNumber4,

                                                            (byte)_sendHeader.ReserveTwo1,
                                                            (byte)_sendHeader.ReserveTwo2,
                                                            (byte)_sendHeader.ReserveTwo3,
                                                            (byte)_sendHeader.ReserveTwo4,

                                                            (byte)_sendHeader.ReserveTwo5,
                                                            (byte)_sendHeader.ReserveTwo6,
                                                            (byte)_sendHeader.ReserveTwo7,
                                                            (byte)_sendHeader.ReserveTwo8,

                                                            (byte)_sendHeader.CommandNumber1,
                                                            (byte)_sendHeader.CommandNumber2,
                                                            (byte)_sendHeader.Instance1,
                                                            (byte)_sendHeader.Instance2,

                                                            (byte)_sendHeader.Attribute,
                                                            (byte)_sendHeader.Service,
                                                            (byte)_sendHeader.Padding1,
                                                            (byte)_sendHeader.Padding2,

                                                            (byte)_sendHeader.DataByte1,
                                                            (byte)_sendHeader.DataByte2,
                                                            (byte)_sendHeader.DataByte3,
                                                            (byte)_sendHeader.DataByte4 };
            #endregion


            for (int j = 0; j < 36; j++)
            {
                //Header Part Filling.
                sendPackage[j] = sendingheaderPartArray[j];
            }

            try
            {
                int bytesTransferred = client.Send(sendPackage);
                int bytesReceived = client.Receive(receivedPackage);
            }
            catch (Exception)
            {
                Console.WriteLine("RegWrite komutu paket gönderim hatası");
            }

            fillReceivedDataHeader(_receiveHeader, receivedPackage);

            if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
            {
                client.Close();

                Console.WriteLine("RegWrite komutu iletişim hatası");
            }
            else
            {
                client.Close();
            }
        }
        public void RegWrite(int VariableNumber, int Count, ushort[] Data)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();

            MyData myData = new MyData(500);

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            byte[] sendPackage = new byte[36 + (Count * 2)];
            byte[] receivedPackage = new byte[750];



            // Decimal değerini byte dizisine dönüştürelim
            byte[] bytes2 = Decimal.GetBits((Count * 2) + 4).SelectMany(BitConverter.GetBytes).ToArray();

            // Byte dizisini hexadecimal olarak çevirelim
            string hexValue2 = BitConverter.ToString(bytes2).Replace("-", "");

            // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
            string[] hexArray2 = new string[hexValue2.Length / 2];
            for (int i = 0; i < hexArray2.Length; i++)
            {
                hexArray2[i] = hexValue2.Substring(i * 2, 2);
            }

            // Convert hex string to UInt16
            _sendHeader.DataPartSize1 = Convert.ToUInt16(hexArray2[0], 16);
            _sendHeader.DataPartSize2 = Convert.ToUInt16(hexArray2[1], 16);
            _sendHeader.RequestID = 0x01;
            _sendHeader.CommandNumber1 = 0x01;
            _sendHeader.CommandNumber2 = 0x03;
            _sendHeader.Instance1 = Convert.ToUInt16(VariableNumber);
            _sendHeader.Attribute = 0;
            _sendHeader.Service = 0x34;

            #region KAÇ ADET VERİYE YAZILACAK

            // Decimal değerini byte dizisine dönüştürelim
            byte[] bytes = Decimal.GetBits(Count).SelectMany(BitConverter.GetBytes).ToArray();

            // Byte dizisini hexadecimal olarak çevirelim
            string hexValue = BitConverter.ToString(bytes).Replace("-", "");

            // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
            string[] hexArray = new string[hexValue.Length / 2];
            for (int i = 0; i < hexArray.Length; i++)
            {
                hexArray[i] = hexValue.Substring(i * 2, 2);
            }

            // Convert hex string to UInt16
            _sendHeader.DataByte1 = Convert.ToUInt16(hexArray[0], 16);
            _sendHeader.DataByte2 = Convert.ToUInt16(hexArray[1], 16);
            _sendHeader.DataByte3 = Convert.ToUInt16(hexArray[2], 16);
            _sendHeader.DataByte4 = Convert.ToUInt16(hexArray[3], 16);

            #endregion

            for (int a = 0; a < Data.Length; a++)
            {
                // Decimal değerini byte dizisine dönüştürelim
                byte[] bytes1 = Decimal.GetBits(Data[a]).SelectMany(BitConverter.GetBytes).ToArray();

                // Byte dizisini hexadecimal olarak çevirelim
                string hexValue1 = BitConverter.ToString(bytes1).Replace("-", "");

                // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
                string[] hexArray1 = new string[hexValue1.Length / 2];

                for (int i = 0; i < hexArray1.Length; i++)
                {
                    hexArray1[i] = hexValue1.Substring(i * 2, 2);
                }

                for (int i = 0; i < 2; i++)
                {
                    myData[(a * 2) + i] = Convert.ToUInt16(hexArray1[i], 16);
                }
            }

            #region sendingHeaderArray...

            byte[] sendingheaderPartArray = new byte[36 + (Data.Length * 2)];

            sendingheaderPartArray[0] = Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0];
            sendingheaderPartArray[1] = Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0];
            sendingheaderPartArray[2] = Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0];
            sendingheaderPartArray[3] = Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0];

            sendingheaderPartArray[4] = (byte)_sendHeader.HeaderPartSize1;
            sendingheaderPartArray[5] = (byte)_sendHeader.HeaderPartSize2;
            sendingheaderPartArray[6] = (byte)_sendHeader.DataPartSize1;
            sendingheaderPartArray[7] = (byte)_sendHeader.DataPartSize2;

            sendingheaderPartArray[8] = (byte)_sendHeader.ReserveOne;
            sendingheaderPartArray[9] = (byte)_sendHeader.ProcessingDivision;
            sendingheaderPartArray[10] = (byte)_sendHeader.ACK;
            sendingheaderPartArray[11] = (byte)_sendHeader.RequestID;

            sendingheaderPartArray[12] = (byte)_sendHeader.BlockNumber1;
            sendingheaderPartArray[13] = (byte)_sendHeader.BlockNumber2;
            sendingheaderPartArray[14] = (byte)_sendHeader.BlockNumber3;
            sendingheaderPartArray[15] = (byte)_sendHeader.BlockNumber4;

            sendingheaderPartArray[16] = (byte)_sendHeader.ReserveTwo1;
            sendingheaderPartArray[17] = (byte)_sendHeader.ReserveTwo2;
            sendingheaderPartArray[18] = (byte)_sendHeader.ReserveTwo3;
            sendingheaderPartArray[19] = (byte)_sendHeader.ReserveTwo4;

            sendingheaderPartArray[20] = (byte)_sendHeader.ReserveTwo5;
            sendingheaderPartArray[21] = (byte)_sendHeader.ReserveTwo6;
            sendingheaderPartArray[22] = (byte)_sendHeader.ReserveTwo7;
            sendingheaderPartArray[23] = (byte)_sendHeader.ReserveTwo8;

            sendingheaderPartArray[24] = (byte)_sendHeader.CommandNumber1;
            sendingheaderPartArray[25] = (byte)_sendHeader.CommandNumber2;
            sendingheaderPartArray[26] = (byte)_sendHeader.Instance1;
            sendingheaderPartArray[27] = (byte)_sendHeader.Instance2;

            sendingheaderPartArray[28] = (byte)_sendHeader.Attribute;
            sendingheaderPartArray[29] = (byte)_sendHeader.Service;
            sendingheaderPartArray[30] = (byte)_sendHeader.Padding1;
            sendingheaderPartArray[31] = (byte)_sendHeader.Padding2;

            sendingheaderPartArray[32] = (byte)_sendHeader.DataByte1;
            sendingheaderPartArray[33] = (byte)_sendHeader.DataByte2;
            sendingheaderPartArray[34] = (byte)_sendHeader.DataByte3;
            sendingheaderPartArray[35] = (byte)_sendHeader.DataByte4;

            #endregion


            for (int i = 0; i < (Data.Length * 2); i++)
            {
                sendingheaderPartArray[36 + i] = (byte)myData[i];
            }


            for (int j = 0; j < (36 + Count * 2); j++)
            {
                //Header Part Filling.
                sendPackage[j] = sendingheaderPartArray[j];
            }

            try
            {
                int bytesTransferred = client.Send(sendPackage);
                int bytesReceived = client.Receive(receivedPackage);
            }
            catch (Exception)
            {
                Console.WriteLine("RegWrite komutu paket gönderim hatası");
            }

            fillReceivedDataHeader(_receiveHeader, receivedPackage);

            if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
            {
                client.Close();

                Console.WriteLine("RegWrite komutu iletişim hatası");
            }
            else
            {
                client.Close();
            }
        }

        public void IORead(int VariableNumber, out byte Data)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            byte[] sendPackage = new byte[32];
            byte[] receivedPackage = new byte[750];


            // Decimal değerini byte dizisine dönüştürelim
            byte[] bytes = Decimal.GetBits(VariableNumber).SelectMany(BitConverter.GetBytes).ToArray();

            // Byte dizisini hexadecimal olarak çevirelim
            string hexValue = BitConverter.ToString(bytes).Replace("-", "");

            // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
            string[] hexArray = new string[hexValue.Length / 2];
            for (int i = 0; i < hexArray.Length; i++)
            {
                hexArray[i] = hexValue.Substring(i * 2, 2);
            }

            _sendHeader.DataPartSize1 = 0x00;
            _sendHeader.DataPartSize2 = 0x00;
            _sendHeader.RequestID = 0x00;
            _sendHeader.CommandNumber1 = 0x78;
            _sendHeader.CommandNumber2 = 0x00;
            _sendHeader.Instance1 = Convert.ToUInt16(hexArray[0], 16);
            _sendHeader.Instance2 = Convert.ToUInt16(hexArray[1], 16);
            _sendHeader.Attribute = 1;
            _sendHeader.Service = 0x0E;

            #region sendingHeaderArray...

            byte[] sendingheaderPartArray = new byte[32] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                            (byte)_sendHeader.HeaderPartSize1,
                                                            (byte)_sendHeader.HeaderPartSize2,
                                                            (byte)_sendHeader.DataPartSize1,
                                                            (byte)_sendHeader.DataPartSize2,

                                                            (byte)_sendHeader.ReserveOne,
                                                            (byte)_sendHeader.ProcessingDivision,
                                                            (byte)_sendHeader.ACK,
                                                            (byte)_sendHeader.RequestID,

                                                            (byte)_sendHeader.BlockNumber1,
                                                            (byte)_sendHeader.BlockNumber2,
                                                            (byte)_sendHeader.BlockNumber3,
                                                            (byte)_sendHeader.BlockNumber4,

                                                            (byte)_sendHeader.ReserveTwo1,
                                                            (byte)_sendHeader.ReserveTwo2,
                                                            (byte)_sendHeader.ReserveTwo3,
                                                            (byte)_sendHeader.ReserveTwo4,

                                                            (byte)_sendHeader.ReserveTwo5,
                                                            (byte)_sendHeader.ReserveTwo6,
                                                            (byte)_sendHeader.ReserveTwo7,
                                                            (byte)_sendHeader.ReserveTwo8,

                                                            (byte)_sendHeader.CommandNumber1,
                                                            (byte)_sendHeader.CommandNumber2,
                                                            (byte)_sendHeader.Instance1,
                                                            (byte)_sendHeader.Instance2,

                                                            (byte)_sendHeader.Attribute,
                                                            (byte)_sendHeader.Service,
                                                            (byte)_sendHeader.Padding1,
                                                            (byte)_sendHeader.Padding2 };
            #endregion


            for (int j = 0; j < 32; j++)
            {
                //Header Part Filling.
                sendPackage[j] = sendingheaderPartArray[j];
            }

            try
            {
                int bytesTransferred = client.Send(sendPackage);
                int bytesReceived = client.Receive(receivedPackage);
            }
            catch (Exception)
            {
                Console.WriteLine($"IORead komutu paket gönderim hatası. IP: {IPAdresi}, V.No: {VariableNumber}, {DateTime.Now}");
            }

            fillReceivedDataHeader(_receiveHeader, receivedPackage);

            if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
            {
                client.Close();

                Data = 0;
                Console.WriteLine($"IORead komutu iletişim hatası. IP: {IPAdresi}, V.No: {VariableNumber}, {DateTime.Now}");
            }
            else
            {
                string HexValue = $"{receivedPackage[32].ToString("X2")}";

                int decValue = int.Parse(HexValue, System.Globalization.NumberStyles.HexNumber);
                byte Sayi1 = Convert.ToByte(decValue);

                client.Close();

                Data = Sayi1;
            }
        }
        public void IORead(int VariableNumber, int Count, out byte[] Data)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            byte[] sendPackage = new byte[36];
            byte[] receivedPackage = new byte[750];



            // Decimal değerini byte dizisine dönüştürelim
            byte[] bytes1 = Decimal.GetBits(VariableNumber).SelectMany(BitConverter.GetBytes).ToArray();

            // Byte dizisini hexadecimal olarak çevirelim
            string hexValue1 = BitConverter.ToString(bytes1).Replace("-", "");

            // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
            string[] hexArray1 = new string[hexValue1.Length / 2];
            for (int i = 0; i < hexArray1.Length; i++)
            {
                hexArray1[i] = hexValue1.Substring(i * 2, 2);
            }

            _sendHeader.DataPartSize1 = 0x04;
            _sendHeader.DataPartSize2 = 0x00;
            _sendHeader.RequestID = 0x00;
            _sendHeader.CommandNumber1 = 0x00;
            _sendHeader.CommandNumber2 = 0x03;
            _sendHeader.Instance1 = Convert.ToUInt16(hexArray1[0], 16);
            _sendHeader.Instance2 = Convert.ToUInt16(hexArray1[1], 16);
            _sendHeader.Attribute = 0;
            _sendHeader.Service = 0x33;


            // Decimal değerini byte dizisine dönüştürelim
            byte[] bytes = Decimal.GetBits(Count).SelectMany(BitConverter.GetBytes).ToArray();

            // Byte dizisini hexadecimal olarak çevirelim
            string hexValue = BitConverter.ToString(bytes).Replace("-", "");

            // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
            string[] hexArray = new string[hexValue.Length / 2];
            for (int i = 0; i < hexArray.Length; i++)
            {
                hexArray[i] = hexValue.Substring(i * 2, 2);
            }

            // Convert hex string to UInt16
            _sendHeader.DataByte1 = Convert.ToUInt16(hexArray[0], 16);
            _sendHeader.DataByte2 = Convert.ToUInt16(hexArray[1], 16);
            _sendHeader.DataByte3 = Convert.ToUInt16(hexArray[2], 16);
            _sendHeader.DataByte4 = Convert.ToUInt16(hexArray[3], 16);

            #region sendingHeaderArray...

            byte[] sendingheaderPartArray = new byte[36] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                            (byte)_sendHeader.HeaderPartSize1,
                                                            (byte)_sendHeader.HeaderPartSize2,
                                                            (byte)_sendHeader.DataPartSize1,
                                                            (byte)_sendHeader.DataPartSize2,

                                                            (byte)_sendHeader.ReserveOne,
                                                            (byte)_sendHeader.ProcessingDivision,
                                                            (byte)_sendHeader.ACK,
                                                            (byte)_sendHeader.RequestID,

                                                            (byte)_sendHeader.BlockNumber1,
                                                            (byte)_sendHeader.BlockNumber2,
                                                            (byte)_sendHeader.BlockNumber3,
                                                            (byte)_sendHeader.BlockNumber4,

                                                            (byte)_sendHeader.ReserveTwo1,
                                                            (byte)_sendHeader.ReserveTwo2,
                                                            (byte)_sendHeader.ReserveTwo3,
                                                            (byte)_sendHeader.ReserveTwo4,

                                                            (byte)_sendHeader.ReserveTwo5,
                                                            (byte)_sendHeader.ReserveTwo6,
                                                            (byte)_sendHeader.ReserveTwo7,
                                                            (byte)_sendHeader.ReserveTwo8,

                                                            (byte)_sendHeader.CommandNumber1,
                                                            (byte)_sendHeader.CommandNumber2,
                                                            (byte)_sendHeader.Instance1,
                                                            (byte)_sendHeader.Instance2,

                                                            (byte)_sendHeader.Attribute,
                                                            (byte)_sendHeader.Service,
                                                            (byte)_sendHeader.Padding1,
                                                            (byte)_sendHeader.Padding2,

                                                            (byte)_sendHeader.DataByte1,
                                                            (byte)_sendHeader.DataByte2,
                                                            (byte)_sendHeader.DataByte3,
                                                            (byte)_sendHeader.DataByte4 };
            #endregion


            for (int j = 0; j < 36; j++)
            {
                //Header Part Filling.
                sendPackage[j] = sendingheaderPartArray[j];
            }

            try
            {
                int bytesTransferred = client.Send(sendPackage);
                int bytesReceived = client.Receive(receivedPackage);
            }
            catch (Exception)
            {
                Console.WriteLine($"IORead komutu paket gönderim hatası. IP: {IPAdresi}, V.No: {VariableNumber}, {DateTime.Now}");

            }

            fillReceivedDataHeader(_receiveHeader, receivedPackage);

            if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
            {
                client.Close();
                Console.WriteLine($"IORead komutu iletişim hatası. IP: {IPAdresi}, V.No: {VariableNumber}, {DateTime.Now}");

                byte[] a = { 0 };

                Data = a;
            }
            else
            {
                byte[] Cevap = new byte[Count];

                for (int i = 0; i < Count; i++)
                {
                    string HexValue = $"{receivedPackage[36 + i].ToString("X2")}";

                    int decValue = int.Parse(HexValue, System.Globalization.NumberStyles.HexNumber);
                    byte Sayi1 = Convert.ToByte(decValue);

                    Cevap[i] = Sayi1;
                }

                Data = Cevap;
            }

            client.Close();
        }
        public void IOWrite(int VariableNumber, byte Data)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            byte[] sendPackage = new byte[36];
            byte[] receivedPackage = new byte[750];



            // Decimal değerini byte dizisine dönüştürelim
            byte[] bytes1 = Decimal.GetBits(VariableNumber).SelectMany(BitConverter.GetBytes).ToArray();

            // Byte dizisini hexadecimal olarak çevirelim
            string hexValue1 = BitConverter.ToString(bytes1).Replace("-", "");

            // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
            string[] hexArray1 = new string[hexValue1.Length / 2];
            for (int i = 0; i < hexArray1.Length; i++)
            {
                hexArray1[i] = hexValue1.Substring(i * 2, 2);
            }

            _sendHeader.DataPartSize1 = 0x04;
            _sendHeader.DataPartSize2 = 0x00;
            _sendHeader.RequestID = 0x01;
            _sendHeader.CommandNumber1 = 0x78;
            _sendHeader.CommandNumber2 = 0x00;
            _sendHeader.Instance1 = Convert.ToUInt16(hexArray1[0], 16);
            _sendHeader.Instance2 = Convert.ToUInt16(hexArray1[1], 16);
            _sendHeader.Attribute = 1;
            _sendHeader.Service = 0x10;


            // Decimal değerini byte dizisine dönüştürelim
            byte[] bytes = Decimal.GetBits(Data).SelectMany(BitConverter.GetBytes).ToArray();

            // Byte dizisini hexadecimal olarak çevirelim
            string hexValue = BitConverter.ToString(bytes).Replace("-", "");

            // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
            string[] hexArray = new string[hexValue.Length / 2];
            for (int i = 0; i < hexArray.Length; i++)
            {
                hexArray[i] = hexValue.Substring(i * 2, 2);
            }

            // Convert hex string to UInt16
            _sendHeader.DataByte1 = Convert.ToUInt16(hexArray[0], 16);
            _sendHeader.DataByte2 = Convert.ToUInt16(hexArray[1], 16);
            _sendHeader.DataByte3 = Convert.ToUInt16(hexArray[2], 16);
            _sendHeader.DataByte4 = Convert.ToUInt16(hexArray[3], 16);


            #region sendingHeaderArray...

            byte[] sendingheaderPartArray = new byte[36] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                            (byte)_sendHeader.HeaderPartSize1,
                                                            (byte)_sendHeader.HeaderPartSize2,
                                                            (byte)_sendHeader.DataPartSize1,
                                                            (byte)_sendHeader.DataPartSize2,

                                                            (byte)_sendHeader.ReserveOne,
                                                            (byte)_sendHeader.ProcessingDivision,
                                                            (byte)_sendHeader.ACK,
                                                            (byte)_sendHeader.RequestID,

                                                            (byte)_sendHeader.BlockNumber1,
                                                            (byte)_sendHeader.BlockNumber2,
                                                            (byte)_sendHeader.BlockNumber3,
                                                            (byte)_sendHeader.BlockNumber4,

                                                            (byte)_sendHeader.ReserveTwo1,
                                                            (byte)_sendHeader.ReserveTwo2,
                                                            (byte)_sendHeader.ReserveTwo3,
                                                            (byte)_sendHeader.ReserveTwo4,

                                                            (byte)_sendHeader.ReserveTwo5,
                                                            (byte)_sendHeader.ReserveTwo6,
                                                            (byte)_sendHeader.ReserveTwo7,
                                                            (byte)_sendHeader.ReserveTwo8,

                                                            (byte)_sendHeader.CommandNumber1,
                                                            (byte)_sendHeader.CommandNumber2,
                                                            (byte)_sendHeader.Instance1,
                                                            (byte)_sendHeader.Instance2,

                                                            (byte)_sendHeader.Attribute,
                                                            (byte)_sendHeader.Service,
                                                            (byte)_sendHeader.Padding1,
                                                            (byte)_sendHeader.Padding2,

                                                            (byte)_sendHeader.DataByte1,
                                                            (byte)_sendHeader.DataByte2,
                                                            (byte)_sendHeader.DataByte3,
                                                            (byte)_sendHeader.DataByte4 };
            #endregion


            for (int j = 0; j < 36; j++)
            {
                //Header Part Filling.
                sendPackage[j] = sendingheaderPartArray[j];
            }

            try
            {
                int bytesTransferred = client.Send(sendPackage);
                int bytesReceived = client.Receive(receivedPackage);
            }
            catch (Exception)
            {
                Console.WriteLine("IOWrite komutu paket gönderim hatası");
            }

            fillReceivedDataHeader(_receiveHeader, receivedPackage);

            if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
            {
                client.Close();

                Console.WriteLine("IOWrite komutu iletişim hatası");
            }
            else
            {
                client.Close();
            }
        }
        public void IOWrite(int VariableNumber, int Count, byte[] Data)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();

            MyData myData = new MyData(500);

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            byte[] sendPackage = new byte[36 + Count];
            byte[] receivedPackage = new byte[750];


            // Decimal değerini byte dizisine dönüştürelim
            byte[] bytes2 = Decimal.GetBits(Count + 4).SelectMany(BitConverter.GetBytes).ToArray();

            // Byte dizisini hexadecimal olarak çevirelim
            string hexValue2 = BitConverter.ToString(bytes2).Replace("-", "");

            // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
            string[] hexArray2 = new string[hexValue2.Length / 2];
            for (int i = 0; i < hexArray2.Length; i++)
            {
                hexArray2[i] = hexValue2.Substring(i * 2, 2);
            }



            // Decimal değerini byte dizisine dönüştürelim
            byte[] bytes3 = Decimal.GetBits(VariableNumber).SelectMany(BitConverter.GetBytes).ToArray();

            // Byte dizisini hexadecimal olarak çevirelim
            string hexValue3 = BitConverter.ToString(bytes3).Replace("-", "");

            // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
            string[] hexArray3 = new string[hexValue3.Length / 2];
            for (int i = 0; i < hexArray3.Length; i++)
            {
                hexArray3[i] = hexValue3.Substring(i * 2, 2);
            }



            // Convert hex string to UInt16
            _sendHeader.DataPartSize1 = Convert.ToUInt16(hexArray2[0], 16);
            _sendHeader.DataPartSize2 = Convert.ToUInt16(hexArray2[1], 16);
            _sendHeader.RequestID = 0x01;
            _sendHeader.CommandNumber1 = 0x00;
            _sendHeader.CommandNumber2 = 0x03;
            _sendHeader.Instance1 = Convert.ToUInt16(hexArray3[0], 16);
            _sendHeader.Instance2 = Convert.ToUInt16(hexArray3[1], 16);
            _sendHeader.Attribute = 0;
            _sendHeader.Service = 0x34;


            #region KAÇ ADET VERİYE YAZILACAK

            // Decimal değerini byte dizisine dönüştürelim
            byte[] bytes = Decimal.GetBits(Count).SelectMany(BitConverter.GetBytes).ToArray();

            // Byte dizisini hexadecimal olarak çevirelim
            string hexValue = BitConverter.ToString(bytes).Replace("-", "");

            // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
            string[] hexArray = new string[hexValue.Length / 2];
            for (int i = 0; i < hexArray.Length; i++)
            {
                hexArray[i] = hexValue.Substring(i * 2, 2);
            }

            // Convert hex string to UInt16
            _sendHeader.DataByte1 = Convert.ToUInt16(hexArray[0], 16);
            _sendHeader.DataByte2 = Convert.ToUInt16(hexArray[1], 16);
            _sendHeader.DataByte3 = Convert.ToUInt16(hexArray[2], 16);
            _sendHeader.DataByte4 = Convert.ToUInt16(hexArray[3], 16);

            #endregion


            for (int a = 0; a < Data.Length; a++)
            {
                // Decimal değerini byte dizisine dönüştürelim
                byte[] bytes1 = Decimal.GetBits(Data[a]).SelectMany(BitConverter.GetBytes).ToArray();

                // Byte dizisini hexadecimal olarak çevirelim
                string hexValue1 = BitConverter.ToString(bytes1).Replace("-", "");

                // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
                string[] hexArray1 = new string[hexValue1.Length / 2];

                for (int i = 0; i < hexArray1.Length; i++)
                {
                    hexArray1[i] = hexValue1.Substring(i * 2, 2);
                }

                for (int i = 0; i < 1; i++)
                {
                    myData[a + i] = Convert.ToUInt16(hexArray1[i], 16);
                }
            }

            #region sendingHeaderArray...

            byte[] sendingheaderPartArray = new byte[36 + Data.Length];

            sendingheaderPartArray[0] = Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0];
            sendingheaderPartArray[1] = Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0];
            sendingheaderPartArray[2] = Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0];
            sendingheaderPartArray[3] = Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0];

            sendingheaderPartArray[4] = (byte)_sendHeader.HeaderPartSize1;
            sendingheaderPartArray[5] = (byte)_sendHeader.HeaderPartSize2;
            sendingheaderPartArray[6] = (byte)_sendHeader.DataPartSize1;
            sendingheaderPartArray[7] = (byte)_sendHeader.DataPartSize2;

            sendingheaderPartArray[8] = (byte)_sendHeader.ReserveOne;
            sendingheaderPartArray[9] = (byte)_sendHeader.ProcessingDivision;
            sendingheaderPartArray[10] = (byte)_sendHeader.ACK;
            sendingheaderPartArray[11] = (byte)_sendHeader.RequestID;

            sendingheaderPartArray[12] = (byte)_sendHeader.BlockNumber1;
            sendingheaderPartArray[13] = (byte)_sendHeader.BlockNumber2;
            sendingheaderPartArray[14] = (byte)_sendHeader.BlockNumber3;
            sendingheaderPartArray[15] = (byte)_sendHeader.BlockNumber4;

            sendingheaderPartArray[16] = (byte)_sendHeader.ReserveTwo1;
            sendingheaderPartArray[17] = (byte)_sendHeader.ReserveTwo2;
            sendingheaderPartArray[18] = (byte)_sendHeader.ReserveTwo3;
            sendingheaderPartArray[19] = (byte)_sendHeader.ReserveTwo4;

            sendingheaderPartArray[20] = (byte)_sendHeader.ReserveTwo5;
            sendingheaderPartArray[21] = (byte)_sendHeader.ReserveTwo6;
            sendingheaderPartArray[22] = (byte)_sendHeader.ReserveTwo7;
            sendingheaderPartArray[23] = (byte)_sendHeader.ReserveTwo8;

            sendingheaderPartArray[24] = (byte)_sendHeader.CommandNumber1;
            sendingheaderPartArray[25] = (byte)_sendHeader.CommandNumber2;
            sendingheaderPartArray[26] = (byte)_sendHeader.Instance1;
            sendingheaderPartArray[27] = (byte)_sendHeader.Instance2;

            sendingheaderPartArray[28] = (byte)_sendHeader.Attribute;
            sendingheaderPartArray[29] = (byte)_sendHeader.Service;
            sendingheaderPartArray[30] = (byte)_sendHeader.Padding1;
            sendingheaderPartArray[31] = (byte)_sendHeader.Padding2;

            sendingheaderPartArray[32] = (byte)_sendHeader.DataByte1;
            sendingheaderPartArray[33] = (byte)_sendHeader.DataByte2;
            sendingheaderPartArray[34] = (byte)_sendHeader.DataByte3;
            sendingheaderPartArray[35] = (byte)_sendHeader.DataByte4;

            #endregion

            for (int i = 0; i < Data.Length; i++)
            {
                sendingheaderPartArray[36 + i] = (byte)myData[i];
            }

            for (int j = 0; j < (36 + Count); j++)
            {
                //Header Part Filling.
                sendPackage[j] = sendingheaderPartArray[j];
            }

            try
            {
                int bytesTransferred = client.Send(sendPackage);
                int bytesReceived = client.Receive(receivedPackage);
            }
            catch (Exception)
            {
                Console.WriteLine("IOWrite komutu paket gönderim hatası");
            }

            fillReceivedDataHeader(_receiveHeader, receivedPackage);

            if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
            {
                client.Close();

                Console.WriteLine("IOWrite komutu iletişim hatası");
            }
            else
            {
                client.Close();
            }
        }

        public void GetSystemInformation(int Type, out SystemInformation SI)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();
            SI = new SystemInformation();

            string[] Cevap = new string[3];

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            for (int i = 1; i < 4; i++)
            {
                byte[] sendPackage = new byte[32];
                byte[] receivedPackage = new byte[750];

                _sendHeader.DataPartSize1 = 0x00;
                _sendHeader.DataPartSize2 = 0x00;
                _sendHeader.RequestID = 0x00;
                _sendHeader.CommandNumber1 = 0x89;
                _sendHeader.CommandNumber2 = 0x00;
                _sendHeader.Instance1 = Convert.ToUInt16(Type);
                _sendHeader.Attribute = Convert.ToUInt16(i);
                _sendHeader.Service = 0x0E;

                #region sendingHeaderArray...

                byte[] sendingheaderPartArray = new byte[32] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                            (byte)_sendHeader.HeaderPartSize1,
                                                            (byte)_sendHeader.HeaderPartSize2,
                                                            (byte)_sendHeader.DataPartSize1,
                                                            (byte)_sendHeader.DataPartSize2,

                                                            (byte)_sendHeader.ReserveOne,
                                                            (byte)_sendHeader.ProcessingDivision,
                                                            (byte)_sendHeader.ACK,
                                                            (byte)_sendHeader.RequestID,

                                                            (byte)_sendHeader.BlockNumber1,
                                                            (byte)_sendHeader.BlockNumber2,
                                                            (byte)_sendHeader.BlockNumber3,
                                                            (byte)_sendHeader.BlockNumber4,

                                                            (byte)_sendHeader.ReserveTwo1,
                                                            (byte)_sendHeader.ReserveTwo2,
                                                            (byte)_sendHeader.ReserveTwo3,
                                                            (byte)_sendHeader.ReserveTwo4,

                                                            (byte)_sendHeader.ReserveTwo5,
                                                            (byte)_sendHeader.ReserveTwo6,
                                                            (byte)_sendHeader.ReserveTwo7,
                                                            (byte)_sendHeader.ReserveTwo8,

                                                            (byte)_sendHeader.CommandNumber1,
                                                            (byte)_sendHeader.CommandNumber2,
                                                            (byte)_sendHeader.Instance1,
                                                            (byte)_sendHeader.Instance2,

                                                            (byte)_sendHeader.Attribute,
                                                            (byte)_sendHeader.Service,
                                                            (byte)_sendHeader.Padding1,
                                                            (byte)_sendHeader.Padding2 };
                #endregion


                for (int j = 0; j < 32; j++)
                {
                    //Header Part Filling.
                    sendPackage[j] = sendingheaderPartArray[j];
                }

                try
                {
                    int bytesTransferred = client.Send(sendPackage);
                    int bytesReceived = client.Receive(receivedPackage);
                }
                catch (Exception)
                {
                    Console.WriteLine("GetSystemInformation komutu paket gönderim hatası");
                    return;
                }

                fillReceivedDataHeader(_receiveHeader, receivedPackage);

                if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
                {
                    client.Close();

                    Console.WriteLine("GetSystemInformation komutu iletişim hatası");
                    return;
                }
                else
                {
                    string hexifade = "";

                    for (int j = 32; j < 57; j++)
                    {
                        hexifade = hexifade + receivedPackage[j].ToString("X2");
                    }

                    string hexString = hexifade; // Hexadecimal string
                    byte[] bytes = new byte[hexString.Length / 2];

                    for (int a = 0; a < hexString.Length; a += 2)
                    {
                        bytes[a / 2] = Convert.ToByte(hexString.Substring(a, 2), 16);
                    }

                    string text = System.Text.Encoding.UTF8.GetString(bytes).TrimEnd('\0');

                    Cevap[i - 1] = text;

                    SI.SystemSoftwareVersion = Cevap[0];
                    SI.ModelName = Cevap[1];
                    SI.ParameterVersion = Cevap[2];
                }
            }

            client.Close();
        }
        public void GetManagementTime(int Type, out ManagementTime MT)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();
            MT = new ManagementTime();

            string[] Cevap = new string[2];

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            for (int i = 1; i < 3; i++)
            {
                byte[] sendPackage = new byte[32];
                byte[] receivedPackage = new byte[750];


                // Decimal değerini byte dizisine dönüştürelim
                byte[] bytes1 = Decimal.GetBits(Type).SelectMany(BitConverter.GetBytes).ToArray();

                // Byte dizisini hexadecimal olarak çevirelim
                string hexValue1 = BitConverter.ToString(bytes1).Replace("-", "");

                // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
                string[] hexArray1 = new string[hexValue1.Length / 2];
                for (int b = 0; b < hexArray1.Length; b++)
                {
                    hexArray1[b] = hexValue1.Substring(b * 2, 2);
                }


                _sendHeader.DataPartSize1 = 0x00;
                _sendHeader.DataPartSize2 = 0x00;
                _sendHeader.RequestID = 0x00;
                _sendHeader.CommandNumber1 = 0x88;
                _sendHeader.CommandNumber2 = 0x00;
                _sendHeader.Instance1 = Convert.ToUInt16(hexArray1[0], 16);
                _sendHeader.Instance2 = Convert.ToUInt16(hexArray1[1], 16);
                _sendHeader.Attribute = Convert.ToUInt16(i);
                _sendHeader.Service = 0x0E;

                #region sendingHeaderArray...

                byte[] sendingheaderPartArray = new byte[32] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                            (byte)_sendHeader.HeaderPartSize1,
                                                            (byte)_sendHeader.HeaderPartSize2,
                                                            (byte)_sendHeader.DataPartSize1,
                                                            (byte)_sendHeader.DataPartSize2,

                                                            (byte)_sendHeader.ReserveOne,
                                                            (byte)_sendHeader.ProcessingDivision,
                                                            (byte)_sendHeader.ACK,
                                                            (byte)_sendHeader.RequestID,

                                                            (byte)_sendHeader.BlockNumber1,
                                                            (byte)_sendHeader.BlockNumber2,
                                                            (byte)_sendHeader.BlockNumber3,
                                                            (byte)_sendHeader.BlockNumber4,

                                                            (byte)_sendHeader.ReserveTwo1,
                                                            (byte)_sendHeader.ReserveTwo2,
                                                            (byte)_sendHeader.ReserveTwo3,
                                                            (byte)_sendHeader.ReserveTwo4,

                                                            (byte)_sendHeader.ReserveTwo5,
                                                            (byte)_sendHeader.ReserveTwo6,
                                                            (byte)_sendHeader.ReserveTwo7,
                                                            (byte)_sendHeader.ReserveTwo8,

                                                            (byte)_sendHeader.CommandNumber1,
                                                            (byte)_sendHeader.CommandNumber2,
                                                            (byte)_sendHeader.Instance1,
                                                            (byte)_sendHeader.Instance2,

                                                            (byte)_sendHeader.Attribute,
                                                            (byte)_sendHeader.Service,
                                                            (byte)_sendHeader.Padding1,
                                                            (byte)_sendHeader.Padding2 };
                #endregion


                for (int j = 0; j < 32; j++)
                {
                    //Header Part Filling.
                    sendPackage[j] = sendingheaderPartArray[j];
                }

                try
                {
                    int bytesTransferred = client.Send(sendPackage);
                    int bytesReceived = client.Receive(receivedPackage);
                }
                catch (Exception)
                {
                    Console.WriteLine("GetManagementTime komutu paket gönderim hatası");
                    return;
                }

                fillReceivedDataHeader(_receiveHeader, receivedPackage);

                if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
                {
                    client.Close();

                    Console.WriteLine("GetManagementTime komutu iletişim hatası");
                    return;
                }
                else
                {
                    string hexifade = "";

                    for (int j = 32; j < 57; j++)
                    {
                        hexifade = hexifade + receivedPackage[j].ToString("X2");
                    }

                    string hexString = hexifade; // Hexadecimal string
                    byte[] bytes = new byte[hexString.Length / 2];

                    for (int a = 0; a < hexString.Length; a += 2)
                    {
                        bytes[a / 2] = Convert.ToByte(hexString.Substring(a, 2), 16);
                    }

                    string text = System.Text.Encoding.UTF8.GetString(bytes).TrimEnd('\0');

                    Cevap[i - 1] = text;

                    MT.OperationStartTime = Cevap[0];
                    MT.ElapsedTime = Cevap[1];
                }
            }
            client.Close();
        }

        public void RobotPositionDataRead(int ControlGroup, out RobotPositionData RPD)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();
            RPD = new RobotPositionData();

            uint[] Cevap = new uint[4];
            int[] Eksenler = new int[8];

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            for (int i = 1; i < 14; i++)
            {


                byte[] sendPackage = new byte[32];
                byte[] receivedPackage = new byte[750];

                _sendHeader.DataPartSize1 = 0x00;
                _sendHeader.DataPartSize2 = 0x00;
                _sendHeader.RequestID = 0x00;
                _sendHeader.CommandNumber1 = 0x75;
                _sendHeader.CommandNumber2 = 0x00;
                _sendHeader.Instance1 = Convert.ToUInt16(ControlGroup);
                _sendHeader.Attribute = Convert.ToUInt16(i);
                _sendHeader.Service = 0x0E;

                #region sendingHeaderArray...

                byte[] sendingheaderPartArray = new byte[32] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                            (byte)_sendHeader.HeaderPartSize1,
                                                            (byte)_sendHeader.HeaderPartSize2,
                                                            (byte)_sendHeader.DataPartSize1,
                                                            (byte)_sendHeader.DataPartSize2,

                                                            (byte)_sendHeader.ReserveOne,
                                                            (byte)_sendHeader.ProcessingDivision,
                                                            (byte)_sendHeader.ACK,
                                                            (byte)_sendHeader.RequestID,

                                                            (byte)_sendHeader.BlockNumber1,
                                                            (byte)_sendHeader.BlockNumber2,
                                                            (byte)_sendHeader.BlockNumber3,
                                                            (byte)_sendHeader.BlockNumber4,

                                                            (byte)_sendHeader.ReserveTwo1,
                                                            (byte)_sendHeader.ReserveTwo2,
                                                            (byte)_sendHeader.ReserveTwo3,
                                                            (byte)_sendHeader.ReserveTwo4,

                                                            (byte)_sendHeader.ReserveTwo5,
                                                            (byte)_sendHeader.ReserveTwo6,
                                                            (byte)_sendHeader.ReserveTwo7,
                                                            (byte)_sendHeader.ReserveTwo8,

                                                            (byte)_sendHeader.CommandNumber1,
                                                            (byte)_sendHeader.CommandNumber2,
                                                            (byte)_sendHeader.Instance1,
                                                            (byte)_sendHeader.Instance2,

                                                            (byte)_sendHeader.Attribute,
                                                            (byte)_sendHeader.Service,
                                                            (byte)_sendHeader.Padding1,
                                                            (byte)_sendHeader.Padding2 };
                #endregion

                for (int j = 0; j < 32; j++)
                {
                    //Header Part Filling.
                    sendPackage[j] = sendingheaderPartArray[j];
                }

                try
                {
                    int bytesTransferred = client.Send(sendPackage);
                    int bytesReceived = client.Receive(receivedPackage);
                }
                catch (Exception)
                {
                    Console.WriteLine("RobotPositionDataRead komutu paket gönderim hatası");
                    return;
                }

                fillReceivedDataHeader(_receiveHeader, receivedPackage);

                if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
                {
                    client.Close();

                    Console.WriteLine("RobotPositionDataRead komutu iletişim hatası");
                    return;
                }
                else
                {
                    if (i <= 4 && i != 2)
                    {
                        string HexIfade = $"{receivedPackage[35].ToString("X2")}{receivedPackage[34].ToString("X2")}{receivedPackage[33].ToString("X2")}{receivedPackage[32].ToString("X2")}";

                        int decValue = int.Parse(HexIfade, System.Globalization.NumberStyles.HexNumber);

                        Cevap[i - 1] = Convert.ToUInt16(decValue);

                        RPD.DataType = Cevap[0];
                        RPD.ToolNumber = Cevap[2];
                        RPD.UserCoordinateNumber = Cevap[3];
                    }

                    if (i == 2)
                    {
                        string HexDegeri = $"{receivedPackage[35].ToString("X2")}{receivedPackage[34].ToString("X2")}{receivedPackage[33].ToString("X2")}{receivedPackage[32].ToString("X2")}";

                        byte[] binaryArray = HexToBinary(HexDegeri);

                        RPD.FrontBack = Convert.ToInt32(binaryArray[7]);
                        RPD.Arm = Convert.ToInt32(binaryArray[6]);
                        RPD.Flip = Convert.ToInt32(binaryArray[5]);
                        RPD.R180 = Convert.ToInt32(binaryArray[4]);
                        RPD.T180 = Convert.ToInt32(binaryArray[3]);
                        RPD.S180 = Convert.ToInt32(binaryArray[2]);
                        RPD.Redundant = Convert.ToInt32(binaryArray[1]);
                        RPD.Conversion = Convert.ToInt32(binaryArray[0]);
                    }

                    if (i == 5)
                    {
                        string HexDegeri = $"{receivedPackage[35].ToString("X2")}{receivedPackage[34].ToString("X2")}{receivedPackage[33].ToString("X2")}{receivedPackage[32].ToString("X2")}";

                        byte[] binaryArray = HexToBinary(HexDegeri);

                        RPD.L180 = Convert.ToInt32(binaryArray[7]);
                        RPD.U180 = Convert.ToInt32(binaryArray[6]);
                        RPD.B180 = Convert.ToInt32(binaryArray[5]);
                        RPD.E180 = Convert.ToInt32(binaryArray[4]);
                        RPD.W180 = Convert.ToInt32(binaryArray[3]);
                    }

                    if (i <= 14 && i >= 6)
                    {

                        string HexDegeri = $"{receivedPackage[35].ToString("X2")}{receivedPackage[34].ToString("X2")}{receivedPackage[33].ToString("X2")}{receivedPackage[32].ToString("X2")}";

                        int decValue = int.Parse(HexDegeri, System.Globalization.NumberStyles.HexNumber);

                        Eksenler[i - 6] = decValue;

                        RPD.AxisData = Eksenler;
                    }
                }
            }
            client.Close();
        }
        public void TorqueDataRead(int ControlGroup, out int[] Data)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();

            int[] Cevap = new int[8];
            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.


            byte[] sendPackage = new byte[32];
            byte[] receivedPackage = new byte[750];

            _sendHeader.DataPartSize1 = 0x00;
            _sendHeader.DataPartSize2 = 0x00;
            _sendHeader.RequestID = 0x00;
            _sendHeader.CommandNumber1 = 0x77;
            _sendHeader.CommandNumber2 = 0x00;
            _sendHeader.Instance1 = Convert.ToUInt16(ControlGroup);
            _sendHeader.Attribute = 0x00;
            _sendHeader.Service = 0x01;

            #region sendingHeaderArray...

            byte[] sendingheaderPartArray = new byte[32] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                            (byte)_sendHeader.HeaderPartSize1,
                                                            (byte)_sendHeader.HeaderPartSize2,
                                                            (byte)_sendHeader.DataPartSize1,
                                                            (byte)_sendHeader.DataPartSize2,

                                                            (byte)_sendHeader.ReserveOne,
                                                            (byte)_sendHeader.ProcessingDivision,
                                                            (byte)_sendHeader.ACK,
                                                            (byte)_sendHeader.RequestID,

                                                            (byte)_sendHeader.BlockNumber1,
                                                            (byte)_sendHeader.BlockNumber2,
                                                            (byte)_sendHeader.BlockNumber3,
                                                            (byte)_sendHeader.BlockNumber4,

                                                            (byte)_sendHeader.ReserveTwo1,
                                                            (byte)_sendHeader.ReserveTwo2,
                                                            (byte)_sendHeader.ReserveTwo3,
                                                            (byte)_sendHeader.ReserveTwo4,

                                                            (byte)_sendHeader.ReserveTwo5,
                                                            (byte)_sendHeader.ReserveTwo6,
                                                            (byte)_sendHeader.ReserveTwo7,
                                                            (byte)_sendHeader.ReserveTwo8,

                                                            (byte)_sendHeader.CommandNumber1,
                                                            (byte)_sendHeader.CommandNumber2,
                                                            (byte)_sendHeader.Instance1,
                                                            (byte)_sendHeader.Instance2,

                                                            (byte)_sendHeader.Attribute,
                                                            (byte)_sendHeader.Service,
                                                            (byte)_sendHeader.Padding1,
                                                            (byte)_sendHeader.Padding2 };
            #endregion


            for (int j = 0; j < 32; j++)
            {
                //Header Part Filling.
                sendPackage[j] = sendingheaderPartArray[j];
            }

            try
            {
                int bytesTransferred = client.Send(sendPackage);
                int bytesReceived = client.Receive(receivedPackage);
            }
            catch (Exception)
            {
                Console.WriteLine("TorqueDataRead komutu paket gönderim hatası");
                int[] a = { 0 };
                Data = a;
                return;
            }

            fillReceivedDataHeader(_receiveHeader, receivedPackage);

            if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
            {
                client.Close();

                int[] a = { 10001 };
                Data = a;
                //Console.WriteLine("TorqueDataRead komutu iletişim hatası");
                return;

            }
            else
            {
                for (int i = 0; i < 6; i++)
                {
                    string HexValue = $"{receivedPackage[35 + (i * 4)].ToString("X2")}{receivedPackage[34 + (i * 4)].ToString("X2")}{receivedPackage[33 + (i * 4)].ToString("X2")}{receivedPackage[32 + (i * 4)].ToString("X2")}";

                    int decValue = int.Parse(HexValue, System.Globalization.NumberStyles.HexNumber);

                    Cevap[i] = decValue;
                }
            }

            Data = Cevap;

            client.Close();
        }

        public void ReadAlarmHistoryData(int AlarmNumber, out Alarm_Data AD)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();

            AD = new Alarm_Data();

            uint[] Cevap_F = new uint[3];
            string[] Cevap_4_5 = new string[2];

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            for (int a = 1; a <= 5; a++)
            {


                byte[] sendPackage = new byte[32];
                byte[] receivedPackage = new byte[750];


                // Decimal değerini byte dizisine dönüştürelim
                byte[] bytes1 = Decimal.GetBits(AlarmNumber).SelectMany(BitConverter.GetBytes).ToArray();

                // Byte dizisini hexadecimal olarak çevirelim
                string hexValue1 = BitConverter.ToString(bytes1).Replace("-", "");

                // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
                string[] hexArray1 = new string[hexValue1.Length / 2];
                for (int i = 0; i < hexArray1.Length; i++)
                {
                    hexArray1[i] = hexValue1.Substring(i * 2, 2);
                }

                _sendHeader.DataPartSize1 = 0x00;
                _sendHeader.DataPartSize2 = 0x00;
                _sendHeader.RequestID = 0x00;
                _sendHeader.CommandNumber1 = 0x71;
                _sendHeader.CommandNumber2 = 0x00;
                _sendHeader.Instance1 = Convert.ToUInt16(hexArray1[0], 16);
                _sendHeader.Instance2 = Convert.ToUInt16(hexArray1[1], 16);
                _sendHeader.Attribute = Convert.ToUInt16(a);
                _sendHeader.Service = 0x0E;

                #region sendingHeaderArray...

                byte[] sendingheaderPartArray = new byte[32] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                            (byte)_sendHeader.HeaderPartSize1,
                                                            (byte)_sendHeader.HeaderPartSize2,
                                                            (byte)_sendHeader.DataPartSize1,
                                                            (byte)_sendHeader.DataPartSize2,

                                                            (byte)_sendHeader.ReserveOne,
                                                            (byte)_sendHeader.ProcessingDivision,
                                                            (byte)_sendHeader.ACK,
                                                            (byte)_sendHeader.RequestID,

                                                            (byte)_sendHeader.BlockNumber1,
                                                            (byte)_sendHeader.BlockNumber2,
                                                            (byte)_sendHeader.BlockNumber3,
                                                            (byte)_sendHeader.BlockNumber4,

                                                            (byte)_sendHeader.ReserveTwo1,
                                                            (byte)_sendHeader.ReserveTwo2,
                                                            (byte)_sendHeader.ReserveTwo3,
                                                            (byte)_sendHeader.ReserveTwo4,

                                                            (byte)_sendHeader.ReserveTwo5,
                                                            (byte)_sendHeader.ReserveTwo6,
                                                            (byte)_sendHeader.ReserveTwo7,
                                                            (byte)_sendHeader.ReserveTwo8,

                                                            (byte)_sendHeader.CommandNumber1,
                                                            (byte)_sendHeader.CommandNumber2,
                                                            (byte)_sendHeader.Instance1,
                                                            (byte)_sendHeader.Instance2,

                                                            (byte)_sendHeader.Attribute,
                                                            (byte)_sendHeader.Service,
                                                            (byte)_sendHeader.Padding1,
                                                            (byte)_sendHeader.Padding2 };
                #endregion


                for (int j = 0; j < 32; j++)
                {
                    //Header Part Filling.
                    sendPackage[j] = sendingheaderPartArray[j];
                }

                try
                {
                    int bytesTransferred = client.Send(sendPackage);
                    int bytesReceived = client.Receive(receivedPackage);
                }
                catch (Exception)
                {
                    Console.WriteLine("ReadAlarmHistoryData komutu paket gönderim hatası");
                    return;
                }

                fillReceivedDataHeader(_receiveHeader, receivedPackage);

                if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
                {
                    client.Close();

                    Console.WriteLine("ReadAlarmHistoryData komutu iletişim hatası");
                    return;
                }
                else
                {
                    if (a <= 3)
                    {
                        string HexDegeri = $"{receivedPackage[35].ToString("X2")}{receivedPackage[34].ToString("X2")}{receivedPackage[33].ToString("X2")}{receivedPackage[32].ToString("X2")}";

                        uint decValue = uint.Parse(HexDegeri, System.Globalization.NumberStyles.HexNumber);

                        Cevap_F[a - 1] = decValue;

                        AD.AlarmCode = Cevap_F[0];
                        AD.AlarmData = Cevap_F[1];
                        AD.AlarmType = Cevap_F[2];
                    }

                    if (a == 4 || a == 5)
                    {
                        int Bitis = 0;

                        if (a == 4)
                        {
                            Bitis = 48;
                        }
                        else if (a == 5)
                        {
                            Bitis = 64;
                        }

                        string hexifade = "";

                        for (int j = 32; j < Bitis; j++)
                        {
                            hexifade = hexifade + receivedPackage[j].ToString("X2");
                        }

                        string hexString = hexifade; // Hexadecimal string
                        byte[] bytes = new byte[hexString.Length / 2];

                        for (int i = 0; i < hexString.Length; i += 2)
                        {
                            bytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
                        }

                        //string text = System.Text.Encoding.UTF8.GetString(bytes);
                        string text = System.Text.Encoding.UTF8.GetString(bytes).TrimEnd('\0');

                        Cevap_4_5[a - 4] = text;

                        AD.AlarmTime = Cevap_4_5[0];
                        AD.AlarmName = Cevap_4_5[1];
                    }
                }
            }
            client.Close();
        }
        public void ReadAlarmHistoryData(int AlarmNumber, out Alarm_Data_Sub AD)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();

            AD = new Alarm_Data_Sub();

            uint[] Cevap_F = new uint[3];
            string[] Cevap_4_5 = new string[5];

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            for (int a = 1; a <= 8; a++)
            {


                byte[] sendPackage = new byte[32];
                byte[] receivedPackage = new byte[750];


                // Decimal değerini byte dizisine dönüştürelim
                byte[] bytes1 = Decimal.GetBits(AlarmNumber).SelectMany(BitConverter.GetBytes).ToArray();

                // Byte dizisini hexadecimal olarak çevirelim
                string hexValue1 = BitConverter.ToString(bytes1).Replace("-", "");

                // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
                string[] hexArray1 = new string[hexValue1.Length / 2];
                for (int i = 0; i < hexArray1.Length; i++)
                {
                    hexArray1[i] = hexValue1.Substring(i * 2, 2);
                }

                _sendHeader.DataPartSize1 = 0x00;
                _sendHeader.DataPartSize2 = 0x00;
                _sendHeader.RequestID = 0x00;
                _sendHeader.CommandNumber1 = 0x0B;
                _sendHeader.CommandNumber2 = 0x03;
                _sendHeader.Instance1 = Convert.ToUInt16(hexArray1[0], 16);
                _sendHeader.Instance2 = Convert.ToUInt16(hexArray1[1], 16);
                _sendHeader.Attribute = Convert.ToUInt16(a);
                _sendHeader.Service = 0x0E;

                #region sendingHeaderArray...

                byte[] sendingheaderPartArray = new byte[32] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                            (byte)_sendHeader.HeaderPartSize1,
                                                            (byte)_sendHeader.HeaderPartSize2,
                                                            (byte)_sendHeader.DataPartSize1,
                                                            (byte)_sendHeader.DataPartSize2,

                                                            (byte)_sendHeader.ReserveOne,
                                                            (byte)_sendHeader.ProcessingDivision,
                                                            (byte)_sendHeader.ACK,
                                                            (byte)_sendHeader.RequestID,

                                                            (byte)_sendHeader.BlockNumber1,
                                                            (byte)_sendHeader.BlockNumber2,
                                                            (byte)_sendHeader.BlockNumber3,
                                                            (byte)_sendHeader.BlockNumber4,

                                                            (byte)_sendHeader.ReserveTwo1,
                                                            (byte)_sendHeader.ReserveTwo2,
                                                            (byte)_sendHeader.ReserveTwo3,
                                                            (byte)_sendHeader.ReserveTwo4,

                                                            (byte)_sendHeader.ReserveTwo5,
                                                            (byte)_sendHeader.ReserveTwo6,
                                                            (byte)_sendHeader.ReserveTwo7,
                                                            (byte)_sendHeader.ReserveTwo8,

                                                            (byte)_sendHeader.CommandNumber1,
                                                            (byte)_sendHeader.CommandNumber2,
                                                            (byte)_sendHeader.Instance1,
                                                            (byte)_sendHeader.Instance2,

                                                            (byte)_sendHeader.Attribute,
                                                            (byte)_sendHeader.Service,
                                                            (byte)_sendHeader.Padding1,
                                                            (byte)_sendHeader.Padding2 };
                #endregion


                for (int j = 0; j < 32; j++)
                {
                    //Header Part Filling.
                    sendPackage[j] = sendingheaderPartArray[j];
                }

                try
                {
                    int bytesTransferred = client.Send(sendPackage);
                    int bytesReceived = client.Receive(receivedPackage);
                }
                catch (Exception)
                {
                    Console.WriteLine("ReadAlarmHistoryData komutu paket gönderim hatası");
                    return;
                }

                fillReceivedDataHeader(_receiveHeader, receivedPackage);

                if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
                {
                    client.Close();

                    Console.WriteLine("ReadAlarmHistoryData komutu iletişim hatası");
                    return;
                }
                else
                {
                    if (a <= 3)
                    {
                        string HexDegeri = $"{receivedPackage[35].ToString("X2")}{receivedPackage[34].ToString("X2")}{receivedPackage[33].ToString("X2")}{receivedPackage[32].ToString("X2")}";

                        uint decValue = uint.Parse(HexDegeri, System.Globalization.NumberStyles.HexNumber);

                        Cevap_F[a - 1] = decValue;

                        AD.AlarmCode = Cevap_F[0];
                        AD.AlarmData = Cevap_F[1];
                        AD.AlarmType = Cevap_F[2];
                    }

                    if (a > 3 && a <= 8)
                    {
                        int Bitis = 0;

                        if (a == 4 || a == 6)
                        {
                            Bitis = 48;
                        }
                        else if (a == 5)
                        {
                            Bitis = 64;
                        }
                        else if (a == 7 || a == 8)
                        {
                            Bitis = 128;
                        }

                        string hexifade = "";

                        for (int j = 32; j < Bitis; j++)
                        {
                            hexifade = hexifade + receivedPackage[j].ToString("X2");
                        }

                        string hexString = hexifade; // Hexadecimal string
                        byte[] bytes = new byte[hexString.Length / 2];

                        for (int i = 0; i < hexString.Length; i += 2)
                        {
                            bytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
                        }
                        string text = System.Text.Encoding.UTF8.GetString(bytes).TrimEnd('\0');
                        //string text = System.Text.Encoding.UTF8.GetString(bytes);

                        Cevap_4_5[a - 4] = text;

                        AD.AlarmTime = Cevap_4_5[0];
                        AD.AlarmName = Cevap_4_5[1];


                        AD.SubCodeDataAdditionalInfo = Cevap_4_5[2];
                        AD.SubCodeDataCharacterStrings = Cevap_4_5[3];
                        AD.SubCodeDataCharacterStringsRev = Cevap_4_5[4];
                    }



                }
            }

            client.Close();
        }

        public void ReadAlarmData(int AlarmNumber, out Alarm_Data AD)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();

            AD = new Alarm_Data();

            uint[] Cevap_F = new uint[3];
            string[] Cevap_4_5 = new string[2];


            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            for (int a = 1; a <= 5; a++)
            {
                byte[] sendPackage = new byte[32];
                byte[] receivedPackage = new byte[750];


                // Decimal değerini byte dizisine dönüştürelim
                byte[] bytes1 = Decimal.GetBits(AlarmNumber).SelectMany(BitConverter.GetBytes).ToArray();

                // Byte dizisini hexadecimal olarak çevirelim
                string hexValue1 = BitConverter.ToString(bytes1).Replace("-", "");

                // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
                string[] hexArray1 = new string[hexValue1.Length / 2];
                for (int i = 0; i < hexArray1.Length; i++)
                {
                    hexArray1[i] = hexValue1.Substring(i * 2, 2);
                }

                _sendHeader.DataPartSize1 = 0x00;
                _sendHeader.DataPartSize2 = 0x00;
                _sendHeader.RequestID = 0x00;
                _sendHeader.CommandNumber1 = 0x70;
                _sendHeader.CommandNumber2 = 0x00;
                _sendHeader.Instance1 = Convert.ToUInt16(hexArray1[0], 16);
                _sendHeader.Instance2 = Convert.ToUInt16(hexArray1[1], 16);
                _sendHeader.Attribute = Convert.ToUInt16(a);
                _sendHeader.Service = 0x0E;

                #region sendingHeaderArray...

                byte[] sendingheaderPartArray = new byte[32] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                            (byte)_sendHeader.HeaderPartSize1,
                                                            (byte)_sendHeader.HeaderPartSize2,
                                                            (byte)_sendHeader.DataPartSize1,
                                                            (byte)_sendHeader.DataPartSize2,

                                                            (byte)_sendHeader.ReserveOne,
                                                            (byte)_sendHeader.ProcessingDivision,
                                                            (byte)_sendHeader.ACK,
                                                            (byte)_sendHeader.RequestID,

                                                            (byte)_sendHeader.BlockNumber1,
                                                            (byte)_sendHeader.BlockNumber2,
                                                            (byte)_sendHeader.BlockNumber3,
                                                            (byte)_sendHeader.BlockNumber4,

                                                            (byte)_sendHeader.ReserveTwo1,
                                                            (byte)_sendHeader.ReserveTwo2,
                                                            (byte)_sendHeader.ReserveTwo3,
                                                            (byte)_sendHeader.ReserveTwo4,

                                                            (byte)_sendHeader.ReserveTwo5,
                                                            (byte)_sendHeader.ReserveTwo6,
                                                            (byte)_sendHeader.ReserveTwo7,
                                                            (byte)_sendHeader.ReserveTwo8,

                                                            (byte)_sendHeader.CommandNumber1,
                                                            (byte)_sendHeader.CommandNumber2,
                                                            (byte)_sendHeader.Instance1,
                                                            (byte)_sendHeader.Instance2,

                                                            (byte)_sendHeader.Attribute,
                                                            (byte)_sendHeader.Service,
                                                            (byte)_sendHeader.Padding1,
                                                            (byte)_sendHeader.Padding2 };
                #endregion


                for (int j = 0; j < 32; j++)
                {
                    //Header Part Filling.
                    sendPackage[j] = sendingheaderPartArray[j];
                }

                try
                {
                    int bytesTransferred = client.Send(sendPackage);
                    int bytesReceived = client.Receive(receivedPackage);
                }
                catch (Exception)
                {
                    Console.WriteLine("ReadAlarmData komutu paket gönderim hatası");

                    return;
                }

                fillReceivedDataHeader(_receiveHeader, receivedPackage);

                if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
                {
                    client.Close();

                    Console.WriteLine("ReadAlarmData komutu iletişim hatası");

                    return;
                }
                else
                {
                    if (a <= 3)
                    {
                        string HexDegeri = $"{receivedPackage[35].ToString("X2")}{receivedPackage[34].ToString("X2")}{receivedPackage[33].ToString("X2")}{receivedPackage[32].ToString("X2")}";

                        uint decValue = uint.Parse(HexDegeri, System.Globalization.NumberStyles.HexNumber);

                        Cevap_F[a - 1] = decValue;

                        AD.AlarmCode = Cevap_F[0];
                        AD.AlarmData = Cevap_F[1];
                        AD.AlarmType = Cevap_F[2];
                    }

                    if (a == 4 || a == 5)
                    {
                        int Bitis = 0;

                        if (a == 4)
                        {
                            Bitis = 48;
                        }
                        else if (a == 5)
                        {
                            Bitis = 64;
                        }

                        string hexifade = "";

                        for (int j = 32; j < Bitis; j++)
                        {
                            hexifade = hexifade + receivedPackage[j].ToString("X2");
                        }

                        string hexString = hexifade; // Hexadecimal string
                        byte[] bytes = new byte[hexString.Length / 2];

                        for (int i = 0; i < hexString.Length; i += 2)
                        {
                            bytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
                        }

                        //hexifade = hexifade.TrimStart('0').TrimEnd('0');

                        //// Kırpılmış hexadecimal dizeyi byte dizisine dönüştürme
                        //int byteCount = hexifade.Length / 2;
                        //byte[] bytes = new byte[byteCount];

                        //for (int i = 0; i < hexifade.Length; i += 2)
                        //{
                        //    bytes[i / 2] = Convert.ToByte(hexifade.Substring(i, 2), 16);
                        //}

                        //string text = System.Text.Encoding.UTF8.GetString(bytes);
                        string text = System.Text.Encoding.UTF8.GetString(bytes).TrimEnd('\0');

                        Cevap_4_5[a - 4] = text;

                        AD.AlarmTime = Cevap_4_5[0];
                        AD.AlarmName = Cevap_4_5[1];
                    }
                }
            }

            client.Close();
        }
        public void ReadAlarmData(int AlarmNumber, out Alarm_Data_Sub AD)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();

            AD = new Alarm_Data_Sub();

            uint[] Cevap_F = new uint[3];
            string[] Cevap_4_5 = new string[5];

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            for (int a = 1; a <= 8; a++)
            {


                byte[] sendPackage = new byte[32];
                byte[] receivedPackage = new byte[750];


                // Decimal değerini byte dizisine dönüştürelim
                byte[] bytes1 = Decimal.GetBits(AlarmNumber).SelectMany(BitConverter.GetBytes).ToArray();

                // Byte dizisini hexadecimal olarak çevirelim
                string hexValue1 = BitConverter.ToString(bytes1).Replace("-", "");

                // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
                string[] hexArray1 = new string[hexValue1.Length / 2];
                for (int i = 0; i < hexArray1.Length; i++)
                {
                    hexArray1[i] = hexValue1.Substring(i * 2, 2);
                }

                _sendHeader.DataPartSize1 = 0x00;
                _sendHeader.DataPartSize2 = 0x00;
                _sendHeader.RequestID = 0x00;
                _sendHeader.CommandNumber1 = 0x0A;
                _sendHeader.CommandNumber2 = 0x03;
                _sendHeader.Instance1 = Convert.ToUInt16(hexArray1[0], 16);
                _sendHeader.Instance2 = Convert.ToUInt16(hexArray1[1], 16);
                _sendHeader.Attribute = Convert.ToUInt16(a);
                _sendHeader.Service = 0x0E;

                #region sendingHeaderArray...

                byte[] sendingheaderPartArray = new byte[32] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                            (byte)_sendHeader.HeaderPartSize1,
                                                            (byte)_sendHeader.HeaderPartSize2,
                                                            (byte)_sendHeader.DataPartSize1,
                                                            (byte)_sendHeader.DataPartSize2,

                                                            (byte)_sendHeader.ReserveOne,
                                                            (byte)_sendHeader.ProcessingDivision,
                                                            (byte)_sendHeader.ACK,
                                                            (byte)_sendHeader.RequestID,

                                                            (byte)_sendHeader.BlockNumber1,
                                                            (byte)_sendHeader.BlockNumber2,
                                                            (byte)_sendHeader.BlockNumber3,
                                                            (byte)_sendHeader.BlockNumber4,

                                                            (byte)_sendHeader.ReserveTwo1,
                                                            (byte)_sendHeader.ReserveTwo2,
                                                            (byte)_sendHeader.ReserveTwo3,
                                                            (byte)_sendHeader.ReserveTwo4,

                                                            (byte)_sendHeader.ReserveTwo5,
                                                            (byte)_sendHeader.ReserveTwo6,
                                                            (byte)_sendHeader.ReserveTwo7,
                                                            (byte)_sendHeader.ReserveTwo8,

                                                            (byte)_sendHeader.CommandNumber1,
                                                            (byte)_sendHeader.CommandNumber2,
                                                            (byte)_sendHeader.Instance1,
                                                            (byte)_sendHeader.Instance2,

                                                            (byte)_sendHeader.Attribute,
                                                            (byte)_sendHeader.Service,
                                                            (byte)_sendHeader.Padding1,
                                                            (byte)_sendHeader.Padding2 };
                #endregion


                for (int j = 0; j < 32; j++)
                {
                    //Header Part Filling.
                    sendPackage[j] = sendingheaderPartArray[j];
                }

                try
                {
                    int bytesTransferred = client.Send(sendPackage);
                    int bytesReceived = client.Receive(receivedPackage);
                }
                catch (Exception)
                {
                    Console.WriteLine("ReadAlarmData komutu paket gönderim hatası");
                    return;
                }

                fillReceivedDataHeader(_receiveHeader, receivedPackage);

                if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
                {
                    client.Close();

                    Console.WriteLine("ReadAlarmData komutu iletişim hatası");
                    return;
                }
                else
                {
                    if (a <= 3)
                    {
                        string HexDegeri = $"{receivedPackage[35].ToString("X2")}{receivedPackage[34].ToString("X2")}{receivedPackage[33].ToString("X2")}{receivedPackage[32].ToString("X2")}";

                        uint decValue = uint.Parse(HexDegeri, System.Globalization.NumberStyles.HexNumber);

                        Cevap_F[a - 1] = decValue;

                        AD.AlarmCode = Cevap_F[0];
                        AD.AlarmData = Cevap_F[1];
                        AD.AlarmType = Cevap_F[2];
                    }

                    if (a > 3 && a <= 8)
                    {
                        int Bitis = 0;

                        if (a == 4 || a == 6)
                        {
                            Bitis = 48;
                        }
                        else if (a == 5)
                        {
                            Bitis = 64;
                        }
                        else if (a == 7 || a == 8)
                        {
                            Bitis = 128;
                        }

                        string hexifade = "";

                        for (int j = 32; j < Bitis; j++)
                        {
                            hexifade = hexifade + receivedPackage[j].ToString("X2");
                        }

                        string hexString = hexifade; // Hexadecimal string
                        byte[] bytes = new byte[hexString.Length / 2];

                        for (int i = 0; i < hexString.Length; i += 2)
                        {
                            bytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
                        }

                        //hexifade = hexifade.TrimStart('0').TrimEnd('0');

                        //// Kırpılmış hexadecimal dizeyi byte dizisine dönüştürme
                        //int byteCount = hexifade.Length / 2;
                        //byte[] bytes = new byte[byteCount];

                        //for (int i = 0; i < hexifade.Length; i += 2)
                        //{
                        //    bytes[i / 2] = Convert.ToByte(hexifade.Substring(i, 2), 16);
                        //}

                        //string text = System.Text.Encoding.UTF8.GetString(bytes);
                        string text = System.Text.Encoding.UTF8.GetString(bytes).TrimEnd('\0');

                        Cevap_4_5[a - 4] = text;

                        AD.AlarmTime = Cevap_4_5[0];
                        AD.AlarmName = Cevap_4_5[1];


                        AD.SubCodeDataAdditionalInfo = Cevap_4_5[2];
                        AD.SubCodeDataCharacterStrings = Cevap_4_5[3];
                        AD.SubCodeDataCharacterStringsRev = Cevap_4_5[4];
                    }



                }
            }

            client.Close();
        }


        public void StatusInformationRead(out Status_Information AD)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();
            AD = new Status_Information();
            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.


            byte[] sendPackage = new byte[32];
            byte[] receivedPackage = new byte[750];

            _sendHeader.DataPartSize1 = 0x00;
            _sendHeader.DataPartSize2 = 0x00;
            _sendHeader.RequestID = 0x00;
            _sendHeader.CommandNumber1 = 0x72;
            _sendHeader.CommandNumber2 = 0x00;
            _sendHeader.Instance1 = 1;
            _sendHeader.Attribute = 0x00;
            _sendHeader.Service = 0x01;

            #region sendingHeaderArray...

            byte[] sendingheaderPartArray = new byte[32] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                            (byte)_sendHeader.HeaderPartSize1,
                                                            (byte)_sendHeader.HeaderPartSize2,
                                                            (byte)_sendHeader.DataPartSize1,
                                                            (byte)_sendHeader.DataPartSize2,

                                                            (byte)_sendHeader.ReserveOne,
                                                            (byte)_sendHeader.ProcessingDivision,
                                                            (byte)_sendHeader.ACK,
                                                            (byte)_sendHeader.RequestID,

                                                            (byte)_sendHeader.BlockNumber1,
                                                            (byte)_sendHeader.BlockNumber2,
                                                            (byte)_sendHeader.BlockNumber3,
                                                            (byte)_sendHeader.BlockNumber4,

                                                            (byte)_sendHeader.ReserveTwo1,
                                                            (byte)_sendHeader.ReserveTwo2,
                                                            (byte)_sendHeader.ReserveTwo3,
                                                            (byte)_sendHeader.ReserveTwo4,

                                                            (byte)_sendHeader.ReserveTwo5,
                                                            (byte)_sendHeader.ReserveTwo6,
                                                            (byte)_sendHeader.ReserveTwo7,
                                                            (byte)_sendHeader.ReserveTwo8,

                                                            (byte)_sendHeader.CommandNumber1,
                                                            (byte)_sendHeader.CommandNumber2,
                                                            (byte)_sendHeader.Instance1,
                                                            (byte)_sendHeader.Instance2,

                                                            (byte)_sendHeader.Attribute,
                                                            (byte)_sendHeader.Service,
                                                            (byte)_sendHeader.Padding1,
                                                            (byte)_sendHeader.Padding2 };
            #endregion


            for (int j = 0; j < 32; j++)
            {
                //Header Part Filling.
                sendPackage[j] = sendingheaderPartArray[j];
            }

            try
            {
                int bytesTransferred = client.Send(sendPackage);
                int bytesReceived = client.Receive(receivedPackage);
            }
            catch (Exception)
            {
                Console.WriteLine("StatusInformationRead komutu paket gönderim hatası");
                return;
            }

            fillReceivedDataHeader(_receiveHeader, receivedPackage);

            if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
            {
                client.Close();

                Console.WriteLine("StatusInformationRead komutu iletişim hatası");
                return;
            }
            else
            {
                string HexDegeri = $"{receivedPackage[35].ToString("X2")}{receivedPackage[34].ToString("X2")}{receivedPackage[33].ToString("X2")}{receivedPackage[32].ToString("X2")}";

                byte[] binaryArray = HexToBinary(HexDegeri);

                AD.Step = Convert.ToBoolean(binaryArray[7]);
                AD.Cycle1 = Convert.ToBoolean(binaryArray[6]);
                AD.AutomaticAndContinous = Convert.ToBoolean(binaryArray[5]);
                AD.Running = Convert.ToBoolean(binaryArray[4]);
                AD.InGuardSafeOperation = Convert.ToBoolean(binaryArray[3]);
                AD.Teach = Convert.ToBoolean(binaryArray[2]);
                AD.Play = Convert.ToBoolean(binaryArray[1]);
                AD.CommandRemote = Convert.ToBoolean(binaryArray[0]);

                string HexDegeri1 = $"{receivedPackage[39].ToString("X2")}{receivedPackage[38].ToString("X2")}{receivedPackage[37].ToString("X2")}{receivedPackage[36].ToString("X2")}";

                byte[] binaryArray1 = HexToBinary(HexDegeri1);

                AD.InHoldStatusByPP = Convert.ToBoolean(binaryArray1[6]);
                AD.InHoldStatusExt = Convert.ToBoolean(binaryArray1[5]);
                AD.InHoldStatusCmd = Convert.ToBoolean(binaryArray1[4]);
                AD.Alarming = Convert.ToBoolean(binaryArray1[3]);
                AD.ErrorOccuring = Convert.ToBoolean(binaryArray1[2]);
                AD.ServoON = Convert.ToBoolean(binaryArray1[1]);

            }

            client.Close();
        }

        public void P_Read(int VariableNumber, out RobotPositionData Data)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();
            Data = new RobotPositionData();

            uint[] Cevap = new uint[4];
            int[] Eksenler = new int[8];

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            for (int i = 1; i < 14; i++)
            {


                byte[] sendPackage = new byte[32];
                byte[] receivedPackage = new byte[750];

                _sendHeader.DataPartSize1 = 0x00;
                _sendHeader.DataPartSize2 = 0x00;
                _sendHeader.RequestID = 0x00;
                _sendHeader.CommandNumber1 = 0x7F;
                _sendHeader.CommandNumber2 = 0x00;
                _sendHeader.Instance1 = Convert.ToUInt16(VariableNumber);
                _sendHeader.Attribute = Convert.ToUInt16(i);
                _sendHeader.Service = 0x01;

                #region sendingHeaderArray...

                byte[] sendingheaderPartArray = new byte[32] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                            (byte)_sendHeader.HeaderPartSize1,
                                                            (byte)_sendHeader.HeaderPartSize2,
                                                            (byte)_sendHeader.DataPartSize1,
                                                            (byte)_sendHeader.DataPartSize2,

                                                            (byte)_sendHeader.ReserveOne,
                                                            (byte)_sendHeader.ProcessingDivision,
                                                            (byte)_sendHeader.ACK,
                                                            (byte)_sendHeader.RequestID,

                                                            (byte)_sendHeader.BlockNumber1,
                                                            (byte)_sendHeader.BlockNumber2,
                                                            (byte)_sendHeader.BlockNumber3,
                                                            (byte)_sendHeader.BlockNumber4,

                                                            (byte)_sendHeader.ReserveTwo1,
                                                            (byte)_sendHeader.ReserveTwo2,
                                                            (byte)_sendHeader.ReserveTwo3,
                                                            (byte)_sendHeader.ReserveTwo4,

                                                            (byte)_sendHeader.ReserveTwo5,
                                                            (byte)_sendHeader.ReserveTwo6,
                                                            (byte)_sendHeader.ReserveTwo7,
                                                            (byte)_sendHeader.ReserveTwo8,

                                                            (byte)_sendHeader.CommandNumber1,
                                                            (byte)_sendHeader.CommandNumber2,
                                                            (byte)_sendHeader.Instance1,
                                                            (byte)_sendHeader.Instance2,

                                                            (byte)_sendHeader.Attribute,
                                                            (byte)_sendHeader.Service,
                                                            (byte)_sendHeader.Padding1,
                                                            (byte)_sendHeader.Padding2 };
                #endregion


                for (int j = 0; j < 32; j++)
                {
                    //Header Part Filling.
                    sendPackage[j] = sendingheaderPartArray[j];
                }

                try
                {
                    int bytesTransferred = client.Send(sendPackage);
                    int bytesReceived = client.Receive(receivedPackage);
                }
                catch (Exception)
                {
                    Console.WriteLine("P_Read komutu paket gönderim hatası");
                    return;
                }

                fillReceivedDataHeader(_receiveHeader, receivedPackage);

                if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
                {
                    client.Close();

                    Console.WriteLine("P_Read komutu iletişim hatası");
                    return;
                }
                else
                {
                    if (i <= 4 && i != 2)
                    {
                        string HexIfade = $"{receivedPackage[35].ToString("X2")}{receivedPackage[34].ToString("X2")}{receivedPackage[33].ToString("X2")}{receivedPackage[32].ToString("X2")}";

                        int decValue = int.Parse(HexIfade, System.Globalization.NumberStyles.HexNumber);

                        Cevap[i - 1] = Convert.ToUInt16(decValue);

                        Data.DataType = Cevap[0];
                        Data.ToolNumber = Cevap[2];
                        Data.UserCoordinateNumber = Cevap[3];
                    }

                    if (i == 2)
                    {
                        string HexDegeri = $"{receivedPackage[35].ToString("X2")}{receivedPackage[34].ToString("X2")}{receivedPackage[33].ToString("X2")}{receivedPackage[32].ToString("X2")}";

                        byte[] binaryArray = HexToBinary(HexDegeri);

                        Data.FrontBack = Convert.ToInt32(binaryArray[7]);
                        Data.Arm = Convert.ToInt32(binaryArray[6]);
                        Data.Flip = Convert.ToInt32(binaryArray[5]);
                        Data.R180 = Convert.ToInt32(binaryArray[4]);
                        Data.T180 = Convert.ToInt32(binaryArray[3]);
                        Data.S180 = Convert.ToInt32(binaryArray[2]);
                        Data.Redundant = Convert.ToInt32(binaryArray[1]);
                        Data.Conversion = Convert.ToInt32(binaryArray[0]);
                    }

                    if (i == 5)
                    {
                        string HexDegeri = $"{receivedPackage[35].ToString("X2")}{receivedPackage[34].ToString("X2")}{receivedPackage[33].ToString("X2")}{receivedPackage[32].ToString("X2")}";

                        byte[] binaryArray = HexToBinary(HexDegeri);

                        Data.L180 = Convert.ToInt32(binaryArray[7]);
                        Data.U180 = Convert.ToInt32(binaryArray[6]);
                        Data.B180 = Convert.ToInt32(binaryArray[5]);
                        Data.E180 = Convert.ToInt32(binaryArray[4]);
                        Data.W180 = Convert.ToInt32(binaryArray[3]);
                    }

                    if (i <= 14 && i >= 6)
                    {

                        string HexDegeri = $"{receivedPackage[35].ToString("X2")}{receivedPackage[34].ToString("X2")}{receivedPackage[33].ToString("X2")}{receivedPackage[32].ToString("X2")}";

                        int decValue = int.Parse(HexDegeri, System.Globalization.NumberStyles.HexNumber);

                        Eksenler[i - 6] = decValue;

                        Data.AxisData = Eksenler;

                    }
                }
            }
            client.Close();
        }
        public void P_Read(int VariableNumber, int Count, out RobotPositionData[] Data)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();
            Data = new RobotPositionData[Count];

            /*
            RPD[0] = new RobotPositionData();
            RPD[1] = new RobotPositionData();
            */

            for (int i = 0; i < Data.Length; i++)
            {
                Data[i] = new RobotPositionData();
            }

            // uint[] Cevap = new uint[4];
            // int[] Eksenler = new int[8];

            uint[] Cevap_1_3_4 = new uint[Count * 13];
            int[] Eksen = new int[Count * 13];
            int[] Form_Expendat = new int[Count * 13];

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            for (int a = 0; a < Count; a++)
            {
                byte[] sendPackage = new byte[36];
                byte[] receivedPackage = new byte[750];

                _sendHeader.DataPartSize1 = 0x04;
                _sendHeader.DataPartSize2 = 0x00;
                _sendHeader.RequestID = 0x00;
                _sendHeader.CommandNumber1 = 0x07;
                _sendHeader.CommandNumber2 = 0x03;
                _sendHeader.Instance1 = Convert.ToUInt16(VariableNumber);
                _sendHeader.Attribute = 0;
                _sendHeader.Service = 0x33;


                // Decimal değerini byte dizisine dönüştürelim
                byte[] bytes = Decimal.GetBits(Count).SelectMany(BitConverter.GetBytes).ToArray();

                // Byte dizisini hexadecimal olarak çevirelim
                string hexValue = BitConverter.ToString(bytes).Replace("-", "");

                // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
                string[] hexArray = new string[hexValue.Length / 2];
                for (int x = 0; x < hexArray.Length; x++)
                {
                    hexArray[x] = hexValue.Substring(x * 2, 2);
                }

                // Convert hex string to UInt16
                _sendHeader.DataByte1 = Convert.ToUInt16(hexArray[0], 16);
                _sendHeader.DataByte2 = Convert.ToUInt16(hexArray[1], 16);
                _sendHeader.DataByte3 = Convert.ToUInt16(hexArray[2], 16);
                _sendHeader.DataByte4 = Convert.ToUInt16(hexArray[3], 16);

                #region sendingHeaderArray...

                byte[] sendingheaderPartArray = new byte[36] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                            (byte)_sendHeader.HeaderPartSize1,
                                                            (byte)_sendHeader.HeaderPartSize2,
                                                            (byte)_sendHeader.DataPartSize1,
                                                            (byte)_sendHeader.DataPartSize2,

                                                            (byte)_sendHeader.ReserveOne,
                                                            (byte)_sendHeader.ProcessingDivision,
                                                            (byte)_sendHeader.ACK,
                                                            (byte)_sendHeader.RequestID,

                                                            (byte)_sendHeader.BlockNumber1,
                                                            (byte)_sendHeader.BlockNumber2,
                                                            (byte)_sendHeader.BlockNumber3,
                                                            (byte)_sendHeader.BlockNumber4,

                                                            (byte)_sendHeader.ReserveTwo1,
                                                            (byte)_sendHeader.ReserveTwo2,
                                                            (byte)_sendHeader.ReserveTwo3,
                                                            (byte)_sendHeader.ReserveTwo4,

                                                            (byte)_sendHeader.ReserveTwo5,
                                                            (byte)_sendHeader.ReserveTwo6,
                                                            (byte)_sendHeader.ReserveTwo7,
                                                            (byte)_sendHeader.ReserveTwo8,

                                                            (byte)_sendHeader.CommandNumber1,
                                                            (byte)_sendHeader.CommandNumber2,
                                                            (byte)_sendHeader.Instance1,
                                                            (byte)_sendHeader.Instance2,

                                                            (byte)_sendHeader.Attribute,
                                                            (byte)_sendHeader.Service,
                                                            (byte)_sendHeader.Padding1,
                                                            (byte)_sendHeader.Padding2,

                                                            (byte)_sendHeader.DataByte1,
                                                            (byte)_sendHeader.DataByte2,
                                                            (byte)_sendHeader.DataByte3,
                                                            (byte)_sendHeader.DataByte4 };
                #endregion


                for (int j = 0; j < 36; j++)
                {
                    //Header Part Filling.
                    sendPackage[j] = sendingheaderPartArray[j];
                }

                try
                {
                    int bytesTransferred = client.Send(sendPackage);
                    int bytesReceived = client.Receive(receivedPackage);
                }
                catch (Exception)
                {
                    Console.WriteLine("P_Read komutu paket gönderim hatası");
                    return;
                }

                fillReceivedDataHeader(_receiveHeader, receivedPackage);

                if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
                {
                    client.Close();

                    Console.WriteLine("P_Read komutu iletişim hatası");
                    return;
                }
                else
                {
                    for (int i = 1; i <= 13; i++)
                    {
                        if (i <= 4 && i != 2)
                        {
                            string HexIfade = $"{receivedPackage[(a * 52) + (35 + (i * 4))].ToString("X2")}{receivedPackage[(a * 52) + (34 + (i * 4))].ToString("X2")}{receivedPackage[(a * 52) + (33 + (i * 4))].ToString("X2")}{receivedPackage[(a * 52) + (32 + (i * 4))].ToString("X2")}";

                            int decValue = int.Parse(HexIfade, System.Globalization.NumberStyles.HexNumber);

                            Cevap_1_3_4[i] = Convert.ToUInt16(decValue);

                            Data[a].DataType = Cevap_1_3_4[1];
                            Data[a].ToolNumber = Cevap_1_3_4[3];
                            Data[a].UserCoordinateNumber = Cevap_1_3_4[4];
                        }


                        if (i == 2)
                        {
                            string HexDegeri = $"{receivedPackage[(a * 52) + (35 + (i * 4))].ToString("X2")}{receivedPackage[(a * 52) + (34 + (i * 4))].ToString("X2")}{receivedPackage[(a * 52) + (33 + (i * 4))].ToString("X2")}{receivedPackage[(a * 52) + (32 + (i * 4))].ToString("X2")}";

                            byte[] binaryArray = HexToBinary(HexDegeri);

                            Data[a].FrontBack = Convert.ToInt32(binaryArray[7]);
                            Data[a].Arm = Convert.ToInt32(binaryArray[6]);
                            Data[a].Flip = Convert.ToInt32(binaryArray[5]);
                            Data[a].R180 = Convert.ToInt32(binaryArray[4]);
                            Data[a].T180 = Convert.ToInt32(binaryArray[3]);
                            Data[a].S180 = Convert.ToInt32(binaryArray[2]);
                            Data[a].Redundant = Convert.ToInt32(binaryArray[1]);
                            Data[a].Conversion = Convert.ToInt32(binaryArray[0]);
                        }

                        if (i == 5)
                        {
                            string HexDegeri = $"{receivedPackage[(a * 52) + (35 + (i * 4))].ToString("X2")}{receivedPackage[(a * 52) + (34 + (i * 4))].ToString("X2")}{receivedPackage[(a * 52) + (33 + (i * 4))].ToString("X2")}{receivedPackage[(a * 52) + (32 + (i * 4))].ToString("X2")}";

                            byte[] binaryArray = HexToBinary(HexDegeri);

                            Data[a].L180 = Convert.ToInt32(binaryArray[7]);
                            Data[a].U180 = Convert.ToInt32(binaryArray[6]);
                            Data[a].B180 = Convert.ToInt32(binaryArray[5]);
                            Data[a].E180 = Convert.ToInt32(binaryArray[4]);
                            Data[a].W180 = Convert.ToInt32(binaryArray[3]);
                        }

                        if (i <= 13 && i >= 6)
                        {
                            string HexDegeri = $"{receivedPackage[(a * 52) + (35 + (i * 4))].ToString("X2")}{receivedPackage[(a * 52) + (34 + (i * 4))].ToString("X2")}{receivedPackage[(a * 52) + (33 + (i * 4))].ToString("X2")}{receivedPackage[(a * 52) + (32 + (i * 4))].ToString("X2")}";

                            int decValue = int.Parse(HexDegeri, System.Globalization.NumberStyles.HexNumber);

                            Eksen[i - 6] = decValue;

                            Data[a].AxisData[i - 6] = decValue;
                        }
                    }
                }
            }
            client.Close();
        }
        public void P_Write(int VariableNumber, RobotPositionData Data)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();

            byte[] bytes = { };

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            for (int a = 1; a <= 13; a++)
            {
                byte[] sendPackage = new byte[36];
                byte[] receivedPackage = new byte[750];

                _sendHeader.DataPartSize1 = 0x04;
                _sendHeader.DataPartSize2 = 0x00;
                _sendHeader.RequestID = 0x01;
                _sendHeader.CommandNumber1 = 0x7F;
                _sendHeader.CommandNumber2 = 0x00;
                _sendHeader.Instance1 = Convert.ToUInt16(VariableNumber);
                _sendHeader.Attribute = Convert.ToUInt16(a);
                _sendHeader.Service = 0x02;


                if ((Data.FrontBack == 0 || Data.FrontBack == 1) && (Data.Arm == 0 || Data.Arm == 1) && (Data.R180 == 0 || Data.R180 == 1) && (Data.T180 == 0 || Data.T180 == 1) && (Data.S180 == 0 || Data.S180 == 1) && (Data.Redundant == 0 || Data.Redundant == 1) && (Data.Conversion == 0 || Data.Conversion == 1))
                {

                }
                else
                {
                    Console.WriteLine("FrontBack, Arm, R180, T180, S180, Redundant, Conversion hatalı girdi");
                    return;
                }

                if ((Data.L180 == 0 || Data.L180 == 1) && (Data.U180 == 0 || Data.U180 == 1) && (Data.B180 == 0 || Data.B180 == 1) && (Data.E180 == 0 || Data.E180 == 1) && (Data.W180 == 0 || Data.W180 == 1))
                {

                }
                else
                {
                    Console.WriteLine("L180, U180, B180, E180, W180 hatalı girdi");
                    return;
                }


                if (a == 1)
                {
                    bytes = Decimal.GetBits(Data.DataType).SelectMany(BitConverter.GetBytes).ToArray();
                }

                else if (a == 2)
                {
                    string BinaryValue = $"{Data.Conversion}{Data.Redundant}{Data.S180}{Data.T180}{Data.R180}{Data.Flip}{Data.Arm}{Data.FrontBack}";
                    int decimalDeger = Convert.ToInt32(BinaryValue, 2);

                    bytes = Decimal.GetBits(decimalDeger).SelectMany(BitConverter.GetBytes).ToArray();
                }
                else if (a == 3)
                {
                    bytes = Decimal.GetBits(Data.ToolNumber).SelectMany(BitConverter.GetBytes).ToArray();
                }
                else if (a == 4)
                {
                    bytes = Decimal.GetBits(Data.UserCoordinateNumber).SelectMany(BitConverter.GetBytes).ToArray();
                }
                else if (a == 5)
                {
                    string BinaryValue = $"{Data.W180}{Data.E180}{Data.B180}{Data.U180}{Data.L180}";
                    int decimalDeger = Convert.ToInt32(BinaryValue, 2);

                    bytes = Decimal.GetBits(decimalDeger).SelectMany(BitConverter.GetBytes).ToArray();
                }
                else if (a >= 6 && a <= 13)
                {
                    bytes = Decimal.GetBits(Data.AxisData[a - 6]).SelectMany(BitConverter.GetBytes).ToArray();
                }
                else
                {
                    Console.WriteLine("Veri iletimi hatası");
                }


                // Byte dizisini hexadecimal olarak çevirelim
                string hexValue = BitConverter.ToString(bytes).Replace("-", "");

                // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
                string[] hexArray = new string[hexValue.Length / 2];
                for (int i = 0; i < hexArray.Length; i++)
                {
                    hexArray[i] = hexValue.Substring(i * 2, 2);
                }

                // Convert hex string to UInt16
                _sendHeader.DataByte1 = Convert.ToUInt16(hexArray[0], 16);
                _sendHeader.DataByte2 = Convert.ToUInt16(hexArray[1], 16);
                _sendHeader.DataByte3 = Convert.ToUInt16(hexArray[2], 16);
                _sendHeader.DataByte4 = Convert.ToUInt16(hexArray[3], 16);


                #region sendingHeaderArray...

                byte[] sendingheaderPartArray = new byte[36] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                            (byte)_sendHeader.HeaderPartSize1,
                                                            (byte)_sendHeader.HeaderPartSize2,
                                                            (byte)_sendHeader.DataPartSize1,
                                                            (byte)_sendHeader.DataPartSize2,

                                                            (byte)_sendHeader.ReserveOne,
                                                            (byte)_sendHeader.ProcessingDivision,
                                                            (byte)_sendHeader.ACK,
                                                            (byte)_sendHeader.RequestID,

                                                            (byte)_sendHeader.BlockNumber1,
                                                            (byte)_sendHeader.BlockNumber2,
                                                            (byte)_sendHeader.BlockNumber3,
                                                            (byte)_sendHeader.BlockNumber4,

                                                            (byte)_sendHeader.ReserveTwo1,
                                                            (byte)_sendHeader.ReserveTwo2,
                                                            (byte)_sendHeader.ReserveTwo3,
                                                            (byte)_sendHeader.ReserveTwo4,

                                                            (byte)_sendHeader.ReserveTwo5,
                                                            (byte)_sendHeader.ReserveTwo6,
                                                            (byte)_sendHeader.ReserveTwo7,
                                                            (byte)_sendHeader.ReserveTwo8,

                                                            (byte)_sendHeader.CommandNumber1,
                                                            (byte)_sendHeader.CommandNumber2,
                                                            (byte)_sendHeader.Instance1,
                                                            (byte)_sendHeader.Instance2,

                                                            (byte)_sendHeader.Attribute,
                                                            (byte)_sendHeader.Service,
                                                            (byte)_sendHeader.Padding1,
                                                            (byte)_sendHeader.Padding2,

                                                            (byte)_sendHeader.DataByte1,
                                                            (byte)_sendHeader.DataByte2,
                                                            (byte)_sendHeader.DataByte3,
                                                            (byte)_sendHeader.DataByte4 };
                #endregion


                for (int j = 0; j < 36; j++)
                {
                    //Header Part Filling.
                    sendPackage[j] = sendingheaderPartArray[j];
                }

                try
                {
                    int bytesTransferred = client.Send(sendPackage);
                    int bytesReceived = client.Receive(receivedPackage);
                }
                catch (Exception)
                {
                    Console.WriteLine("P_Write komutu paket gönderim hatası");
                }

                fillReceivedDataHeader(_receiveHeader, receivedPackage);

                if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
                {
                    client.Close();

                    Console.WriteLine("P_Write komutu iletişim hatası");
                }
                else
                {
                    //client.Close();
                }
            }
            client.Close();
        }
        public void P_Write(int VariableNumber, int Count, RobotPositionData[] Data)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();

            MyData myData = new MyData(1000);

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            byte[] sendPackage = new byte[36 + (Count * 52)];
            byte[] receivedPackage = new byte[1000];

            #region DATAPART SİZE
            // Decimal değerini byte dizisine dönüştürelim
            byte[] bytes2 = Decimal.GetBits((Count * 52) + 4).SelectMany(BitConverter.GetBytes).ToArray();

            // Byte dizisini hexadecimal olarak çevirelim
            string hexValue2 = BitConverter.ToString(bytes2).Replace("-", "");

            // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
            string[] hexArray2 = new string[hexValue2.Length / 2];
            for (int i = 0; i < hexArray2.Length; i++)
            {
                hexArray2[i] = hexValue2.Substring(i * 2, 2);
            }
            #endregion

            // Convert hex string to UInt16
            _sendHeader.DataPartSize1 = Convert.ToUInt16(hexArray2[0], 16);
            _sendHeader.DataPartSize2 = Convert.ToUInt16(hexArray2[1], 16);
            _sendHeader.RequestID = 0x01;
            _sendHeader.CommandNumber1 = 0x07;
            _sendHeader.CommandNumber2 = 0x03;
            _sendHeader.Instance1 = Convert.ToUInt16(VariableNumber);
            _sendHeader.Attribute = 0;
            _sendHeader.Service = 0x34;

            #region KAÇ ADET VERİYE YAZILACAK

            // Decimal değerini byte dizisine dönüştürelim
            byte[] bytes_Veri = Decimal.GetBits(Count).SelectMany(BitConverter.GetBytes).ToArray();

            // Byte dizisini hexadecimal olarak çevirelim
            string hexValue_Veri = BitConverter.ToString(bytes_Veri).Replace("-", "");

            // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
            string[] hexArray_Veri = new string[hexValue_Veri.Length / 2];
            for (int i = 0; i < hexArray_Veri.Length; i++)
            {
                hexArray_Veri[i] = hexValue_Veri.Substring(i * 2, 2);
            }

            // Convert hex string to UInt16
            _sendHeader.DataByte1 = Convert.ToUInt16(hexArray_Veri[0], 16);
            _sendHeader.DataByte2 = Convert.ToUInt16(hexArray_Veri[1], 16);
            _sendHeader.DataByte3 = Convert.ToUInt16(hexArray_Veri[2], 16);
            _sendHeader.DataByte4 = Convert.ToUInt16(hexArray_Veri[3], 16);

            #endregion

            byte[] bytesDataType = { };
            byte[] bytesForm = { };
            byte[] bytesToolnumber = { };
            byte[] bytesUserCordinat = { };
            byte[] bytesExtendedform = { };
            byte[] bytesEksenler = { };

            for (int a = 0; a < Count; a++)
            {
                if ((Data[a].FrontBack == 0 || Data[a].FrontBack == 1) && (Data[a].Arm == 0 || Data[a].Arm == 1) && (Data[a].R180 == 0 || Data[a].R180 == 1) && (Data[a].T180 == 0 || Data[a].T180 == 1) && (Data[a].S180 == 0 || Data[a].S180 == 1) && (Data[a].Redundant == 0 || Data[a].Redundant == 1) && (Data[a].Conversion == 0 || Data[a].Conversion == 1))
                {

                }
                else
                {
                    Console.WriteLine("FrontBack, Arm, R180, T180, S180, Redundant, Conversion hatalı girdi");
                    return;
                }

                if ((Data[a].L180 == 0 || Data[a].L180 == 1) && (Data[a].U180 == 0 || Data[a].U180 == 1) && (Data[a].B180 == 0 || Data[a].B180 == 1) && (Data[a].E180 == 0 || Data[a].E180 == 1) && (Data[a].W180 == 0 || Data[a].W180 == 1))
                {

                }
                else
                {
                    Console.WriteLine("L180, U180, B180, E180, W180 hatalı girdi");
                    return;
                }

                bytesDataType = Decimal.GetBits(Data[a].DataType).SelectMany(BitConverter.GetBytes).ToArray();

                // Byte dizisini hexadecimal olarak çevirelim
                string hexValueDataType = BitConverter.ToString(bytesDataType).Replace("-", "");

                // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
                string[] hexArrayDataType = new string[hexValueDataType.Length / 2];
                for (int i = 0; i < hexArrayDataType.Length; i++)
                {
                    hexArrayDataType[i] = hexValueDataType.Substring(i * 2, 2);
                }
                for (int i = 0; i < 4; i++)
                {
                    myData[(a * 52) + i] = Convert.ToUInt16(hexArrayDataType[i], 16);
                }

                /////////////////////////

                string BinaryValueForm = $"{Data[a].Conversion}{Data[a].Redundant}{Data[a].S180}{Data[a].T180}{Data[a].R180}{Data[a].Flip}{Data[a].Arm}{Data[a].FrontBack}";
                int decimalDegerForm = Convert.ToInt32(BinaryValueForm, 2);

                bytesForm = Decimal.GetBits(decimalDegerForm).SelectMany(BitConverter.GetBytes).ToArray();

                // Byte dizisini hexadecimal olarak çevirelim
                string hexValueForm = BitConverter.ToString(bytesForm).Replace("-", "");

                // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
                string[] hexArrayForm = new string[hexValueForm.Length / 2];
                for (int i = 0; i < hexArrayForm.Length; i++)
                {
                    hexArrayForm[i] = hexValueForm.Substring(i * 2, 2);
                }

                for (int i = 0; i < 4; i++)
                {
                    myData[(a * 52) + (i + 4)] = Convert.ToUInt16(hexArrayForm[i], 16);
                }

                /////////////////////////

                bytesToolnumber = Decimal.GetBits(Data[a].ToolNumber).SelectMany(BitConverter.GetBytes).ToArray();

                // Byte dizisini hexadecimal olarak çevirelim
                string hexValueToolnumber = BitConverter.ToString(bytesToolnumber).Replace("-", "");

                // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
                string[] hexArrayToolnumber = new string[hexValueToolnumber.Length / 2];
                for (int i = 0; i < hexArrayToolnumber.Length; i++)
                {
                    hexArrayToolnumber[i] = hexValueToolnumber.Substring(i * 2, 2);
                }

                for (int i = 0; i < 4; i++)
                {
                    myData[(a * 52) + (i + 8)] = Convert.ToUInt16(hexArrayToolnumber[i], 16);
                }

                /////////////////////////

                bytesUserCordinat = Decimal.GetBits(Data[a].UserCoordinateNumber).SelectMany(BitConverter.GetBytes).ToArray();

                // Byte dizisini hexadecimal olarak çevirelim
                string hexValueUserCordinat = BitConverter.ToString(bytesUserCordinat).Replace("-", "");

                // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
                string[] hexArrayUserCordinat = new string[hexValueUserCordinat.Length / 2];
                for (int i = 0; i < hexArrayUserCordinat.Length; i++)
                {
                    hexArrayUserCordinat[i] = hexValueUserCordinat.Substring(i * 2, 2);
                }

                for (int i = 0; i < 4; i++)
                {
                    myData[(a * 52) + (i + 12)] = Convert.ToUInt16(hexArrayUserCordinat[i], 16);
                }

                /////////////////////////

                string BinaryValueExtendedform = $"{Data[a].W180}{Data[a].E180}{Data[a].B180}{Data[a].U180}{Data[a].L180}";
                int decimalDegerExtendedform = Convert.ToInt32(BinaryValueExtendedform, 2);

                bytesExtendedform = Decimal.GetBits(decimalDegerExtendedform).SelectMany(BitConverter.GetBytes).ToArray();

                // Byte dizisini hexadecimal olarak çevirelim
                string hexValueExtendedform = BitConverter.ToString(bytesExtendedform).Replace("-", "");

                // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
                string[] hexArrayExtendedform = new string[hexValueExtendedform.Length / 2];
                for (int i = 0; i < hexArrayExtendedform.Length; i++)
                {
                    hexArrayExtendedform[i] = hexValueExtendedform.Substring(i * 2, 2);
                }

                for (int i = 0; i < 4; i++)
                {
                    myData[(a * 52) + (i + 16)] = Convert.ToUInt16(hexArrayExtendedform[i], 16);
                }

                /////////////////////////

                for (int x = 0; x < 8; x++)
                {
                    bytesEksenler = Decimal.GetBits(Data[a].AxisData[x]).SelectMany(BitConverter.GetBytes).ToArray();

                    // Byte dizisini hexadecimal olarak çevirelim
                    string hexValueEksenler = BitConverter.ToString(bytesEksenler).Replace("-", "");

                    // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
                    string[] hexArrayEksenler = new string[hexValueEksenler.Length / 2];
                    for (int i = 0; i < hexArrayEksenler.Length; i++)
                    {
                        hexArrayEksenler[i] = hexValueEksenler.Substring(i * 2, 2);
                    }

                    for (int i = 0; i < 4; i++)
                    {
                        myData[(a * 52) + (i + (x * 4) + 20)] = Convert.ToUInt16(hexArrayEksenler[i], 16);
                    }
                }

                /////////////////////////
            }

            #region sendingHeaderArray...

            byte[] sendingheaderPartArray = new byte[36 + (Count * 52)];

            sendingheaderPartArray[0] = Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0];
            sendingheaderPartArray[1] = Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0];
            sendingheaderPartArray[2] = Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0];
            sendingheaderPartArray[3] = Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0];

            sendingheaderPartArray[4] = (byte)_sendHeader.HeaderPartSize1;
            sendingheaderPartArray[5] = (byte)_sendHeader.HeaderPartSize2;
            sendingheaderPartArray[6] = (byte)_sendHeader.DataPartSize1;
            sendingheaderPartArray[7] = (byte)_sendHeader.DataPartSize2;

            sendingheaderPartArray[8] = (byte)_sendHeader.ReserveOne;
            sendingheaderPartArray[9] = (byte)_sendHeader.ProcessingDivision;
            sendingheaderPartArray[10] = (byte)_sendHeader.ACK;
            sendingheaderPartArray[11] = (byte)_sendHeader.RequestID;

            sendingheaderPartArray[12] = (byte)_sendHeader.BlockNumber1;
            sendingheaderPartArray[13] = (byte)_sendHeader.BlockNumber2;
            sendingheaderPartArray[14] = (byte)_sendHeader.BlockNumber3;
            sendingheaderPartArray[15] = (byte)_sendHeader.BlockNumber4;

            sendingheaderPartArray[16] = (byte)_sendHeader.ReserveTwo1;
            sendingheaderPartArray[17] = (byte)_sendHeader.ReserveTwo2;
            sendingheaderPartArray[18] = (byte)_sendHeader.ReserveTwo3;
            sendingheaderPartArray[19] = (byte)_sendHeader.ReserveTwo4;

            sendingheaderPartArray[20] = (byte)_sendHeader.ReserveTwo5;
            sendingheaderPartArray[21] = (byte)_sendHeader.ReserveTwo6;
            sendingheaderPartArray[22] = (byte)_sendHeader.ReserveTwo7;
            sendingheaderPartArray[23] = (byte)_sendHeader.ReserveTwo8;

            sendingheaderPartArray[24] = (byte)_sendHeader.CommandNumber1;
            sendingheaderPartArray[25] = (byte)_sendHeader.CommandNumber2;
            sendingheaderPartArray[26] = (byte)_sendHeader.Instance1;
            sendingheaderPartArray[27] = (byte)_sendHeader.Instance2;

            sendingheaderPartArray[28] = (byte)_sendHeader.Attribute;
            sendingheaderPartArray[29] = (byte)_sendHeader.Service;
            sendingheaderPartArray[30] = (byte)_sendHeader.Padding1;
            sendingheaderPartArray[31] = (byte)_sendHeader.Padding2;

            sendingheaderPartArray[32] = (byte)_sendHeader.DataByte1;
            sendingheaderPartArray[33] = (byte)_sendHeader.DataByte2;
            sendingheaderPartArray[34] = (byte)_sendHeader.DataByte3;
            sendingheaderPartArray[35] = (byte)_sendHeader.DataByte4;

            #endregion

            for (int i = 0; i < (Count * 52); i++)
            {
                sendingheaderPartArray[36 + i] = (byte)myData[i];
            }

            for (int j = 0; j < 36 + (Count * 52); j++)
            {
                //Header Part Filling.
                sendPackage[j] = sendingheaderPartArray[j];
            }

            try
            {
                int bytesTransferred = client.Send(sendPackage);
                int bytesReceived = client.Receive(receivedPackage);
            }
            catch (Exception)
            {
                Console.WriteLine("P_Write komutu paket gönderim hatası");
            }

            fillReceivedDataHeader(_receiveHeader, receivedPackage);

            if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
            {
                client.Close();

                Console.WriteLine("P_Write komutu iletişim hatası");
            }
            else
            {
                client.Close();
            }
        }

        public void SetStepMode()
        {
            Step_Cycle_Auto(1, "SetStepMode");
        }
        public void SetCycleMode()
        {
            Step_Cycle_Auto(2, "SetCycleMode");
        }
        public void SetContinousMode()
        {
            Step_Cycle_Auto(3, "SetContinousMode");
        }

        public void AlarmReset()
        {
            Error_Alarm(1, "AlarmReset");
        }
        public void ErrorCancel()
        {
            Error_Alarm(2, "ErrorCancel");
        }

        public void ExecutingJobInformationRead(int VariableNumber, out ExecutingJobInfo EJI)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();

            EJI = new ExecutingJobInfo();

            uint[] Cevap = new uint[3];

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            for (int a = 1; a <= 4; a++)
            {
                byte[] sendPackage = new byte[32];
                byte[] receivedPackage = new byte[750];

                _sendHeader.DataPartSize1 = 0x00;
                _sendHeader.DataPartSize2 = 0x00;
                _sendHeader.RequestID = 0x00;
                _sendHeader.CommandNumber1 = 0x73;
                _sendHeader.CommandNumber2 = 0x00;
                _sendHeader.Instance1 = Convert.ToUInt16(VariableNumber);
                _sendHeader.Attribute = Convert.ToUInt16(a);
                _sendHeader.Service = 0x0E;

                #region sendingHeaderArray...

                byte[] sendingheaderPartArray = new byte[32] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                            (byte)_sendHeader.HeaderPartSize1,
                                                            (byte)_sendHeader.HeaderPartSize2,
                                                            (byte)_sendHeader.DataPartSize1,
                                                            (byte)_sendHeader.DataPartSize2,

                                                            (byte)_sendHeader.ReserveOne,
                                                            (byte)_sendHeader.ProcessingDivision,
                                                            (byte)_sendHeader.ACK,
                                                            (byte)_sendHeader.RequestID,

                                                            (byte)_sendHeader.BlockNumber1,
                                                            (byte)_sendHeader.BlockNumber2,
                                                            (byte)_sendHeader.BlockNumber3,
                                                            (byte)_sendHeader.BlockNumber4,

                                                            (byte)_sendHeader.ReserveTwo1,
                                                            (byte)_sendHeader.ReserveTwo2,
                                                            (byte)_sendHeader.ReserveTwo3,
                                                            (byte)_sendHeader.ReserveTwo4,

                                                            (byte)_sendHeader.ReserveTwo5,
                                                            (byte)_sendHeader.ReserveTwo6,
                                                            (byte)_sendHeader.ReserveTwo7,
                                                            (byte)_sendHeader.ReserveTwo8,

                                                            (byte)_sendHeader.CommandNumber1,
                                                            (byte)_sendHeader.CommandNumber2,
                                                            (byte)_sendHeader.Instance1,
                                                            (byte)_sendHeader.Instance2,

                                                            (byte)_sendHeader.Attribute,
                                                            (byte)_sendHeader.Service,
                                                            (byte)_sendHeader.Padding1,
                                                            (byte)_sendHeader.Padding2 };
                #endregion


                for (int j = 0; j < 32; j++)
                {
                    //Header Part Filling.
                    sendPackage[j] = sendingheaderPartArray[j];
                }

                try
                {
                    int bytesTransferred = client.Send(sendPackage);
                    int bytesReceived = client.Receive(receivedPackage);
                }
                catch (Exception)
                {
                    Console.WriteLine("ExecutingJobInformationRead komutu paket gönderim hatası");
                    return;
                }

                fillReceivedDataHeader(_receiveHeader, receivedPackage);

                if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
                {
                    client.Close();
                    Console.WriteLine("ExecutingJobInformationRead komutu iletişim hatası");
                }
                else
                {
                    if (a == 1)
                    {
                        string hexifade = "";

                        for (int j = 32; j < 65; j++)
                        {
                            hexifade = hexifade + receivedPackage[j].ToString("X2");
                        }

                        string hexString = hexifade; // Hexadecimal string
                        byte[] bytes = new byte[hexString.Length / 2];

                        for (int i = 0; i < hexString.Length; i += 2)
                        {
                            bytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
                        }

                        string text = System.Text.Encoding.UTF8.GetString(bytes).TrimEnd('\0');

                        EJI.JobName = text;
                    }

                    if (a <= 4 && a != 1)
                    {
                        string HexDegeri = $"{receivedPackage[35].ToString("X2")}{receivedPackage[34].ToString("X2")}{receivedPackage[33].ToString("X2")}{receivedPackage[32].ToString("X2")}";

                        uint decValue = uint.Parse(HexDegeri, System.Globalization.NumberStyles.HexNumber);

                        Cevap[a - 2] = decValue;

                        EJI.LineNo = Cevap[0];
                        EJI.StepNo = Cevap[1];
                        EJI.SpeedOverrideValue = Cevap[2];
                    }

                }
            }
            client.Close();
        }

        public void AxisConfigurationInformationRead(int VariableNumber, out string[] AxisNames)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();

            string[] Cevap = new string[8];

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            for (int a = 1; a <= 8; a++)
            {


                byte[] sendPackage = new byte[32];
                byte[] receivedPackage = new byte[750];

                _sendHeader.DataPartSize1 = 0x00;
                _sendHeader.DataPartSize2 = 0x00;
                _sendHeader.RequestID = 0x00;
                _sendHeader.CommandNumber1 = 0x74;
                _sendHeader.CommandNumber2 = 0x00;
                _sendHeader.Instance1 = Convert.ToUInt16(VariableNumber);
                _sendHeader.Attribute = Convert.ToUInt16(a);
                _sendHeader.Service = 0x0E;

                #region sendingHeaderArray...

                byte[] sendingheaderPartArray = new byte[32] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                            (byte)_sendHeader.HeaderPartSize1,
                                                            (byte)_sendHeader.HeaderPartSize2,
                                                            (byte)_sendHeader.DataPartSize1,
                                                            (byte)_sendHeader.DataPartSize2,

                                                            (byte)_sendHeader.ReserveOne,
                                                            (byte)_sendHeader.ProcessingDivision,
                                                            (byte)_sendHeader.ACK,
                                                            (byte)_sendHeader.RequestID,

                                                            (byte)_sendHeader.BlockNumber1,
                                                            (byte)_sendHeader.BlockNumber2,
                                                            (byte)_sendHeader.BlockNumber3,
                                                            (byte)_sendHeader.BlockNumber4,

                                                            (byte)_sendHeader.ReserveTwo1,
                                                            (byte)_sendHeader.ReserveTwo2,
                                                            (byte)_sendHeader.ReserveTwo3,
                                                            (byte)_sendHeader.ReserveTwo4,

                                                            (byte)_sendHeader.ReserveTwo5,
                                                            (byte)_sendHeader.ReserveTwo6,
                                                            (byte)_sendHeader.ReserveTwo7,
                                                            (byte)_sendHeader.ReserveTwo8,

                                                            (byte)_sendHeader.CommandNumber1,
                                                            (byte)_sendHeader.CommandNumber2,
                                                            (byte)_sendHeader.Instance1,
                                                            (byte)_sendHeader.Instance2,

                                                            (byte)_sendHeader.Attribute,
                                                            (byte)_sendHeader.Service,
                                                            (byte)_sendHeader.Padding1,
                                                            (byte)_sendHeader.Padding2 };
                #endregion


                for (int j = 0; j < 32; j++)
                {
                    //Header Part Filling.
                    sendPackage[j] = sendingheaderPartArray[j];
                }

                try
                {
                    int bytesTransferred = client.Send(sendPackage);
                    int bytesReceived = client.Receive(receivedPackage);
                }
                catch (Exception)
                {
                    Console.WriteLine("AxisConfigurationInformationRead komutu paket gönderim hatası");

                    string[] c = { "0" };
                    AxisNames = c;
                    return;
                }

                fillReceivedDataHeader(_receiveHeader, receivedPackage);

                if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
                {
                    client.Close();
                    Console.WriteLine("AxisConfigurationInformationRead komutu iletişim hatası");

                    string[] c = { "0" };
                    AxisNames = c;
                    return;
                }
                else
                {
                    string hexifade = "";

                    for (int j = 32; j < 49; j++)
                    {
                        hexifade = hexifade + receivedPackage[j].ToString("X2");
                    }

                    string hexString = hexifade; // Hexadecimal string
                    byte[] bytes = new byte[hexString.Length / 2];

                    for (int i = 0; i < hexString.Length; i += 2)
                    {
                        bytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
                    }

                    string text = System.Text.Encoding.UTF8.GetString(bytes);

                    Cevap[a - 1] = text;


                }
            }

            AxisNames = Cevap;

            client.Close();
        }
        public void PositionErrorRead(int ControlGroup, out int[] PositionErrors)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();

            int[] Cevap = new int[8];

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            for (int a = 1; a <= 8; a++)
            {
                byte[] sendPackage = new byte[32];
                byte[] receivedPackage = new byte[750];

                _sendHeader.DataPartSize1 = 0x00;
                _sendHeader.DataPartSize2 = 0x00;
                _sendHeader.RequestID = 0x00;
                _sendHeader.CommandNumber1 = 0x76;
                _sendHeader.CommandNumber2 = 0x00;
                _sendHeader.Instance1 = Convert.ToUInt16(ControlGroup);
                _sendHeader.Attribute = Convert.ToUInt16(a);
                _sendHeader.Service = 0x0E;

                #region sendingHeaderArray...

                byte[] sendingheaderPartArray = new byte[32] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                            (byte)_sendHeader.HeaderPartSize1,
                                                            (byte)_sendHeader.HeaderPartSize2,
                                                            (byte)_sendHeader.DataPartSize1,
                                                            (byte)_sendHeader.DataPartSize2,

                                                            (byte)_sendHeader.ReserveOne,
                                                            (byte)_sendHeader.ProcessingDivision,
                                                            (byte)_sendHeader.ACK,
                                                            (byte)_sendHeader.RequestID,

                                                            (byte)_sendHeader.BlockNumber1,
                                                            (byte)_sendHeader.BlockNumber2,
                                                            (byte)_sendHeader.BlockNumber3,
                                                            (byte)_sendHeader.BlockNumber4,

                                                            (byte)_sendHeader.ReserveTwo1,
                                                            (byte)_sendHeader.ReserveTwo2,
                                                            (byte)_sendHeader.ReserveTwo3,
                                                            (byte)_sendHeader.ReserveTwo4,

                                                            (byte)_sendHeader.ReserveTwo5,
                                                            (byte)_sendHeader.ReserveTwo6,
                                                            (byte)_sendHeader.ReserveTwo7,
                                                            (byte)_sendHeader.ReserveTwo8,

                                                            (byte)_sendHeader.CommandNumber1,
                                                            (byte)_sendHeader.CommandNumber2,
                                                            (byte)_sendHeader.Instance1,
                                                            (byte)_sendHeader.Instance2,

                                                            (byte)_sendHeader.Attribute,
                                                            (byte)_sendHeader.Service,
                                                            (byte)_sendHeader.Padding1,
                                                            (byte)_sendHeader.Padding2 };
                #endregion


                for (int j = 0; j < 32; j++)
                {
                    //Header Part Filling.
                    sendPackage[j] = sendingheaderPartArray[j];
                }

                try
                {
                    int bytesTransferred = client.Send(sendPackage);
                    int bytesReceived = client.Receive(receivedPackage);
                }
                catch (Exception)
                {
                    Console.WriteLine("PositionErrorRead komutu paket gönderim hatası");

                    int[] c = { 0 };
                    PositionErrors = c;
                    return;
                }

                fillReceivedDataHeader(_receiveHeader, receivedPackage);

                if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
                {
                    client.Close();
                    Console.WriteLine("PositionErrorRead komutu iletişim hatası");

                    int[] c = { 0 };
                    PositionErrors = c;
                    return;
                }
                else
                {
                    string HexDegeri = $"{receivedPackage[35].ToString("X2")}{receivedPackage[34].ToString("X2")}{receivedPackage[33].ToString("X2")}{receivedPackage[32].ToString("X2")}";

                    int decValue = int.Parse(HexDegeri, System.Globalization.NumberStyles.HexNumber);

                    Cevap[a - 1] = decValue;

                    //client.Close();
                }
            }

            PositionErrors = Cevap;

            client.Close();
        }

        public void StringDisplay(string Text)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();

            if (Text.Length > 30)
            {
                Console.WriteLine("Girdiğiniz Text ifadesi 30 karakterden fazladır !");
                return;
            }

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            byte[] sendPackage = new byte[62];
            byte[] receivedPackage = new byte[750];

            _sendHeader.DataPartSize1 = 0x1E;
            _sendHeader.DataPartSize2 = 0x00;
            _sendHeader.RequestID = 0x01;
            _sendHeader.CommandNumber1 = 0x85;
            _sendHeader.CommandNumber2 = 0x00;
            _sendHeader.Instance1 = 1;
            _sendHeader.Attribute = 1;
            _sendHeader.Service = 0x10;


            char[] harfDizisi = new char[30]; // 16 elemanlı bir karakter dizisi oluşturuldu

            // String ifadeyi harflere bölmek için bir döngü kullanıyoruz
            for (int i = 0; i < Text.Length; i++)
            {
                // Her bir harfi diziye ekliyoruz
                harfDizisi[i] = Text[i];
            }

            // Geri kalan elemanlar '\0' karakteriyle dolduruldu
            for (int i = Text.Length; i < harfDizisi.Length; i++)
            {
                harfDizisi[i] = '\0';
            }

            _sendHeader.StringYazma1 = harfDizisi[0].ToString();
            _sendHeader.StringYazma2 = harfDizisi[1].ToString();
            _sendHeader.StringYazma3 = harfDizisi[2].ToString();
            _sendHeader.StringYazma4 = harfDizisi[3].ToString();

            _sendHeader.StringYazma5 = harfDizisi[4].ToString();
            _sendHeader.StringYazma6 = harfDizisi[5].ToString();
            _sendHeader.StringYazma7 = harfDizisi[6].ToString();
            _sendHeader.StringYazma8 = harfDizisi[7].ToString();

            _sendHeader.StringYazma9 = harfDizisi[8].ToString();
            _sendHeader.StringYazma10 = harfDizisi[9].ToString();
            _sendHeader.StringYazma11 = harfDizisi[10].ToString();
            _sendHeader.StringYazma12 = harfDizisi[11].ToString();

            _sendHeader.StringYazma13 = harfDizisi[12].ToString();
            _sendHeader.StringYazma14 = harfDizisi[13].ToString();
            _sendHeader.StringYazma15 = harfDizisi[14].ToString();
            _sendHeader.StringYazma16 = harfDizisi[15].ToString();

            _sendHeader.StringYazma17 = harfDizisi[16].ToString();
            _sendHeader.StringYazma18 = harfDizisi[17].ToString();
            _sendHeader.StringYazma19 = harfDizisi[18].ToString();
            _sendHeader.StringYazma20 = harfDizisi[19].ToString();

            _sendHeader.StringYazma21 = harfDizisi[20].ToString();
            _sendHeader.StringYazma22 = harfDizisi[21].ToString();
            _sendHeader.StringYazma23 = harfDizisi[22].ToString();
            _sendHeader.StringYazma24 = harfDizisi[23].ToString();

            _sendHeader.StringYazma25 = harfDizisi[24].ToString();
            _sendHeader.StringYazma26 = harfDizisi[25].ToString();
            _sendHeader.StringYazma27 = harfDizisi[26].ToString();
            _sendHeader.StringYazma28 = harfDizisi[27].ToString();

            _sendHeader.StringYazma29 = harfDizisi[28].ToString();
            _sendHeader.StringYazma30 = harfDizisi[29].ToString();
            //_sendHeader.StringYazma31 = harfDizisi[30].ToString();
            //_sendHeader.StringYazma32 = harfDizisi[31].ToString();


            #region sendingHeaderArray...

            byte[] sendingheaderPartArray = new byte[62] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                            (byte)_sendHeader.HeaderPartSize1,
                                                            (byte)_sendHeader.HeaderPartSize2,
                                                            (byte)_sendHeader.DataPartSize1,
                                                            (byte)_sendHeader.DataPartSize2,

                                                            (byte)_sendHeader.ReserveOne,
                                                            (byte)_sendHeader.ProcessingDivision,
                                                            (byte)_sendHeader.ACK,
                                                            (byte)_sendHeader.RequestID,

                                                            (byte)_sendHeader.BlockNumber1,
                                                            (byte)_sendHeader.BlockNumber2,
                                                            (byte)_sendHeader.BlockNumber3,
                                                            (byte)_sendHeader.BlockNumber4,

                                                            (byte)_sendHeader.ReserveTwo1,
                                                            (byte)_sendHeader.ReserveTwo2,
                                                            (byte)_sendHeader.ReserveTwo3,
                                                            (byte)_sendHeader.ReserveTwo4,

                                                            (byte)_sendHeader.ReserveTwo5,
                                                            (byte)_sendHeader.ReserveTwo6,
                                                            (byte)_sendHeader.ReserveTwo7,
                                                            (byte)_sendHeader.ReserveTwo8,

                                                            (byte)_sendHeader.CommandNumber1,
                                                            (byte)_sendHeader.CommandNumber2,
                                                            (byte)_sendHeader.Instance1,
                                                            (byte)_sendHeader.Instance2,

                                                            (byte)_sendHeader.Attribute,
                                                            (byte)_sendHeader.Service,
                                                            (byte)_sendHeader.Padding1,
                                                            (byte)_sendHeader.Padding2,

                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma1)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma2)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma3)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma4)[0],

                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma5)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma6)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma7)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma8)[0],

                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma9)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma10)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma11)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma12)[0],

                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma13)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma14)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma15)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma16)[0],

                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma17)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma18)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma19)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma20)[0],

                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma21)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma22)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma23)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma24)[0],

                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma25)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma26)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma27)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma28)[0],

                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma29)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma30)[0]

            };

            #endregion


            for (int j = 0; j < 62; j++)
            {
                //Header Part Filling.
                sendPackage[j] = sendingheaderPartArray[j];
            }

            try
            {
                int bytesTransferred = client.Send(sendPackage);
                int bytesReceived = client.Receive(receivedPackage);
            }
            catch (Exception)
            {
                Console.WriteLine("StringDisplay komutu paket gönderim hatası");
            }

            fillReceivedDataHeader(_receiveHeader, receivedPackage);

            if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
            {
                client.Close();

                Console.WriteLine("StringDisplay komutu iletişim hatası");
            }
            else
            {
                client.Close();
            }
        }
        public void StartJob()
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            byte[] sendPackage = new byte[36];
            byte[] receivedPackage = new byte[750];

            _sendHeader.DataPartSize1 = 0x04;
            _sendHeader.DataPartSize2 = 0x00;
            _sendHeader.RequestID = 0x01;
            _sendHeader.CommandNumber1 = 0x86;
            _sendHeader.CommandNumber2 = 0x00;
            _sendHeader.Instance1 = 1;
            _sendHeader.Attribute = 1;
            _sendHeader.Service = 0x10;

            // Convert hex string to UInt16
            _sendHeader.DataByte1 = 0x01;
            _sendHeader.DataByte2 = 0x00;
            _sendHeader.DataByte3 = 0x00;
            _sendHeader.DataByte4 = 0x00;

            #region sendingHeaderArray...

            byte[] sendingheaderPartArray = new byte[36] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                            (byte)_sendHeader.HeaderPartSize1,
                                                            (byte)_sendHeader.HeaderPartSize2,
                                                            (byte)_sendHeader.DataPartSize1,
                                                            (byte)_sendHeader.DataPartSize2,

                                                            (byte)_sendHeader.ReserveOne,
                                                            (byte)_sendHeader.ProcessingDivision,
                                                            (byte)_sendHeader.ACK,
                                                            (byte)_sendHeader.RequestID,

                                                            (byte)_sendHeader.BlockNumber1,
                                                            (byte)_sendHeader.BlockNumber2,
                                                            (byte)_sendHeader.BlockNumber3,
                                                            (byte)_sendHeader.BlockNumber4,

                                                            (byte)_sendHeader.ReserveTwo1,
                                                            (byte)_sendHeader.ReserveTwo2,
                                                            (byte)_sendHeader.ReserveTwo3,
                                                            (byte)_sendHeader.ReserveTwo4,

                                                            (byte)_sendHeader.ReserveTwo5,
                                                            (byte)_sendHeader.ReserveTwo6,
                                                            (byte)_sendHeader.ReserveTwo7,
                                                            (byte)_sendHeader.ReserveTwo8,

                                                            (byte)_sendHeader.CommandNumber1,
                                                            (byte)_sendHeader.CommandNumber2,
                                                            (byte)_sendHeader.Instance1,
                                                            (byte)_sendHeader.Instance2,

                                                            (byte)_sendHeader.Attribute,
                                                            (byte)_sendHeader.Service,
                                                            (byte)_sendHeader.Padding1,
                                                            (byte)_sendHeader.Padding2,

                                                            (byte)_sendHeader.DataByte1,
                                                            (byte)_sendHeader.DataByte2,
                                                            (byte)_sendHeader.DataByte3,
                                                            (byte)_sendHeader.DataByte4 };
            #endregion


            for (int j = 0; j < 36; j++)
            {
                //Header Part Filling.
                sendPackage[j] = sendingheaderPartArray[j];
            }

            try
            {
                int bytesTransferred = client.Send(sendPackage);
                int bytesReceived = client.Receive(receivedPackage);
            }
            catch (Exception)
            {
                Console.WriteLine("StartJob komutu paket gönderim hatası");
            }

            fillReceivedDataHeader(_receiveHeader, receivedPackage);

            if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
            {
                client.Close();

                Console.WriteLine("StartJob komutu iletişim hatası (Robotu PLAY moduna alıp SERVO verildiğinden emin olun !)");
            }
            else
            {
                client.Close();
            }
        }
        public void MovePulse(int OperationNumber, MoveDataPulse MD)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();

            MyData myData = new MyData(500);

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.


            byte[] sendPackage = new byte[120];
            byte[] receivedPackage = new byte[750];

            _sendHeader.DataPartSize1 = 0x58;
            _sendHeader.DataPartSize2 = 0x00;
            _sendHeader.RequestID = 0x01;
            _sendHeader.CommandNumber1 = 0x8B;
            _sendHeader.CommandNumber2 = 0x00;
            _sendHeader.Instance1 = Convert.ToUInt16(OperationNumber);
            _sendHeader.Attribute = 1;
            _sendHeader.Service = 0x02;


            byte[] bytesRobotNo;
            byte[] bytesStationNo = { };
            byte[] bytesClassification = { };
            byte[] bytesSpeed = { };
            byte[] bytesRobotAxisPulseValue = { };
            byte[] bytesToolNo = { };
            byte[] bytesBaseAxisPosition = { };
            byte[] bytesStationAxisPosition = { };


            for (int a = 0; a < 22; a++)
            {
                if (a == 0)
                {
                    bytesRobotNo = Decimal.GetBits(MD.RobotNo).SelectMany(BitConverter.GetBytes).ToArray();

                    // Byte dizisini hexadecimal olarak çevirelim
                    string hexValueRobotNo = BitConverter.ToString(bytesRobotNo).Replace("-", "");

                    // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
                    string[] hexArrayRobotNo = new string[hexValueRobotNo.Length / 2];
                    for (int i = 0; i < hexArrayRobotNo.Length; i++)
                    {
                        hexArrayRobotNo[i] = hexValueRobotNo.Substring(i * 2, 2);
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        myData[i] = Convert.ToUInt16(hexArrayRobotNo[i], 16);
                    }
                }
                else if (a == 1)
                {
                    bytesStationNo = Decimal.GetBits(MD.StationNo).SelectMany(BitConverter.GetBytes).ToArray();

                    // Byte dizisini hexadecimal olarak çevirelim
                    string hexValueStationNo = BitConverter.ToString(bytesStationNo).Replace("-", "");

                    // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
                    string[] hexArrayStationNo = new string[hexValueStationNo.Length / 2];
                    for (int i = 0; i < hexArrayStationNo.Length; i++)
                    {
                        hexArrayStationNo[i] = hexValueStationNo.Substring(i * 2, 2);
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        myData[i + (a * 4)] = Convert.ToUInt16(hexArrayStationNo[i], 16);
                    }
                }
                else if (a == 2)
                {
                    bytesClassification = Decimal.GetBits(MD.Classification).SelectMany(BitConverter.GetBytes).ToArray();

                    // Byte dizisini hexadecimal olarak çevirelim
                    string hexValueClassification = BitConverter.ToString(bytesClassification).Replace("-", "");

                    // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
                    string[] hexArrayClassification = new string[hexValueClassification.Length / 2];
                    for (int i = 0; i < hexArrayClassification.Length; i++)
                    {
                        hexArrayClassification[i] = hexValueClassification.Substring(i * 2, 2);
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        myData[i + (a * 4)] = Convert.ToUInt16(hexArrayClassification[i], 16);
                    }
                }
                else if (a == 3)
                {
                    bytesSpeed = Decimal.GetBits(MD.Speed).SelectMany(BitConverter.GetBytes).ToArray();

                    // Byte dizisini hexadecimal olarak çevirelim
                    string hexValueSpeed = BitConverter.ToString(bytesSpeed).Replace("-", "");

                    // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
                    string[] hexArraySpeed = new string[hexValueSpeed.Length / 2];
                    for (int i = 0; i < hexArraySpeed.Length; i++)
                    {
                        hexArraySpeed[i] = hexValueSpeed.Substring(i * 2, 2);
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        myData[i + (a * 4)] = Convert.ToUInt16(hexArraySpeed[i], 16);
                    }
                }
                else if (a >= 4 && a <= 11)
                {
                    //  bytesRobotAxisPulseValue = Decimal.GetBits(MD.RobotAxisPulseValue[a - 4]).SelectMany(BitConverter.GetBytes).ToArray();

                    bytesRobotAxisPulseValue = BitConverter.GetBytes(MD.RobotAxisPulseValue[a - 4]);

                    // Byte dizisini hexadecimal olarak çevirelim
                    string hexValueRobotAxisPulseValue = BitConverter.ToString(bytesRobotAxisPulseValue).Replace("-", "");

                    // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
                    string[] hexArrayRobotAxisPulseValue = new string[hexValueRobotAxisPulseValue.Length / 2];
                    for (int i = 0; i < hexArrayRobotAxisPulseValue.Length; i++)
                    {
                        hexArrayRobotAxisPulseValue[i] = hexValueRobotAxisPulseValue.Substring(i * 2, 2);
                    }

                    for (int i = 0; i < 4; i++)
                    {
                        myData[i + (a * 4)] = Convert.ToUInt16(hexArrayRobotAxisPulseValue[i], 16);
                    }
                }
                else if (a == 12)
                {
                    bytesToolNo = Decimal.GetBits(MD.ToolNo).SelectMany(BitConverter.GetBytes).ToArray();

                    // Byte dizisini hexadecimal olarak çevirelim
                    string hexValueToolNo = BitConverter.ToString(bytesToolNo).Replace("-", "");

                    // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
                    string[] hexArrayToolNo = new string[hexValueToolNo.Length / 2];
                    for (int i = 0; i < hexArrayToolNo.Length; i++)
                    {
                        hexArrayToolNo[i] = hexValueToolNo.Substring(i * 2, 2);
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        myData[i + (a * 4)] = Convert.ToUInt16(hexArrayToolNo[i], 16);
                    }
                }
                else if (a >= 13 && a <= 15)
                {
                    //bytesBaseAxisPosition = Decimal.GetBits(MD.BaseAxisPosition[a - 13]).SelectMany(BitConverter.GetBytes).ToArray();
                    bytesBaseAxisPosition = BitConverter.GetBytes(MD.BaseAxisPosition[a - 13]);
                    // Byte dizisini hexadecimal olarak çevirelim
                    string hexValueBaseAxisPosition = BitConverter.ToString(bytesBaseAxisPosition).Replace("-", "");

                    // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
                    string[] hexArrayBaseAxisPosition = new string[hexValueBaseAxisPosition.Length / 2];
                    for (int i = 0; i < hexArrayBaseAxisPosition.Length; i++)
                    {
                        hexArrayBaseAxisPosition[i] = hexValueBaseAxisPosition.Substring(i * 2, 2);
                    }

                    for (int i = 0; i < 4; i++)
                    {
                        myData[i + (a * 4)] = Convert.ToUInt16(hexArrayBaseAxisPosition[i], 16);
                    }

                }
                else if (a >= 16 && a <= 21)
                {
                    // bytesStationAxisPosition = Decimal.GetBits(MD.StationAxisPosition[a - 16]).SelectMany(BitConverter.GetBytes).ToArray();
                    bytesStationAxisPosition = BitConverter.GetBytes(MD.StationAxisPosition[a - 16]);
                    // Byte dizisini hexadecimal olarak çevirelim
                    string hexValueStationAxisPosition = BitConverter.ToString(bytesStationAxisPosition).Replace("-", "");

                    // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
                    string[] hexArrayStationAxisPosition = new string[hexValueStationAxisPosition.Length / 2];
                    for (int i = 0; i < hexArrayStationAxisPosition.Length; i++)
                    {
                        hexArrayStationAxisPosition[i] = hexValueStationAxisPosition.Substring(i * 2, 2);
                    }

                    for (int i = 0; i < 4; i++)
                    {
                        myData[i + (a * 4)] = Convert.ToUInt16(hexArrayStationAxisPosition[i], 16);
                    }
                }
            }

            #region sendingHeaderArray...

            byte[] sendingheaderPartArray = new byte[120];

            sendingheaderPartArray[0] = Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0];
            sendingheaderPartArray[1] = Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0];
            sendingheaderPartArray[2] = Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0];
            sendingheaderPartArray[3] = Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0];

            sendingheaderPartArray[4] = (byte)_sendHeader.HeaderPartSize1;
            sendingheaderPartArray[5] = (byte)_sendHeader.HeaderPartSize2;
            sendingheaderPartArray[6] = (byte)_sendHeader.DataPartSize1;
            sendingheaderPartArray[7] = (byte)_sendHeader.DataPartSize2;

            sendingheaderPartArray[8] = (byte)_sendHeader.ReserveOne;
            sendingheaderPartArray[9] = (byte)_sendHeader.ProcessingDivision;
            sendingheaderPartArray[10] = (byte)_sendHeader.ACK;
            sendingheaderPartArray[11] = (byte)_sendHeader.RequestID;

            sendingheaderPartArray[12] = (byte)_sendHeader.BlockNumber1;
            sendingheaderPartArray[13] = (byte)_sendHeader.BlockNumber2;
            sendingheaderPartArray[14] = (byte)_sendHeader.BlockNumber3;
            sendingheaderPartArray[15] = (byte)_sendHeader.BlockNumber4;

            sendingheaderPartArray[16] = (byte)_sendHeader.ReserveTwo1;
            sendingheaderPartArray[17] = (byte)_sendHeader.ReserveTwo2;
            sendingheaderPartArray[18] = (byte)_sendHeader.ReserveTwo3;
            sendingheaderPartArray[19] = (byte)_sendHeader.ReserveTwo4;

            sendingheaderPartArray[20] = (byte)_sendHeader.ReserveTwo5;
            sendingheaderPartArray[21] = (byte)_sendHeader.ReserveTwo6;
            sendingheaderPartArray[22] = (byte)_sendHeader.ReserveTwo7;
            sendingheaderPartArray[23] = (byte)_sendHeader.ReserveTwo8;

            sendingheaderPartArray[24] = (byte)_sendHeader.CommandNumber1;
            sendingheaderPartArray[25] = (byte)_sendHeader.CommandNumber2;
            sendingheaderPartArray[26] = (byte)_sendHeader.Instance1;
            sendingheaderPartArray[27] = (byte)_sendHeader.Instance2;

            sendingheaderPartArray[28] = (byte)_sendHeader.Attribute;
            sendingheaderPartArray[29] = (byte)_sendHeader.Service;
            sendingheaderPartArray[30] = (byte)_sendHeader.Padding1;
            sendingheaderPartArray[31] = (byte)_sendHeader.Padding2;


            #endregion

            for (int i = 0; i < 88; i++)
            {
                sendingheaderPartArray[32 + i] = (byte)myData[i];
            }

            for (int j = 0; j < 120; j++)
            {
                //Header Part Filling.
                sendPackage[j] = sendingheaderPartArray[j];
            }

            try
            {
                int bytesTransferred = client.Send(sendPackage);
                int bytesReceived = client.Receive(receivedPackage);
            }
            catch (Exception)
            {
                Console.WriteLine("MovePulse komutu paket gönderim hatası");
            }

            fillReceivedDataHeader(_receiveHeader, receivedPackage);

            if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
            {
                client.Close();

                Console.WriteLine("MovePulse komutu iletişim hatası");
            }
            else
            {
                client.Close();
            }

        }
        public void MoveCartesian(int OperationNumber, MoveDataCartesian MD)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();

            MyData myData = new MyData(500);

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.


            byte[] sendPackage = new byte[136];
            byte[] receivedPackage = new byte[750];

            _sendHeader.DataPartSize1 = 0x68;
            _sendHeader.DataPartSize2 = 0x00;
            _sendHeader.RequestID = 0x01;
            _sendHeader.CommandNumber1 = 0x8A;
            _sendHeader.CommandNumber2 = 0x00;
            _sendHeader.Instance1 = Convert.ToUInt16(OperationNumber);
            _sendHeader.Attribute = 1;
            _sendHeader.Service = 0x02;


            byte[] bytesRobotNo;
            byte[] bytesStationNo = { };
            byte[] bytesClassification = { };
            byte[] bytesSpeed = { };
            byte[] bytesRobotAxisPulseValue = { };
            byte[] bytesToolNo = { };
            byte[] bytesBaseAxisPosition = { };
            byte[] bytesStationAxisPosition = { };
            byte[] bytesForm = { };

            int[] Degerler_1_11 = { MD.RobotNo, MD.StationNo, MD.Classification, MD.Speed, MD.Coordinate, MD.X, MD.Y, MD.Z, MD.Tx, MD.Ty, MD.Tz };
            int[] Type = { MD.FrontBack, MD.Arm, MD.Flip, MD.R180, MD.T180, MD.S180 };
            int[] ExtendedForm = { MD.L180, MD.U180, MD.B180, MD.E180, MD.W180 };


            for (int a = 0; a < 26; a++)
            {
                if (a <= 10)
                {
                    bytesRobotNo = BitConverter.GetBytes(Degerler_1_11[a]);

                    // bytesRobotNo = Decimal.GetBits(Degerler_1_11[a]).SelectMany(BitConverter.GetBytes).ToArray();

                    // Byte dizisini hexadecimal olarak çevirelim
                    string hexValueRobotNo = BitConverter.ToString(bytesRobotNo).Replace("-", "");

                    // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
                    string[] hexArrayRobotNo = new string[hexValueRobotNo.Length / 2];
                    for (int i = 0; i < hexArrayRobotNo.Length; i++)
                    {
                        hexArrayRobotNo[i] = hexValueRobotNo.Substring(i * 2, 2);
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        myData[i + (a * 4)] = Convert.ToUInt16(hexArrayRobotNo[i], 16);
                    }
                }

                else if (a == 12 || a == 11)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        myData[i + (a * 4)] = 0;
                    }
                }

                else if (a == 13)
                {
                    string BinaryValueForm = $"{MD.S180}{MD.T180}{MD.R180}{MD.Flip}{MD.Arm}{MD.FrontBack}";
                    int decimalDegerForm = Convert.ToInt32(BinaryValueForm, 2);

                    bytesForm = BitConverter.GetBytes(decimalDegerForm);

                    // Byte dizisini hexadecimal olarak çevirelim
                    string hexValueRobotNo = BitConverter.ToString(bytesForm).Replace("-", "");

                    // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
                    string[] hexArrayRobotNo = new string[hexValueRobotNo.Length / 2];
                    for (int i = 0; i < hexArrayRobotNo.Length; i++)
                    {
                        hexArrayRobotNo[i] = hexValueRobotNo.Substring(i * 2, 2);
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        myData[i + (a * 4)] = Convert.ToUInt16(hexArrayRobotNo[i], 16);
                    }
                }
                else if (a == 14)
                {
                    string BinaryValueForm = $"{MD.W180}{MD.E180}{MD.B180}{MD.U180}{MD.L180}";
                    int decimalDegerForm = Convert.ToInt32(BinaryValueForm, 2);

                    /// bytesForm = Decimal.GetBits(decimalDegerForm).SelectMany(BitConverter.GetBytes).ToArray();
                    bytesForm = BitConverter.GetBytes(decimalDegerForm);
                    // Byte dizisini hexadecimal olarak çevirelim
                    string hexValueRobotNo = BitConverter.ToString(bytesForm).Replace("-", "");

                    // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
                    string[] hexArrayRobotNo = new string[hexValueRobotNo.Length / 2];
                    for (int i = 0; i < hexArrayRobotNo.Length; i++)
                    {
                        hexArrayRobotNo[i] = hexValueRobotNo.Substring(i * 2, 2);
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        myData[i + (a * 4)] = Convert.ToUInt16(hexArrayRobotNo[i], 16);
                    }
                }
                else if (a == 15)
                {
                    bytesToolNo = Decimal.GetBits(MD.ToolNo).SelectMany(BitConverter.GetBytes).ToArray();

                    // Byte dizisini hexadecimal olarak çevirelim
                    string hexValueToolNo = BitConverter.ToString(bytesToolNo).Replace("-", "");

                    // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
                    string[] hexArrayToolNo = new string[hexValueToolNo.Length / 2];
                    for (int i = 0; i < hexArrayToolNo.Length; i++)
                    {
                        hexArrayToolNo[i] = hexValueToolNo.Substring(i * 2, 2);
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        myData[i + (a * 4)] = Convert.ToUInt16(hexArrayToolNo[i], 16);
                    }
                }
                else if (a == 16)
                {
                    bytesToolNo = Decimal.GetBits(MD.UserCoordinateNo).SelectMany(BitConverter.GetBytes).ToArray();

                    // Byte dizisini hexadecimal olarak çevirelim
                    string hexValueToolNo = BitConverter.ToString(bytesToolNo).Replace("-", "");

                    // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
                    string[] hexArrayToolNo = new string[hexValueToolNo.Length / 2];
                    for (int i = 0; i < hexArrayToolNo.Length; i++)
                    {
                        hexArrayToolNo[i] = hexValueToolNo.Substring(i * 2, 2);
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        myData[i + (a * 4)] = Convert.ToUInt16(hexArrayToolNo[i], 16);
                    }
                }
                else if (a >= 17 && a <= 19)
                {
                    // bytesBaseAxisPosition = Decimal.GetBits(MD.BaseAxisPosition[a - 17]).SelectMany(BitConverter.GetBytes).ToArray();
                    bytesBaseAxisPosition = BitConverter.GetBytes(MD.BaseAxisPosition[a - 17]);
                    // Byte dizisini hexadecimal olarak çevirelim
                    string hexValueBaseAxisPosition = BitConverter.ToString(bytesBaseAxisPosition).Replace("-", "");

                    // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
                    string[] hexArrayBaseAxisPosition = new string[hexValueBaseAxisPosition.Length / 2];
                    for (int i = 0; i < hexArrayBaseAxisPosition.Length; i++)
                    {
                        hexArrayBaseAxisPosition[i] = hexValueBaseAxisPosition.Substring(i * 2, 2);
                    }

                    for (int i = 0; i < 4; i++)
                    {
                        myData[i + (a * 4)] = Convert.ToUInt16(hexArrayBaseAxisPosition[i], 16);
                    }

                }
                else if (a >= 20 && a < 26)
                {
                    // bytesStationAxisPosition = Decimal.GetBits(MD.StationAxisPosition[a - 20]).SelectMany(BitConverter.GetBytes).ToArray();
                    bytesStationAxisPosition = BitConverter.GetBytes(MD.StationAxisPosition[a - 20]);
                    // Byte dizisini hexadecimal olarak çevirelim
                    string hexValueStationAxisPosition = BitConverter.ToString(bytesStationAxisPosition).Replace("-", "");

                    // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
                    string[] hexArrayStationAxisPosition = new string[hexValueStationAxisPosition.Length / 2];
                    for (int i = 0; i < hexArrayStationAxisPosition.Length; i++)
                    {
                        hexArrayStationAxisPosition[i] = hexValueStationAxisPosition.Substring(i * 2, 2);
                    }

                    for (int i = 0; i < 4; i++)
                    {
                        myData[i + (a * 4)] = Convert.ToUInt16(hexArrayStationAxisPosition[i], 16);
                    }
                }
            }

            #region sendingHeaderArray...

            byte[] sendingheaderPartArray = new byte[136];

            sendingheaderPartArray[0] = Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0];
            sendingheaderPartArray[1] = Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0];
            sendingheaderPartArray[2] = Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0];
            sendingheaderPartArray[3] = Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0];

            sendingheaderPartArray[4] = (byte)_sendHeader.HeaderPartSize1;
            sendingheaderPartArray[5] = (byte)_sendHeader.HeaderPartSize2;
            sendingheaderPartArray[6] = (byte)_sendHeader.DataPartSize1;
            sendingheaderPartArray[7] = (byte)_sendHeader.DataPartSize2;

            sendingheaderPartArray[8] = (byte)_sendHeader.ReserveOne;
            sendingheaderPartArray[9] = (byte)_sendHeader.ProcessingDivision;
            sendingheaderPartArray[10] = (byte)_sendHeader.ACK;
            sendingheaderPartArray[11] = (byte)_sendHeader.RequestID;

            sendingheaderPartArray[12] = (byte)_sendHeader.BlockNumber1;
            sendingheaderPartArray[13] = (byte)_sendHeader.BlockNumber2;
            sendingheaderPartArray[14] = (byte)_sendHeader.BlockNumber3;
            sendingheaderPartArray[15] = (byte)_sendHeader.BlockNumber4;

            sendingheaderPartArray[16] = (byte)_sendHeader.ReserveTwo1;
            sendingheaderPartArray[17] = (byte)_sendHeader.ReserveTwo2;
            sendingheaderPartArray[18] = (byte)_sendHeader.ReserveTwo3;
            sendingheaderPartArray[19] = (byte)_sendHeader.ReserveTwo4;

            sendingheaderPartArray[20] = (byte)_sendHeader.ReserveTwo5;
            sendingheaderPartArray[21] = (byte)_sendHeader.ReserveTwo6;
            sendingheaderPartArray[22] = (byte)_sendHeader.ReserveTwo7;
            sendingheaderPartArray[23] = (byte)_sendHeader.ReserveTwo8;

            sendingheaderPartArray[24] = (byte)_sendHeader.CommandNumber1;
            sendingheaderPartArray[25] = (byte)_sendHeader.CommandNumber2;
            sendingheaderPartArray[26] = (byte)_sendHeader.Instance1;
            sendingheaderPartArray[27] = (byte)_sendHeader.Instance2;

            sendingheaderPartArray[28] = (byte)_sendHeader.Attribute;
            sendingheaderPartArray[29] = (byte)_sendHeader.Service;
            sendingheaderPartArray[30] = (byte)_sendHeader.Padding1;
            sendingheaderPartArray[31] = (byte)_sendHeader.Padding2;


            #endregion

            for (int i = 0; i < 104; i++)
            {
                sendingheaderPartArray[32 + i] = (byte)myData[i];
            }

            for (int j = 0; j < 136; j++)
            {
                //Header Part Filling.
                sendPackage[j] = sendingheaderPartArray[j];
            }

            //  al = sendPackage;

            try
            {
                int bytesTransferred = client.Send(sendPackage);
                int bytesReceived = client.Receive(receivedPackage);
            }
            catch (Exception)
            {
                Console.WriteLine("MoveCartesian komutu paket gönderim hatası");
            }

            fillReceivedDataHeader(_receiveHeader, receivedPackage);

            if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
            {
                client.Close();

                Console.WriteLine("MoveCartesian komutu iletişim hatası");
            }
            else
            {
                client.Close();
            }
        }

        public bool FileDelete(string Filename)
        {
            try
            {
                int jobName_Length = Filename.Length;

                deleteJobHeader _sendHeader = new deleteJobHeader();
                receivedPackageHeader _receiveHeader = new receivedPackageHeader();

                // Connect the socket to the remote end point.
                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                client.Connect(IPAddress.Parse(IPAdresi), 10041); // dosya işlemlerinde 10041 portunu kullan.

                byte[] sendPackage = new byte[(jobName_Length + 32)];
                byte[] receivedPackage = new byte[750];

                //sending Job Delete Package
                _sendHeader.ACK = 0;
                _sendHeader.BlockNumber1 = 0;
                _sendHeader.BlockNumber4 = 0x00;

                _sendHeader.DataPartSize1 = (UInt16)jobName_Length;
                _sendHeader.RequestID = 11;

                #region sendingHeaderArray...

                byte[] sendingheaderPartArray = new byte[32] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                            (byte)_sendHeader.HeaderPartSize1,
                                                            (byte)_sendHeader.HeaderPartSize2,
                                                            (byte)_sendHeader.DataPartSize1,
                                                            (byte)_sendHeader.DataPartSize2,

                                                            (byte)_sendHeader.ReserveOne,
                                                            (byte)_sendHeader.ProcessingDivision,
                                                            (byte)_sendHeader.ACK,
                                                            (byte)_sendHeader.RequestID,

                                                            (byte)_sendHeader.BlockNumber1,
                                                            (byte)_sendHeader.BlockNumber2,
                                                            (byte)_sendHeader.BlockNumber3,
                                                            (byte)_sendHeader.BlockNumber4,

                                                            (byte)_sendHeader.ReserveTwo1,
                                                            (byte)_sendHeader.ReserveTwo2,
                                                            (byte)_sendHeader.ReserveTwo3,
                                                            (byte)_sendHeader.ReserveTwo4,

                                                            (byte)_sendHeader.ReserveTwo5,
                                                            (byte)_sendHeader.ReserveTwo6,
                                                            (byte)_sendHeader.ReserveTwo7,
                                                            (byte)_sendHeader.ReserveTwo8,

                                                            (byte)_sendHeader.CommandNumber1,
                                                            (byte)_sendHeader.CommandNumber2,
                                                            (byte)_sendHeader.Instance1,
                                                            (byte)_sendHeader.Instance2,

                                                            (byte)_sendHeader.Attribute,
                                                            (byte)_sendHeader.Service,
                                                            (byte)_sendHeader.Padding1,
                                                            (byte)_sendHeader.Padding2 };

                #endregion

                for (int j = 0; j < 32; j++)
                {
                    //Header Part Filling.
                    sendPackage[j] = sendingheaderPartArray[j];
                }

                for (int k = 0; k < jobName_Length; k++)
                {
                    sendPackage[k + 32] = Encoding.ASCII.GetBytes(Filename)[k];
                }

                try
                {
                    int bytesTransferred = client.Send(sendPackage);
                    int bytesReceived = client.Receive(receivedPackage);
                }
                catch (Exception)
                {
                    Console.WriteLine("FileDelete komutu paket gönderim hatası");
                }

                fillReceivedDataHeader(_receiveHeader, receivedPackage);

                if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
                {
                    client.Close();

                    Console.WriteLine("FileDelete komutu iletişim hatası");
                }
                else
                {
                    client.Close();
                }

                client.Close();
            }
            catch (Exception)
            {
                Console.WriteLine("Remote Moda alınız");
                return false;
            }

            return true;
        }
        public bool FileLoad(string FileDirectoryPath)
        {
            try
            {
                #region Code...

                string jobData = "";
                int packageCounter = 0;
                int errorOccured = 0;

                sendJobHeader _sendHeader = new sendJobHeader();
                receivedPackageHeader _receiveHeader = new receivedPackageHeader();

                // Connect the socket to the remote end point.
                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                client.Connect(IPAddress.Parse(IPAdresi), 10041); // dosya işlemlerinde 10041 portunu kullan.


                if (string.CompareOrdinal(FileDirectoryPath, "NULL") != 0)
                {
                    //int jobName_Length = Filename.Length;
                    //string jobName = Filename;

                    int jobName_Length = getJobNameFromDirectoryPath(FileDirectoryPath).Length;
                    string jobName = getJobNameFromDirectoryPath(FileDirectoryPath);

                    jobData = dosyaOku(FileDirectoryPath);

                    int perPackLength = 250; //Sending Lenght : 250 byte
                                             //   packageCounter = jobData.Length / perPackLength;

                    byte[] jobDataArray = new byte[jobData.Length];

                    jobDataArray = Encoding.ASCII.GetBytes(jobData);

                    // jobDataArray = Data;

                    UInt16 ackCounter = 0;

                    #region Sending Job Name Package...

                    for (int i = 0; i < 1; i++)
                    {
                        byte[] sendPackage = new byte[(jobName_Length + 32)];
                        byte[] receivedPackage = new byte[750];

                        //sending Job Name info.
                        _sendHeader.ACK = 0;
                        _sendHeader.BlockNumber1 = 0;
                        _sendHeader.BlockNumber4 = 0x00;

                        _sendHeader.DataPartSize1 = (UInt16)jobName_Length;
                        _sendHeader.RequestID = 1;

                        #region sendingHeaderArray...

                        byte[] sendingheaderPartArray = new byte[32] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                            (byte)_sendHeader.HeaderPartSize1,
                                                            (byte)_sendHeader.HeaderPartSize2,
                                                            (byte)_sendHeader.DataPartSize1,
                                                            (byte)_sendHeader.DataPartSize2,

                                                            (byte)_sendHeader.ReserveOne,
                                                            (byte)_sendHeader.ProcessingDivision,
                                                            (byte)_sendHeader.ACK,
                                                            (byte)_sendHeader.RequestID,

                                                            (byte)_sendHeader.BlockNumber1,
                                                            (byte)_sendHeader.BlockNumber2,
                                                            (byte)_sendHeader.BlockNumber3,
                                                            (byte)_sendHeader.BlockNumber4,

                                                            (byte)_sendHeader.ReserveTwo1,
                                                            (byte)_sendHeader.ReserveTwo2,
                                                            (byte)_sendHeader.ReserveTwo3,
                                                            (byte)_sendHeader.ReserveTwo4,

                                                            (byte)_sendHeader.ReserveTwo5,
                                                            (byte)_sendHeader.ReserveTwo6,
                                                            (byte)_sendHeader.ReserveTwo7,
                                                            (byte)_sendHeader.ReserveTwo8,

                                                            (byte)_sendHeader.CommandNumber1,
                                                            (byte)_sendHeader.CommandNumber2,
                                                            (byte)_sendHeader.Instance1,
                                                            (byte)_sendHeader.Instance2,

                                                            (byte)_sendHeader.Attribute,
                                                            (byte)_sendHeader.Service,
                                                            (byte)_sendHeader.Padding1,
                                                            (byte)_sendHeader.Padding2 };

                        #endregion

                        for (int j = 0; j < 32; j++)
                        {
                            //Header Part Filling.
                            sendPackage[j] = sendingheaderPartArray[j];
                        }

                        for (int k = 0; k < jobName_Length; k++)
                        {
                            sendPackage[k + 32] = Encoding.ASCII.GetBytes(jobName)[k];
                        }

                        try
                        {
                            int bytesTransferred = client.Send(sendPackage);
                            int bytesReceived = client.Receive(receivedPackage);
                        }
                        catch (Exception)
                        {
                            if (errorOccured == 0)
                            {
                                errorOccured = 1;
                                // setLoadError();
                            }
                            break;
                        }

                        fillReceivedDataHeader(_receiveHeader, receivedPackage);

                        if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
                        {
                            errorOccured = 1;
                            // setLoadError();
                            break;
                        }


                        ackCounter++;

                        try
                        {
                            if (ackCounter * (100 / packageCounter) <= 100)
                            {
                                //   loading_ProgressBar.Value = ackCounter * (100 / packageCounter);
                            }
                            else
                            {
                                //  loading_ProgressBar.Value = 100;
                            }
                        }
                        catch (Exception)
                        {
                            // loading_ProgressBar.Value = 100;
                        }
                    }

                    #endregion

                    #region Sending N. Package...

                    for (int i = 0; i < packageCounter; i++)
                    {
                        byte[] sendPackage = new byte[perPackLength + 32];
                        byte[] receivedPackage = new byte[750];

                        //sending start until last package...
                        _sendHeader.ACK = 1;
                        _sendHeader.BlockNumber1 = ackCounter;
                        _sendHeader.BlockNumber4 = 0x00;

                        _sendHeader.DataPartSize1 = (UInt16)perPackLength;
                        _sendHeader.RequestID = 1;

                        #region sendingHeaderArray...

                        byte[] sendingheaderPartArray = new byte[32] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                            (byte)_sendHeader.HeaderPartSize1,
                                                            (byte)_sendHeader.HeaderPartSize2,
                                                            (byte)_sendHeader.DataPartSize1,
                                                            (byte)_sendHeader.DataPartSize2,

                                                            (byte)_sendHeader.ReserveOne,
                                                            (byte)_sendHeader.ProcessingDivision,
                                                            (byte)_sendHeader.ACK,
                                                            (byte)_sendHeader.RequestID,

                                                            (byte)_sendHeader.BlockNumber1,
                                                            (byte)_sendHeader.BlockNumber2,
                                                            (byte)_sendHeader.BlockNumber3,
                                                            (byte)_sendHeader.BlockNumber4,

                                                            (byte)_sendHeader.ReserveTwo1,
                                                            (byte)_sendHeader.ReserveTwo2,
                                                            (byte)_sendHeader.ReserveTwo3,
                                                            (byte)_sendHeader.ReserveTwo4,

                                                            (byte)_sendHeader.ReserveTwo5,
                                                            (byte)_sendHeader.ReserveTwo6,
                                                            (byte)_sendHeader.ReserveTwo7,
                                                            (byte)_sendHeader.ReserveTwo8,

                                                            (byte)_sendHeader.CommandNumber1,
                                                            (byte)_sendHeader.CommandNumber2,
                                                            (byte)_sendHeader.Instance1,
                                                            (byte)_sendHeader.Instance2,

                                                            (byte)_sendHeader.Attribute,
                                                            (byte)_sendHeader.Service,
                                                            (byte)_sendHeader.Padding1,
                                                            (byte)_sendHeader.Padding2 };

                        #endregion

                        for (int j = 0; j < 32; j++)
                        {
                            sendPackage[j] = sendingheaderPartArray[j];
                        }

                        for (int k = 0; k < perPackLength; k++)
                        {
                            sendPackage[k + 32] = jobDataArray[k + perPackLength * i];
                        }

                        try
                        {
                            int bytesTransferred = client.Send(sendPackage);
                            int bytesReceived = client.Receive(receivedPackage);
                        }
                        catch (Exception)
                        {
                            if (errorOccured == 0)
                            {
                                errorOccured = 1;
                                // setLoadError();
                            }
                            break;
                        }


                        fillReceivedDataHeader(_receiveHeader, receivedPackage);

                        if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
                        {
                            errorOccured = 1;
                            // setLoadError();
                            break;
                        }


                        ackCounter++;

                        try
                        {
                            if (ackCounter * (100 / packageCounter) <= 100)
                            {
                                //  loading_ProgressBar.Value = ackCounter * (100 / packageCounter);
                            }
                            else
                            {
                                //  loading_ProgressBar.Value = 100;
                            }
                        }
                        catch (Exception)
                        {
                            // loading_ProgressBar.Value = 100;
                        }

                    }

                    #endregion

                    #region Sending Last Package...

                    for (int i = 0; i < 1; i++)
                    {
                        byte[] sendPackage = new byte[jobDataArray.Length - (perPackLength * (packageCounter)) + 32];
                        byte[] receivedPackage = new byte[750];

                        _sendHeader.ACK = 1;
                        _sendHeader.BlockNumber1 = ackCounter;
                        _sendHeader.BlockNumber4 = 0x80;

                        _sendHeader.DataPartSize1 = (UInt16)(jobDataArray.Length - (perPackLength * (packageCounter)));
                        _sendHeader.RequestID = 1;

                        #region sendingHeaderArray...

                        byte[] sendingheaderPartArray = new byte[32] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                            (byte)_sendHeader.HeaderPartSize1,
                                                            (byte)_sendHeader.HeaderPartSize2,
                                                            (byte)_sendHeader.DataPartSize1,
                                                            (byte)_sendHeader.DataPartSize2,

                                                            (byte)_sendHeader.ReserveOne,
                                                            (byte)_sendHeader.ProcessingDivision,
                                                            (byte)_sendHeader.ACK,
                                                            (byte)_sendHeader.RequestID,

                                                            (byte)_sendHeader.BlockNumber1,
                                                            (byte)_sendHeader.BlockNumber2,
                                                            (byte)_sendHeader.BlockNumber3,
                                                            (byte)_sendHeader.BlockNumber4,

                                                            (byte)_sendHeader.ReserveTwo1,
                                                            (byte)_sendHeader.ReserveTwo2,
                                                            (byte)_sendHeader.ReserveTwo3,
                                                            (byte)_sendHeader.ReserveTwo4,

                                                            (byte)_sendHeader.ReserveTwo5,
                                                            (byte)_sendHeader.ReserveTwo6,
                                                            (byte)_sendHeader.ReserveTwo7,
                                                            (byte)_sendHeader.ReserveTwo8,

                                                            (byte)_sendHeader.CommandNumber1,
                                                            (byte)_sendHeader.CommandNumber2,
                                                            (byte)_sendHeader.Instance1,
                                                            (byte)_sendHeader.Instance2,

                                                            (byte)_sendHeader.Attribute,
                                                            (byte)_sendHeader.Service,
                                                            (byte)_sendHeader.Padding1,
                                                            (byte)_sendHeader.Padding2 };

                        #endregion

                        for (int j = 0; j < 32; j++)
                        {
                            sendPackage[j] = sendingheaderPartArray[j];
                        }

                        for (int k = 0; k < jobDataArray.Length - (perPackLength * (packageCounter)); k++)
                        {
                            sendPackage[k + 32] = jobDataArray[k + perPackLength * (packageCounter)];
                        }

                        try
                        {
                            int bytesTransferred = client.Send(sendPackage);
                            int bytesReceived = client.Receive(receivedPackage);
                        }
                        catch (Exception)
                        {
                            if (errorOccured == 0)
                            {
                                errorOccured = 1;
                                // setLoadError();
                            }

                            break;
                        }


                        fillReceivedDataHeader(_receiveHeader, receivedPackage);
                        if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
                        {
                            errorOccured = 1;
                            //  setLoadError();
                            break;
                        }

                        ackCounter++;

                        try
                        {
                            if (ackCounter * (100 / packageCounter) <= 100)
                            {
                                //  loading_ProgressBar.Value = ackCounter * (100 / packageCounter);
                            }
                            else
                            {
                                //  loading_ProgressBar.Value = 100;
                            }
                        }
                        catch (Exception)
                        {
                            // loading_ProgressBar.Value = 100;
                        }

                    }

                    #endregion

                    client.Close();

                    if (errorOccured == 1)
                    {
                        //  setLoadError();
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    //  loadJobErrors_TextBox.Text = "Jobs can not be reading.";
                    //  loading_ProgressBar.Visible = false;
                    return false;
                }

                //  loading_ProgressBar.Visible = false;

                #endregion
            }
            catch (Exception)
            {
                return false;
            }


            return false;

        }

        private HSEConnection conn;

        public bool FileList(string Filter, out List<string> Filenames)
        {
            HSEServerAdapter hSEServerAdapter = new HSEServerAdapter(conn = new HSEConnection(IPAdresi, 10040, 10041, 300000));
            return hSEServerAdapter.FileList(Filter, out Filenames);
        }

        public bool FileSave(string Filename, string SavingDirectoryPath)
        {
            HSEServerAdapter hSEServerAdapter = new HSEServerAdapter(conn = new HSEConnection(IPAdresi, 10040, 10041, 300000));
            return hSEServerAdapter.FileSave(Filename, SavingDirectoryPath);
        }

        public void Ex_Read(int VariableNumber, out ExternalAxis Data)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();

            Data = new ExternalAxis();

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            for (int i = 1; i < 10; i++)
            {

                byte[] sendPackage = new byte[32];
                byte[] receivedPackage = new byte[750];

                _sendHeader.DataPartSize1 = 0x00;
                _sendHeader.DataPartSize2 = 0x00;
                _sendHeader.RequestID = 0x00;
                _sendHeader.CommandNumber1 = 0x81;
                _sendHeader.CommandNumber2 = 0x00;
                _sendHeader.Instance1 = Convert.ToUInt16(VariableNumber);
                _sendHeader.Attribute = Convert.ToUInt16(i);
                _sendHeader.Service = 0x0E;

                #region sendingHeaderArray...

                byte[] sendingheaderPartArray = new byte[32] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                            (byte)_sendHeader.HeaderPartSize1,
                                                            (byte)_sendHeader.HeaderPartSize2,
                                                            (byte)_sendHeader.DataPartSize1,
                                                            (byte)_sendHeader.DataPartSize2,

                                                            (byte)_sendHeader.ReserveOne,
                                                            (byte)_sendHeader.ProcessingDivision,
                                                            (byte)_sendHeader.ACK,
                                                            (byte)_sendHeader.RequestID,

                                                            (byte)_sendHeader.BlockNumber1,
                                                            (byte)_sendHeader.BlockNumber2,
                                                            (byte)_sendHeader.BlockNumber3,
                                                            (byte)_sendHeader.BlockNumber4,

                                                            (byte)_sendHeader.ReserveTwo1,
                                                            (byte)_sendHeader.ReserveTwo2,
                                                            (byte)_sendHeader.ReserveTwo3,
                                                            (byte)_sendHeader.ReserveTwo4,

                                                            (byte)_sendHeader.ReserveTwo5,
                                                            (byte)_sendHeader.ReserveTwo6,
                                                            (byte)_sendHeader.ReserveTwo7,
                                                            (byte)_sendHeader.ReserveTwo8,

                                                            (byte)_sendHeader.CommandNumber1,
                                                            (byte)_sendHeader.CommandNumber2,
                                                            (byte)_sendHeader.Instance1,
                                                            (byte)_sendHeader.Instance2,

                                                            (byte)_sendHeader.Attribute,
                                                            (byte)_sendHeader.Service,
                                                            (byte)_sendHeader.Padding1,
                                                            (byte)_sendHeader.Padding2 };
                #endregion

                for (int j = 0; j < 32; j++)
                {
                    //Header Part Filling.
                    sendPackage[j] = sendingheaderPartArray[j];
                }

                try
                {
                    int bytesTransferred = client.Send(sendPackage);
                    int bytesReceived = client.Receive(receivedPackage);
                }
                catch (Exception)
                {
                    Console.WriteLine("Ex_Read komutu paket gönderim hatası");
                }

                fillReceivedDataHeader(_receiveHeader, receivedPackage);

                if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
                {
                    client.Close();

                    // Data.CoordinateData = { };

                    Console.WriteLine("Ex_Read komutu iletişim hatası");
                    return;
                }
                else
                {
                    string HexDegeri = $"{receivedPackage[35].ToString("X2")}{receivedPackage[34].ToString("X2")}{receivedPackage[33].ToString("X2")}{receivedPackage[32].ToString("X2")}";
                    int decValue = int.Parse(HexDegeri, System.Globalization.NumberStyles.HexNumber);

                    if (i == 1)
                    {
                        Data.DataType = Convert.ToUInt32(decValue);
                    }
                    else if (i > 1)
                    {
                        Data.CoordinateData[i - 2] = decValue;
                    }
                }
            }
            client.Close();
        }
        public void Ex_Write(int VariableNumber, ExternalAxis Data)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();

            byte[] bytes = { };

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            for (int a = 1; a <= 9; a++)
            {
                byte[] sendPackage = new byte[36];
                byte[] receivedPackage = new byte[750];

                _sendHeader.DataPartSize1 = 0x04;
                _sendHeader.DataPartSize2 = 0x00;
                _sendHeader.RequestID = 0x01;
                _sendHeader.CommandNumber1 = 0x81;
                _sendHeader.CommandNumber2 = 0x00;
                _sendHeader.Instance1 = Convert.ToUInt16(VariableNumber);
                _sendHeader.Attribute = Convert.ToUInt16(a);
                _sendHeader.Service = 0x02;


                if (a == 1)
                {
                    // Decimal değerini byte dizisine dönüştürelim
                    bytes = Decimal.GetBits(Data.DataType).SelectMany(BitConverter.GetBytes).ToArray();
                }
                else if (a > 1)
                {
                    bytes = Decimal.GetBits(Data.CoordinateData[a - 2]).SelectMany(BitConverter.GetBytes).ToArray();
                }

                // Byte dizisini hexadecimal olarak çevirelim
                string hexValue = BitConverter.ToString(bytes).Replace("-", "");

                // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
                string[] hexArray = new string[hexValue.Length / 2];
                for (int i = 0; i < hexArray.Length; i++)
                {
                    hexArray[i] = hexValue.Substring(i * 2, 2);
                }

                // Convert hex string to UInt16
                _sendHeader.DataByte1 = Convert.ToUInt16(hexArray[0], 16);
                _sendHeader.DataByte2 = Convert.ToUInt16(hexArray[1], 16);
                _sendHeader.DataByte3 = Convert.ToUInt16(hexArray[2], 16);
                _sendHeader.DataByte4 = Convert.ToUInt16(hexArray[3], 16);


                #region sendingHeaderArray...

                byte[] sendingheaderPartArray = new byte[36] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                            (byte)_sendHeader.HeaderPartSize1,
                                                            (byte)_sendHeader.HeaderPartSize2,
                                                            (byte)_sendHeader.DataPartSize1,
                                                            (byte)_sendHeader.DataPartSize2,

                                                            (byte)_sendHeader.ReserveOne,
                                                            (byte)_sendHeader.ProcessingDivision,
                                                            (byte)_sendHeader.ACK,
                                                            (byte)_sendHeader.RequestID,

                                                            (byte)_sendHeader.BlockNumber1,
                                                            (byte)_sendHeader.BlockNumber2,
                                                            (byte)_sendHeader.BlockNumber3,
                                                            (byte)_sendHeader.BlockNumber4,

                                                            (byte)_sendHeader.ReserveTwo1,
                                                            (byte)_sendHeader.ReserveTwo2,
                                                            (byte)_sendHeader.ReserveTwo3,
                                                            (byte)_sendHeader.ReserveTwo4,

                                                            (byte)_sendHeader.ReserveTwo5,
                                                            (byte)_sendHeader.ReserveTwo6,
                                                            (byte)_sendHeader.ReserveTwo7,
                                                            (byte)_sendHeader.ReserveTwo8,

                                                            (byte)_sendHeader.CommandNumber1,
                                                            (byte)_sendHeader.CommandNumber2,
                                                            (byte)_sendHeader.Instance1,
                                                            (byte)_sendHeader.Instance2,

                                                            (byte)_sendHeader.Attribute,
                                                            (byte)_sendHeader.Service,
                                                            (byte)_sendHeader.Padding1,
                                                            (byte)_sendHeader.Padding2,

                                                            (byte)_sendHeader.DataByte1,
                                                            (byte)_sendHeader.DataByte2,
                                                            (byte)_sendHeader.DataByte3,
                                                            (byte)_sendHeader.DataByte4 };
                #endregion


                for (int j = 0; j < 36; j++)
                {
                    //Header Part Filling.
                    sendPackage[j] = sendingheaderPartArray[j];
                }

                try
                {
                    int bytesTransferred = client.Send(sendPackage);
                    int bytesReceived = client.Receive(receivedPackage);
                }
                catch (Exception)
                {
                    Console.WriteLine("Ex_Write komutu paket gönderim hatası");
                    return;
                }

                fillReceivedDataHeader(_receiveHeader, receivedPackage);

                if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
                {
                    client.Close();

                    Console.WriteLine("Ex_Write komutu iletişim hatası");
                    return;
                }
                else
                {
                    //client.Close();
                }
            }
            client.Close();
        }

        public void Bp_Read(int VariableNumber, out BasePosition Data)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();

            Data = new BasePosition();

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            for (int i = 1; i < 10; i++)
            {
                byte[] sendPackage = new byte[32];
                byte[] receivedPackage = new byte[750];

                _sendHeader.DataPartSize1 = 0x00;
                _sendHeader.DataPartSize2 = 0x00;
                _sendHeader.RequestID = 0x00;
                _sendHeader.CommandNumber1 = 0x80;
                _sendHeader.CommandNumber2 = 0x00;
                _sendHeader.Instance1 = Convert.ToUInt16(VariableNumber);
                _sendHeader.Attribute = Convert.ToUInt16(i);
                _sendHeader.Service = 0x0E;

                #region sendingHeaderArray...

                byte[] sendingheaderPartArray = new byte[32] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                            (byte)_sendHeader.HeaderPartSize1,
                                                            (byte)_sendHeader.HeaderPartSize2,
                                                            (byte)_sendHeader.DataPartSize1,
                                                            (byte)_sendHeader.DataPartSize2,

                                                            (byte)_sendHeader.ReserveOne,
                                                            (byte)_sendHeader.ProcessingDivision,
                                                            (byte)_sendHeader.ACK,
                                                            (byte)_sendHeader.RequestID,

                                                            (byte)_sendHeader.BlockNumber1,
                                                            (byte)_sendHeader.BlockNumber2,
                                                            (byte)_sendHeader.BlockNumber3,
                                                            (byte)_sendHeader.BlockNumber4,

                                                            (byte)_sendHeader.ReserveTwo1,
                                                            (byte)_sendHeader.ReserveTwo2,
                                                            (byte)_sendHeader.ReserveTwo3,
                                                            (byte)_sendHeader.ReserveTwo4,

                                                            (byte)_sendHeader.ReserveTwo5,
                                                            (byte)_sendHeader.ReserveTwo6,
                                                            (byte)_sendHeader.ReserveTwo7,
                                                            (byte)_sendHeader.ReserveTwo8,

                                                            (byte)_sendHeader.CommandNumber1,
                                                            (byte)_sendHeader.CommandNumber2,
                                                            (byte)_sendHeader.Instance1,
                                                            (byte)_sendHeader.Instance2,

                                                            (byte)_sendHeader.Attribute,
                                                            (byte)_sendHeader.Service,
                                                            (byte)_sendHeader.Padding1,
                                                            (byte)_sendHeader.Padding2 };
                #endregion

                for (int j = 0; j < 32; j++)
                {
                    //Header Part Filling.
                    sendPackage[j] = sendingheaderPartArray[j];
                }

                try
                {
                    int bytesTransferred = client.Send(sendPackage);
                    int bytesReceived = client.Receive(receivedPackage);
                }
                catch (Exception)
                {
                    Console.WriteLine("Bp_Read komutu paket gönderim hatası");
                }

                fillReceivedDataHeader(_receiveHeader, receivedPackage);

                if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
                {
                    client.Close();

                    // Data.CoordinateData = { };

                    Console.WriteLine("Bp_Read komutu iletişim hatası");
                    return;
                }
                else
                {
                    string HexDegeri = $"{receivedPackage[35].ToString("X2")}{receivedPackage[34].ToString("X2")}{receivedPackage[33].ToString("X2")}{receivedPackage[32].ToString("X2")}";
                    int decValue = int.Parse(HexDegeri, System.Globalization.NumberStyles.HexNumber);

                    if (i == 1)
                    {
                        Data.DataType = Convert.ToUInt32(decValue);
                    }
                    else if (i > 1)
                    {
                        Data.CoordinateData[i - 2] = decValue;
                    }
                }
            }
            client.Close();
        }
        public void Bp_Write(int VariableNumber, BasePosition Data)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();

            byte[] bytes = { };

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            for (int a = 1; a <= 9; a++)
            {
                byte[] sendPackage = new byte[36];
                byte[] receivedPackage = new byte[750];

                _sendHeader.DataPartSize1 = 0x04;
                _sendHeader.DataPartSize2 = 0x00;
                _sendHeader.RequestID = 0x01;
                _sendHeader.CommandNumber1 = 0x80;
                _sendHeader.CommandNumber2 = 0x00;
                _sendHeader.Instance1 = Convert.ToUInt16(VariableNumber);
                _sendHeader.Attribute = Convert.ToUInt16(a);
                _sendHeader.Service = 0x02;

                if (a == 1)
                {
                    // Decimal değerini byte dizisine dönüştürelim
                    bytes = Decimal.GetBits(Data.DataType).SelectMany(BitConverter.GetBytes).ToArray();
                }
                else if (a > 1)
                {
                    bytes = Decimal.GetBits(Data.CoordinateData[a - 2]).SelectMany(BitConverter.GetBytes).ToArray();
                }

                // Byte dizisini hexadecimal olarak çevirelim
                string hexValue = BitConverter.ToString(bytes).Replace("-", "");

                // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
                string[] hexArray = new string[hexValue.Length / 2];
                for (int i = 0; i < hexArray.Length; i++)
                {
                    hexArray[i] = hexValue.Substring(i * 2, 2);
                }

                // Convert hex string to UInt16
                _sendHeader.DataByte1 = Convert.ToUInt16(hexArray[0], 16);
                _sendHeader.DataByte2 = Convert.ToUInt16(hexArray[1], 16);
                _sendHeader.DataByte3 = Convert.ToUInt16(hexArray[2], 16);
                _sendHeader.DataByte4 = Convert.ToUInt16(hexArray[3], 16);


                #region sendingHeaderArray...

                byte[] sendingheaderPartArray = new byte[36] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                            (byte)_sendHeader.HeaderPartSize1,
                                                            (byte)_sendHeader.HeaderPartSize2,
                                                            (byte)_sendHeader.DataPartSize1,
                                                            (byte)_sendHeader.DataPartSize2,

                                                            (byte)_sendHeader.ReserveOne,
                                                            (byte)_sendHeader.ProcessingDivision,
                                                            (byte)_sendHeader.ACK,
                                                            (byte)_sendHeader.RequestID,

                                                            (byte)_sendHeader.BlockNumber1,
                                                            (byte)_sendHeader.BlockNumber2,
                                                            (byte)_sendHeader.BlockNumber3,
                                                            (byte)_sendHeader.BlockNumber4,

                                                            (byte)_sendHeader.ReserveTwo1,
                                                            (byte)_sendHeader.ReserveTwo2,
                                                            (byte)_sendHeader.ReserveTwo3,
                                                            (byte)_sendHeader.ReserveTwo4,

                                                            (byte)_sendHeader.ReserveTwo5,
                                                            (byte)_sendHeader.ReserveTwo6,
                                                            (byte)_sendHeader.ReserveTwo7,
                                                            (byte)_sendHeader.ReserveTwo8,

                                                            (byte)_sendHeader.CommandNumber1,
                                                            (byte)_sendHeader.CommandNumber2,
                                                            (byte)_sendHeader.Instance1,
                                                            (byte)_sendHeader.Instance2,

                                                            (byte)_sendHeader.Attribute,
                                                            (byte)_sendHeader.Service,
                                                            (byte)_sendHeader.Padding1,
                                                            (byte)_sendHeader.Padding2,

                                                            (byte)_sendHeader.DataByte1,
                                                            (byte)_sendHeader.DataByte2,
                                                            (byte)_sendHeader.DataByte3,
                                                            (byte)_sendHeader.DataByte4 };
                #endregion


                for (int j = 0; j < 36; j++)
                {
                    //Header Part Filling.
                    sendPackage[j] = sendingheaderPartArray[j];
                }

                try
                {
                    int bytesTransferred = client.Send(sendPackage);
                    int bytesReceived = client.Receive(receivedPackage);
                }
                catch (Exception)
                {
                    Console.WriteLine("Bp_Write komutu paket gönderim hatası");
                    return;
                }

                fillReceivedDataHeader(_receiveHeader, receivedPackage);

                if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
                {
                    client.Close();

                    Console.WriteLine("Bp_Write komutu iletişim hatası");
                    return;
                }
                else
                {
                    //client.Close();
                }
            }
            client.Close();
        }
        public void Bp_Read(int VariableNumber, int Count, out BasePosition[] Data)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();
            Data = new BasePosition[Count];

            for (int i = 0; i < Data.Length; i++)
            {
                Data[i] = new BasePosition();
            }

            int[] Eksenler = new int[Count * 9];
            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            for (int a = 0; a < Count; a++)
            {


                byte[] sendPackage = new byte[36];
                byte[] receivedPackage = new byte[750];

                _sendHeader.DataPartSize1 = 0x04;
                _sendHeader.DataPartSize2 = 0x00;
                _sendHeader.RequestID = 0x00;
                _sendHeader.CommandNumber1 = 0x08;
                _sendHeader.CommandNumber2 = 0x03;
                _sendHeader.Instance1 = Convert.ToUInt16(VariableNumber);
                _sendHeader.Attribute = 0;
                _sendHeader.Service = 0x33;


                // Decimal değerini byte dizisine dönüştürelim
                byte[] bytes = Decimal.GetBits(Count).SelectMany(BitConverter.GetBytes).ToArray();

                // Byte dizisini hexadecimal olarak çevirelim
                string hexValue = BitConverter.ToString(bytes).Replace("-", "");

                // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
                string[] hexArray = new string[hexValue.Length / 2];
                for (int x = 0; x < hexArray.Length; x++)
                {
                    hexArray[x] = hexValue.Substring(x * 2, 2);
                }

                // Convert hex string to UInt16
                _sendHeader.DataByte1 = Convert.ToUInt16(hexArray[0], 16);
                _sendHeader.DataByte2 = Convert.ToUInt16(hexArray[1], 16);
                _sendHeader.DataByte3 = Convert.ToUInt16(hexArray[2], 16);
                _sendHeader.DataByte4 = Convert.ToUInt16(hexArray[3], 16);

                #region sendingHeaderArray...

                byte[] sendingheaderPartArray = new byte[36] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                            (byte)_sendHeader.HeaderPartSize1,
                                                            (byte)_sendHeader.HeaderPartSize2,
                                                            (byte)_sendHeader.DataPartSize1,
                                                            (byte)_sendHeader.DataPartSize2,

                                                            (byte)_sendHeader.ReserveOne,
                                                            (byte)_sendHeader.ProcessingDivision,
                                                            (byte)_sendHeader.ACK,
                                                            (byte)_sendHeader.RequestID,

                                                            (byte)_sendHeader.BlockNumber1,
                                                            (byte)_sendHeader.BlockNumber2,
                                                            (byte)_sendHeader.BlockNumber3,
                                                            (byte)_sendHeader.BlockNumber4,

                                                            (byte)_sendHeader.ReserveTwo1,
                                                            (byte)_sendHeader.ReserveTwo2,
                                                            (byte)_sendHeader.ReserveTwo3,
                                                            (byte)_sendHeader.ReserveTwo4,

                                                            (byte)_sendHeader.ReserveTwo5,
                                                            (byte)_sendHeader.ReserveTwo6,
                                                            (byte)_sendHeader.ReserveTwo7,
                                                            (byte)_sendHeader.ReserveTwo8,

                                                            (byte)_sendHeader.CommandNumber1,
                                                            (byte)_sendHeader.CommandNumber2,
                                                            (byte)_sendHeader.Instance1,
                                                            (byte)_sendHeader.Instance2,

                                                            (byte)_sendHeader.Attribute,
                                                            (byte)_sendHeader.Service,
                                                            (byte)_sendHeader.Padding1,
                                                            (byte)_sendHeader.Padding2,

                                                            (byte)_sendHeader.DataByte1,
                                                            (byte)_sendHeader.DataByte2,
                                                            (byte)_sendHeader.DataByte3,
                                                            (byte)_sendHeader.DataByte4 };
                #endregion


                for (int j = 0; j < 36; j++)
                {
                    //Header Part Filling.
                    sendPackage[j] = sendingheaderPartArray[j];
                }

                try
                {
                    int bytesTransferred = client.Send(sendPackage);
                    int bytesReceived = client.Receive(receivedPackage);
                }
                catch (Exception)
                {
                    Console.WriteLine("Bp_Read komutu paket gönderim hatası");
                    return;
                }

                fillReceivedDataHeader(_receiveHeader, receivedPackage);

                if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
                {
                    client.Close();

                    Console.WriteLine("Bp_Read komutu iletişim hatası");
                    return;
                }
                else
                {
                    for (int i = 1; i <= 9; i++)
                    {
                        if (i == 1)
                        {
                            string HexIfade = $"{receivedPackage[(a * 36) + (35 + (i * 4))].ToString("X2")}{receivedPackage[(a * 36) + (34 + (i * 4))].ToString("X2")}{receivedPackage[(a * 36) + (33 + (i * 4))].ToString("X2")}{receivedPackage[(a * 36) + (32 + (i * 4))].ToString("X2")}";

                            int decValue = int.Parse(HexIfade, System.Globalization.NumberStyles.HexNumber);

                            Data[a].DataType = Convert.ToUInt16(decValue);
                        }
                        else if (i > 1)
                        {
                            string HexDegeri = $"{receivedPackage[(a * 36) + (35 + (i * 4))].ToString("X2")}{receivedPackage[(a * 36) + (34 + (i * 4))].ToString("X2")}{receivedPackage[(a * 36) + (33 + (i * 4))].ToString("X2")}{receivedPackage[(a * 36) + (32 + (i * 4))].ToString("X2")}";

                            Data[a].CoordinateData[i - 2] = int.Parse(HexDegeri, System.Globalization.NumberStyles.HexNumber);
                        }
                    }

                    //client.Close();
                }
            }
            client.Close();
        }
        public void Bp_Write(int VariableNumber, int Count, BasePosition[] Data)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();

            MyData myData = new MyData(1000);

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            byte[] sendPackage = new byte[36 + (Count * 36)];
            byte[] receivedPackage = new byte[1000];

            #region DATAPART SİZE
            // Decimal değerini byte dizisine dönüştürelim
            byte[] bytes2 = Decimal.GetBits((Count * 36) + 4).SelectMany(BitConverter.GetBytes).ToArray();

            // Byte dizisini hexadecimal olarak çevirelim
            string hexValue2 = BitConverter.ToString(bytes2).Replace("-", "");

            // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
            string[] hexArray2 = new string[hexValue2.Length / 2];
            for (int i = 0; i < hexArray2.Length; i++)
            {
                hexArray2[i] = hexValue2.Substring(i * 2, 2);
            }
            #endregion

            // Convert hex string to UInt16
            _sendHeader.DataPartSize1 = Convert.ToUInt16(hexArray2[0], 16);
            _sendHeader.DataPartSize2 = Convert.ToUInt16(hexArray2[1], 16);
            _sendHeader.RequestID = 0x01;
            _sendHeader.CommandNumber1 = 0x08;
            _sendHeader.CommandNumber2 = 0x03;
            _sendHeader.Instance1 = Convert.ToUInt16(VariableNumber);
            _sendHeader.Attribute = 0;
            _sendHeader.Service = 0x34;

            #region KAÇ ADET VERİYE YAZILACAK

            // Decimal değerini byte dizisine dönüştürelim
            byte[] bytes_Veri = Decimal.GetBits(Count).SelectMany(BitConverter.GetBytes).ToArray();

            // Byte dizisini hexadecimal olarak çevirelim
            string hexValue_Veri = BitConverter.ToString(bytes_Veri).Replace("-", "");

            // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
            string[] hexArray_Veri = new string[hexValue_Veri.Length / 2];
            for (int i = 0; i < hexArray_Veri.Length; i++)
            {
                hexArray_Veri[i] = hexValue_Veri.Substring(i * 2, 2);
            }

            // Convert hex string to UInt16
            _sendHeader.DataByte1 = Convert.ToUInt16(hexArray_Veri[0], 16);
            _sendHeader.DataByte2 = Convert.ToUInt16(hexArray_Veri[1], 16);
            _sendHeader.DataByte3 = Convert.ToUInt16(hexArray_Veri[2], 16);
            _sendHeader.DataByte4 = Convert.ToUInt16(hexArray_Veri[3], 16);

            #endregion

            byte[] bytesDataType = { };
            byte[] bytesEksenler = { };

            for (int a = 0; a < Count; a++)
            {
                bytesDataType = Decimal.GetBits(Data[a].DataType).SelectMany(BitConverter.GetBytes).ToArray();

                // Byte dizisini hexadecimal olarak çevirelim
                string hexValueDataType = BitConverter.ToString(bytesDataType).Replace("-", "");

                // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
                string[] hexArrayDataType = new string[hexValueDataType.Length / 2];
                for (int i = 0; i < hexArrayDataType.Length; i++)
                {
                    hexArrayDataType[i] = hexValueDataType.Substring(i * 2, 2);
                }
                for (int i = 0; i < 4; i++)
                {
                    myData[(a * 36) + i] = Convert.ToUInt16(hexArrayDataType[i], 16);
                }

                /////////////////////////

                for (int x = 0; x < 8; x++)
                {
                    bytesEksenler = Decimal.GetBits(Data[a].CoordinateData[x]).SelectMany(BitConverter.GetBytes).ToArray();

                    // Byte dizisini hexadecimal olarak çevirelim
                    string hexValueEksenler = BitConverter.ToString(bytesEksenler).Replace("-", "");

                    // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
                    string[] hexArrayEksenler = new string[hexValueEksenler.Length / 2];
                    for (int i = 0; i < hexArrayEksenler.Length; i++)
                    {
                        hexArrayEksenler[i] = hexValueEksenler.Substring(i * 2, 2);
                    }

                    for (int i = 0; i < 4; i++)
                    {
                        myData[(a * 36) + 4 + i + (x * 4)] = Convert.ToUInt16(hexArrayEksenler[i], 16);
                    }
                }

                /////////////////////////
            }

            #region sendingHeaderArray...

            byte[] sendingheaderPartArray = new byte[36 + (Count * 36)];

            sendingheaderPartArray[0] = Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0];
            sendingheaderPartArray[1] = Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0];
            sendingheaderPartArray[2] = Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0];
            sendingheaderPartArray[3] = Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0];

            sendingheaderPartArray[4] = (byte)_sendHeader.HeaderPartSize1;
            sendingheaderPartArray[5] = (byte)_sendHeader.HeaderPartSize2;
            sendingheaderPartArray[6] = (byte)_sendHeader.DataPartSize1;
            sendingheaderPartArray[7] = (byte)_sendHeader.DataPartSize2;

            sendingheaderPartArray[8] = (byte)_sendHeader.ReserveOne;
            sendingheaderPartArray[9] = (byte)_sendHeader.ProcessingDivision;
            sendingheaderPartArray[10] = (byte)_sendHeader.ACK;
            sendingheaderPartArray[11] = (byte)_sendHeader.RequestID;

            sendingheaderPartArray[12] = (byte)_sendHeader.BlockNumber1;
            sendingheaderPartArray[13] = (byte)_sendHeader.BlockNumber2;
            sendingheaderPartArray[14] = (byte)_sendHeader.BlockNumber3;
            sendingheaderPartArray[15] = (byte)_sendHeader.BlockNumber4;

            sendingheaderPartArray[16] = (byte)_sendHeader.ReserveTwo1;
            sendingheaderPartArray[17] = (byte)_sendHeader.ReserveTwo2;
            sendingheaderPartArray[18] = (byte)_sendHeader.ReserveTwo3;
            sendingheaderPartArray[19] = (byte)_sendHeader.ReserveTwo4;

            sendingheaderPartArray[20] = (byte)_sendHeader.ReserveTwo5;
            sendingheaderPartArray[21] = (byte)_sendHeader.ReserveTwo6;
            sendingheaderPartArray[22] = (byte)_sendHeader.ReserveTwo7;
            sendingheaderPartArray[23] = (byte)_sendHeader.ReserveTwo8;

            sendingheaderPartArray[24] = (byte)_sendHeader.CommandNumber1;
            sendingheaderPartArray[25] = (byte)_sendHeader.CommandNumber2;
            sendingheaderPartArray[26] = (byte)_sendHeader.Instance1;
            sendingheaderPartArray[27] = (byte)_sendHeader.Instance2;

            sendingheaderPartArray[28] = (byte)_sendHeader.Attribute;
            sendingheaderPartArray[29] = (byte)_sendHeader.Service;
            sendingheaderPartArray[30] = (byte)_sendHeader.Padding1;
            sendingheaderPartArray[31] = (byte)_sendHeader.Padding2;

            sendingheaderPartArray[32] = (byte)_sendHeader.DataByte1;
            sendingheaderPartArray[33] = (byte)_sendHeader.DataByte2;
            sendingheaderPartArray[34] = (byte)_sendHeader.DataByte3;
            sendingheaderPartArray[35] = (byte)_sendHeader.DataByte4;

            #endregion

            for (int i = 0; i < (Count * 36); i++)
            {
                sendingheaderPartArray[36 + i] = (byte)myData[i];
            }

            for (int j = 0; j < 36 + (Count * 36); j++)
            {
                //Header Part Filling.
                sendPackage[j] = sendingheaderPartArray[j];
            }

            try
            {
                int bytesTransferred = client.Send(sendPackage);
                int bytesReceived = client.Receive(receivedPackage);
            }
            catch (Exception)
            {
                Console.WriteLine("Bp_Write komutu paket gönderim hatası");
            }

            fillReceivedDataHeader(_receiveHeader, receivedPackage);

            if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
            {
                client.Close();

                Console.WriteLine("Bp_Write komutu iletişim hatası");
            }
            else
            {
                client.Close();
            }
        }

        // REQUEST YOLLANDIĞINDA DATAPAARTSİZE DEĞİŞİKLİK GÖSTERİYOR DÜZELT ONU!!!!!!

        public void JobSelect(int Type, string JobName, uint LineNumber)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();

            byte[] sendPackage = { };
            byte[] sendingheaderPartArray = { };

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            for (int a = 1; a <= 2; a++)
            {
                // byte[] sendPackage = new byte[48];
                byte[] receivedPackage = new byte[750];

                //_sendHeader.DataPartSize1 = 0x10;
                //_sendHeader.DataPartSize2 = 0x00;
                _sendHeader.RequestID = 0x01;
                _sendHeader.CommandNumber1 = 0x87;
                _sendHeader.CommandNumber2 = 0x00;
                _sendHeader.Instance1 = Convert.ToUInt16(Type);
                _sendHeader.Attribute = Convert.ToUInt16(a);
                _sendHeader.Service = 0x02;

                if (a == 1)
                {
                    sendPackage = new byte[64];

                    _sendHeader.DataPartSize1 = 0x20;
                    _sendHeader.DataPartSize2 = 0x00;

                    char[] harfDizisi = new char[32]; // 16 elemanlı bir karakter dizisi oluşturuldu

                    // JobName = JobName + ".jbi";

                    // String ifadeyi harflere bölmek için bir döngü kullanıyoruz
                    for (int i = 0; i < JobName.Length; i++)
                    {
                        // Her bir harfi diziye ekliyoruz
                        harfDizisi[i] = JobName[i];
                    }

                    // Geri kalan elemanlar '\0' karakteriyle dolduruldu
                    for (int i = JobName.Length; i < harfDizisi.Length; i++)
                    {
                        harfDizisi[i] = '\0';
                    }

                    #region PAKET
                    _sendHeader.StringYazma1 = harfDizisi[0].ToString();
                    _sendHeader.StringYazma2 = harfDizisi[1].ToString();
                    _sendHeader.StringYazma3 = harfDizisi[2].ToString();
                    _sendHeader.StringYazma4 = harfDizisi[3].ToString();

                    _sendHeader.StringYazma5 = harfDizisi[4].ToString();
                    _sendHeader.StringYazma6 = harfDizisi[5].ToString();
                    _sendHeader.StringYazma7 = harfDizisi[6].ToString();
                    _sendHeader.StringYazma8 = harfDizisi[7].ToString();

                    _sendHeader.StringYazma9 = harfDizisi[8].ToString();
                    _sendHeader.StringYazma10 = harfDizisi[9].ToString();
                    _sendHeader.StringYazma11 = harfDizisi[10].ToString();
                    _sendHeader.StringYazma12 = harfDizisi[11].ToString();

                    _sendHeader.StringYazma13 = harfDizisi[12].ToString();
                    _sendHeader.StringYazma14 = harfDizisi[13].ToString();
                    _sendHeader.StringYazma15 = harfDizisi[14].ToString();
                    _sendHeader.StringYazma16 = harfDizisi[15].ToString();

                    _sendHeader.StringYazma17 = harfDizisi[16].ToString();
                    _sendHeader.StringYazma18 = harfDizisi[17].ToString();
                    _sendHeader.StringYazma19 = harfDizisi[18].ToString();
                    _sendHeader.StringYazma20 = harfDizisi[19].ToString();

                    _sendHeader.StringYazma21 = harfDizisi[20].ToString();
                    _sendHeader.StringYazma22 = harfDizisi[21].ToString();
                    _sendHeader.StringYazma23 = harfDizisi[22].ToString();
                    _sendHeader.StringYazma24 = harfDizisi[23].ToString();

                    _sendHeader.StringYazma25 = harfDizisi[24].ToString();
                    _sendHeader.StringYazma26 = harfDizisi[25].ToString();
                    _sendHeader.StringYazma27 = harfDizisi[26].ToString();
                    _sendHeader.StringYazma28 = harfDizisi[27].ToString();

                    _sendHeader.StringYazma29 = harfDizisi[28].ToString();
                    _sendHeader.StringYazma30 = harfDizisi[29].ToString();
                    _sendHeader.StringYazma31 = harfDizisi[30].ToString();
                    _sendHeader.StringYazma32 = harfDizisi[31].ToString();
                    #endregion

                    #region sendingHeaderArray...

                    sendingheaderPartArray = new byte[64] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                            (byte)_sendHeader.HeaderPartSize1,
                                                            (byte)_sendHeader.HeaderPartSize2,
                                                            (byte)_sendHeader.DataPartSize1,
                                                            (byte)_sendHeader.DataPartSize2,

                                                            (byte)_sendHeader.ReserveOne,
                                                            (byte)_sendHeader.ProcessingDivision,
                                                            (byte)_sendHeader.ACK,
                                                            (byte)_sendHeader.RequestID,

                                                            (byte)_sendHeader.BlockNumber1,
                                                            (byte)_sendHeader.BlockNumber2,
                                                            (byte)_sendHeader.BlockNumber3,
                                                            (byte)_sendHeader.BlockNumber4,

                                                            (byte)_sendHeader.ReserveTwo1,
                                                            (byte)_sendHeader.ReserveTwo2,
                                                            (byte)_sendHeader.ReserveTwo3,
                                                            (byte)_sendHeader.ReserveTwo4,

                                                            (byte)_sendHeader.ReserveTwo5,
                                                            (byte)_sendHeader.ReserveTwo6,
                                                            (byte)_sendHeader.ReserveTwo7,
                                                            (byte)_sendHeader.ReserveTwo8,

                                                            (byte)_sendHeader.CommandNumber1,
                                                            (byte)_sendHeader.CommandNumber2,
                                                            (byte)_sendHeader.Instance1,
                                                            (byte)_sendHeader.Instance2,

                                                            (byte)_sendHeader.Attribute,
                                                            (byte)_sendHeader.Service,
                                                            (byte)_sendHeader.Padding1,
                                                            (byte)_sendHeader.Padding2,

                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma1)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma2)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma3)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma4)[0],

                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma5)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma6)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma7)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma8)[0],

                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma9)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma10)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma11)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma12)[0],

                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma13)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma14)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma15)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma16)[0],

                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma17)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma18)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma19)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma20)[0],

                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma21)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma22)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma23)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma24)[0],

                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma25)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma26)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma27)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma28)[0],

                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma29)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma30)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma31)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma32)[0],
            };
                    #endregion
                }
                else if (a == 2)
                {
                    sendPackage = new byte[36];

                    _sendHeader.DataPartSize1 = 0x04;
                    _sendHeader.DataPartSize2 = 0x00;

                    // Decimal değerini byte dizisine dönüştürelim
                    byte[] bytes = Decimal.GetBits(LineNumber).SelectMany(BitConverter.GetBytes).ToArray();

                    // Byte dizisini hexadecimal olarak çevirelim
                    string hexValue = BitConverter.ToString(bytes).Replace("-", "");

                    // hexValue stringini iki karakterlik parçalara ayırarak bir dizi oluştur
                    string[] hexArray = new string[hexValue.Length / 2];
                    for (int i = 0; i < hexArray.Length; i++)
                    {
                        hexArray[i] = hexValue.Substring(i * 2, 2);
                    }

                    // Convert hex string to UInt16
                    _sendHeader.DataByte1 = Convert.ToUInt16(hexArray[0], 16);
                    _sendHeader.DataByte2 = Convert.ToUInt16(hexArray[1], 16);
                    _sendHeader.DataByte3 = Convert.ToUInt16(hexArray[2], 16);
                    _sendHeader.DataByte4 = Convert.ToUInt16(hexArray[3], 16);

                    #region sendingHeaderArray...
                    sendingheaderPartArray = new byte[36] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                                Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                                Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                                Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                                (byte)_sendHeader.HeaderPartSize1,
                                                                (byte)_sendHeader.HeaderPartSize2,
                                                                (byte)_sendHeader.DataPartSize1,
                                                                (byte)_sendHeader.DataPartSize2,

                                                                (byte)_sendHeader.ReserveOne,
                                                                (byte)_sendHeader.ProcessingDivision,
                                                                (byte)_sendHeader.ACK,
                                                                (byte)_sendHeader.RequestID,

                                                                (byte)_sendHeader.BlockNumber1,
                                                                (byte)_sendHeader.BlockNumber2,
                                                                (byte)_sendHeader.BlockNumber3,
                                                                (byte)_sendHeader.BlockNumber4,

                                                                (byte)_sendHeader.ReserveTwo1,
                                                                (byte)_sendHeader.ReserveTwo2,
                                                                (byte)_sendHeader.ReserveTwo3,
                                                                (byte)_sendHeader.ReserveTwo4,

                                                                (byte)_sendHeader.ReserveTwo5,
                                                                (byte)_sendHeader.ReserveTwo6,
                                                                (byte)_sendHeader.ReserveTwo7,
                                                                (byte)_sendHeader.ReserveTwo8,

                                                                (byte)_sendHeader.CommandNumber1,
                                                                (byte)_sendHeader.CommandNumber2,
                                                                (byte)_sendHeader.Instance1,
                                                                (byte)_sendHeader.Instance2,

                                                                (byte)_sendHeader.Attribute,
                                                                (byte)_sendHeader.Service,
                                                                (byte)_sendHeader.Padding1,
                                                                (byte)_sendHeader.Padding2,

                                                                (byte)_sendHeader.DataByte1,
                                                                (byte)_sendHeader.DataByte2,
                                                                (byte)_sendHeader.DataByte3,
                                                                (byte)_sendHeader.DataByte4 };
                    #endregion
                }


                /*
                char[] harfDizisi = new char[16]; // 16 elemanlı bir karakter dizisi oluşturuldu

                // String ifadeyi harflere bölmek için bir döngü kullanıyoruz
                for (int i = 0; i < Data.Length; i++)
                {
                    // Her bir harfi diziye ekliyoruz
                    harfDizisi[i] = Data[i];
                }

                // Geri kalan elemanlar '\0' karakteriyle dolduruldu
                for (int i = Data.Length; i < harfDizisi.Length; i++)
                {
                    harfDizisi[i] = '\0';
                }

                _sendHeader.StringYazma1 = harfDizisi[0].ToString();
                _sendHeader.StringYazma2 = harfDizisi[1].ToString();
                _sendHeader.StringYazma3 = harfDizisi[2].ToString();
                _sendHeader.StringYazma4 = harfDizisi[3].ToString();

                _sendHeader.StringYazma5 = harfDizisi[4].ToString();
                _sendHeader.StringYazma6 = harfDizisi[5].ToString();
                _sendHeader.StringYazma7 = harfDizisi[6].ToString();
                _sendHeader.StringYazma8 = harfDizisi[7].ToString();

                _sendHeader.StringYazma9 = harfDizisi[8].ToString();
                _sendHeader.StringYazma10 = harfDizisi[9].ToString();
                _sendHeader.StringYazma11 = harfDizisi[10].ToString();
                _sendHeader.StringYazma12 = harfDizisi[11].ToString();

                _sendHeader.StringYazma13 = harfDizisi[12].ToString();
                _sendHeader.StringYazma14 = harfDizisi[13].ToString();
                _sendHeader.StringYazma15 = harfDizisi[14].ToString();
                _sendHeader.StringYazma16 = harfDizisi[15].ToString();
                */

                /*
                #region sendingHeaderArray...

                byte[] sendingheaderPartArray = new byte[48] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                            (byte)_sendHeader.HeaderPartSize1,
                                                            (byte)_sendHeader.HeaderPartSize2,
                                                            (byte)_sendHeader.DataPartSize1,
                                                            (byte)_sendHeader.DataPartSize2,

                                                            (byte)_sendHeader.ReserveOne,
                                                            (byte)_sendHeader.ProcessingDivision,
                                                            (byte)_sendHeader.ACK,
                                                            (byte)_sendHeader.RequestID,

                                                            (byte)_sendHeader.BlockNumber1,
                                                            (byte)_sendHeader.BlockNumber2,
                                                            (byte)_sendHeader.BlockNumber3,
                                                            (byte)_sendHeader.BlockNumber4,

                                                            (byte)_sendHeader.ReserveTwo1,
                                                            (byte)_sendHeader.ReserveTwo2,
                                                            (byte)_sendHeader.ReserveTwo3,
                                                            (byte)_sendHeader.ReserveTwo4,

                                                            (byte)_sendHeader.ReserveTwo5,
                                                            (byte)_sendHeader.ReserveTwo6,
                                                            (byte)_sendHeader.ReserveTwo7,
                                                            (byte)_sendHeader.ReserveTwo8,

                                                            (byte)_sendHeader.CommandNumber1,
                                                            (byte)_sendHeader.CommandNumber2,
                                                            (byte)_sendHeader.Instance1,
                                                            (byte)_sendHeader.Instance2,

                                                            (byte)_sendHeader.Attribute,
                                                            (byte)_sendHeader.Service,
                                                            (byte)_sendHeader.Padding1,
                                                            (byte)_sendHeader.Padding2,

                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma1)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma2)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma3)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma4)[0],

                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma5)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma6)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma7)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma8)[0],

                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma9)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma10)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma11)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma12)[0],

                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma13)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma14)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma15)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.StringYazma16)[0]

            };
                #endregion
                */

                if (a == 1)
                {
                    for (int j = 0; j < 64; j++)
                    {
                        //Header Part Filling.
                        sendPackage[j] = sendingheaderPartArray[j];
                    }
                }
                else if (a == 2)
                {
                    for (int j = 0; j < 32; j++)
                    {
                        //Header Part Filling.
                        sendPackage[j] = sendingheaderPartArray[j];
                    }
                }

                try
                {
                    int bytesTransferred = client.Send(sendPackage);
                    int bytesReceived = client.Receive(receivedPackage);
                }
                catch (Exception)
                {
                    Console.WriteLine("JobSelect komutu paket gönderim hatası");
                }

                fillReceivedDataHeader(_receiveHeader, receivedPackage);

                if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
                {
                    client.Close();

                    Console.WriteLine("JobSelect komutu iletişim hatası");
                }
                else
                {
                    //client.Close();
                }
            }
            client.Close();
        }

        private string dosyaOku(string path)
        {
            StreamReader sr = new StreamReader(path);
            string readedData = sr.ReadToEnd();
            sr.Close();
            return readedData;
        }

        public string getJobNameFromDirectoryPath(string jobPath)
        {
            string jobName = "NULL";
            int lastSeperatorIndeks = 0;
            int counter = 0;

            try
            {
                for (int i = 0; i < jobPath.Length; i++)
                {
                    counter = jobPath.IndexOf("\\", lastSeperatorIndeks + 1);

                    if (counter >= 0)
                    {
                        lastSeperatorIndeks = counter;
                    }
                    else
                    {
                        break;
                    }
                }

                jobName = jobPath.Substring(lastSeperatorIndeks + 1, jobPath.Length - lastSeperatorIndeks - 1);
            }
            catch (Exception)
            {
                //  Console.WriteLine("Wrong Job Path!");
            }

            return jobName;
        }

        #region FONKSİYONLAR
        private byte[] HexToBinary(string hexString)
        {
            // Gelen hexadecimal stringi integer'a çevir
            int intValue = Convert.ToInt32(hexString, 16);

            // Integer'ı binary temsile çevir
            string binaryString = Convert.ToString(intValue, 2).PadLeft(8, '0');

            // Binary stringi 8-bitlik parçalara bölmek için diziye ayır
            byte[] binaryArray = new byte[8];
            for (int i = 0; i < binaryString.Length; i++)
            {
                binaryArray[i] = byte.Parse(binaryString[i].ToString());
            }

            return binaryArray;
        }
        private float HexToFloat(string hex)
        {
            // Hexadecimal'i float tipine dönüştürelim
            int intValue = Convert.ToInt32(hex, 16);
            byte[] bytes = BitConverter.GetBytes(intValue);
            float result = BitConverter.ToSingle(bytes, 0);
            return result;
        }
        private void fillReceivedDataHeader(receivedPackageHeader headerClass, byte[] receivedData)
        {
            headerClass.ReserveOne = receivedData[8];
            headerClass.ProcessingDivision = receivedData[9];
            headerClass.ACK = receivedData[10];
            headerClass.RequestID = receivedData[11];

            headerClass.BlockNumber1 = receivedData[12];
            headerClass.BlockNumber2 = receivedData[13];
            headerClass.BlockNumber3 = receivedData[14];
            headerClass.BlockNumber4 = receivedData[15];

            headerClass.Service = receivedData[24];
            headerClass.Status = receivedData[25];
            headerClass.AddesStatusSize = receivedData[26];
            headerClass.Padding = receivedData[27];

            headerClass.AddedStatus1 = receivedData[28];
            headerClass.AddedStatus2 = receivedData[29];
            headerClass.Padding1 = receivedData[30];
            headerClass.Padding2 = receivedData[31];

            headerClass.DataByte1 = receivedData[32];
            headerClass.DataByte2 = receivedData[33];
            headerClass.DataByte3 = receivedData[34];
            headerClass.DataByte4 = receivedData[35];
        }
        private void Step_Cycle_Auto(UInt16 dataByte, string fonsiyonTipi)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            byte[] sendPackage = new byte[36];
            byte[] receivedPackage = new byte[750];

            _sendHeader.DataPartSize1 = 0x04;
            _sendHeader.DataPartSize2 = 0x00;
            _sendHeader.RequestID = 0x01;
            _sendHeader.CommandNumber1 = 0x84;
            _sendHeader.CommandNumber2 = 0x00;
            _sendHeader.Instance1 = 2;
            _sendHeader.Attribute = 1;
            _sendHeader.Service = 0x10;

            // Convert hex string to UInt16
            _sendHeader.DataByte1 = dataByte;
            _sendHeader.DataByte2 = 0x00;
            _sendHeader.DataByte3 = 0x00;
            _sendHeader.DataByte4 = 0x00;

            #region sendingHeaderArray...

            byte[] sendingheaderPartArray = new byte[36] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                            (byte)_sendHeader.HeaderPartSize1,
                                                            (byte)_sendHeader.HeaderPartSize2,
                                                            (byte)_sendHeader.DataPartSize1,
                                                            (byte)_sendHeader.DataPartSize2,

                                                            (byte)_sendHeader.ReserveOne,
                                                            (byte)_sendHeader.ProcessingDivision,
                                                            (byte)_sendHeader.ACK,
                                                            (byte)_sendHeader.RequestID,

                                                            (byte)_sendHeader.BlockNumber1,
                                                            (byte)_sendHeader.BlockNumber2,
                                                            (byte)_sendHeader.BlockNumber3,
                                                            (byte)_sendHeader.BlockNumber4,

                                                            (byte)_sendHeader.ReserveTwo1,
                                                            (byte)_sendHeader.ReserveTwo2,
                                                            (byte)_sendHeader.ReserveTwo3,
                                                            (byte)_sendHeader.ReserveTwo4,

                                                            (byte)_sendHeader.ReserveTwo5,
                                                            (byte)_sendHeader.ReserveTwo6,
                                                            (byte)_sendHeader.ReserveTwo7,
                                                            (byte)_sendHeader.ReserveTwo8,

                                                            (byte)_sendHeader.CommandNumber1,
                                                            (byte)_sendHeader.CommandNumber2,
                                                            (byte)_sendHeader.Instance1,
                                                            (byte)_sendHeader.Instance2,

                                                            (byte)_sendHeader.Attribute,
                                                            (byte)_sendHeader.Service,
                                                            (byte)_sendHeader.Padding1,
                                                            (byte)_sendHeader.Padding2,

                                                            (byte)_sendHeader.DataByte1,
                                                            (byte)_sendHeader.DataByte2,
                                                            (byte)_sendHeader.DataByte3,
                                                            (byte)_sendHeader.DataByte4 };
            #endregion

            for (int j = 0; j < 36; j++)
            {
                //Header Part Filling.
                sendPackage[j] = sendingheaderPartArray[j];
            }

            try
            {
                int bytesTransferred = client.Send(sendPackage);
                int bytesReceived = client.Receive(receivedPackage);
            }
            catch (Exception)
            {
                Console.WriteLine($"{fonsiyonTipi} komutu paket gönderim hatası");
            }

            fillReceivedDataHeader(_receiveHeader, receivedPackage);

            if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
            {
                client.Close();

                Console.WriteLine($"{fonsiyonTipi} komutu iletişim hatası");
            }
            else
            {
                client.Close();
            }
        }
        private void Error_Alarm(UInt16 dataByte, string fonsiyonTipi)
        {
            RobotControl_Data _sendHeader = new RobotControl_Data();
            receivedPackageHeader _receiveHeader = new receivedPackageHeader();

            // Connect the socket to the remote end point.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Connect(IPAddress.Parse(IPAdresi), 10040); // STATUS DURUMLARINDA 10040 portunu kullan.

            byte[] sendPackage = new byte[36];
            byte[] receivedPackage = new byte[750];

            _sendHeader.DataPartSize1 = 0x04;
            _sendHeader.DataPartSize2 = 0x00;
            _sendHeader.RequestID = 0x01;
            _sendHeader.CommandNumber1 = 0x82;
            _sendHeader.CommandNumber2 = 0x00;
            _sendHeader.Instance1 = dataByte;
            _sendHeader.Attribute = 1;
            _sendHeader.Service = 0x10;

            // Convert hex string to UInt16
            _sendHeader.DataByte1 = 0x01;
            _sendHeader.DataByte2 = 0x00;
            _sendHeader.DataByte3 = 0x00;
            _sendHeader.DataByte4 = 0x00;

            #region sendingHeaderArray...

            byte[] sendingheaderPartArray = new byte[36] { Encoding.ASCII.GetBytes(_sendHeader.Identifier_Y)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_E)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_R)[0],
                                                            Encoding.ASCII.GetBytes(_sendHeader.Identifier_C)[0],

                                                            (byte)_sendHeader.HeaderPartSize1,
                                                            (byte)_sendHeader.HeaderPartSize2,
                                                            (byte)_sendHeader.DataPartSize1,
                                                            (byte)_sendHeader.DataPartSize2,

                                                            (byte)_sendHeader.ReserveOne,
                                                            (byte)_sendHeader.ProcessingDivision,
                                                            (byte)_sendHeader.ACK,
                                                            (byte)_sendHeader.RequestID,

                                                            (byte)_sendHeader.BlockNumber1,
                                                            (byte)_sendHeader.BlockNumber2,
                                                            (byte)_sendHeader.BlockNumber3,
                                                            (byte)_sendHeader.BlockNumber4,

                                                            (byte)_sendHeader.ReserveTwo1,
                                                            (byte)_sendHeader.ReserveTwo2,
                                                            (byte)_sendHeader.ReserveTwo3,
                                                            (byte)_sendHeader.ReserveTwo4,

                                                            (byte)_sendHeader.ReserveTwo5,
                                                            (byte)_sendHeader.ReserveTwo6,
                                                            (byte)_sendHeader.ReserveTwo7,
                                                            (byte)_sendHeader.ReserveTwo8,

                                                            (byte)_sendHeader.CommandNumber1,
                                                            (byte)_sendHeader.CommandNumber2,
                                                            (byte)_sendHeader.Instance1,
                                                            (byte)_sendHeader.Instance2,

                                                            (byte)_sendHeader.Attribute,
                                                            (byte)_sendHeader.Service,
                                                            (byte)_sendHeader.Padding1,
                                                            (byte)_sendHeader.Padding2,

                                                            (byte)_sendHeader.DataByte1,
                                                            (byte)_sendHeader.DataByte2,
                                                            (byte)_sendHeader.DataByte3,
                                                            (byte)_sendHeader.DataByte4 };
            #endregion

            for (int j = 0; j < 36; j++)
            {
                //Header Part Filling.
                sendPackage[j] = sendingheaderPartArray[j];
            }

            try
            {
                int bytesTransferred = client.Send(sendPackage);
                int bytesReceived = client.Receive(receivedPackage);
            }
            catch (Exception)
            {
                Console.WriteLine($"{fonsiyonTipi} komutu paket gönderim hatası");
            }

            fillReceivedDataHeader(_receiveHeader, receivedPackage);

            if (_receiveHeader.AddedStatus1 > 0 || _receiveHeader.AddedStatus2 > 0)
            {
                client.Close();

                Console.WriteLine($"{fonsiyonTipi} komutu iletişim hatası");
            }
            else
            {
                client.Close();
            }
        }
        private void getDataFromPackage(byte[] buffer, byte[] receivedData, int receivedDataSize)
        {
            for (int i = 0; i < receivedDataSize; i++)
            {
                buffer[i] = receivedData[i + 32];
            }
        }
        private void dosyaOlustur(string path)
        {
            if (File.Exists(path) != true)
            {
                FileStream fs = File.Create(path);
                fs.Close();
            }
        }
        private void dosyaYaz(string path, string data)
        {
            StreamWriter sw = new StreamWriter(path);
            sw.Write(data);
            sw.Close();
        }
        #endregion


        public class MyData
        {
            private UInt16[] dataBytes;

            public MyData(int numBytes)
            {
                dataBytes = new UInt16[numBytes];
            }

            public UInt16 this[int index]
            {
                get { return dataBytes[index]; }
                set { dataBytes[index] = value; }
            }
        }
        public class MyString
        {
            private string[] dataBytes;

            public MyString(int numBytes)
            {
                dataBytes = new string[numBytes];
            }

            public string this[int index]
            {
                get { return dataBytes[index]; }
                set { dataBytes[index] = value; }
            }
        }
        public class RobotControl_Data
        {
            public string Identifier_Y = "Y";
            public string Identifier_E = "E";
            public string Identifier_R = "R";
            public string Identifier_C = "C";

            public UInt16 HeaderPartSize1 = 0x20;
            public UInt16 HeaderPartSize2 = 0x00;
            public UInt16 DataPartSize1 { get; set; }
            public UInt16 DataPartSize2 { get; set; }

            public UInt16 ReserveOne = 3;
            public UInt16 ProcessingDivision = 1;
            public UInt16 ACK = 0x00;
            public UInt16 RequestID { get; set; }

            public UInt16 BlockNumber1 = 0;
            public UInt16 BlockNumber2 = 0;
            public UInt16 BlockNumber3 = 0;
            public UInt16 BlockNumber4 = 0;

            public UInt16 ReserveTwo1 = 9;
            public UInt16 ReserveTwo2 = 9;
            public UInt16 ReserveTwo3 = 9;
            public UInt16 ReserveTwo4 = 9;

            public UInt16 ReserveTwo5 = 9;
            public UInt16 ReserveTwo6 = 9;
            public UInt16 ReserveTwo7 = 9;
            public UInt16 ReserveTwo8 = 9;

            public UInt16 CommandNumber1 { get; set; }
            public UInt16 CommandNumber2 { get; set; }
            public UInt16 Instance1 { get; set; }
            public UInt16 Instance2 = 0;

            public UInt16 Attribute { get; set; }
            public UInt16 Service { get; set; }
            public UInt16 Padding1 = 0;
            public UInt16 Padding2 = 0;

            public UInt16 DataByte1 { get; set; }
            public UInt16 DataByte2 { get; set; }
            public UInt16 DataByte3 { get; set; }
            public UInt16 DataByte4 { get; set; }



            public string StringYazma1 { get; set; }
            public string StringYazma2 { get; set; }
            public string StringYazma3 { get; set; }
            public string StringYazma4 { get; set; }

            public string StringYazma5 { get; set; }
            public string StringYazma6 { get; set; }
            public string StringYazma7 { get; set; }
            public string StringYazma8 { get; set; }

            public string StringYazma9 { get; set; }
            public string StringYazma10 { get; set; }
            public string StringYazma11 { get; set; }
            public string StringYazma12 { get; set; }

            public string StringYazma13 { get; set; }
            public string StringYazma14 { get; set; }
            public string StringYazma15 { get; set; }
            public string StringYazma16 { get; set; }

            public string StringYazma17 { get; set; }
            public string StringYazma18 { get; set; }
            public string StringYazma19 { get; set; }
            public string StringYazma20 { get; set; }

            public string StringYazma21 { get; set; }
            public string StringYazma22 { get; set; }
            public string StringYazma23 { get; set; }
            public string StringYazma24 { get; set; }

            public string StringYazma25 { get; set; }
            public string StringYazma26 { get; set; }
            public string StringYazma27 { get; set; }
            public string StringYazma28 { get; set; }

            public string StringYazma29 { get; set; }
            public string StringYazma30 { get; set; }
            public string StringYazma31 { get; set; }
            public string StringYazma32 { get; set; }
        }
        public class receivedPackageHeader
        {
            public string Identifier_Y { get; set; }
            public string Identifier_E { get; set; }
            public string Identifier_R { get; set; }
            public string Identifier_C { get; set; }

            public UInt16 HeaderPartSize1 { get; set; }
            public UInt16 HeaderPartSize2 { get; set; }
            public UInt16 DataPartSize1 { get; set; }
            public UInt16 DataPartSize2 { get; set; }

            public UInt16 ReserveOne { get; set; }
            public UInt16 ProcessingDivision { get; set; }
            public UInt16 ACK { get; set; }
            public UInt16 RequestID { get; set; }

            public UInt16 BlockNumber1 { get; set; }
            public UInt16 BlockNumber2 { get; set; }
            public UInt16 BlockNumber3 { get; set; }
            public UInt16 BlockNumber4 { get; set; }

            public UInt16 ReserveTwo1 { get; set; }
            public UInt16 ReserveTwo2 { get; set; }
            public UInt16 ReserveTwo3 { get; set; }
            public UInt16 ReserveTwo4 { get; set; }

            public UInt16 ReserveTwo5 { get; set; }
            public UInt16 ReserveTwo6 { get; set; }
            public UInt16 ReserveTwo7 { get; set; }
            public UInt16 ReserveTwo8 { get; set; }

            public UInt16 Service { get; set; }
            public UInt16 Status { get; set; }
            public UInt16 AddesStatusSize { get; set; }
            public UInt16 Padding { get; set; }

            public UInt16 AddedStatus1 { get; set; }
            public UInt16 AddedStatus2 { get; set; }
            public UInt16 Padding1 { get; set; }
            public UInt16 Padding2 { get; set; }

            public UInt16 DataByte1 { get; set; }
            public UInt16 DataByte2 { get; set; }
            public UInt16 DataByte3 { get; set; }
            public UInt16 DataByte4 { get; set; }

            public UInt16 DataByte5 { get; set; }
            public UInt16 DataByte6 { get; set; }
            public UInt16 DataByte7 { get; set; }
            public UInt16 DataByte8 { get; set; }
        }
        public class deleteJobHeader
        {
            public string Identifier_Y = "Y";
            public string Identifier_E = "E";
            public string Identifier_R = "R";
            public string Identifier_C = "C";

            public UInt16 HeaderPartSize1 = 0x20;
            public UInt16 HeaderPartSize2 = 0x00;
            public UInt16 DataPartSize1 { get; set; }
            public UInt16 DataPartSize2 = 0;

            public UInt16 ReserveOne = 3;
            public UInt16 ProcessingDivision = 2;
            public UInt16 ACK { get; set; }
            public UInt16 RequestID { get; set; }

            public UInt16 BlockNumber1 { get; set; }
            public UInt16 BlockNumber2 = 0;
            public UInt16 BlockNumber3 = 0;
            public UInt16 BlockNumber4 { get; set; }

            public UInt16 ReserveTwo1 = 9;
            public UInt16 ReserveTwo2 = 9;
            public UInt16 ReserveTwo3 = 9;
            public UInt16 ReserveTwo4 = 9;

            public UInt16 ReserveTwo5 = 9;
            public UInt16 ReserveTwo6 = 9;
            public UInt16 ReserveTwo7 = 9;
            public UInt16 ReserveTwo8 = 9;

            public UInt16 CommandNumber1 = 0;
            public UInt16 CommandNumber2 = 0;
            public UInt16 Instance1 = 0;
            public UInt16 Instance2 = 0;

            public UInt16 Attribute = 0;
            public UInt16 Service = 0x09;
            public UInt16 Padding1 = 0;
            public UInt16 Padding2 = 0;


        }
        public class getJobListHeader
        {
            public string Identifier_Y = "Y";
            public string Identifier_E = "E";
            public string Identifier_R = "R";
            public string Identifier_C = "C";

            public UInt16 HeaderPartSize1 = 0x20;
            public UInt16 HeaderPartSize2 = 0x00;
            public UInt16 DataPartSize1 { get; set; }
            public UInt16 DataPartSize2 = 0;

            public UInt16 ReserveOne = 3;
            public UInt16 ProcessingDivision = 2;
            public UInt16 ACK { get; set; }
            public UInt16 RequestID { get; set; }

            public UInt16 BlockNumber1 { get; set; }
            public UInt16 BlockNumber2 = 0;
            public UInt16 BlockNumber3 = 0;
            public UInt16 BlockNumber4 { get; set; }

            public UInt16 ReserveTwo1 = 9;
            public UInt16 ReserveTwo2 = 9;
            public UInt16 ReserveTwo3 = 9;
            public UInt16 ReserveTwo4 = 9;

            public UInt16 ReserveTwo5 = 9;
            public UInt16 ReserveTwo6 = 9;
            public UInt16 ReserveTwo7 = 9;
            public UInt16 ReserveTwo8 = 9;

            public UInt16 CommandNumber1 = 0;
            public UInt16 CommandNumber2 = 0;
            public UInt16 Instance1 = 0;
            public UInt16 Instance2 = 0;

            public UInt16 Attribute = 0;
            public UInt16 Service = 0x32;
            public UInt16 Padding1 = 0;
            public UInt16 Padding2 = 0;
        }
        public class sendJobHeader
        {
            public string Identifier_Y = "Y";
            public string Identifier_E = "E";
            public string Identifier_R = "R";
            public string Identifier_C = "C";

            public UInt16 HeaderPartSize1 = 0x20;
            public UInt16 HeaderPartSize2 = 0x00;
            public UInt16 DataPartSize1 { get; set; }
            public UInt16 DataPartSize2 = 0;

            public UInt16 ReserveOne = 3;
            public UInt16 ProcessingDivision = 2;
            public UInt16 ACK { get; set; }
            public UInt16 RequestID { get; set; }

            public UInt16 BlockNumber1 { get; set; }
            public UInt16 BlockNumber2 = 0;
            public UInt16 BlockNumber3 = 0;
            public UInt16 BlockNumber4 { get; set; }

            public UInt16 ReserveTwo1 = 9;
            public UInt16 ReserveTwo2 = 9;
            public UInt16 ReserveTwo3 = 9;
            public UInt16 ReserveTwo4 = 9;

            public UInt16 ReserveTwo5 = 9;
            public UInt16 ReserveTwo6 = 9;
            public UInt16 ReserveTwo7 = 9;
            public UInt16 ReserveTwo8 = 9;

            public UInt16 CommandNumber1 = 0;
            public UInt16 CommandNumber2 = 0;
            public UInt16 Instance1 = 0;
            public UInt16 Instance2 = 0;

            public UInt16 Attribute = 0;
            public UInt16 Service = 0x15;
            public UInt16 Padding1 = 0;
            public UInt16 Padding2 = 0;


        }
        public class receiveJobHeader
        {
            public string Identifier_Y = "Y";
            public string Identifier_E = "E";
            public string Identifier_R = "R";
            public string Identifier_C = "C";

            public UInt16 HeaderPartSize1 = 0x20;
            public UInt16 HeaderPartSize2 = 0x00;
            public UInt16 DataPartSize1 { get; set; }
            public UInt16 DataPartSize2 = 0;

            public UInt16 ReserveOne = 3;
            public UInt16 ProcessingDivision = 2;
            public UInt16 ACK { get; set; }
            public UInt16 RequestID { get; set; }

            public UInt16 BlockNumber1 { get; set; }
            public UInt16 BlockNumber2 = 0;
            public UInt16 BlockNumber3 = 0;
            public UInt16 BlockNumber4 { get; set; }

            public UInt16 ReserveTwo1 = 9;
            public UInt16 ReserveTwo2 = 9;
            public UInt16 ReserveTwo3 = 9;
            public UInt16 ReserveTwo4 = 9;

            public UInt16 ReserveTwo5 = 9;
            public UInt16 ReserveTwo6 = 9;
            public UInt16 ReserveTwo7 = 9;
            public UInt16 ReserveTwo8 = 9;

            public UInt16 CommandNumber1 = 0;
            public UInt16 CommandNumber2 = 0;
            public UInt16 Instance1 = 0;
            public UInt16 Instance2 = 0;

            public UInt16 Attribute = 0;
            public UInt16 Service = 0x16;
            public UInt16 Padding1 = 0;
            public UInt16 Padding2 = 0;
        }




        private class HSEComm
        {
            private readonly IPAddress inetAddress;
            private UdpClient udpSock;
            private int reqID = 0;
            private readonly HSEConnection conn;

            public HSEComm(HSEConnection conn)
            {
                this.conn = conn;
                try
                {
                    inetAddress = IPAddress.Parse(conn.Ip);
                }
                catch (FormatException e)
                {
                    Console.WriteLine("Could not resolve address: " + conn.Ip + ". " + e.Message);
                }
            }
            public void OpenConnection()
            {
                try
                {
                    this.udpSock = new UdpClient();
                    this.udpSock.Client.ReceiveTimeout = this.conn.Timeout;
                }
                catch (SocketException e)
                {
                    Console.WriteLine("Error establishing UDP connection on ip: " + this.inetAddress + " and port: " + this.conn.Port + " . ExceptionMessage:" + e.Message);
                }
            }
            public static byte[] CreateMessage(int commandNo, int instance, int attribute, int service, int processDiv, int requestID, long blockNo, params byte[] data)
            {
                if (processDiv != 1 && processDiv != 2)
                {
                    Console.WriteLine("ProcessDiv can only be 1 for robot control or 2 for file manipulation");
                }

                byte[] request = {
        89, 69, 82, 67, 32, 0, (byte)(data.Length & 0xFF), (byte)((data.Length & 0xFF00) >> 8), 3, (byte)processDiv, 0, (byte)requestID,
        (byte)(blockNo & 0xFFL), (byte)((long)(blockNo & 0xFF00) >> 8), (byte)((long)(blockNo & 0xFF0000) >> 16), (byte)((long)(blockNo & 0xFF000000) >> 24),
        57, 57, 57, 57, 57, 57, 57, 57, (byte)(commandNo & 0xFF), (byte)((commandNo & 0xFF00) >> 8),
        (byte)(instance & 0xFF), (byte)((instance & 0xFF00) >> 8), (byte)attribute, (byte)service, 0, 0
    };

                if (data.Length == 0)
                    return request;

                byte[] resultArray = new byte[request.Length + data.Length];
                Array.Copy(request, 0, resultArray, 0, request.Length);
                Array.Copy(data, 0, resultArray, request.Length, data.Length);

                return resultArray;
            }
            public void SendMessage(byte[] msg, string context)
            {
                IPEndPoint endPoint = null;
                ValidateOpenedConnection();

                if (msg.Length < 32)
                    Console.WriteLine("Message too short. Message has to have header included, which is 32 bytes.", context);

                if (msg[9] == 2)
                {
                    endPoint = new IPEndPoint(this.inetAddress, this.conn.PortFiles);
                }
                else if (msg[9] == 1)
                {
                    endPoint = new IPEndPoint(this.inetAddress, this.conn.Port);
                }
                else
                {
                    Console.WriteLine("Incorrect processing division. Values can be only 1 or 2. Value given: " + msg[9], context);
                }

                try
                {
                    this.udpSock.Send(msg, msg.Length, endPoint);
                }
                catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
                {
                    Console.WriteLine("Request timed out (timeout=" + this.conn.Timeout + "ms)");
                }
                catch (IOException e)
                {
                    Console.WriteLine("Could not send message to controller. IOException: " + e.Message, context);
                }
            }

            public byte[] ReadMessage(int processUnit, string context)
            {
                IPEndPoint endPoint = null;
                ValidateOpenedConnection();
                byte[] buf = new byte[512];

                if (processUnit == 1)
                {
                    endPoint = new IPEndPoint(this.inetAddress, this.conn.Port);
                }
                else if (processUnit == 2)
                {
                    endPoint = new IPEndPoint(this.inetAddress, this.conn.PortFiles);
                }
                else
                {
                    Console.WriteLine("Wrong processing unit. Only 1 or 2 is allowed. Given value: " + processUnit, context);
                }

                try
                {
                    buf = this.udpSock.Receive(ref endPoint);
                }
                catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
                {
                    Console.WriteLine("Request timed out (timeout=" + this.conn.Timeout + "ms)");
                }
                catch (IOException e)
                {
                    Console.WriteLine("Could not receive packet from the UDP socket. Closing socket. Reason: " + e.Message + " \n Call context: " + context);
                }

                if (ValidateResponseHeader(buf, context))
                {
                    return buf;

                }
                else
                {
                    return null;
                }
            }
            public bool ValidateResponseHeader(byte[] msg, string context)
            {
                if (msg.Length < 32)
                    Console.WriteLine("Response header is not valid. Header too short. length: " + msg.Length, context);

                byte statusCode = msg[25];

                if (statusCode != 0)
                {
                    int sizeOfStatusCode = msg[26];
                    StringBuilder errorCode = new StringBuilder();
                    if (sizeOfStatusCode != 0)
                    {
                        for (int i = 1; i <= sizeOfStatusCode; i++)
                        {
                            for (int u = 27 + 2 * i; u > 27 + 2 * (i - 1); u--)
                            {
                                errorCode.Append(HSEParserUtil.FormatByteToHex(msg[u]));
                            }
                        }
                        return false;
                        //Console.WriteLine("Device responded with error code. " + errorCode.ToString() + ". error code: " + errorCode, context);
                    }
                    return false;
                    //Console.WriteLine("Response header is not valid. Error code: not included. Status code: " + statusCode, context);
                }

                return true;
            }

            public void CloseConnection()
            {
                if (this.udpSock != null)
                {
                    this.udpSock.Close();
                }
            }
            public static string GetFileString(byte[] msg, int offset, int len)
            {
                StringBuilder text = new StringBuilder();
                for (int i = 0; i < len; i++)
                {
                    char val = (char)(msg[offset + i] & 0xFF);
                    if (val == '\0')
                        break;
                    text.Append(val);
                }
                return text.ToString();
            }
            public static long GetBlockNo(byte[] array)
            {
                if (array == null)
                    return 0L;

                long number = 0L;
                for (int i = 12, u = 0; i <= 15; i++, u += 8)
                    number |= ((long)array[i] & 0xFF) << u;

                return number;
            }
            public static void SetBlockNo(byte[] msg, long blockNr)
            {
                msg[12] = (byte)(blockNr & 0xFFL);
                msg[13] = (byte)((blockNr & 0xFF00L) >> 8);
                msg[14] = (byte)((blockNr & 0xFF0000L) >> 16);
                msg[15] = (byte)((blockNr & 0xFF000000L) >> 24);
            }
            public int GetReqID()
            {
                return this.reqID;
            }

            public int GetAndIncrementReqID()
            {
                int temp = this.reqID;
                this.reqID++;
                return temp;
            }
            public static void SetACK(byte[] msg, int val)
            {
                msg[10] = (byte)val;
            }
            private void ValidateOpenedConnection()
            {
                if (this.udpSock == null)
                {
                    Console.WriteLine("UDP socket is not opened for IP: " + this.conn.Ip);
                }
            }
        }
        private class HSEServerAdapter
        {
            private readonly HSEComm comm;

            public HSEServerAdapter(HSEConnection conn)
            {
                this.comm = new HSEComm(conn);
            }

            public bool FileSave(string Filename, string SavingDirectoryPath)
            {
                byte[] byteFileName = Encoding.ASCII.GetBytes(Filename);
                long blockNo = 0L;
                StringBuilder file = new StringBuilder();
                byte[] request = HSEComm.CreateMessage(0, 0, 0, 22, 2, this.comm.GetReqID(), (byte)blockNo, byteFileName);
                byte[] ack = HSEComm.CreateMessage(0, 0, 0, 22, 2, this.comm.GetAndIncrementReqID(), (byte)blockNo, new byte[0]);
                HSEComm.SetACK(ack, 1);
                byte[] response = null;
                string context = $"getFile name={Filename} blockNo=";

                try
                {
                    this.comm.OpenConnection();
                    this.comm.SendMessage(request, context + "0");
                    do
                    {
                        response = this.comm.ReadMessage(2, context + context);

                        if (response == null)
                        {
                            return false;
                        }

                        HSEComm.SetBlockNo(ack, HSEComm.GetBlockNo(response));
                        this.comm.SendMessage(ack, context + context);
                        file.Append(HSEComm.GetFileString(response, 32, HSEParserUtil.GetDataPartLen(response)));
                    } while (HSEComm.GetBlockNo(response) < 2147483648L);
                }
                catch (Exception)
                {
                    return false; // If an exception occurs, return false.
                }
                finally
                {
                    this.comm.CloseConnection();
                }

                string FilePAth = $"{SavingDirectoryPath}\\{Filename}";

                File.WriteAllText(FilePAth, file.ToString());

                return true;
            }

            public bool FileList(string Filter, out List<string> Filenames)
            {
                FileType type;
                switch (Filter)
                {
                    case "*.JBI":
                        type = FileType.JBI;
                        break;
                    case "*.DAT":
                        type = FileType.DAT;
                        break;
                    case "*.CND":
                        type = FileType.CND;
                        break;
                    case "*.PRM":
                        type = FileType.PRM;
                        break;
                    case "*.SYS":
                        type = FileType.SYS;
                        break;
                    case "*.LST":
                        type = FileType.LST;
                        break;
                    case "*.LOG":
                        type = FileType.LOG;
                        break;
                    default:
                        Filenames = null;
                        return false;
                }


                int blockNo = 0;
                byte[] request = HSEComm.CreateMessage(0, 0, 0, 50, 2, comm.GetReqID(), blockNo, type.GetByteArr());
                byte[] ack = HSEComm.CreateMessage(0, 0, 0, 50, 2, comm.GetAndIncrementReqID(), blockNo, new byte[0]);
                HSEComm.SetACK(ack, 1);

                StringBuilder buffer = new StringBuilder();
                byte[] response = null;
                string context = "getFileList type=" + type.GetString() + " blockNo=";
                List<string> result = new List<string>();

                try
                {
                    comm.OpenConnection();

                    comm.SendMessage(request, context + "0");

                    do
                    {
                        response = comm.ReadMessage(2, context + context);
                        HSEComm.SetBlockNo(ack, HSEComm.GetBlockNo(response));
                        comm.SendMessage(ack, context + context);
                        result.AddRange(HSEParserUtil.ParseToFileNames(response, type.GetString(), buffer));
                    }
                    while (HSEComm.GetBlockNo(response) < 2147483648L);
                }
                catch (Exception)
                {
                    Filenames = null;
                    return false; // If an exception occurs, return false.
                }
                finally
                {
                    this.comm.CloseConnection();
                }

                Filenames = result;

                return true;
            }


            public class FileType
            {
                public static readonly FileType JBI = new FileType(new byte[] { 42, 46, 74, 66, 73 }, ".JBI");
                public static readonly FileType DAT = new FileType(new byte[] { 42, 46, 68, 65, 84 }, ".DAT");
                public static readonly FileType CND = new FileType(new byte[] { 42, 46, 67, 78, 68 }, ".CND");
                public static readonly FileType PRM = new FileType(new byte[] { 42, 46, 80, 82, 77 }, ".PRM");
                public static readonly FileType SYS = new FileType(new byte[] { 42, 46, 83, 89, 83 }, ".SYS");
                public static readonly FileType LST = new FileType(new byte[] { 42, 46, 76, 83, 84 }, ".LST");
                public static readonly FileType LOG = new FileType(new byte[] { 42, 46, 76, 79, 71 }, ".LOG");

                private readonly byte[] code;
                private readonly string name;

                private FileType(byte[] code, string name)
                {
                    this.code = code;
                    this.name = name;
                }

                public byte[] GetByteArr()
                {
                    return code;
                }

                public string GetString()
                {
                    return name;
                }

                // İsterseniz kullanım kolaylığı için tüm türleri dönen bir liste de ekleyebiliriz
                public static IEnumerable<FileType> Values()
                {
                    yield return JBI;
                    yield return DAT;
                    yield return CND;
                    yield return PRM;
                    yield return SYS;
                    yield return LST;
                    yield return LOG;
                }
            }

        }
        private class HSEConnection
        {
            public readonly string Ip;
            public readonly int Port;
            public readonly int PortFiles;
            public readonly int Timeout;
            //public readonly int rs022;

            public HSEConnection(string ip, int port, int portFiles, int timeout)
            {
                this.Ip = ip;
                this.Port = port;
                this.PortFiles = portFiles;
                this.Timeout = timeout;
                //this.rs022 = rs022;
            }

            public override string ToString()
            {
                // Burada Java'daki toString metodunun C# karşılığı olan override kullanılıyor.
                return base.ToString();
            }
        }
        private class HSEParserUtil
        {
            public static int GetDataPartLen(byte[] msg)
            {
                int sizeOfData = msg[6] & 0xFF;
                int secondByte = msg[7] & 0xFF;
                sizeOfData |= secondByte << 8;
                return sizeOfData;
            }
            public static string FormatByteToHex(byte num)
            {
                return string.Format("{0:x2}", num);
            }

            public static List<string> ParseToFileNames(byte[] msg, string fileType, StringBuilder nameBuffer)
            {
                List<string> result = new List<string>();
                int reader = 32;

                while (reader < msg.Length && msg[reader] != 0)
                {
                    if (msg[reader] == 13)
                    {
                        reader++;
                        continue;
                    }

                    if ((char)msg[reader] == '\n')
                    {
                        if (nameBuffer.ToString().Contains(fileType))
                        {
                            result.Add(nameBuffer.ToString());
                        }
                        nameBuffer.Clear(); // StringBuilder'ı sıfırla
                    }
                    else
                    {
                        nameBuffer.Append((char)msg[reader]);
                    }

                    reader++;
                }

                return result;
            }
        }

    }
}
