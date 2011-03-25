Param($leaveLatest)
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
$targetFolderPath = "dist.springframework.org/snapshot/SPRNET"
$newline = [Environment]::NewLine

#verify the args aren't null before we proceed
if ((!($leaveLatest)))
{
	Write-Output "***ERROR: INVALID ARGUMENTS***"
	Write-Output $newline
	Write-Output "Correct usage is..."
	Write-Output "powershell.exe <scriptname> -leaveLatest <number-of-files-to-leave>"
	Write-Output $newline
	Write-Output "example:"
	Write-Output "powershell.exe .\delete.ps1 -leaveLatest 10"
	Write-Output "This will delete all but the 10 most recent files (by date) on S3"

	#exit with exit code 1 (failure) so calling procs can know WTF happened
	exit 1
}


#ensure the CloudBerry Labs snap-ins are loaded before we try to call them...
Add-PSSnapin CloudBerryLab.Explorer.PSSnapIn


#get the connection to s3
Write-Output "Connecting to S3..."
$s3 = Get-CloudS3Connection -Key $key -Secret $secret
Write-Output "...connection successful."
Write-Output $newline

#get a folder object that represents the target path
$targetFolder = $s3 | Select-CloudFolder -Path $targetFolderPath

#get the connection to s3
Write-Output ("Deleting old uploads so only the most recent " + $leaveLatest + " files remain...") $newline
$files = $targetFolder | Get-CloudItem | Sort-Object ModifyDate -Descending | Select-Object -Skip $leaveLatest

if($files)
{
	$files  | ForEach-Object	{
									Write-Output ("Removing file: " +  $_.DisplayName + " ( originally uploaded: " + $_.ModifyDate + " )...")
									Remove-CloudItem -Filter $_.DisplayName -Folder $targetFolder
									Write-Output "...file deletion successful."
								}
}
else
{
	Write-Output (">>>There are less than " + $leaveLatest + " files on S3 so no files need to be removed from S3 at this time.")
}



Write-Output $newline



#if we get this far, we should return ERRORCODE 0 so calling procs can know we succeeded
#technically this is redundant, but best to be explicit :)
Write-Output $newline
Write-Output "Exiting Powershell script normally."
exit 0