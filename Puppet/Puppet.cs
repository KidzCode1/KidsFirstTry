
using System;

public class Puppet
{
    public string Name { get; set; }

    public Puppet(string name)
    {
        Name = name;
    }

    public Puppet()
    {

    }
    public string GetInitialPrompt()
    {
        return Prompts.Initial;
    }
}
