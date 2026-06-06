// - base URL is proxied via Vite to http://localhost:5000
const BASE = '/api/images';

export interface UploadedJob {
  jobId: string;
  fileName: string;
  size: number;
}

export interface ResizeRequest {
  jobId: string;
  scalePercent: number;
}

// - upload multiple files, returns job list from server
export async function uploadImages(files: File[]): Promise<UploadedJob[]> {
  const form = new FormData();
  for (const file of files) {
    form.append('files', file);
  }

  const res = await fetch(`${BASE}/upload`, {
    method: 'POST',
    credentials: 'include',
    body: form,
  });

  if (!res.ok) {
    const text = await res.text();
    throw new Error(text || 'Upload failed');
  }

  return res.json();
}

// - enqueue a resize job, server returns 202 Accepted
export async function requestResize(payload: ResizeRequest): Promise<void> {
  const res = await fetch(`${BASE}/resize`, {
    method: 'POST',
    credentials: 'include',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  });

  if (!res.ok) {
    const text = await res.text();
    throw new Error(text || 'Resize request failed');
  }
}

// - download resized file by jobId
export function getDownloadUrl(jobId: string): string {
  return `${BASE}/download/${jobId}`;
}
