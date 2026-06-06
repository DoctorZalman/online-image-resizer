import { create } from 'zustand';
import { persist } from 'zustand/middleware';

export type JobStatus = 'queued' | 'processing' | 'done' | 'failed';

// - shape of a single resize job tracked on the client
export interface JobState {
  jobId: string;
  fileName: string;
  size: number;
  status: JobStatus;
  progress: number;
  downloadUrl?: string;
  error?: string;
}

interface AppStore {
  sessionId: string | null;
  jobs: Record<string, JobState>;
  // - derived: true while any job is queued or processing
  isProcessing: boolean;
  theme: 'light' | 'dark';
  actions: {
    addJobs: (jobs: Omit<JobState, 'status' | 'progress'>[]) => void;
    updateJob: (jobId: string, patch: Partial<JobState>) => void;
    setTheme: (theme: 'light' | 'dark') => void;
    reset: () => void;
  };
}

// - recompute isProcessing from current jobs map
const deriveIsProcessing = (jobs: Record<string, JobState>): boolean =>
  Object.values(jobs).some((j) => j.status === 'queued' || j.status === 'processing');

export const useAppStore = create<AppStore>()(
  persist(
    (set) => ({
      sessionId: null,
      jobs: {},
      isProcessing: false,
      theme: 'light',
      actions: {
        // - register new jobs returned from upload endpoint
        addJobs: (newJobs) =>
          set((state) => {
            const added: Record<string, JobState> = {};
            for (const j of newJobs) {
              added[j.jobId] = { ...j, status: 'queued', progress: 0 };
            }
            const jobs = { ...state.jobs, ...added };
            return { jobs, isProcessing: deriveIsProcessing(jobs) };
          }),

        // - partial update for a job (progress, status, downloadUrl, error)
        updateJob: (jobId, patch) =>
          set((state) => {
            const existing = state.jobs[jobId];
            if (!existing) return state;
            const jobs = { ...state.jobs, [jobId]: { ...existing, ...patch } };
            return { jobs, isProcessing: deriveIsProcessing(jobs) };
          }),

        setTheme: (theme) => set({ theme }),

        reset: () => set({ jobs: {}, isProcessing: false }),
      },
    }),
    {
      name: 'image-resizer-store',
      // - persist only theme preference
      partialize: (state) => ({ theme: state.theme }),
    },
  ),
);
