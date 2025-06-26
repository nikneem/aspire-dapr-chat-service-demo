# GitHub Actions Setup for Infrastructure Deployment

This document explains how to configure GitHub Actions for automated infrastructure deployment.

## Required Azure Setup

### 1. Create Azure Service Principal

You need to create a service principal with appropriate permissions for your Azure subscription:

```bash
# Create service principal
az ad sp create-for-rbac \
  --name "hexchat-github-actions" \
  --role "Contributor" \
  --scopes "/subscriptions/{subscription-id}" \
  --sdk-auth

# For federated credentials (recommended)
az ad sp create-for-rbac \
  --name "hexchat-github-actions" \
  --role "Contributor" \
  --scopes "/subscriptions/{subscription-id}"
```

### 2. Setup Federated Identity (Recommended)

For enhanced security, use OpenID Connect instead of storing secrets:

```bash
# Get the application ID
APP_ID=$(az ad app list --display-name "hexchat-github-actions" --query "[0].appId" -o tsv)

# Create federated credential for main branch
az ad app federated-credential create \
  --id $APP_ID \
  --parameters '{
    "name": "hexchat-main-branch",
    "issuer": "https://token.actions.githubusercontent.com",
    "subject": "repo:YOUR_GITHUB_USERNAME/aspire-dapr-chat-service-demo:ref:refs/heads/main",
    "audiences": ["api://AzureADTokenExchange"]
  }'

# Create federated credential for pull requests
az ad app federated-credential create \
  --id $APP_ID \
  --parameters '{
    "name": "hexchat-pull-requests",
    "issuer": "https://token.actions.githubusercontent.com",
    "subject": "repo:YOUR_GITHUB_USERNAME/aspire-dapr-chat-service-demo:pull_request",
    "audiences": ["api://AzureADTokenExchange"]
  }'
```

## Required GitHub Secrets

Navigate to your GitHub repository → Settings → Secrets and variables → Actions

Add the following **Repository secrets**:

| Secret Name | Description | Example Value |
|-------------|-------------|---------------|
| `AZURE_CLIENT_ID` | Application (client) ID | `12345678-1234-1234-1234-123456789012` |
| `AZURE_TENANT_ID` | Directory (tenant) ID | `87654321-4321-4321-4321-210987654321` |
| `AZURE_SUBSCRIPTION_ID` | Azure subscription ID | `abcdefgh-abcd-abcd-abcd-abcdefghijkl` |

> **Note**: If not using federated identity, you'll also need `AZURE_CLIENT_SECRET`

## GitHub Environments Setup

Create the following environments in your repository (Settings → Environments):

### 1. Development Environment
- **Name**: `development`
- **Protection rules**: None (for fast iteration)
- **Deployment branches**: Any branch

### 2. Staging Environment  
- **Name**: `staging`
- **Protection rules**: 
  - Required reviewers (optional)
  - Wait timer: 0 minutes
- **Deployment branches**: Any branch

### 3. Production Environment
- **Name**: `production`
- **Protection rules**:
  - Required reviewers (recommended)
  - Wait timer: 5 minutes (recommended)
- **Deployment branches**: Selected branches only
  - Add: `main`

## Workflow Triggers

### Automatic Triggers

1. **Pull Request** (Development deployment)
   - Triggers when: PR opened, updated, or reopened
   - Condition: Changes in `infrastructure/**` folder
   - Deploys to: Development environment
   - Special: Adds deployment status comment to PR

2. **Push to Main** (Production deployment)
   - Triggers when: Code pushed to main branch
   - Condition: Changes in `infrastructure/**` folder  
   - Deploys to: Production environment

3. **PR Closed** (Cleanup)
   - Triggers when: PR is closed
   - Action: Deletes development resource group (optional)

### Manual Trigger

**Workflow Dispatch**: Allows manual deployment to any environment
- Available environments: `dev`, `staging`, `prod`
- Can be triggered from GitHub Actions tab

## Workflow Features

### ✅ Template Validation
- Validates all Bicep templates before deployment
- Fails fast if templates have syntax errors

### ✅ What-If Analysis  
- Shows what changes will be made before deployment
- Helps prevent unexpected resource modifications

### ✅ Environment Protection
- Uses GitHub Environments for deployment approval
- Production requires manual approval (configurable)

### ✅ Artifact Storage
- Stores deployment outputs as artifacts
- Available for 30 days (dev/staging) or 90 days (prod)

### ✅ PR Integration
- Comments on PRs with deployment status
- Shows deployed resource information

### ✅ Deployment Naming
- Uses timestamps for unique deployment names
- Easy to track in Azure Portal

## Security Best Practices

1. **Use Federated Identity**: Avoid storing long-lived secrets
2. **Least Privilege**: Service principal has only necessary permissions
3. **Environment Protection**: Production requires approval
4. **Branch Protection**: Only main branch can deploy to production
5. **Audit Trail**: All deployments are logged and tracked

## Troubleshooting

### Common Issues

1. **Authentication Failures**
   - Verify service principal permissions
   - Check federated identity configuration
   - Ensure secrets are correctly set

2. **Bicep Validation Errors**
   - Check Bicep syntax in templates
   - Verify parameter file format
   - Ensure all required parameters are provided

3. **Deployment Failures**
   - Check Azure resource quotas
   - Verify resource naming conflicts
   - Review deployment logs in Azure Portal

### Debugging Commands

```bash
# Test service principal authentication
az login --service-principal \
  --username $AZURE_CLIENT_ID \
  --password $AZURE_CLIENT_SECRET \
  --tenant $AZURE_TENANT_ID

# Validate Bicep template locally
az bicep build --file infrastructure/main.bicep

# Test deployment with what-if
az deployment sub what-if \
  --location "West Europe" \
  --template-file infrastructure/main.bicep \
  --parameters infrastructure/main.dev.bicepparam
```

## Monitoring

### GitHub Actions
- Monitor workflow runs in the Actions tab
- Check deployment artifacts for detailed outputs
- Review PR comments for deployment status

### Azure Portal
- Track deployments in Resource Group → Deployments
- Monitor resource health and costs
- Set up alerts for deployment failures

## Cost Management

The workflow includes automatic cleanup of development resources when PRs are closed to minimize costs. You can customize this behavior by modifying the cleanup job in the workflow file.
