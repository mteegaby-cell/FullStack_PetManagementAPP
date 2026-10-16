import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173, // Your frontend React server port
    proxy: {
      // Forwards any frontend request starting with '/api' to your .NET backend
      "/api": {
        target: "https://localhost:7242", // Updated with your .NET port
        changeOrigin: true,
        secure: false, // Prevents SSL certificate blocks during local development
      },
    },
  },
});
