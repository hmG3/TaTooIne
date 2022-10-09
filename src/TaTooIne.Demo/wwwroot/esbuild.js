import esbuild from "esbuild";
import { sassPlugin } from "esbuild-sass-plugin";

esbuild
  .build({
    entryPoints: {
      "styles.min": "_scss/main.scss",
      "bundle.min": "_ts/SiteJsInterop.ts",
      "gh-spa.min": "node_modules/ghspa/ghspa.js"
    },
    outdir: "dist",
    bundle: true,
    minify: true,
    keepNames: true,
    plugins: [sassPlugin()],
  })
  .then(() => console.log("⚡ Build complete! ⚡"))
  .catch(() => process.exit(1));
