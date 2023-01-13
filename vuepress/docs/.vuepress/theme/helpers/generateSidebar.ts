// loop over files in docs folder excluding .vuepress
// top level files are in data.title array
// other folders get their own array
import type { SidebarConfig } from '@vuepress/theme-default'
import matter from 'gray-matter';
import glob from 'glob';
import * as data from "../../data.json";
import { readFileSync, readdirSync } from 'fs'
import { getDirname, path } from '@vuepress/utils';

const __dirname = getDirname(import.meta.url);
const docsDirectory = __dirname + '/../../../';

type Directory = {
    name: string,
    path: string,
    files: File[],
}

type File = {
    name: string,
    isIndex: boolean,
    order: number,
}

export const generateSidebar = (): SidebarConfig => {
    const sidebar: SidebarConfig = {};
    sidebar['/'] = [];

    let files = glob.sync(`**/**.md`, {
        cwd: docsDirectory,
        mark: true,
    });

    let rootDirectory: Directory = {
        name: data.title,
        path: '/',
        files: [],
    };
    let directories: Directory[] = [];

    files.forEach(file => {
        const splittedFilePath = file.split('/');

        // If it's a file in the root, add it to the root directory
        if (splittedFilePath.length == 1) {
            rootDirectory.files.push({
                name: splittedFilePath[0],
                isIndex: isIndexFile(splittedFilePath[0]),
                order: getPageOrder('/', splittedFilePath[0]),
            });
            return;
        }

        // We're dealing with a file in a nested directory
        let directory = directories.find(d => d.path == splittedFilePath[0]);

        if (!directory) {
            directory = {
                name: prettifyDirectoryName(splittedFilePath[0]),
                path: splittedFilePath[0],
                files: [],
            }
            directories.push(directory);
        }

        directory.files.push({
            name: splittedFilePath[1],
            isIndex: isIndexFile(splittedFilePath[1]),
            order: getPageOrder(splittedFilePath[0], splittedFilePath[1]),
        });
    });

    // Add root directory to sidebar
    sidebar['/'].push({
        text: rootDirectory.name,
        children: sortByPageOrder(rootDirectory).map(f => `/${f.name}`),
    });

    // Add all other directories to sidebar
    directories.forEach(directory => {
        sidebar['/'].push({
            text: directory.name,
            children: sortByPageOrder(directory).map(f => `/${directory.path}/${f.name}`),
            collapsible: data.collapseSidebarSections,
        });
    });

    return sidebar;
}

const isIndexFile = fileName => {
    return fileName.toLowerCase().includes('readme') || fileName == 'index.md';
}

const sortByPageOrder = (directory: Directory): File[] => {
    return directory.files.sort((a, b) => {
        if (a.order < b.order) return -1;
        if (a.order > b.order) return 1;
        return 0;
    });
}

const getPageOrder = (directory, fileName) => {
    if (isIndexFile(fileName)) {
        return -1;
    }

    const fileContents = readFileSync(docsDirectory + '/' + directory + '/' + fileName).toString();
    const frontmatter = matter(fileContents);

    if (frontmatter.data.order == null) {
        return 999;
    }

    return frontmatter.data.order;
}

const prettifyDirectoryName = (name) => {
    name = name.replaceAll("-", " ").replaceAll("_", " ");
    return name.charAt(0).toUpperCase() + name.slice(1);
}
