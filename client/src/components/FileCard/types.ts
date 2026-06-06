export interface FileCardProps {
  jobId: string;
  fileName: string;
  size: number;
  status: 'queued' | 'processing' | 'done' | 'failed';
  progress: number;
  error?: string;
}