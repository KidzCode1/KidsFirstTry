
using System;
using System.Timers;
using System.IO.Ports;
using UsbLibrary;
using System.Threading.Channels;
using System.Reflection.Metadata;

public class ServoMotorController
{
    static readonly object bufferLock = new object();
    public ushort boardID = 0xffff;
    System.Timers.Timer checkBoardStatusTimer;
    bool checkID = true;
    public byte id = 0;
    bool isOnline;
    public List<Message>[] listMSG = new List<Message>[16];
    Message message;
    public byte OnlineBoard = 0;
    CancellationTokenSource portDetectCancellationTokenSource = new CancellationTokenSource();
    Task PortDetectTask;
    bool PortStop = true;
    byte[] rxBuffer = new byte[1000];
    ushort rxCount = 0;
    SerialPort serialPort = null!;
    ushort totalLength = 5;
    UsbWindowMessageListener? usbWindowMessageListener;

    public ServoMotorController()
    {
        //Form form = new Form();
        //form.Show();
        CreateUsbWindowMessageListener();
        checkBoardStatusTimer = new System.Timers.Timer(500);
        checkBoardStatusTimer.Elapsed += CheckBoardStatusTimer_Elapsed;
        checkBoardStatusTimer.Start();
        PortDetectTask = Task.Run(async () => { await SetPort(portDetectCancellationTokenSource.Token); }, portDetectCancellationTokenSource.Token);

        //PortDetectThread.Start(); //启动线程
        CreateSerialPort();
        PortStop = false;
    }

    private void CreateUsbWindowMessageListener()
    {
        usbWindowMessageListener = new UsbWindowMessageListener();
        usbWindowMessageListener.OnMotorControllerConnected += UsbWindowMessageListener_OnMotorControllerConnected;
        usbWindowMessageListener.OnMotorControllerRemoved += UsbWindowMessageListener_OnMotorControllerRemoved;
        usbWindowMessageListener.OnDataReceived += UsbWindowMessageListener_OnDataReceived;
        usbWindowMessageListener.Visible = false;
        usbWindowMessageListener.Show();
    }

    void ByteCopy(byte[] fromArray, byte[] toArray, ushort fromIndex, ushort toIndex, ushort length)
    {
        for (int i = 0; i < length; i++)
            toArray[toIndex + i] = fromArray[fromIndex + i];
    }

    void ChangePortName(string newPortName)
    {
        try
        {
            if (serialPort.PortName != newPortName)
            {
                if (serialPort.IsOpen)
                    serialPort.Close();

                serialPort.PortName = newPortName;
            }
            PortStop = false;
        } catch (Exception ex)
        {
            ExceptionThrown?.Invoke(this, ex);
        }
    }

    void CheckBoardStatusTimer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        if (OnlineBoard == 0)
        {
            OnBoardIsOffline(this, EventArgs.Empty);
            return;
        }
        if (checkID)
        {
            checkID = false;
            boardID = 0xffff;
            Send(0x00, 0x12, 0);
        } else
        {
            checkID = true;

            if (boardID == 0xffff)
                OnBoardIsOffline(this, EventArgs.Empty);
            else
                OnBoardIsOnline(this, EventArgs.Empty);

            if (isOnline && (boardID & (0x01 << id)) == 0)
            {
                //textBoxBoard.BackColor = Color.Lime;
            }
        }
    }

    bool CheckHead(byte[] data, byte[] headTemp, int headLength)
    {
        for (byte i = 0; i < headLength; i++)
            if (data[i] != headTemp[i])
                return false;

        return true;
    }

    void CreateSerialPort()
    {
        serialPort = new SerialPort();
        serialPort.DataReceived += SerialPort_DataReceived;
        serialPort.BaudRate = 9600;
        serialPort.DataBits = 8;
        serialPort.DiscardNull = false;
        serialPort.DtrEnable = false;
        serialPort.Handshake = Handshake.None;
        serialPort.Parity = Parity.None;
        serialPort.ParityReplace = 63;
        serialPort.PortName = ServoMotorControllerConstants.CommunicationPortName;
        serialPort.ReadBufferSize = 4096;
        serialPort.ReadTimeout = -1;
        serialPort.ReceivedBytesThreshold = 1;
        serialPort.RtsEnable = false;
        serialPort.StopBits = StopBits.One;
        serialPort.WriteBufferSize = 2048;
        serialPort.WriteTimeout = -1;
    }

    string GetLocalText(string ChString, string EnString)
    {
        if (Thread.CurrentThread.CurrentUICulture.Name == "zh-CN")
            return ChString;
        else
            return EnString;
    }

    async Task PortClose()
    {
        PortStop = true;
        if (serialPort.IsOpen)
        {
            await Task.Yield();
            serialPort.Dispose();
            serialPort.Close();
        }
    }

    void SendUSBMsg(byte ucType, byte[] byteSend, byte ucLength)
    {
        try
        {
            if (usbWindowMessageListener?.SpecifiedDevice != null)
            {
                byte[] byteUSB = new byte[0x43];
                byteUSB[1] = ucLength;
                byteUSB[2] = ucType;
                byteSend.CopyTo(byteUSB, 3);
                usbWindowMessageListener.SpecifiedDevice.SendData(byteUSB);
            }
        } catch (Exception ex)
        {
            ExceptionThrown?.Invoke(this, ex);
        }
    }

    void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        byte[] byteTemp = new byte[1000];
        try
        {
            ushort usLength = (ushort)serialPort.Read(byteTemp, 0, 500);
            DecodeData(byteTemp, usLength);
        } catch (Exception ex)
        {
            ExceptionThrown?.Invoke(this, ex);
        }
    }

    void SetBaudRate(int baudRate)
    {
        try
        {
            serialPort.BaudRate = baudRate;

            byte[] byteChipCMD = new byte[4];
            switch (baudRate)
            {
                case 2400:
                    byteChipCMD[1] = (byte)Baud.BAUD_2400;
                    break;
                case 4800:
                    byteChipCMD[1] = (byte)Baud.BAUD_4800;
                    break;
                case 9600:
                    byteChipCMD[1] = (byte)Baud.BAUD_9600;
                    break;
                case 19200:
                    byteChipCMD[1] = (byte)Baud.BAUD_19200;
                    break;
                case 38400:
                    byteChipCMD[1] = (byte)Baud.BAUD_38400;
                    break;
                case 57600:
                    byteChipCMD[1] = (byte)Baud.BAUD_57600;
                    break;
                case 115200:
                    byteChipCMD[1] = (byte)Baud.BAUD_115200;
                    break;
                case 230400:
                    byteChipCMD[1] = (byte)Baud.BAUD_230400;
                    break;
                case 460800:
                    byteChipCMD[1] = (byte)Baud.BAUD_460800;
                    break;
                case 921600:
                    byteChipCMD[1] = (byte)Baud.BAUD_921600;
                    break;
                case 1382400:
                    byteChipCMD[1] = (byte)Baud.BAUD_1382400;
                    break;
                default:
                    byteChipCMD[1] = (byte)Baud.BAUD_NONE;
                    break;
            }
            byteChipCMD[2] = 0;

            byteChipCMD[0] = (byte)UsbCommand.UART1;
            SendUSBMsg((byte)DataType.CHIP_CMD, byteChipCMD, 3);
            Thread.Sleep(100);

            byteChipCMD[0] = (byte)UsbCommand.UART2;
            SendUSBMsg((byte)DataType.CHIP_CMD, byteChipCMD, 3);
            Thread.Sleep(100);

            byteChipCMD[0] = (byte)UsbCommand.UART3;
            SendUSBMsg((byte)DataType.CHIP_CMD, byteChipCMD, 3);
        } catch (Exception ex)
        {
            ExceptionThrown?.Invoke(this, ex);
        }
    }

    Task SetPort(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            Task.Delay(500);
            if (PortStop || token.IsCancellationRequested)
                continue;
            if (serialPort.IsOpen == false)
            {
                OnlineBoard &= 0xfe;
                try
                {
                    serialPort.Open();
                    OnlineBoard |= 0x01;
                } catch (Exception err)
                {
                    ExceptionThrown?.Invoke(this, err);
                }
            } else
                OnlineBoard |= 0x01;
        }
        return Task.CompletedTask;
    }

    void UsbWindowMessageListener_OnDataReceived(object sender, DataRecievedEventArgs args)
    {
        byte byteLength = args.data[1];
        switch (args.data[2])
        {
            case (byte)DataType.CHIP_DATA:
                break;
            case (byte)DataType.MODULE_DATA:
                for (int i = 0; i < byteLength; i++)
                    args.data[i] = args.data[i + 3];
                DecodeData(args.data, byteLength);
                break;
        }
    }

    void UsbWindowMessageListener_OnMotorControllerConnected(object? sender, EventArgs e)
    {
        //  USB Motor Controller connected.
        OnlineBoard |= 0x02;
        SetBaudRate(serialPort.BaudRate);
    }

    void UsbWindowMessageListener_OnMotorControllerRemoved(object? sender, EventArgs e)
    {
        OnlineBoard &= 0xfd;
    }

    protected virtual void OnBoardIsOffline(object? sender, EventArgs e)
    {
        if (!isOnline)
            return;
        isOnline = false;
        BoardIsOffline?.Invoke(sender, e);
    }

    protected virtual void OnBoardIsOnline(object? sender, EventArgs e)
    {
        if (isOnline)
            return;
        isOnline = true;
        BoardIsOnline?.Invoke(sender, e);
    }

    /// <summary>
    /// I'm not sure what this actually did, but it seemed to be attached to the  sample buttons that were part of the
    /// motor controller application.
    /// </summary>
    /// <param name="Index"></param>
    internal void RunActionIndex(int Index)
    {
        if (Index == 16)
        {
            Send(0x0b, 0, 1);
        } else if (Index == 17)
        {
            Send(0x0b, 0, 0);
        } else
        {
            Send(0x09, 0, Index);
        }
    }

    public async void Close()
    {
        portDetectCancellationTokenSource.Cancel();
        await PortClose();
    }

    public void DecodeData(byte[] byteArray, ushort length)
    {
        lock (bufferLock)
        {
            ByteCopy(byteArray, rxBuffer, 0, rxCount, length);
            rxCount += length;
            while (rxCount >= totalLength)
            {
                if (CheckHead(rxBuffer, new byte[2] { 0xFF, 0xf0 }, 2) == false)
                {
                    ByteCopy(rxBuffer, rxBuffer, 1, 0, rxCount);
                    rxCount--;
                    continue;
                }
                if (rxBuffer[1] == 0xf0)
                {
                    short sID = BitConverter.ToInt16(rxBuffer, 3);
                    if ((sID == 0) | ((sID & 0xff) == 0xff))
                        boardID = 0;
                    //   onBoardID &= (ushort)(~(0x01 <<  BitConverter.ToInt16(byteTemp, 3)));
                }

                ByteCopy(rxBuffer, rxBuffer, totalLength, 0, (ushort)(rxCount - totalLength));
                rxCount -= totalLength;
            }
        }
    }

    public List<string> GetAvailableCommunicationPortNames()
    {
        return SerialPort.GetPortNames().ToList();
    }

    public void Send(byte cmd, byte ch, int data)
    {
        byte[] byteSend = new byte[5];
        short data16 = (short)data;
        byteSend[0] = 0xff;
        byteSend[1] = cmd;
        byteSend[2] = ch;
        byteSend[3] = (byte)(data16 & 0xff);
        byteSend[4] = (byte)(data16 >> 8);
        try
        {
            SendUSBMsg((byte)DataType.MODULE_CMD, byteSend, (byte)byteSend.Length);

            if (serialPort.IsOpen)
                serialPort.Write(byteSend, 0, 5);
            if (cmd == 0x00)
                return;
            if (OnlineBoard == 0)
                CommunicationError?.Invoke(
                this,
                $"Port not open, data to send are: {byteSend[0]:x} {byteSend[1]:x} {byteSend[2]:x} {byteSend[3]:x} {byteSend[4]:x}");
            else
                CommunicationSuccess?.Invoke(
                this,
                $"Successfully sent: {byteSend[0]:x} {byteSend[1]:x} {byteSend[2]:x} {byteSend[3]:x} {byteSend[4]:x}");
        } catch (Exception ex)
        {
            ExceptionThrown?.Invoke(this, ex);
        }
    }

    public void SendMessage(byte cmd, byte ch, int data)
    {
        message.command = (Command)(cmd | (id << 4));
        message.motorChannel = ch;
        message.data = data;
        Send((byte)message.command, message.motorChannel, message.data);
    }

    public byte Channel { get; set; } = 8;

    public int iMaxPWM = 2500;
    public int iMinPWM = 500;
    public void MoveToAngle(int angle)
    {
        var iAngleUs = (int)(iMinPWM + angle / 180.0 * (iMaxPWM - iMinPWM));
        SendMessage(2, Channel, iAngleUs);
    }

    public void SetSpeed(int angleDegreesPerSecond)
    {
        var speedIndex = angleDegreesPerSecond / 18;
        int speedValue = (speedIndex + 1) * 2;
        SendMessage(1, Channel, speedValue);
    }

    public event EventHandler? BoardIsOffline;

    public event EventHandler? BoardIsOnline;

    public event EventHandler<string>? CommunicationError;

    public event EventHandler<string>? CommunicationSuccess;

    public event EventHandler<Exception>? ExceptionThrown;
}
