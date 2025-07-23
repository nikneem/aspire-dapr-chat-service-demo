# SonarCloud Coverage Configuration

This document explains the fix for unit test coverage not being correctly published to SonarCloud.

## Problem

The original SonarCloud workflow was not correctly finding and publishing test coverage reports because:

1. **Incorrect path pattern**: The pattern `**/coverage.opencover.xml` was not matching the actual location of coverage files
2. **Missing results directory**: Tests were generating coverage files in random GUID directories without a predictable path structure

## Solution

### Changes Made

1. **Updated coverage path pattern** in `.github/workflows/pr-check.yml`:
   - Changed from: `**/coverage.opencover.xml`
   - Changed to: `TestResults/**/coverage.opencover.xml`

2. **Standardized results directory**:
   - Added `--results-directory TestResults` to the `dotnet test` command
   - This ensures coverage files are always generated in a predictable location

### How It Works

1. Tests run with: `dotnet test src --collect:"XPlat Code Coverage;Format=opencover" --results-directory TestResults`
2. Coverage files are generated in: `TestResults/{guid}/coverage.opencover.xml`
3. SonarCloud finds them using pattern: `TestResults/**/coverage.opencover.xml`

### Verification

The fix can be verified by:

1. Running tests with coverage collection
2. Checking that files are generated in `TestResults/*/coverage.opencover.xml`
3. Verifying the pattern matches: `ls TestResults/**/coverage.opencover.xml`

## Coverage File Format

The coverage files are generated in OpenCover XML format and include:
- Sequence coverage metrics
- Branch coverage metrics  
- Absolute file paths for proper SonarCloud integration
- Module and method level coverage data