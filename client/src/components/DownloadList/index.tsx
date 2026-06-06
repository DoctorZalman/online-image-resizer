import { useAppStore } from '../../store/useAppStore';
import { getDownloadUrl } from '../../api/imageApi';
import * as React from 'react';

export function DownloadList(): React.ReactElement | null {
  const jobs = useAppStore((s) => s.jobs);
  const doneJobs = Object.values(jobs).filter((j) => j.status === 'done');

  if (doneJobs.length === 0) return null;

  return (
    <div className="flex flex-col gap-3">
      <h2 className="text-sm font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wide">
        Ready to Download
      </h2>
      {doneJobs.map((job) => (
        <div
          key={job.jobId}
          className="flex items-center justify-between gap-2 p-3 rounded-xl border border-green-200 dark:border-green-800 bg-green-50 dark:bg-green-950"
        >
          <span className="text-sm text-gray-800 dark:text-gray-100 truncate">{job.fileName}</span>
          {/* - use downloadUrl from SignalR if available, fallback to REST endpoint */}
          <a
            href={job.downloadUrl ?? getDownloadUrl(job.jobId)}
            download
            className="shrink-0 py-1 px-3 rounded-lg bg-green-600 text-white text-xs font-medium
              hover:bg-green-700 transition-colors"
          >
            Download
          </a>
        </div>
      ))}
    </div>
  );
}
