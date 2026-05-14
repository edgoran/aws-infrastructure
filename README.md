# AWS Infrastructure

Infrastructure as code for all my AWS resources, defined using AWS CDK in C#.

## Stacks

| Stack | Resources | Purpose |
|-------|-----------|---------|
| SharedStack | Route 53, ACM Certificate | Shared domain and SSL |
| PortfolioStack | S3, CloudFront, DNS record | Portfolio website hosting |
| CiCdStack | CodeBuild, SNS | Continuous deployment pipelines |

## Architecture

```text
edgoran.co.uk (Route 53)
    |
    |-- portfolio.edgoran.co.uk
    |   +-- CloudFront -> S3 (portfolio site)
    |
    +-- (future subdomains)

GitHub Push -> CodeBuild -> S3 + CloudFront Invalidation
                |
                +-- SNS -> Email Notification
```

## Prerequisites

- AWS CLI configured
- .NET 8 SDK
- AWS CDK CLI
- Environment variables:
  - CDK_DEFAULT_ACCOUNT: AWS account ID
  - CERTIFICATE_ARN: ACM certificate ARN (us-east-1)

## Deployment

Deploy all stacks:

```bash
cd cdk
dotnet build Infrastructure.Cdk
cdk deploy --all
```

Deploy a specific stack:

```bash
cdk deploy PortfolioStack
```

## Cost

| Service | Monthly Cost |
|---------|-------------|
| Route 53 hosted zone | $0.50 |
| S3 | ~$0.01 |
| CloudFront | ~$0.00 |
| CodeBuild | Free (100 mins/month) |
| SNS | Free |
| Total | Under $1/month |

## Project Repos

| Project | Repo | Deployed By |
|---------|------|-------------|
| Portfolio Site | github.com/edgoran/portfolio-site | CodeBuild (auto on push) |
| League Stats | github.com/edgoran/league-builds | Manual |