using Amazon.CDK;
using Infrastructure.Cdk.Config;
using Infrastructure.Cdk.Stacks;

namespace Infrastructure.Cdk;

class Program
{
    static void Main(string[] args)
    {
        var app = new App();

        var env = new Amazon.CDK.Environment
        {
            Account = EnvironmentConfig.AccountId,
            Region = EnvironmentConfig.PrimaryRegion
        };

        // Shared resources (domain, certificates)
        var sharedStack = new SharedStack(app, "SharedStack", new StackProps { Env = env });

        // Portfolio site
        var portfolioStack = new PortfolioStack(app, "PortfolioStack", new StackProps { Env = env }, sharedStack);

        // CI/CD pipelines
        var cicdStack = new CiCdStack(app, "CiCdStack", new StackProps { Env = env }, portfolioStack);

        app.Synth();
    }
}