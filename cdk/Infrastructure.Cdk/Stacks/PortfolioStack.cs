using Amazon.CDK;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.CloudFront;
using Amazon.CDK.AWS.CloudFront.Origins;
using Amazon.CDK.AWS.Route53;
using Amazon.CDK.AWS.Route53.Targets;
using Constructs;
using Infrastructure.Cdk.Config;

namespace Infrastructure.Cdk.Stacks;

public class PortfolioStack : Stack
{
    public IBucket SiteBucket { get; }
    public IDistribution Distribution { get; }

    public PortfolioStack(Construct scope, string id, IStackProps? props, SharedStack shared) : base(scope, id, props)
    {
        // ============================================================
        // S3 Bucket
        // ============================================================
        SiteBucket = new Bucket(this, "PortfolioBucket", new BucketProps
        {
            BucketName = $"edgoran-portfolio-{Account}",
            RemovalPolicy = RemovalPolicy.DESTROY,
            AutoDeleteObjects = true,
            BlockPublicAccess = BlockPublicAccess.BLOCK_ALL
        });

        // ============================================================
        // CloudFront Distribution
        // ============================================================
        Distribution = new Distribution(this, "PortfolioDistribution", new DistributionProps
        {
            DefaultBehavior = new BehaviorOptions
            {
                Origin = S3BucketOrigin.WithOriginAccessControl(SiteBucket),
                ViewerProtocolPolicy = ViewerProtocolPolicy.REDIRECT_TO_HTTPS
            },
            DefaultRootObject = "index.html",
            DomainNames = new[] { EnvironmentConfig.PortfolioDomain },
            Certificate = shared.Certificate,
            ErrorResponses = new[]
            {
                new ErrorResponse
                {
                    HttpStatus = 404,
                    ResponseHttpStatus = 200,
                    ResponsePagePath = "/index.html"
                }
            }
        });

        // ============================================================
        // DNS Record
        // ============================================================
        new ARecord(this, "PortfolioAliasRecord", new ARecordProps
        {
            Zone = shared.HostedZone,
            Target = RecordTarget.FromAlias(new CloudFrontTarget((Distribution)Distribution)),
            RecordName = EnvironmentConfig.PortfolioSubdomain
        });

        // ============================================================
        // Outputs
        // ============================================================
        new CfnOutput(this, "SiteUrl", new CfnOutputProps
        {
            Value = $"https://{EnvironmentConfig.PortfolioDomain}",
            Description = "Portfolio site URL"
        });

        new CfnOutput(this, "BucketName", new CfnOutputProps
        {
            Value = SiteBucket.BucketName,
            Description = "S3 bucket name"
        });

        new CfnOutput(this, "DistributionId", new CfnOutputProps
        {
            Value = Distribution.DistributionId,
            Description = "CloudFront distribution ID"
        });
    }
}