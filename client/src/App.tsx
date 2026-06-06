import * as React from "react";
import { useResizeSignalR } from './hooks/useResizeSignalR';
import { DropZone } from './components/DropZone';
import { FileList } from './components/FileList';
import { ResizeControls } from './components/ResizeControls';
import { DownloadList } from './components/DownloadList';
import {ThemeToggle} from "./components/ThemeToggle";

export function App(): React.ReactElement {
  // - establish SignalR connection for the lifetime of the app
  useResizeSignalR();

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-950 transition-colors">
      <header className="flex items-center justify-between px-6 py-4 border-b border-gray-200 dark:border-gray-800">
        <h1 className="text-lg font-bold text-gray-900 dark:text-white">Image Resizer</h1>
        <ThemeToggle />
      </header>

      <main className="max-w-2xl mx-auto px-4 py-8 flex flex-col gap-6">
        <DropZone />
        <FileList />
        <ResizeControls />
        <DownloadList />
      </main>
    </div>
  );
}