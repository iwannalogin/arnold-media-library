// See the Electron documentation for details on how to use preload scripts:
// https://www.electronjs.org/docs/latest/tutorial/process-model#preload-scripts

import { contextBridge, ipcRenderer } from "electron";
import { AcceptDialogRequest, GetTagRequest } from "./Global";

contextBridge.exposeInMainWorld('electronAPI', {
    getLibraries: () => ipcRenderer.invoke('get-libraries'),
    selectFiles: () => ipcRenderer.invoke('select-files'),
    selectDirectories: () => ipcRenderer.invoke('select-directories'),
    cancelDialog: () => ipcRenderer.invoke('cancel-dialog'),
    acceptDialog: (parameters: AcceptDialogRequest) => ipcRenderer.invoke('accept-dialog', parameters),
    getTags: (parameters: GetTagRequest ) => ipcRenderer.invoke('get-tags', parameters),
    getStartupParameters: () => ipcRenderer.invoke('get-startup'),
});