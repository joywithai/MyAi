/** @type {import('next').NextConfig} */
const nextConfig = {
  output: 'standalone',
  reactStrictMode: true,
  async rewrites() {
    const api = process.env.API_PROXY_TARGET;
    if (!api) return [];
    // Proxy /api/v1/* to the backend so the browser never needs the backend origin.
    return [{ source: '/api/v1/:path*', destination: `${api}/api/v1/:path*` }];
  }
};

export default nextConfig;
