import type { JobStatus } from '../../store/types.ts';

export interface FileCardProps {
  jobId: string;
  fileName: string;
  size: number;
  status: JobStatus;
  progress: number;
  error?: string;
}
