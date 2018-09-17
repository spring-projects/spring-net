#borrowed from https://habaneroconsulting.com/Insights/Create-a-SQL-Alias-with-a-PowerShell-Script.aspx

#This is the name of your SQL Alias
$AliasName = "SpringQA"
 
#This is the name of your SQL server (the actual name!)
$ServerName = $env:COMPUTERNAME

$InstanceName = "SQL2017"
 
#These are the two Registry locations for the SQL Alias locations
$x86 = "HKLM:\Software\Microsoft\MSSQLServer\Client\ConnectTo"
$x64 = "HKLM:\Software\Wow6432Node\Microsoft\MSSQLServer\Client\ConnectTo"
 
#We're going to see if the ConnectTo key already exists, and create it if it doesn't.
if ((test-path -path $x86) -ne $True)
{
    write-host "$x86 doesn't exist"
    New-Item $x86
}
if ((test-path -path $x64) -ne $True)
{
    write-host "$x64 doesn't exist"
    New-Item $x64
}
 
#Adding the extra "fluff" to tell the machine what type of alias it is
$TCPAlias = "DBMSSOCN," + $ServerName + "\" + $InstanceName
#$NamedPipesAlias = "DBNMPNTW,\\" + $ServerName + "\PIPE\" + "MSSQL$" + $InstanceName + "\sql\query"
$NamedPipesAlias = "DBNMPNTW,\\.\PIPE\MSSQL`$SQL2017\sql\query"

#Creating our TCP/IP Aliases
#New-ItemProperty -Path $x86 -Name $AliasName -PropertyType String -Value $TCPAlias -Force
#New-ItemProperty -Path $x64 -Name $AliasName -PropertyType String -Value $TCPAlias -Force
 
#WARNING: left here in case we ever want to switch to named-piped, but since we can't have two aliases
#          named the same for two diff. conn. protcols, this is commented out here in favor of the TCPIP alias above
 
#Creating our Named Pipes Aliases
New-ItemProperty -Path $x86 –Name $AliasName -PropertyType String -Value $NamedPipesAlias
New-ItemProperty -Path $x64 –Name $AliasName -PropertyType String -Value $NamedPipesAlias