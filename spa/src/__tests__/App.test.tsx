import React from 'react';
import { render } from '@testing-library/react';
import App from '../App';

test('renders the app', () => {
  // Basic "smoke test" to see if the app renders without crashing
  render(<App />);
});
