Function GetVersionFromGitHubRef {
    Param (
        # refs/tags/v* 
        # refs/heads/release/*
        
        [string]$GitHubRef
    )

    $segments = $GitHubRef -split '/'
    $versionSegment = $segments[-1]

    Write-Host "Version segment: '$versionSegment'"

    # SemVer Regex with optional 'v' character at the beginning.
    $versionRegex = '^(v)?(?<major>0|[1-9]\d*)\.(?<minor>0|[1-9]\d*)\.(?<patch>0|[1-9]\d*)(?:-(?<prerelease>(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+(?<buildmetadata>[0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$'

    if ($versionSegment -match $versionRegex) {

        if ($matches -eq $null) {
            return $null
        }
        
        $MajorTag = $matches['major']
        $MinorTag = $matches['minor']
        $PatchTag = $matches['patch']

        $PreReleaseTag = $matches['prerelease']
        $BuildMetadataTag = $matches['buildmetadata']

        $Version = "$MajorTag.$MinorTag.$PatchTag"

        if ($PreReleaseTag) {
            $Version += "-$PreReleaseTag"
        }

        if ($BuildMetadataTag) {
            $Version += "+$BuildMetadataTag"
        }

        return $Version
    } else {
        return $null
    }
}