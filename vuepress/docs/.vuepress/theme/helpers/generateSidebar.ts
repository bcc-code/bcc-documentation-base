import type { SidebarConfig } from "@vuepress/theme-default";
import matter from "gray-matter";
import glob from "glob";
import { readFileSync, existsSync, statSync } from "fs";
import { getDirname } from "@vuepress/utils";
import path from "path";

const __dirname = getDirname(import.meta.url);
const docsDirectory = __dirname + "/../../../";

export const generateSidebar = (): SidebarConfig => {
  const sidebar: SidebarConfig = {};
  sidebar["/"] = [];

  // Get all repo directories (top-level folders in docs)
  const repoDirs = glob
    .sync("*", {
      cwd: docsDirectory,
      mark: true,
    })
    .filter((d: string) => statSync(path.join(docsDirectory, d)).isDirectory());

  // get all md files in the root of docs
  const rootMdFiles = glob
    .sync("*.md", {
      cwd: docsDirectory,
      nodir: true,
    })
    .filter((f: string) => !isIndexFile(f));

  repoDirs.forEach((repoDir: string) => {
    const repoPath = repoDir.replace(/\/$/, "");
    const children = [];

    const indexFile = existsSync(
      path.join(docsDirectory, repoPath, "README.md")
    )
      ? "README.md"
      : existsSync(path.join(docsDirectory, repoPath, "index.md"))
      ? "index.md"
      : null;

    if (indexFile) {
      let indexTitle = "Home";
      const frontmatter = getFrontmatter(repoPath, indexFile);
      if (frontmatter.title) {
        indexTitle = frontmatter.title;
      }
      children.push({
        text: indexTitle,
        link: `/${repoPath}/${indexFile}`,
      });
    }

    children.push(...getNestedSidebar(repoPath));

    sidebar["/"].push({
      text: prettifyDirectoryName(repoPath),
      collapsible: true,
      link: indexFile ? `/${repoPath}/${indexFile}` : undefined,
      children,
    });
  });

  rootMdFiles.forEach((file: string) => {
    const frontmatter = getFrontmatter("", file);
    sidebar["/"].push({
      text: frontmatter.title
        ? frontmatter.title
        : prettifyDirectoryName(file.replace(".md", "")),
      link: `/${file}`,
      icon: frontmatter.icon,
    });
  });

  return sidebar;
};

function getNestedSidebar(currentDir: string): any[] {
  const fullPath = path.join(docsDirectory, currentDir);
  if (!existsSync(fullPath) || !statSync(fullPath).isDirectory()) return [];

  const entries = glob.sync("*", { cwd: fullPath, mark: true });
  const files = entries.filter(
    (e: string) => e.endsWith(".md") && !isIndexFile(e)
  );
  const dirs = entries.filter((e: string) => !e.endsWith(".md"));

  const fileItems = files.map((file: string) => {
    const frontmatter = getFrontmatter(currentDir, file);
    return {
      text: frontmatter.title
        ? frontmatter.title
        : prettifyDirectoryName(file.replace(".md", "")),
      link: `/${currentDir}/${file}`,
      icon: frontmatter.icon,
    };
  });

  const dirItems = dirs
    .map((dir: string) => {
      const dirName = dir.replace("/", "");
      const dirFullPath = path.join(docsDirectory, currentDir, dirName);
      let icon;
      let indexTitle = "Home";

      if (!statSync(dirFullPath).isDirectory()) return null;

      const sectionFrontmatter = getSectionFrontmatter(
        path.posix.join(currentDir, dirName)
      );
      if (sectionFrontmatter["hideSection"] === true) return null;

      const indexFile = existsSync(path.join(dirFullPath, "README.md"))
        ? "README.md"
        : existsSync(path.join(dirFullPath, "index.md"))
        ? "index.md"
        : null;
      const children = [];

      if (indexFile) {
        const frontmatter = getFrontmatter(
          path.posix.join(currentDir, dirName),
          indexFile
        );
        if (frontmatter.title) {
          indexTitle = frontmatter.title;
        }
        if (frontmatter.icon) {
          icon = frontmatter.icon;
        }

        children.push({
          text: indexTitle,
          link: `/${currentDir}/${dirName}/${indexFile}`,
          icon,
        });
      }

      children.push(...getNestedSidebar(path.posix.join(currentDir, dirName)));

      const mdFiles = glob
        .sync("*.md", { cwd: dirFullPath })
        .filter((f: string) => !isIndexFile(f));

      const fallbackFile =
        !indexFile && mdFiles.length > 0 ? mdFiles[0] : undefined;

      const dirLink = indexFile
        ? `/${currentDir}/${dirName}/${indexFile}`
        : fallbackFile
        ? `/${currentDir}/${dirName}/${fallbackFile}`
        : undefined;

      return {
        text:
          sectionFrontmatter["sectionTitle"] ?? prettifyDirectoryName(dirName),
        collapsible: false,
        link: dirLink,
        icon,
        children,
      };
    })
    .filter(Boolean);

  return [...fileItems, ...dirItems];
}

const isIndexFile = (fileName: string) => {
  return fileName.toLowerCase().includes("readme") || fileName == "index.md";
};

const getSectionFrontmatter = (directory: string) => {
  const readmePath = path.join(docsDirectory, directory, "README.md");
  const indexPath = path.join(docsDirectory, directory, "index.md");
  if (existsSync(readmePath)) {
    return getFrontmatter(directory, "README.md");
  }
  if (existsSync(indexPath)) {
    return getFrontmatter(directory, "index.md");
  }
  return {};
};

const getFrontmatter = (directory: string, fileName: string) => {
  const filePath = path.join(docsDirectory, directory, fileName);
  if (!existsSync(filePath)) return {};
  const fileContents = readFileSync(filePath).toString();
  const frontmatter = matter(fileContents);

  // If no title in frontmatter, try to extract from first H1 in markdown
  if (!frontmatter.data.title) {
    const match = fileContents.match(/^#\s+(.+)$/m);
    if (match) {
      frontmatter.data.title = match[1].trim();
    }
  }
  return frontmatter.data;
};

const prettifyDirectoryName = (name: string) => {
  name = name.replaceAll("-", " ").replaceAll("_", " ");
  return name.charAt(0).toUpperCase() + name.slice(1);
};
