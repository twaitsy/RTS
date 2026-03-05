param(
    [string]$ProjectRoot = (Resolve-Path ".").Path,
    [string]$UnityEditorPath = $env:UNITY_EDITOR_PATH
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Find-ConflictMarkers {
    $files = Get-ChildItem -Path "Assets","ProjectSettings","Packages" -Recurse -File -ErrorAction SilentlyContinue
    if (-not $files) {
        return @()
    }

    return $files | Select-String -Pattern "^(<<<<<<<|=======|>>>>>>>)" -SimpleMatch:$false
}

Push-Location $ProjectRoot
try {
    Write-Host "[gate] scanning for merge conflict markers"
    $markers = @(Find-ConflictMarkers)
    if ($markers.Count -gt 0) {
        $markers | ForEach-Object { Write-Host ("{0}:{1}: {2}" -f $_.Path, $_.LineNumber, $_.Line.Trim()) }
        throw "merge conflict markers detected"
    }

    Write-Host "[gate] dotnet build"
    dotnet build "Assembly-CSharp.csproj" -nologo

    if ([string]::IsNullOrWhiteSpace($UnityEditorPath)) {
        Write-Warning "UNITY_EDITOR_PATH not set; skipping Unity definition validation gate."
        return
    }

    Write-Host "[gate] Unity definition validation"
    & $UnityEditorPath -batchmode -quit -projectPath $ProjectRoot -executeMethod DefinitionValidationMenu.ValidateAllForCI -logFile -

    if ($LASTEXITCODE -ne 0) {
        throw "Unity validation failed"
    }

    Write-Host "[gate] Unity canonical stat validation"
    & $UnityEditorPath -batchmode -quit -projectPath $ProjectRoot -executeMethod StatIdValidationMenu.ValidateCanonicalStatIdsForCI -logFile -
    if ($LASTEXITCODE -ne 0) {
        throw "Unity canonical stat validation failed"
    }

    Write-Host "[gate] Unity runtime smoke validation"
    & $UnityEditorPath -batchmode -quit -projectPath $ProjectRoot -executeMethod RuntimeSmokeValidationMenu.ValidateRuntimeSmokeForCI -logFile -
    if ($LASTEXITCODE -ne 0) {
        throw "Unity runtime smoke validation failed"
    }
}
finally {
    Pop-Location
}

