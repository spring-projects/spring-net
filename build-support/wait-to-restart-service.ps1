[CmdletBinding()]
Param(
  [Parameter(Mandatory=$True)]
  [string]$serviceName
)
write-output 'Attempting to restart service $serviceName'

$svc = get-service $serviceName

write-output 'Initial service status:'
write-output $svc

if($svc.Status -ne 'Running') {
  start-service $svc
  $svc.WaitForStatus('Running', '00:01:00')
}
else {
  $svc.WaitForStatus('Running', '00:01:00')
  restart-service $svc
  $svc.WaitForStatus('Running', '00:01:00')
}

write-output 'Final service status:'
write-output $svc