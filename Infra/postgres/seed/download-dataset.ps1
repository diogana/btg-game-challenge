$ErrorActionPreference = 'Stop'
$archivePath = Join-Path ([System.IO.Path]::GetTempPath()) ([System.IO.Path]::GetRandomFileName())
try {
    Invoke-WebRequest -Uri 'https://www.kaggle.com/api/v1/datasets/download/evgeny1928/playstation-games-info' -OutFile $archivePath
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $zip = [System.IO.Compression.ZipFile]::OpenRead($archivePath)
    try {
        $entry = $zip.GetEntry('output.json')
        if ($null -eq $entry) { throw 'output.json not found in archive' }
        [System.IO.Compression.ZipFileExtensions]::ExtractToFile($entry, (Join-Path $PSScriptRoot 'output.json'), $true)
    } finally { $zip.Dispose() }
    Write-Output 'Dataset ready'
} finally { Remove-Item $archivePath -ErrorAction SilentlyContinue }
