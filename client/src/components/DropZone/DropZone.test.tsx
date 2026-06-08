import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import { DropZone } from './index';
import { useAppStore } from '../../store/useAppStore';

vi.mock('../../api/imageApi', () => ({
  uploadImages: vi.fn().mockResolvedValue([{ jobId: 'job-1', fileName: 'test.jpg', size: 1024 }]),
}));

vi.mock('sonner', () => ({
  toast: { success: vi.fn(), error: vi.fn() },
}));

beforeEach(() => {
  useAppStore.setState({ jobs: {}, isProcessing: false });
  vi.clearAllMocks();
});

describe('DropZone', () => {
  it('renders drop zone hint text', () => {
    render(<DropZone />);
    expect(screen.getByText(/drag & drop jpeg\/png files here/i)).toBeInTheDocument();
  });

  it('applies disabled styles when isProcessing is true', () => {
    useAppStore.setState({ isProcessing: true });
    render(<DropZone />);
    // - when processing, motion.div gets opacity-50 and cursor-not-allowed classes
    const motionDiv = screen.getByText(/drag & drop/i).closest('div');
    expect(motionDiv?.className).toContain('opacity-50');
    expect(motionDiv?.className).toContain('cursor-not-allowed');
  });

  it('renders file input with correct accepted types', () => {
    render(<DropZone />);
    const input = document.querySelector('input[type="file"]') as HTMLInputElement;
    expect(input).toBeInTheDocument();
    expect(input.accept).toContain('image/jpeg');
    expect(input.accept).toContain('image/png');
  });
});
