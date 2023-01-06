// loop over files in docs folder excluding .vuepress
// top level files are in data.title array
// other folders get their own array
import type { SidebarConfig } from '@vuepress/theme-default'
import { path } from "@vuepress/utils";
import glob from 'glob';
import * as data from "../../data.json";
import { readdirSync } from 'fs'

const baseUrl = data.base;

const getDirectories = source =>
  readdirSync(source, { withFileTypes: true })
    .filter(dirent => dirent.isDirectory() && dirent.name !== '.vuepress')
    .map(dirent => dirent.name)

export const generateSidebar = (): SidebarConfig => {
    const sidebar = {};
    sidebar[baseUrl] = [];

    // Base folder
    const filesPaths = glob.sync(`*.md`, {
        cwd: data.docsDir,
    });
    
    sidebar[baseUrl].push({
        text: data.title,
        children: filesPaths.map(f => '/' + f),
    });

    // Other folders
    const directories = getDirectories(__dirname + '/../../../');

    directories.forEach(directory => {
        const filesPaths2 = glob.sync(`*.md`, {
            cwd: data.docsDir + '/' + directory,
        });
        
        sidebar[baseUrl].push({
            text: directory.charAt(0).toUpperCase() + directory.slice(1),
            children: filesPaths2.map(f => '/' + directory + '/' + f),
        });
    });

    return sidebar;
}