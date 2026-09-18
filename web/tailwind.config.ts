import type { Config } from 'tailwindcss';

const config: Config = {
  darkMode: 'class',
  content: ['./src/**/*.{ts,tsx}'],
  theme: {
    extend: {
      colors: {
        surface: {
          DEFAULT: '#0b0e14',
          raised: '#11151f',
          overlay: '#171c29'
        },
        line: '#232a3b',
        brand: {
          DEFAULT: '#7c5cff',
          strong: '#9b83ff',
          soft: '#2a2350'
        },
        accent: '#39d2c0',
        danger: '#ff5d73',
        muted: '#8b93a7'
      },
      maxWidth: {
        page: '1000px'
      }
    }
  },
  plugins: []
};

export default config;
