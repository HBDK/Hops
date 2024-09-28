using System;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using CliWrap;
using System.Threading.Tasks;
using System.Text;

public class GitRepo(string context)
{
    public bool Exsists()
    {
        try
        {
            using var repo = new Repository(context);
            return true;
    
        }
        catch (Exception)
        {   
            return false;
        }   
    }

    public void Clone(string remote)
    {
        Repository.Clone(remote, context);
    }

    public async Task SshClone(string remote)
    {
        var errorStringBuilder = new StringBuilder();
        var stdStringBuilder = new StringBuilder();
        try
        {
            await Cli.Wrap("git").WithWorkingDirectory("/tmp").WithArguments(["clone", remote, context]).WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdStringBuilder)).WithStandardErrorPipe(PipeTarget.ToStringBuilder(errorStringBuilder)).ExecuteAsync();
            
        }
        catch (Exception)
        {
            Console.WriteLine(stdStringBuilder.ToString());
            Console.WriteLine(errorStringBuilder.ToString());
            throw;
        }
    }

    public void Fetch()
    {
        var log = "";
        using var repo = new Repository(context);
        var remote = repo.Network.Remotes["origin"];
        var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
        Commands.Fetch(repo, remote.Name, refSpecs, null, log);
        Console.WriteLine(log);
    }

    public async Task SshFetch()
    {

        var errorStringBuilder = new StringBuilder();
        var stdStringBuilder = new StringBuilder();
        try
        {
            await Cli.Wrap("git").WithWorkingDirectory(context).WithArguments(["fetch"]).WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdStringBuilder)).WithStandardErrorPipe(PipeTarget.ToStringBuilder(errorStringBuilder)).ExecuteAsync();
            
        }
        catch (Exception)
        {
            Console.WriteLine(stdStringBuilder.ToString());
            Console.WriteLine(errorStringBuilder.ToString());
            throw;
        }
    }

    public async Task SshCheckOutAndReset(string branch = "master")
    {

        var errorStringBuilder = new StringBuilder();
        var stdStringBuilder = new StringBuilder();
        try
        {
            await Cli.Wrap("git").WithWorkingDirectory(context).WithArguments(["checkout", "-b", branch, "--force"]).WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdStringBuilder)).WithStandardErrorPipe(PipeTarget.ToStringBuilder(errorStringBuilder)).ExecuteAsync();
            await Cli.Wrap("git").WithWorkingDirectory(context).WithArguments(["reset", "--hard", "HEAD"]).WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdStringBuilder)).WithStandardErrorPipe(PipeTarget.ToStringBuilder(errorStringBuilder)).ExecuteAsync();
            YeetUntracked();
        }
        catch (Exception)
        {
            Console.WriteLine(stdStringBuilder.ToString());
            Console.WriteLine(errorStringBuilder.ToString());
            throw;
        }
    }

    public void CheckOutAndReset(string branchName = "master")
    {
        using var repo = new Repository(context);
        
        var branch = repo.Branches[branchName];
        if(branch is null)
        {
            Console.WriteLine("branch does not exsist");
            return;
        }
        Commands.Checkout(repo, branch, GetOptions());
        branch = repo.Branches[branchName];
        repo.Reset(ResetMode.Hard, branch.Tip);
        YeetUntracked();
    }

    private void YeetUntracked()
    {
        using var repo = new Repository(context);
        foreach (var item in repo.RetrieveStatus())
        {
            Console.WriteLine(item.FilePath);
            if(item.State == FileStatus.NewInWorkdir)
            {
                File.Delete(context + "/" + item.FilePath);
            }
        }
    }

    private static CheckoutOptions GetOptions()
    {
        var options = new CheckoutOptions();
        options.CheckoutModifiers = CheckoutModifiers.Force;
        return options;
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}