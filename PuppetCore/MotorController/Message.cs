
using System;
public struct Message
{
    public Command command;
    public byte motorChannel;
    public int data;
    public Message(Command command, byte motorChannel, int data)
    {
        this.command = command;
        this.motorChannel = motorChannel;
        this.data = data;
    }
}
