
using System;

public class Puppet
{
    public string Name { get; set; }

    public Puppet(string name)
    {
        Name = name;
    }

    public string GetInitialPrompt()
    {
        return Prompts.Initial;
    }
}
