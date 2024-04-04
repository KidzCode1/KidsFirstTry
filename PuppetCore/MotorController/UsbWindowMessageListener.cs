
using System;
using System.ComponentModel;
using System.IO.Ports;
using System.Windows.Forms;
using UsbLibrary;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

public class UsbWindowMessageListener : Form
{
    /// <summary>
    /// The event that occurs when the USB motor controller is connected.
    /// </summary>
    public event EventHandler? OnMotorControllerConnected;

    /// <summary>
    /// The event that occurs when the USB motor controller is disconnected.
    /// </summary>
    public event EventHandler? OnMotorControllerRemoved;

    /// <summary>
    /// The event that occurs when data is received from the embedded system
    /// </summary>
    public event DataRecievedEventHandler? OnDataReceived;

    /// <summary>
    /// Throne when an exception is caught.
    /// </summary>
    public event EventHandler<Exception>? ExceptionThrown;


    UsbHidPort usb;
    public UsbWindowMessageListener()
    {
        usb = new UsbHidPort();
        usb.ProductId = 256;
        usb.VendorId = 6432;
        usb.OnDataRecieved += usb_OnDataReceived;
        usb.OnSpecifiedDeviceArrived += usb_OnSpecifiedDeviceArrived;
        usb.OnSpecifiedDeviceRemoved += usb_OnSpecifiedDeviceRemoved;
        Opacity = 0;
        ShowInTaskbar = false;
        FormBorderStyle = FormBorderStyle.None;
        Load += UsbWindowMessageListener_Load;

    }

    private void UsbWindowMessageListener_Load(object? sender, EventArgs e)
    {
        Size = new Size(0, 0);
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        try
        {
            base.OnHandleCreated(e);
            usb.RegisterHandle(Handle);
        }
        catch (Exception ex) {
            ExceptionThrown?.Invoke(this, ex);
        }
    }
    protected override void WndProc(ref System.Windows.Forms.Message m)
    {
        try
        {
            usb.ParseMessages(ref m);
            base.WndProc(ref m);    // pass message on to base form
        }
        catch (Exception ex)
        {
            ExceptionThrown?.Invoke(this, ex);
        }
    }

    private void usb_OnDataReceived(object sender, DataRecievedEventArgs args)
    {
        if (InvokeRequired)
        {
            try
            {
                Invoke(new DataRecievedEventHandler(usb_OnDataReceived), new object[] { sender, args });
            }
            catch (Exception ex)
            {
                ExceptionThrown?.Invoke(this, ex);
            }
        }
        else
        {
            OnDataReceived?.Invoke(sender, args);
        }
    }

    private void usb_OnSpecifiedDeviceArrived(object? sender, EventArgs e)
    {
        OnMotorControllerConnected?.Invoke(sender, e);
    }

    private void usb_OnSpecifiedDeviceRemoved(object? sender, EventArgs e)
    {
        OnMotorControllerRemoved?.Invoke(sender, e);
    }

    public SpecifiedDevice SpecifiedDevice => usb.SpecifiedDevice;
}
