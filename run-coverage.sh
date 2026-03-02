#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPORT_DIR="$ROOT_DIR/coverage-report"

echo "Running tests with coverage..."
dotnet test "$ROOT_DIR" \
  /p:CollectCoverage=true \
  /p:CoverletOutputFormat=\"json,lcov,opencover\" \
  /p:Exclude=\"[FinanceTracker.Infrastructure]*Migrations*%2c[FinanceTracker.API]*Program*%2c[FinanceTracker.Application]*DTOs*%2c[FinanceTracker.Domain]*DTOs*\"

echo "Installing ReportGenerator (if needed)..."
dotnet tool update -g dotnet-reportgenerator-globaltool >/dev/null 2>&1 || dotnet tool install -g dotnet-reportgenerator-globaltool

echo "Generating coverage report..."
reportgenerator \
  -reports:\"$ROOT_DIR/tests/**/coverage/*.opencover.xml\" \
  -targetdir:\"$REPORT_DIR\" \
  -reporttypes:HtmlInline_AzurePipelines;TextSummary

echo "Coverage summary:"
if [ -f "$REPORT_DIR/Summary.txt" ]; then
  cat "$REPORT_DIR/Summary.txt"
fi

echo "HTML report generated at: $REPORT_DIR/index.html"

