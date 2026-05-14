using Amazon.CDK;
using Amazon.CDK.AWS.CodeBuild;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.SNS;
using Amazon.CDK.AWS.SNS.Subscriptions;
using Amazon.CDK.AWS.Events.Targets;
using Constructs;
using Infrastructure.Cdk.Config;

namespace Infrastructure.Cdk.Stacks;

public class CiCdStack : Stack
{
    public CiCdStack(Construct scope, string id, IStackProps? props, PortfolioStack portfolio) : base(scope, id, props)
    {
        // ============================================================
        // Notifications
        // ============================================================
        var notificationTopic = new Topic(this, "BuildNotifications", new TopicProps
        {
            TopicName = "build-notifications"
        });

        notificationTopic.AddSubscription(new EmailSubscription("edgoran@gmail.com"));

        // ============================================================
        // Portfolio Site - CodeBuild
        // ============================================================
        var portfolioBuild = new Project(this, "PortfolioBuild", new ProjectProps
        {
            ProjectName = "portfolio-site-deploy",
            Description = "Deploys portfolio site to S3/CloudFront",
            Source = Source.GitHub(new GitHubSourceProps
            {
                Owner = EnvironmentConfig.GitHubOwner,
                Repo = EnvironmentConfig.PortfolioRepo,
                BranchOrRef = "main",
                Webhook = true,
                WebhookFilters = new[]
                {
                    FilterGroup.InEventOf(EventAction.PUSH).AndBranchIs("main")
                }
            }),
            Environment = new BuildEnvironment
            {
                BuildImage = LinuxBuildImage.STANDARD_7_0,
                ComputeType = ComputeType.SMALL
            },
            BuildSpec = BuildSpec.FromObject(new Dictionary<string, object>
            {
                ["version"] = "0.2",
                ["phases"] = new Dictionary<string, object>
                {
                    ["build"] = new Dictionary<string, object>
                    {
                        ["commands"] = new[]
                        {
                            $"aws s3 sync site/ s3://{portfolio.SiteBucket.BucketName} --delete",
                            $"aws cloudfront create-invalidation --distribution-id {portfolio.Distribution.DistributionId} --paths '/*'"
                        }
                    }
                }
            })
        });

        // Permissions
        portfolio.SiteBucket.GrantReadWrite(portfolioBuild);

        portfolioBuild.AddToRolePolicy(new PolicyStatement(new PolicyStatementProps
        {
            Effect = Effect.ALLOW,
            Actions = new[] { "cloudfront:CreateInvalidation" },
            Resources = new[] { $"arn:aws:cloudfront::{Account}:distribution/{portfolio.Distribution.DistributionId}" }
        }));

        // Notifications
        portfolioBuild.OnBuildSucceeded("PortfolioBuildSuccess", new Amazon.CDK.AWS.Events.OnEventOptions
        {
            Target = new SnsTopic(notificationTopic, new SnsTopicProps
            {
                Message = Amazon.CDK.AWS.Events.RuleTargetInput.FromText("Portfolio site deployed successfully")
            })
        });

        portfolioBuild.OnBuildFailed("PortfolioBuildFailed", new Amazon.CDK.AWS.Events.OnEventOptions
        {
            Target = new SnsTopic(notificationTopic, new SnsTopicProps
            {
                Message = Amazon.CDK.AWS.Events.RuleTargetInput.FromText("Portfolio site deployment FAILED - check CodeBuild logs")
            })
        });
    }
}