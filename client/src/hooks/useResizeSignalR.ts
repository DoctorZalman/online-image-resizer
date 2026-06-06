import { useEffect, useRef } from 'react';
import * as signalR from '@microsoft/signalr';
import { toast } from 'sonner';
import { useAppStore } from '../store/useAppStore';

// - SignalR event payloads matching server hub messages
interface ProgressPayload {
  jobId: string;
  progress: number;
  status: string;
}

interface CompletePayload {
  jobId: string;
  downloadUrl: string;
}

interface FailedPayload {
  jobId: string;
  error: string;
}

// - connects to /hubs/resize and syncs job state via Zustand
export function useResizeSignalR(): void {
  const updateJob = useAppStore((s) => s.actions.updateJob);
  const connectionRef = useRef<signalR.HubConnection | null>(null);

  useEffect(() => {
    // - build connection with automatic reconnect
    const connection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/resize', { withCredentials: true })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    // - progress update from server (0-100)
    connection.on('ResizeProgress', (payload: ProgressPayload) => {
      updateJob(payload.jobId, {
        progress: payload.progress,
        status: 'processing',
      });
    });

    // - job finished - update store and notify user
    connection.on('ResizeComplete', (payload: CompletePayload) => {
      updateJob(payload.jobId, {
        status: 'done',
        progress: 100,
        downloadUrl: payload.downloadUrl,
      });
      toast.success('Done! Ready to download', {
        action: {
          label: 'Download',
          onClick: () => window.open(payload.downloadUrl, '_blank'),
        },
      });
    });

    // - job failed - update store and notify user
    connection.on('ResizeFailed', (payload: FailedPayload) => {
      updateJob(payload.jobId, {
        status: 'failed',
        error: payload.error,
      });
      toast.error('Processing failed, please retry');
    });

    connectionRef.current = connection;

    connection.start().catch(() => {
      toast.error('Realtime connection failed, progress may be delayed');
    });

    return () => {
      connection.stop();
    };
  }, [updateJob]);
}
