Param(
    [string]$RootDir = $(Split-Path -Parent $MyInvocation.MyCommand.Path)
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$reportDir = Join-Path $RootDir "coverage-report"

Write-Host "Running tests with coverage..."
dotnet test $RootDir `
  /p:CollectCoverage=true `
  /p:CoverletOutputFormat="json,lcov,opencover" `
  /p:Exclude="[FinanceTracker.Infrastructure]*Migrations*%2c[FinanceTracker.API]*Program*%2c[FinanceTracker.Application]*DTOs*%2c[FinanceTracker.Domain]*DTOs*"

Write-Host "Installing ReportGenerator (if needed)..."
dotnet tool update -g dotnet-reportgenerator-globaltool *> $null 2>&1
if ($LASTEXITCODE -ne 0) {
  dotnet tool install -g dotnet-reportgenerator-globaltool
}

Write-Host "Generating coverage report..."
reportgenerator `
  "-reports:$RootDir/tests/**/coverage/*.opencover.xml" `
  "-targetdir:$reportDir" `
  "-reporttypes:HtmlInline_AzurePipelines;TextSummary"

Write-Host "Coverage summary:"
$summaryPath = Join-Path $reportDir "Summary.txt"
if (Test-Path $summaryPath) {
  Get-Content $summaryPath | Write-Host
}

Write-Host "HTML report generated at: $reportDir/index.html"

