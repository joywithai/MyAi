import type { Metadata, Viewport } from 'next';
import { AppProvider } from '@/lib/app-context';
import { SiteHeader } from '@/components/layout/SiteHeader';
import './globals.css';

export const metadata: Metadata = {
  title: 'MyAi — 3D AI Avatar Chatbot',
  description: 'AI-powered 3D avatar chatbot with Bangla/English voice, expressions and lip-sync.'
};

export const viewport: Viewport = {
  themeColor: '#0b0e14',
  width: 'device-width',
  initialScale: 1
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="bn" className="dark">
      <body className="min-h-screen bg-surface text-gray-100 antialiased">
        <AppProvider>
          <SiteHeader />
          <main className="mx-auto w-full max-w-page px-4 pb-16 pt-6">{children}</main>
          <footer className="mx-auto w-full max-w-page px-4 py-8 text-center text-xs text-muted">
            MyAi — 3D AI Avatar · Built with Next.js + three-vrm
          </footer>
        </AppProvider>
      </body>
    </html>
  );
}
