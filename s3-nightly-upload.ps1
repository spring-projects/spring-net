Param($srcFolderPath, $srcFilename)
#declare named params for ease of calling via command-line
#note: this line is ONLY valid if it is the first line of the file
#PLEASE DO NOT MOVE THE PARAM(...) LINE FROM ITS CURRENT POSITION
# OR THIS SCRIPT WILL BREAK!!!

$credentialsFile = ".\s3-credentials.ps1"

#bail out if the credentials file cannot be found
if (!([IO.File]::Exists($credentialsFile)))
{
	Write-Output ("***ERROR: cannot continue without the S3 credentials contained in " + $credentialsFile)
	exit 1
}
else
{
	. $credentialsFile
}

#CONST values
$key = GetKey
$secret = GetSecret
$uploadFolderPath = "dist.springframework.org/snapshot/SPRNET"
$newline = [Environment]::NewLine


#verify the args aren't null before we proceed
if ((!($srcFolderPath))-or (!($srcFilename)))
{
	Write-Output "***ERROR: INVALID ARGUMENTS***"
	Write-Output $newline $newline
	Write-Output "Correct usage is..."
	Write-Output "powershell.exe <scriptname> -srcFolderPath <folderpath> -srcFilename <filename>"
	Write-Output $newline
	Write-Output "example:"
	Write-Output "powershell.exe .\s3upload.ps1 -srcFolderPath c:\uploads -srcFilename FileToUpload.txt"
	
	#exit with exit code 1 (failure) so calling procs can know WTF happened
	exit 1
}

#bail out if the source folder cannot be found
if (!([IO.Directory]::Exists($srcFolderPath)))
{
	Write-Output "***ERROR***"
	Write-Output ("path '" + $srcFolderPath + "' doesn't exist or in inaccessible.")
	exit 1
}


#bail out if the source file cannot be found

$fullPathToSourceFile = [IO.Path]::Combine($srcFolderPath, $srcFilename)

if (!([IO.File]::Exists($fullPathToSourceFile)))
{
	Write-Output "***ERROR***"
	
	Write-Output ("File '" + $fullPathToSourceFile + "' doesn't exist or in inaccessible.")
	exit 1
}

#ensure the CloudBerry Labs snap-ins are loaded before we try to call them...
Add-PSSnapin CloudBerryLab.Explorer.PSSnapIn


#get the connection to s3
Write-Output "Connecting to S3..."
$s3 = Get-CloudS3Connection -Key $key -Secret $secret
Write-Output "...connection successful."
Write-Output $newline

#get a folder object that represents the destination
$destination = $s3 | Select-CloudFolder -Path $uploadFolderPath

#get a folder object that represents the local source folder
$source = Get-CloudFilesystemConnection | Select-CloudFolder $srcFolderPath

#perform the actual copy to s3
Write-Output ("Uploading file '" + $fullPathToSourceFile + "' to S3 path: '" + $uploadFolderPath +"'...")
$source | Copy-CloudItem $destination $srcFilename
Write-Output "...upload successful."
Write-Output $newline

#if we get this far, we should return ERRORCODE 0 so calling procs can know we succeeded
#technically this is redundant, but best to be explicit :)
Write-Output $newline
Write-Output "Exiting Powershell script normally."
exit 0