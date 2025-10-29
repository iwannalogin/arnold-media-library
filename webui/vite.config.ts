import { defineConfig } from "vite"
import { dirname, resolve } from 'path';
import { fileURLToPath } from "url";

const __dirname = dirname(fileURLToPath(import.meta.url));

export default defineConfig({
    build: {
        rolldownOptions: {
            resolve: {
                alias: {
                    "wc": resolve( __dirname, "./extern/wc-lib/lib")
                }
            }
        }
    }
});