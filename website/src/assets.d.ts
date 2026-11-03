// Ambient declarations for assets handled by webpack (via Gatsby) rather than tsc.
declare module "*.svg" {
  const content: string;
  export default content;
}

declare module "*.scss";
