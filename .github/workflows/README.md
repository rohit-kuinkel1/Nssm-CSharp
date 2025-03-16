# GitHub Workflows for NSSM-C#

This directory contains GitHub Actions workflow configurations that automate testing and quality checks for the NSSM-C# project.

## Available Workflows

### 1. Build and Test (`ci.yml`)

This workflow runs on every push to the main/master branch and on pull requests:

- Sets up .NET 8.0 environment
- Restores dependencies
- Builds the solution
- Runs all tests
- Creates and uploads artifacts for deployment:
  - Standalone executables (CLI and WPF)
  - ZIP packages of both applications
  - TAR.GZ packages of both applications

Use this workflow to ensure that your code builds correctly and all tests pass before merging.

### 2. Code Quality and Coverage (`code-quality.yml`)

This workflow provides more comprehensive testing with code coverage:

- Sets up .NET 8.0 environment
- Builds the solution
- Runs tests with code coverage collection
- Uploads coverage reports to Codecov (if configured)

This workflow can be triggered manually via the GitHub Actions interface using the "workflow_dispatch" event.

## Artifacts

After a successful workflow run, the following artifacts are automatically created and can be downloaded from the GitHub Actions page:

| Artifact Name | Description |
|---------------|-------------|
| `nssm-cli-{branch}` | CLI executable (single file) |
| `nssm-wpf-{branch}` | WPF executable (single file) |
| `nssm-zip-packages-{branch}` | ZIP archives of both applications |
| `nssm-targz-packages-{branch}` | TAR.GZ archives of both applications |

These artifacts are labeled with the branch name and make it easy to distribute the latest builds without creating a formal release.

## Configuring Codecov

To enable code coverage reporting with Codecov:

1. Sign up for a free account at [codecov.io](https://codecov.io/)
2. Add your GitHub repository to Codecov
3. Add a Codecov token to your repository secrets:
   - Go to your GitHub repository → Settings → Secrets → Actions
   - Add a new secret with name `CODECOV_TOKEN` and the value from your Codecov account

## Adding More Tests

To add more tests to the project:

1. Create test classes in the existing test projects
2. Or create new test projects following the same pattern as existing test projects
3. The workflows will automatically discover and run all tests in projects with `IsTestProject` set to `true`

Remember to ensure new test projects reference the necessary test packages (NUnit, etc.)
