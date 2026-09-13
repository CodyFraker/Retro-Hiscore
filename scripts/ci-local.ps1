param(
    [ValidateSet("all", "api", "web")]
    [string]$Job = "all"
)

$ErrorActionPreference = "Stop"
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
Set-Location $repoRoot

function Invoke-CiJob {
    param([string]$ServiceName)
    docker compose -f docker-compose.ci.yml run --rm $ServiceName
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }
}

switch ($Job) {
    "api" { Invoke-CiJob "ci-test-api" }
    "web" { Invoke-CiJob "ci-test-web" }
    default {
        Invoke-CiJob "ci-test-api"
        Invoke-CiJob "ci-test-web"
    }
}
