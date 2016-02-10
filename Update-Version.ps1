param ([Parameter(Mandatory = $true)][string] $version)


function UpdateVersion($path)
{
	Write-Host "Updating $path to $version"
	[xml]$manifest= get-content $path
	$manifest.Package.Identity.Version = $version
	$manifest.Save($path)
}

Get-ChildItem $PSScriptRoot -Include *.appxmanifest -Recurse | % { UpdateVersion $_.FullName }
