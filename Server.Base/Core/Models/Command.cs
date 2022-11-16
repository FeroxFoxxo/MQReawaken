namespace Server.Base.Core.Models;

public class Command
{
    public delegate void RunConsoleCommand(string[] command);

    public string Name { get; set; }
    public string Description { get; set; }
    public RunConsoleCommand CommandMethod { get; set; }

    public Command(string name, string description, RunConsoleCommand commandMethod)
    {
        CommandMethod = commandMethod;
        Name = name;
        Description = description;
    }
}
