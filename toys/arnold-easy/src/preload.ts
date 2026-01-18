// See the Electron documentation for details on how to use preload scripts:
// https://www.electronjs.org/docs/latest/tutorial/process-model#preload-scripts

import { contextBridge, ipcRenderer } from "electron";
import { AcceptDialogRequest, GetTagRequest, InsecureExecRequest, OpenFileRequest, RenameFileRequest, SearchLibraryRequest } from "./electron-api";

contextBridge.exposeInMainWorld('electronAPI', {
    getLibraries: () => ipcRenderer.invoke('get-libraries'),
    searchLibrary: (parameters: SearchLibraryRequest) => ipcRenderer.invoke('search-library', parameters),
    selectFiles: () => ipcRenderer.invoke('select-files'),
    selectDirectories: () => ipcRenderer.invoke('select-directories'),
    cancelDialog: () => ipcRenderer.invoke('cancel-dialog'),
    acceptDialog: (parameters: AcceptDialogRequest) => ipcRenderer.invoke('accept-dialog', parameters),
    getTags: (parameters: GetTagRequest ) => ipcRenderer.invoke('get-tags', parameters),
    getStartupParameters: () => ipcRenderer.invoke('get-startup'),
    renameFile: (parameters: RenameFileRequest) => ipcRenderer.invoke('rename-file', parameters),
    openFile: (parameters: OpenFileRequest) => ipcRenderer.invoke('open-file', parameters),
    insecureExec: (parameters: InsecureExecRequest) => ipcRenderer.invoke('insecure-exec', parameters),
});