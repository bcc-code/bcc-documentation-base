// loop over files in docs folder excluding .vuepress
// top level files are in data.title array
// other folders get their own array
import type { SidebarConfig } from '@vuepress/theme-default'
import glob from 'glob';
import * as data from "../../data.json";
import { readdirSync } from 'fs'

const baseUrl = data.base;

const getDirectories = source =>
  readdirSync(source, { withFileTypes: true })
    .filter(item => item.isDirectory() && item.name !== '.vuepress')
    .map(item => item.name)

export const generateSidebar = (): SidebarConfig => {
    const sidebar = {};
    sidebar[baseUrl] = [];

    // Add files in the base folder as the first section
    const files = glob.sync(`*.md`, {
        cwd: data.docsDir,
    });
    
    sidebar[baseUrl].push({
        text: data.title,
        children: files.map(f => `/${f}`),
    });

    // Add other folders as their own sections
    const directories = getDirectories(__dirname + '/../../../');

    directories.forEach(directory => {
        const filesInDirectory = glob.sync(`*.md`, {
            cwd: `${data.docsDir}/${directory}`, //  `${data.docsDir}/${directory}`
        });
        
        sidebar[baseUrl].push({
            text: directory.charAt(0).toUpperCase() + directory.slice(1),
            children: filesInDirectory.map(f => `/${directory}/${f}`),
        });
    });

    return sidebar;
}