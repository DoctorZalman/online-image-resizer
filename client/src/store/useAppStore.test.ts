import { describe, it, expect, beforeEach } from 'vitest';
import { useAppStore } from './useAppStore';

// - reset store state before each test
beforeEach(() => {
  useAppStore.getState().actions.reset();
  useAppStore.setState({ jobs: {}, isProcessing: false });
});

describe('useAppStore', () => {
  describe('addJobs', () => {
    it('adds jobs with idle status and zero progress', () => {
      useAppStore
        .getState()
        .actions.addJobs([{ jobId: 'job-1', fileName: 'test.jpg', size: 1024 }]);

      const jobs = useAppStore.getState().jobs;
      expect(jobs['job-1']).toMatchObject({
        jobId: 'job-1',
        fileName: 'test.jpg',
        size: 1024,
        status: 'idle',
        progress: 0,
      });
    });

    it('does not set isProcessing when jobs are idle', () => {
      useAppStore
        .getState()
        .actions.addJobs([{ jobId: 'job-1', fileName: 'test.jpg', size: 1024 }]);

      expect(useAppStore.getState().isProcessing).toBe(false);
    });

    it('adds multiple jobs at once', () => {
      useAppStore.getState().actions.addJobs([
        { jobId: 'job-1', fileName: 'a.jpg', size: 100 },
        { jobId: 'job-2', fileName: 'b.jpg', size: 200 },
      ]);

      const jobs = useAppStore.getState().jobs;
      expect(Object.keys(jobs)).toHaveLength(2);
    });
  });

  describe('updateJob', () => {
    it('updates job status and progress', () => {
      useAppStore
        .getState()
        .actions.addJobs([{ jobId: 'job-1', fileName: 'test.jpg', size: 1024 }]);
      useAppStore.getState().actions.updateJob('job-1', {
        status: 'processing',
        progress: 50,
      });

      const job = useAppStore.getState().jobs['job-1'];
      expect(job.status).toBe('processing');
      expect(job.progress).toBe(50);
    });

    it('sets isProcessing true when job is processing', () => {
      useAppStore
        .getState()
        .actions.addJobs([{ jobId: 'job-1', fileName: 'test.jpg', size: 1024 }]);
      useAppStore.getState().actions.updateJob('job-1', { status: 'processing' });

      expect(useAppStore.getState().isProcessing).toBe(true);
    });

    it('sets isProcessing false when all jobs are done', () => {
      useAppStore
        .getState()
        .actions.addJobs([{ jobId: 'job-1', fileName: 'test.jpg', size: 1024 }]);
      useAppStore.getState().actions.updateJob('job-1', { status: 'processing' });
      useAppStore.getState().actions.updateJob('job-1', { status: 'done' });

      expect(useAppStore.getState().isProcessing).toBe(false);
    });

    it('ignores update for non-existent jobId', () => {
      const before = useAppStore.getState().jobs;
      useAppStore.getState().actions.updateJob('non-existent', { status: 'done' });
      const after = useAppStore.getState().jobs;

      expect(after).toEqual(before);
    });
  });

  describe('reset', () => {
    it('clears all jobs and isProcessing', () => {
      useAppStore
        .getState()
        .actions.addJobs([{ jobId: 'job-1', fileName: 'test.jpg', size: 1024 }]);
      useAppStore.getState().actions.reset();

      expect(useAppStore.getState().jobs).toEqual({});
      expect(useAppStore.getState().isProcessing).toBe(false);
    });
  });
});
