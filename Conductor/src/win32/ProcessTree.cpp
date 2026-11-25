// 子と孫をまとめて止めるためのジョブオブジェクト。
#include "ProcessTree.h"

namespace skc::conductor::win32
{
    ProcessTree::ProcessTree()
        : m_job(CreateJobObjectW(nullptr, nullptr)), m_isAssigned(false)
    {
        if (m_job == nullptr) { return; }

        // 常駐プロセス（git の fsmonitor など）が自分から外れたいときは外れられるようにする。
        JOBOBJECT_EXTENDED_LIMIT_INFORMATION limits{};
        limits.BasicLimitInformation.LimitFlags = JOB_OBJECT_LIMIT_BREAKAWAY_OK;
        SetInformationJobObject(m_job, JobObjectExtendedLimitInformation, &limits, sizeof(limits));
    }

    ProcessTree::~ProcessTree()
    {
        if (m_job != nullptr) { CloseHandle(m_job); }
    }

    bool ProcessTree::Assign(HANDLE process)
    {
        m_isAssigned = m_job != nullptr && AssignProcessToJobObject(m_job, process) != FALSE;
        return m_isAssigned;
    }

    void ProcessTree::Terminate(HANDLE process)
    {
        if (m_isAssigned && TerminateJobObject(m_job, 1)) { return; }

        TerminateProcess(process, 1);
    }
}
