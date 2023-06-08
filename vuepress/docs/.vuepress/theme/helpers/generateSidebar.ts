import type { SidebarConfig } from '@vuepress/theme-default'
import matter from 'gray-matter';
import glob from 'glob';
import * as data from "../../data.json";
import { readFileSync, existsSync } from 'fs'
import { getDirname } from '@vuepress/utils';

const __dirname = getDirname(import.meta.url);
const docsDirectory = __dirname + '/../../../';

type Directory = {
    name: string,
    path: string,
    files: File[],
    order: number;
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
        order: -1,
    };
    let directories: Directory[] = [];

    files.forEach(file => {
        const splittedFilePath = file.split('/');

        // If it's a file more than 1 level deep, we'll ignore it
        if (splittedFilePath.length > 2) {
            return;
        }

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
            const frontmatter = getSectionFrontmatter(splittedFilePath[0]);

            if (frontmatter["hideSection"] === true) {
                return;
            }

            const name = frontmatter["sectionTitle"] ?? prettifyDirectoryName(splittedFilePath[0])

            directory = {
                name,
                path: splittedFilePath[0],
                files: [],
                order: frontmatter["sectionOrder"] ?? 1,
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
        children: sortByOrderProperty(rootDirectory.files).map(f => `/${f.name}`),
    });

    // Add all other directories to sidebar
    sortByOrderProperty(directories).forEach(directory => {
        sidebar['/'].push({
            text: directory.name,
            children: sortByOrderProperty(directory.files).map(f => `/${directory.path}/${f.name}`),
            collapsible: data.collapseSidebarSections,
        });
    });

    return sidebar;
}

const isIndexFile = fileName => {
    return fileName.toLowerCase().includes('readme') || fileName == 'index.md';
}

const sortByOrderProperty = (array: Array<T>): Array<T> => {
    return array.sort((a, b) => {
        if (a.order < b.order) return -1;
        if (a.order > b.order) return 1;
        return 0;
    });
}

const getSectionFrontmatter = (directory) => {
    if (existsSync(docsDirectory + '/' + directory + '/' + 'README.md')) {
        return getFrontmatter(directory, 'README.md');
    }

    if (existsSync(docsDirectory + '/' + directory + '/' + 'index.md')) {
        return getFrontmatter(directory, 'index.md');
    }

    return 1;
}

const getPageOrder = (directory, fileName) => {
    if (isIndexFile(fileName)) {
        return -1;
    }

    return getFrontmatterProperty(directory, fileName, 'order');
}

const getFrontmatterProperty = (directory, fileName, frontmatterProperty = 'order', defaultValue = 999) => {
    const fileContents = readFileSync(docsDirectory + '/' + directory + '/' + fileName).toString();
    const frontmatter = matter(fileContents);

    if (frontmatter.data[frontmatterProperty] == null) {
        return defaultValue;
    }

    return frontmatter.data[frontmatterProperty];
}

const getFrontmatter = (directory, fileName) => {
    const fileContents = readFileSync(docsDirectory + '/' + directory + '/' + fileName).toString();
    const frontmatter = matter(fileContents);

    return frontmatter.data;
}

const prettifyDirectoryName = (name) => {
    name = name.replaceAll("-", " ").replaceAll("_", " ");
    return name.charAt(0).toUpperCase() + name.slice(1);
}
