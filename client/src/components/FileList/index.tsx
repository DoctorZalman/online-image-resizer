import { AnimatePresence } from 'framer-motion';
import { useAppStore } from '../../store/useAppStore';
import { FileCard } from '../FileCard';
import * as React from "react";

export function FileList(): React.ReactElement | null {
  const jobs = useAppStore((s) => s.jobs);
  const entries = Object.values(jobs);

  if (entries.length === 0) return null;

  return (
    <div className="flex flex-col gap-3">
      <h2 className="text-sm font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wide">
        Files
      </h2>
      {/* - AnimatePresence enables exit animation when card is removed */}
      <AnimatePresence initial={false}>
        {entries.map((job) => (
          <FileCard
            key={job.jobId}
            jobId={job.jobId}
            fileName={job.fileName}
            size={job.size}
            status={job.status}
            progress={job.progress}
            error={job.error}
          />
        ))}
      </AnimatePresence>
    </div>
  );
}