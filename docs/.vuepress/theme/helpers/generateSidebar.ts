// loop over files in docs folder excluding .vuepress
// top level files are in data.title array
// other folders get their own array
import type { SidebarConfig } from '@vuepress/theme-default'
import matter from 'gray-matter';
import fs from 'fs';
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
    let files = glob.sync(`*.md`, {
        cwd: data.docsDir,
    });
    
    sidebar[baseUrl].push({
        text: data.title,
        children: sortByPageOrder(files).map(f => `/${f}`),
    });

    // Add other folders as their own sections
    const directories = getDirectories(__dirname + '/../../../');

    directories.forEach(directory => {
        const currentDirectory = `${data.docsDir}/${directory}`;

        const filesInDirectory = glob.sync(`*.md`, {
            cwd: currentDirectory,
        });
        
        sidebar[baseUrl].push({
            text: directory.charAt(0).toUpperCase() + directory.slice(1),
            children: sortByPageOrder(filesInDirectory, currentDirectory).map(f => `/${directory}/${f}`),
        });
    });

    return sidebar;
}

const sortByPageOrder = (array, currentDirectory: string|null = null) => {
    return array.sort((a, b) => {
        const pageOrderA = getPageOrder(a, currentDirectory);
        const pageOrderB = getPageOrder(b, currentDirectory);
        console.log(pageOrderA, pageOrderB);
        if (pageOrderA < pageOrderB) return -1;
        if (pageOrderA > pageOrderB) return 1;
        return 0;
    });
}

const getPageOrder = (filePath, currentDirectory: string|null = null) => {
    const path = currentDirectory ?
        currentDirectory + '/' + filePath
        : data.docsDir + '/' + filePath;
    const fileContents = fs.readFileSync(path).toString();
    const frontmatter = matter(fileContents);

    if (frontmatter.data.order == null) {
        return 999;
    }

    return frontmatter.data.order;
}