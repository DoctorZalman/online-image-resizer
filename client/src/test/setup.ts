import '@testing-library/jest-dom';
import { vi } from 'vitest';

// - mock localStorage for Zustand persist middleware in tests
const localStorageMock = {
  getItem: vi.fn(() => null),
  setItem: vi.fn(),
  removeItem: vi.fn(),
  clear: vi.fn(),
  length: 0,
  key: vi.fn(() => null),
};

Object.defineProperty(window, 'localStorage', {
  value: localStorageMock,
  writable: true,
});
