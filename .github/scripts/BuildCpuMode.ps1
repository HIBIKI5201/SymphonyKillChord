# Helpers for switching the CPU usage of a running Unity build (BuildAndRelease.yml).
# BuildRunner/SetBuildCpuMode.ps1 writes the requested percent together with the PID of the
# job's Runner.Worker; the build step dot-sources this file and applies the setting.
# Keep this file ASCII-only so Windows PowerShell 5.1 can read it without a BOM.

$script:CpuModeFile = Join-Path $env:ProgramData 'SymphonyKillChord\build-cpu-mode.json'

# Returns the PID of the Runner.Worker that owns the current process, or $null.
function Get-RunnerWorkerId {
    $currentId = $PID
    $visited = @{}
    while ($currentId -and -not $visited.ContainsKey($currentId)) {
        $visited[$currentId] = $true
        $process = Get-CimInstance Win32_Process -Filter "ProcessId=$currentId" -ErrorAction Stop
        if (-not $process) { return $null }
        if ($process.Name -eq 'Runner.Worker.exe') { return [int]$currentId }
        $currentId = [int]$process.ParentProcessId
    }
    return $null
}

# Returns the requested percent for this job, or $null when there is no valid request.
# A request written for another job is ignored because its Runner.Worker PID differs.
function Get-CpuModeOverridePercent {
    param([int]$RunnerWorkerId, [string[]]$AllowedPercentages)

    if (-not $RunnerWorkerId -or -not (Test-Path -LiteralPath $script:CpuModeFile)) {
        return $null
    }
    try {
        $cpuMode = Get-Content -LiteralPath $script:CpuModeFile -Raw -Encoding UTF8 | ConvertFrom-Json
    } catch {
        return $null
    }
    if ([int]$cpuMode.runner_worker_pid -ne $RunnerWorkerId) { return $null }
    if ("$($cpuMode.percent)%" -notin $AllowedPercentages) { return $null }
    return [int]$cpuMode.percent
}

function Get-CpuCountForPercent {
    param([int]$Percent, [int]$CpuCount, [int]$MaximumAffinityCpuCount)

    $capacity = [Math]::Min($MaximumAffinityCpuCount, $CpuCount)
    return [int][Math]::Max(1, [Math]::Floor($capacity * $Percent / 100))
}

function Get-AffinityMaskForCpuCount {
    param([int]$Count, [int]$MaximumAffinityCpuCount)

    if ($Count -eq $MaximumAffinityCpuCount) { return [Int64]::MaxValue }
    return ([int64]1 -shl $Count) - 1
}

# Applies the mask to the root process and all of its descendants (compilers, Gradle, ...).
# Children inherit the mask when they start, but already running ones must be updated here.
# Returns the number of processes that were updated.
function Set-ProcessTreeAffinity {
    param([int]$RootId, [Int64]$Mask)

    $allProcesses = @(Get-CimInstance Win32_Process -Property ProcessId, ParentProcessId)
    $treeIds = [System.Collections.Generic.List[int]]::new()
    $treeIds.Add($RootId)
    $seen = @{ $RootId = $true }
    for ($index = 0; $index -lt $treeIds.Count; $index++) {
        foreach ($candidate in $allProcesses) {
            $candidateId = [int]$candidate.ProcessId
            if ([int]$candidate.ParentProcessId -eq $treeIds[$index] -and -not $seen.ContainsKey($candidateId)) {
                $seen[$candidateId] = $true
                $treeIds.Add($candidateId)
            }
        }
    }

    $applied = 0
    foreach ($treeId in $treeIds) {
        try {
            (Get-Process -Id $treeId -ErrorAction Stop).ProcessorAffinity = $Mask
            $applied++
        } catch {
            # Skip processes that already exited or cannot be accessed.
        }
    }
    return $applied
}
