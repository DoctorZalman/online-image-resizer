import { useCallback } from 'react';
import { useDropzone } from 'react-dropzone';
import { motion } from 'framer-motion';
import { toast } from 'sonner';
import { uploadImages } from '../../api/imageApi';
import { useAppStore } from '../../store/useAppStore';
import * as React from "react";

const MAX_FILES = 10;
const ACCEPTED_TYPES = { 'image/jpeg': [], 'image/png': [] };

export function DropZone(): React.ReactElement {
  const addJobs = useAppStore((s) => s.actions.addJobs);
  const isProcessing = useAppStore((s) => s.isProcessing);

  const onDrop = useCallback(
    async (accepted: File[], rejected: unknown[]) => {
      if (rejected && (rejected as []).length > 0) {
        toast.error('Only JPEG/PNG allowed');
        return;
      }
      if (accepted.length === 0) return;
      if (accepted.length > MAX_FILES) {
        toast.error(`Max ${MAX_FILES} files per session`);
        return;
      }

      try {
        const jobs = await uploadImages(accepted);
        addJobs(jobs);
        toast.success(`${jobs.length} file${jobs.length > 1 ? 's' : ''} uploaded`);
      } catch (err) {
        toast.error(err instanceof Error ? err.message : 'Upload failed');
      }
    },
    [addJobs],
  );

  const { getRootProps, getInputProps, isDragActive } = useDropzone({
    onDrop,
    accept: ACCEPTED_TYPES,
    maxFiles: MAX_FILES,
    disabled: isProcessing,
  });

  return (
    // - outer div handles dropzone events, inner motion.div handles animation
    <div {...getRootProps()}>
      <motion.div
        animate={{
          scale: isDragActive ? 1.02 : 1,
          borderColor: isDragActive ? '#6366f1' : '#d1d5db',
        }}
        transition={{ duration: 0.2 }}
        className={`
          border-2 border-dashed rounded-xl p-10 text-center cursor-pointer
          transition-colors select-none
          ${isDragActive ? 'bg-indigo-50 dark:bg-indigo-950' : 'bg-white dark:bg-gray-900'}
          ${isProcessing ? 'opacity-50 cursor-not-allowed' : 'hover:border-indigo-400'}
        `}
      >
        <input {...getInputProps()} />
        <p className="text-gray-500 dark:text-gray-400 text-sm">
          {isProcessing
            ? 'Processing in progress...'
            : isDragActive
              ? 'Drop files here'
              : 'Drag & drop JPEG/PNG files here, or click to select (max 10)'}
        </p>
      </motion.div>
    </div>
  );
}