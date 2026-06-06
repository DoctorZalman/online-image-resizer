import { useState } from 'react';
import { toast } from 'sonner';
import { useAppStore } from '../../store/useAppStore';
import { requestResize } from '../../api/imageApi';
import * as React from "react";

export function ResizeControls(): React.ReactElement | null {
  const jobs = useAppStore((s) => s.jobs);
  const isProcessing = useAppStore((s) => s.isProcessing);
  const [scale, setScale] = useState(50);

  // - only show controls when there are uploaded jobs
  const queuedJobs = Object.values(jobs).filter((j) => j.status === 'queued');
  if (Object.values(jobs).length === 0) return null;

  const handleResize = async (): Promise<void> => {
    if (queuedJobs.length === 0) return;

    try {
      await Promise.all(
        queuedJobs.map((job) =>
          requestResize({ jobId: job.jobId, scalePercent: scale }),
        ),
      );
      toast.success('Resize started');
    } catch (err) {
      toast.error(err instanceof Error ? err.message : 'Failed to start resize');
    }
  };

  return (
    <div className="flex flex-col gap-4 p-4 rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900">
      <div className="flex items-center justify-between">
        <label className="text-sm font-medium text-gray-700 dark:text-gray-300">
          Scale
        </label>
        <span className="text-sm font-semibold text-indigo-600 dark:text-indigo-400">
          {scale}%
        </span>
      </div>

      {/* - range input for scale percent 1-100 */}
      <input
        type="range"
        min={1}
        max={100}
        value={scale}
        onChange={(e) => setScale(Number(e.target.value))}
        disabled={isProcessing}
        className="w-full accent-indigo-500 disabled:opacity-50"
      />

      <button
        onClick={handleResize}
        disabled={isProcessing || queuedJobs.length === 0}
        className="w-full py-2 px-4 rounded-lg bg-indigo-600 text-white text-sm font-medium
          hover:bg-indigo-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
      >
        {isProcessing ? 'Processing...' : `Resize ${queuedJobs.length} file${queuedJobs.length !== 1 ? 's' : ''}`}
      </button>
    </div>
  );
}