namespace Infrastructure.Cdk.Config;

public static class EnvironmentConfig
{
    // Domain
    public static string RootDomain => "edgoran.co.uk";
    public static string PortfolioSubdomain => "portfolio";
    public static string PortfolioDomain => $"{PortfolioSubdomain}.{RootDomain}";

    // GitHub
    public static string GitHubOwner => "edgoran";
    public static string PortfolioRepo => "portfolio-site";
    public static string LeagueStatsRepo => "league-builds";

    // Region
    public static string PrimaryRegion => "eu-west-2";

    // These come from environment variables (never hardcoded)
    public static string AccountId => System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT")
        ?? throw new System.Exception("CDK_DEFAULT_ACCOUNT not set");

    public static string CertificateArn => System.Environment.GetEnvironmentVariable("CERTIFICATE_ARN")
        ?? throw new System.Exception("CERTIFICATE_ARN not set");
}