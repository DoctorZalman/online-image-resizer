import { motion } from 'framer-motion';
import { useAppStore } from '../../store/useAppStore.ts';
import {type ReactElement, useEffect} from 'react';

// - sync theme class on <html> element
function applyTheme(theme: 'light' | 'dark'): void {
  document.documentElement.classList.toggle('dark', theme === 'dark');
}

export function ThemeToggle(): ReactElement {
  const theme = useAppStore((s) => s.theme);
  const setTheme = useAppStore((s) => s.actions.setTheme);

  // - apply persisted theme on mount
  useEffect(() => {
    applyTheme(theme);
  }, [theme]);

  const toggle = (): void => {
    const next = theme === 'light' ? 'dark' : 'light';
    setTheme(next);
    applyTheme(next);
  };

  return (
    <button
      onClick={toggle}
      className="p-2 rounded-full hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors"
      aria-label="Toggle theme"
    >
      <motion.span
        key={theme}
        initial={{ rotate: -90, opacity: 0 }}
        animate={{ rotate: 0, opacity: 1 }}
        transition={{ duration: 0.3 }}
        className="block text-xl"
      >
        {theme === 'light' ? '🌙' : '☀️'}
      </motion.span>
    </button>
  );
}