using Amazon.CDK;
using Amazon.CDK.AWS.Route53;
using Amazon.CDK.AWS.CertificateManager;
using Amazon.CDK.AWS.IAM;
using Constructs;
using Infrastructure.Cdk.Config;

namespace Infrastructure.Cdk.Stacks;

public class SharedStack : Stack
{
    public IHostedZone HostedZone { get; }
    public ICertificate Certificate { get; }

    public SharedStack(Construct scope, string id, IStackProps? props = null) : base(scope, id, props)
    {
        // ============================================================
        // Route 53 - DNS
        // ============================================================
        HostedZone = Amazon.CDK.AWS.Route53.HostedZone.FromLookup(this, "HostedZone", new HostedZoneProviderProps
        {
            DomainName = EnvironmentConfig.RootDomain
        });

        // ============================================================
        // ACM Certificate (referenced, created in us-east-1)
        // ============================================================
        Certificate = Amazon.CDK.AWS.CertificateManager.Certificate.FromCertificateArn(
            this, "Certificate", EnvironmentConfig.CertificateArn);

        // ============================================================
        // SSM Parameters
        // ============================================================
        // Note: Riot API key is stored via CLI, not CDK, because it changes regularly
        // aws ssm put-parameter --name "/league-builds/riot-api-key" --value "KEY" --type SecureString

        // ============================================================
        // Budget Alert
        // ============================================================
        new CfnOutput(this, "DomainName", new CfnOutputProps
        {
            Value = EnvironmentConfig.RootDomain,
            Description = "Root domain"
        });
    }
}