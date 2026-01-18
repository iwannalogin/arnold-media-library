export type GetTagRequest = {
    library: string,
    path: string
}

export type SearchLibraryRequest = {
    library: string,
    tags?: string[],
    paths?: string[]
}

export type RenameFileRequest = {
    oldName: string,
    newName: string,
}

export type AcceptDialogRequest = {
    library: string,
    tags: string[],
    paths: string[],
    exit?: boolean | null
}

export type StartupParameters = {
    library?: string,
    tags: string[],
    files: string[],
    folders: string[]
}

export type OpenFileRequest = {
    application?: string,
    file: string
}

export type InsecureExecRequest = {
    command: string,
    args: string[]
}

export type ElectronAPI = {
  getLibraries: () => Promise<string[]>,
  searchLibrary: (parameters: SearchLibraryRequest) => Promise<string[]>,
  getTags: (parameters: GetTagRequest) => Promise<string[]>,
  selectFiles: () => Promise<string[]>,
  selectDirectories: () => Promise<string[]>,
  cancelDialog: () => Promise<void>,
  acceptDialog: (parameters: AcceptDialogRequest) => Promise<void>,
  getStartupParameters: () => Promise<StartupParameters>,
  renameFile: (parameters: RenameFileRequest) => Promise<void>,
  openFile: (parameters: OpenFileRequest) => void,
  insecureExec: (parameters: InsecureExecRequest) => Promise<string>,
};