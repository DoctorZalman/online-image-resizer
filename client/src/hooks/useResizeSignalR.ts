import { useEffect, useRef } from 'react';
import * as signalR from '@microsoft/signalr';
import { toast } from 'sonner';
import { useAppStore } from '../store/useAppStore';

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
    let cancelled = false;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/resize', {
        withCredentials: true,
        transport: signalR.HttpTransportType.LongPolling,
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    connection.on('ResizeProgress', (payload: ProgressPayload) => {
      updateJob(payload.jobId, { progress: payload.progress, status: 'processing' });
    });

    connection.on('ResizeComplete', (payload: CompletePayload) => {
      updateJob(payload.jobId, { status: 'done', progress: 100, downloadUrl: payload.downloadUrl });
      toast.success('Done! Ready to download', {
        action: {
          label: 'Download',
          onClick: () => window.open(payload.downloadUrl, '_blank'),
        },
      });
    });

    connection.on('ResizeFailed', (payload: FailedPayload) => {
      updateJob(payload.jobId, { status: 'failed', error: payload.error });
      toast.error('Processing failed, please retry');
    });

    connectionRef.current = connection;

    // - guard against StrictMode double-invoke: skip start if already cancelled
    connection
      .start()
      .then(() => {
        if (cancelled) connection.stop();
      })
      .catch(() => {
        if (!cancelled) toast.error('Realtime connection failed, progress may be delayed');
      });

    return () => {
      cancelled = true;
      connection.stop();
    };
  }, [updateJob]);
}
