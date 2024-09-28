using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CliWrap;

namespace Tools;

public class DockerCommandBuilder {
    private const string _name = "docker";
    private readonly List<string> _args = new();
    private Command _command;
    
    public DockerCommandBuilder()
    {
        _command = Cli.Wrap(_name);
    }

    public async Task Run()
    {
        var stdOutBuffer = new StringBuilder();
        var errOutBuffer = new StringBuilder();

        _command = _command
            .WithArguments(_args.ToArray())
            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
            .WithStandardErrorPipe(PipeTarget.ToStringBuilder(errOutBuffer));

        await _command.ExecuteAsync();

        Console.WriteLine(stdOutBuffer.ToString());
        Console.WriteLine(errOutBuffer.ToString());
    }

    public DockerCommandBuilder Compose()
    {
        _args.Add("compose");
        return this;
    }
    
    public DockerCommandBuilder WithFile(string path)
    {
        _args.AddRange(["-f", path]);
        return this;
    }
    
    public DockerCommandBuilder Up()
    {
        _args.AddRange(["up", "-d", "--remove-orphans"]);
        return this;
    }

    public DockerCommandBuilder Down()
    {
        _args.Add("down");
        return this;
    }
}