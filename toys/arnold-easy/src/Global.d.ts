export type GetTagRequest = {
    library: string,
    path: string
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