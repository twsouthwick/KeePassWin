param ([Parameter(Mandatory = $true)][string] $version)


function UpdateVersion($path)
{
	Write-Host "Updating $path to $version"
	[xml]$manifest= get-content $path
	$manifest.Package.Identity.Version = $version
	
	# Remove any beta monikers
	$manifest.Package.Identity.Name = $manifest.Package.Identity.Name.Replace("Beta", "")
	$manifest.Package.Properties.DisplayName = $manifest.Package.Properties.DisplayName.Replace("(Beta)", "").Trim()
	$manifest.Package.Applications.Application.VisualElements.DisplayName = $manifest.Package.Applications.Application.VisualElements.DisplayName.Replace("(Beta)", "").Trim()
	
	$manifest.Save($path)
}

Get-ChildItem $PSScriptRoot -Include *.appxmanifest -Recurse | % { UpdateVersion $_.FullName }
