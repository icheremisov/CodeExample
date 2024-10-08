param(

    [string] $Branch = "",
    [string] $Root = $null
)

Set-StrictMode -Version latest
$ErrorActionPreference = "Stop"

# Taken from psake https://github.com/psake/psake
<#
.SYNOPSIS
  This is a helper function that runs a scriptblock and checks the PS variable $lastexitcode
  to see if an error occcured. If an error is detected then an exception is thrown.
  This function allows you to run command-line programs without having to
  explicitly check the $lastexitcode variable.
.EXAMPLE
  exec { svn info $repository_trunk } "Error executing SVN. Please verify SVN command-line client is installed"
#>
function exec
{
    [CmdletBinding()]
    param(
        [Parameter(Position=0,Mandatory=1)][scriptblock]$cmd,
        [Parameter(Position=1,Mandatory=0)][string]$errorMessage = ("Error executing command {0}" -f $cmd)
    )
    & $cmd
    if ($lastexitcode -ne 0) {
        throw ("exec: " + $errorMessage)
    }
}

if(!$Root) {
    $LengthLimit = 30;
    $Root = "./"
    while((Test-Path -Path $Root/.git/HEAD -PathType Leaf) -eq $false) {
        $Root += "../"
        if ($Root.Length -gt $LengthLimit) {
            "Не удается определить рутовую директорию"
            exit 1;
        }
    }
}
"ROOT: $Root"

if($Branch -eq "") {
    $Branch = (Get-Content "${Root}/.git/HEAD").Replace("ref: refs/heads/", "")
}

"BRANCH: $Branch"

function Update-Package {
    param(
	[string] $name,
	[string] $url,
	[string] $branch,
	[string] $subdir = "*",
	[string] $ignore = $null
)
	if (Test-Path -Path ./$name.$branch.tgz ) { 
		"./$name.$branch.tgz is actual version. Skip"
		return
	 }

	if (Test-Path -Path ./$name) {
		Remove-Item -Recurse -Force ./$name
	}
	if (Test-Path -Path ./$name*.tgz) {
		Remove-Item -Recurse -Force ./$name*.tgz
	}

	exec { git clone --depth 1 -b $branch $url ./$name/repo }
	exec { cd ./$name/ && mkdir ./package }
	Move-Item -Path ./repo/$subdir -Destination ./package
	Remove-Item -Recurse -Force ./repo
	if ($ignore) {
		Remove-Item -Recurse -Force ./package/$ignore
	}
	exec { tar -cvzf "$name.tgz" package }
	exec { cd ./../ }
 	Move-Item -Path ./$name/$name.tgz -Destination ./$name.$branch.tgz
	Remove-Item -Recurse -Force ./$name
}

$args = @{
	name="com.coffee.ui-particle"
	url="https://github.com/mob-sakai/ParticleEffectForUGUI.git"
	branch="v4.9.1"
	subdir="Packages/src/*"
}
Update-Package @args

$args = @{
	name="com.elringus.spritedicing"
	url="https://github.com/elringus/sprite-dicing.git"
	branch="v2.0.1"
	subdir="/plugins/unity/Assets/SpriteDicing/*"
}
Update-Package @args

$args = @{
	name="com.redbluegames.mulligan"
	url="https://github.com/redbluegames/unity-mulligan-renamer.git"
	branch="v1.7.8"
	subdir="/Assets/RedBlueGames/MulliganRenamer/*"
}
Update-Package @args

$args = @{
	name="com.sabresaurus.playerprefseditor"
	url="https://github.com/sabresaurus/PlayerPrefsEditor.git"
	branch="1.4.1"
}
Update-Package @args

$args = @{
	name="com.svermeulen.extenject"
	url="https://github.com/modesttree/Zenject.git"
	branch="9.2.1"
	subdir="UnityProject/Assets/Plugins/Zenject/*"
	ignore="OptionalExtras/*"
}
Update-Package @args

