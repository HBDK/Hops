using Tools;
using System;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Linq;

var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

var path = config["repoPath"] ?? throw new ArgumentNullException("repoPath");
var repo = new GitRepo(path);
if(!repo.Exsists())
{
    await repo.SshClone(config["remote"] ?? throw new ArgumentNullException("repoPath"));
}
await repo.SshFetch();
await repo.SshCheckOutAndReset(config["trackingBranch"] ?? throw new ArgumentNullException("repoPath"));

foreach(var folder in Directory.GetDirectories(path).Where(x => !x.StartsWith($"{path}/.")))
{
    Console.WriteLine(folder);
    var composeFile = Directory.GetFiles(folder).Where(x => x.EndsWith("yaml")).FirstOrDefault();
    if(composeFile is null)
    {
       continue;
    }
    Console.WriteLine(composeFile);
    await new DockerCommandBuilder().Compose().WithFile(composeFile).Up().Run();
}