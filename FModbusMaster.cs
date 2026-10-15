using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Windows.Media.Imaging;
using MCF;

namespace MCF.Modbus
{
    /// <summary>
    ///  The MCF.Modbus object type that manages communication with a Modbus slave device.
    /// </summary>
    /// <remarks>This class implements a Modbus TCP master driver.</remarks>
    public class FModbusMaster : FDevice
    {
        /// <summary>The network address of the Modbus slave device.</summary>
        public FString Address { get; private set; }

        /// <summary>The amount of time in milliseconds that a Modbus TCP operation will wait for data.</summary>
        public FInteger ReceiveTimeout { get; private set; }

        private ModbusTcpClient Device = null;  // low-level implementation of the Modbus TCP master driver

        public FModbusMaster(FObject Owner, string Name, string Address = "127.0.0.1")
            : base(Owner, Name)
        {
            Image = new BitmapImage(new Uri("/MCF.Modbus;component/Images/modbus.png", UriKind.Relative));
            Description = "Modbus TCP master driver.";
            Help = "This MCF device manages communication with a Modbus slave device.";
            DefaultFilter = "Text Files|*.txt|All Files|*.*";

            this.Address = new FString(this, "Address", Address, EMemberCategory.Setting)
            {
                Description = "The network address of the Modbus slave device.",
                Help = "Example: 127.0.0.1"
            };
            this.Address.SetMemberFlags(EMemberFlags.StockMCF);
            MoveMember(this.Address, 2);

            ReceiveTimeout = new FInteger(this, "ReceiveTimeout", 500, EMemberCategory.Setting)
            {
                DisplayFormat = "{0} msec",
                Description = "The amount of time that a Modbus TCP operation will wait for data.",
                Help = "Default value is 500 milliseconds."
            };
            ReceiveTimeout.SetMemberFlags(EMemberFlags.StockMCF);
            MoveMember(ReceiveTimeout, 3);

            g.theMachine.Update += Machine_Update;
        }

        private void Machine_Update(object sender, EventArgs e)
        {
            SendCommand("HoldingRegister 0");  // temp
        }

        /// <summary>
        /// Opens communication.  Called by MCF when the machine goes Online.
        /// </summary>
        public override bool GoOnline()
        {
            Device = new ModbusTcpClient();

            bool success;
            lock (Device) // critical section because the device's members are updated by a background thread.
            {
                success = Device.ConnectModbusTCP(Address, ReceiveTimeout);
            }

            if (success)
            {
                DeviceStatus.Value = EDeviceStatus.Open;
                return base.GoOnline();
            }
            else
            {
                DeviceStatus.Value = EDeviceStatus.NotFound;
                return false;
            }
        }

        /// <summary>
        /// Closes communication.  Called by MCF when the machine goes Offline.
        /// </summary>
        public override void GoOffline()
        {
            DeviceStatus.Value = EDeviceStatus.Closed;

            lock (Device) // critical section because the device's members are updated by a background thread.
            {
                Device.CloseModbusTCP();
            }
        }

        /// <summary>
        /// Gets the lock object that the device uses for thread-safe communications.
        /// </summary>
        /// <remarks>Take a lock on this object when multiple commands must be sent without potential interruption by background updates.</remarks>
        public override object LockObject { get { return Device; } }

        /// <summary>
        /// Sends a command string to the Modbus master driver and returns the reply.
        /// </summary>
        /// <param name="command">The command string.</param>
        /// <param name="reply">The reply string.</param>
        /// <returns>The result.</returns>
        public override ECommandResult SendCommand(string command, out string reply)
        {
            reply = "";
            if (DeviceStatus != EDeviceStatus.Open)
                return ECommandResult.Offline;
            if (string.IsNullOrEmpty(command))
                return ECommandResult.Success;

            bool result;
            lock (Device) // critical section because the device's members are updated by a background thread.
            {
                result = Device.SendCommand(command, out reply);
            }

            if (!result && !Device.Connected)
            {
                ReportLost("Established connection failed to respond.");
                return ECommandResult.Offline;
            }

            return result ? ECommandResult.Success : ECommandResult.Error;
        }

        /// <summary>
        /// Used by the framework to download a command file to the device.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">e.Argument is the absolute file path.</param>
        /// <remarks>This method must be implimented if the device will support the <see cref="FDeviceFile"/> member type.</remarks>
        /// <exclude/>
        protected override void DownloadFileLock(object sender, DoWorkEventArgs e)
        {
            lock (Device) // critical section because the device's members are updated by a background thread.
            {
                string file = (string)e.Argument;
                bool success = Device.DownloadFile(file);
                e.Result = new DownloadFileResult(file, success);
            }
        }

        /// <summary>
        /// Obtains a reference to an <see cref="FModbusMaster"/> object by full name.
        /// </summary>
        new public static FModbusMaster GetRef(string fullName, object caller = null)
        {
            FBase element;
            g.Verify(FBase.GetElement(fullName, out element, caller), "\"" + fullName + "\" (FModbusMaster) name not found.");
            g.Verify(element is FModbusMaster, "\"" + fullName + "\" is not an FModbusMaster.");
            return (FModbusMaster)element;
        }

        /// <summary>
        /// Low-level implementation of the Modbus TCP Master Driver.
        /// </summary>
        /// <remarks>
        /// This class should be a thin wrapper around the ActiveX control or .NET assembly provided by the manufacturer.
        /// It should perform exception handling and error and debug message logging.
        /// If the device does not have an ASCII command parser, this class should provide one.
        /// </remarks>
        private sealed class ModbusTcpClient : TcpClient
        {
            /// <summary>Open the Modbus TCP connection.</summary>
            public bool ConnectModbusTCP(string Address, int ReceiveTimeout)
            {
                if (Connected)
                    return true;

                try
                {
                    Connect(Address, 502);  // port 502 for Modbus TCP
                    if (!Connected)
                    {
                        g.Report(this, EReportType.ErrorAlarm, "Failed to Open Modbus TCP connection at " + Address);
                        return false;
                    }

                    this.ReceiveTimeout = ReceiveTimeout;
                    return true;
                }
                catch (Exception exc)
                {
                    g.Report(this, EReportType.ErrorAlarm, "Failed to Open the Modbus TCP connection: " + exc.Message);
                    return false;
                }
            }

            /// <summary>Close the Modbus TCP connection.</summary>
            public void CloseModbusTCP()
            {
                if (!Connected)
                    return;

                try
                {
                    Close();
                }
                catch (Exception exc)
                {
                    g.Report(this, EReportType.ErrorAlarm, "Failed to Close the Modbus TCP connection: " + exc.Message);
                }
            }

            /// <summary>A simple ASCII command parser for the Modbus master driver.</summary>
            public bool SendCommand(string command, out string reply)
            {
                reply = "";
                if (!Connected || string.IsNullOrEmpty(command))
                    return true;

                try
                {
                    #region Command Parser
                    // Reads are of the form "Command Address".  Writes are of the form "Command Address = Value".
                    // Commands are not case-sensitive.  Spaces are ignored.  Address and Value are unsigned decimal numbers.
                    // Commands:
                    //     DiscreteInput   Read-Only   Single bit  (0 or 1)
                    //     Coil            Read+Write  Single bit  (0 or 1)	
                    //     InputRegister   Read-Only   16-bit word (0 to 65535)
                    //     HoldingRegister Read+Write  16-bit word (0 to 65535)
                    // This example would write the largest possible value (0xFFFF) to the first holding register:
                    //     HoldingRegister0=65535
                    
                    command = command.Trim();
                    bool success = false;

                    if (command.Contains("="))  // Write
                    {
                        string[] cmds = command.Split('=');
                        if (cmds.Length == 2)
                        {
                            int value = int.Parse(cmds[1]);
                            if (cmds[0].StartsWith("HoldingRegister", StringComparison.InvariantCultureIgnoreCase))
                            {
                                ushort address = ushort.Parse(cmds[0].Substring(15));
                                success = WriteHoldingRegister(address, (ushort)(value & 0xFFFF));
                            }
                            else if (cmds[0].StartsWith("Coil", StringComparison.InvariantCultureIgnoreCase))
                            {
                                ushort address = ushort.Parse(cmds[0].Substring(4));
                                success = WriteCoil(address, (value != 0));
                            }
                        }
                    }
                    else  // Read
                    {
                        if (command.StartsWith("HoldingRegister", StringComparison.InvariantCultureIgnoreCase))
                        {
                            ushort address = ushort.Parse(command.Substring(15)), value;
                            success = ReadHoldingRegister(address, out value);
                            reply = value.ToString();
                        }
                        else if (command.StartsWith("InputRegister", StringComparison.InvariantCultureIgnoreCase))
                        {
                            ushort address = ushort.Parse(command.Substring(13)), value;
                            success = ReadInputRegister(address, out value);
                            reply = value.ToString();
                        }
                        else if (command.StartsWith("DiscreteInput", StringComparison.InvariantCultureIgnoreCase))
                        {
                            ushort address = ushort.Parse(command.Substring(13));
                            bool bvalue;
                            success = ReadDiscreteInput(address, out bvalue);
                            reply = bvalue ? "1" : "0";
                        }
                        else if (command.StartsWith("Coil", StringComparison.InvariantCultureIgnoreCase))
                        {
                            ushort address = ushort.Parse(command.Substring(4));
                            bool bvalue;
                            success = ReadCoil(address, out bvalue);
                            reply = bvalue ? "1" : "0";
                        }
                    }
                    #endregion Command Parser

                    if (success)
                    {
                        if (reply.Length > 0)
                            g.Report(this, EReportType.Debug, "Modbus TCP Command Sent: \"{0}\" >> \"{1}\"", command, reply);
                        else
                            g.Report(this, EReportType.Debug, "Modbus TCP Command Sent: \"{0}\"", command);
                        return true;
                    }
                    else
                    {
                        g.Report(this, EReportType.ErrorAlarm, "Failed to Send Modbus TCP Command: \"{0}\"", command);
                        return false;
                    }
                }
                catch (Exception exc)
                {
                    g.Report(this, EReportType.ErrorAlarm, "Failed to Send Modbus TCP Command: " + exc.Message);
                    return false;
                }
            }

            /// <summary>A convenience method when the reply isn't wanted</summary>
            public bool SendCommand(string command)
            {
                string reply;
                return SendCommand(command, out reply);
            }

            /// <summary>Send a text file of commands line-by-line to the device.</summary>
            public bool DownloadFile(string file)
            {
                if (!Connected || file.Length == 0)
                    return true;

                if (!File.Exists(file))
                {
                    g.Report(this, EReportType.ErrorAlarm, "Modbus TCP File Download Error: \"{0}\" does not exist", file);
                    return false;
                }

                using (StreamReader sr = new StreamReader(file))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        line = line.Trim();
                        if (line.Length == 0) continue;

                        if (!SendCommand(line))
                        {
                            g.Report(this, EReportType.ErrorAlarm, "Modbus TCP File Download Error: \"{0}\"", file);
                            return false;
                        }
                    }
                }

                return true;
            }

            #region Modbus TCP Reads and Writes
            // Derived from "Modbus TCP class" by Stephan Stricker
            // http://www.codeproject.com/Articles/16260/Modbus-TCP-class
            private const byte fctReadCoils = 1;
            private const byte fctReadDiscreteInputs = 2;
            private const byte fctReadHoldingRegisters = 3;
            private const byte fctReadInputRegister = 4;
            private const byte fctWriteSingleCoil = 5;
            private const byte fctWriteSingleRegister = 6;
            private const byte fctWriteMultipleCoils = 15;
            private const byte fctWriteMultipleRegisters = 16;
            private const byte fctReadWriteMultipleRegisters = 23;

            public bool ReadHoldingRegister(ushort address, out ushort value)
            {
                value = 0;
                byte[] bytes = SendThenReceive(CreateReadHeader(address, 1, fctReadHoldingRegisters), 11);
                if (bytes == null)
                    return false;
                value = (ushort)((bytes[0] << 8) + bytes[1]);
                return true;
            }

            public bool WriteHoldingRegister(ushort address, ushort value)
            {
                byte[] bytes = CreateWriteHeader(address, 1, 1, fctWriteSingleRegister);
                bytes[10] = (byte)((value >> 8) & 0xFF);
                bytes[11] = (byte)(value & 0xFF);
                return (SendThenReceive(bytes, 12) != null);
            }

            public bool ReadCoil(ushort address, out bool value)
            {
                value = false;
                byte[] bytes = SendThenReceive(CreateReadHeader(address, 1, fctReadCoils), 10);
                if (bytes == null)
                    return false;
                value = (bytes[0] != 0);
                return true;
            }

            public bool WriteCoil(ushort address, bool value)
            {
                byte[] bytes = CreateWriteHeader(address, 1, 1, fctWriteSingleCoil);
                bytes[10] = (byte)(value ? 0xFF : 0);
                return (SendThenReceive(bytes, 12) != null);
            }

            public bool ReadInputRegister(ushort address, out ushort value)
            {
                value = 0;
                byte[] bytes = SendThenReceive(CreateReadHeader(address, 1, fctReadInputRegister), 11);
                if (bytes == null)
                    return false;
                value = (ushort)((bytes[0] << 8) + bytes[1]);
                return true;
            }

            public bool ReadDiscreteInput(ushort address, out bool value)
            {
                value = false;
                byte[] bytes = SendThenReceive(CreateReadHeader(address, 1, fctReadDiscreteInputs), 10);
                if (bytes == null)
                    return false;
                value = (bytes[0] != 0);
                return true;
            }

            private byte[] CreateReadHeader(ushort startAddress, ushort length, byte function)
            {
                // Create modbus header for read action
                byte[] data = new byte[12];
                data[0] = 0;				    // Transaction id high byte
                data[1] = 0;				    // Transaction id low byte
                data[5] = 6;					// Message size
                data[6] = 0;					// Slave address
                data[7] = function;				// Function code
                byte[] _adr = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)startAddress));
                data[8] = _adr[0];				// Start address
                data[9] = _adr[1];				// Start address
                byte[] _length = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)length));
                data[10] = _length[0];			// Number of data to read
                data[11] = _length[1];			// Number of data to read
                return data;
            }

            private byte[] CreateWriteHeader(ushort startAddress, ushort numData, ushort numBytes, byte function)
            {
                // Create modbus header for write action
                byte[] data = new byte[numBytes + 11];
                data[0] = 0;				    // Transaction id high byte
                data[1] = 0;				    // Transaction id low byte
                byte[] _size = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)(5 + numBytes)));
                data[4] = _size[0];				// Complete message size in bytes
                data[5] = _size[1];				// Complete message size in bytes
                data[6] = 0;					// Slave address
                data[7] = function;				// Function code
                byte[] _adr = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)startAddress));
                data[8] = _adr[0];				// Start address
                data[9] = _adr[1];				// Start address
                if (function >= fctWriteMultipleCoils)
                {
                    byte[] _cnt = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)numData));
                    data[10] = _cnt[0];			// Number of bytes
                    data[11] = _cnt[1];			// Number of bytes
                    data[12] = (byte)(numBytes - 2);
                }
                return data;
            }

            private byte[] SendThenReceive(byte[] tx_bytes, int rx_byte_count)
            {
                try
                {
                    // Write data and and wait for response
                    NetworkStream stream = GetStream();
                    stream.Write(tx_bytes, 0, tx_bytes.Length);

                    byte[] rx_bytes = new byte[rx_byte_count];
                    int byte_count = stream.Read(rx_bytes, 0, rx_bytes.Length);
                    
                    if (byte_count < rx_byte_count)
                    {
                        g.Report(this, EReportType.ErrorAlarm, "Modbus TCP Error: fewer bytes received than expected");
                        Close();
                        return null;
                    }

                    byte function = rx_bytes[7];
                    if (function > 128)  // Response data is slave exception
                    {
                        switch (function - 128)
                        {
                            case 1: g.Report(this, EReportType.ErrorAlarm, "Modbus TCP Error: illegal function"); break;
                            case 2: g.Report(this, EReportType.ErrorAlarm, "Modbus TCP Error: illegal data address"); break;
                            case 3: g.Report(this, EReportType.ErrorAlarm, "Modbus TCP Error: illegal data value"); break;
                            case 4: g.Report(this, EReportType.ErrorAlarm, "Modbus TCP Error: slave device failure"); break;
                            case 5: g.Report(this, EReportType.ErrorAlarm, "Modbus TCP Error: acknowledge"); break;
                            case 6: g.Report(this, EReportType.ErrorAlarm, "Modbus TCP Error: slave is busy/booting up"); break;
                            case 10: g.Report(this, EReportType.ErrorAlarm, "Modbus TCP Error: gate path unavailable"); break;
                            case 100: g.Report(this, EReportType.ErrorAlarm, "Modbus TCP Error: send fault"); break;
                            default: g.Report(this, EReportType.ErrorAlarm, "Modbus TCP Error: {0}", function - 128); break;
                        }
                        Close();
                        return null;
                    }

                    if (function >= fctWriteSingleCoil && function != fctReadWriteMultipleRegisters)  // Write response data
                    {
                        byte[] data = new byte[2];
                        Array.Copy(rx_bytes, 10, data, 0, 2);
                        return data;
                    }
                    else  // Read response data
                    {
                        byte[] data = new byte[rx_bytes[8]];
                        Array.Copy(rx_bytes, 9, data, 0, data.Length);
                        return data;
                    }
                }
                catch (Exception exc)
                {
                    g.Report(this, EReportType.ErrorAlarm, "Failed to Send Modbus TCP Command: {1}", exc.Message);
                    Close();
                    return null;
                }
            }
            #endregion
        }
    }
}
