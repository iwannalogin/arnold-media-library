import { ElectronAPI, SearchLibraryRequest } from "src/electron-api"

const formHtml = /*html*/ `
<section class="user-form d-content">
    <div class="d-flex flex-column flex-grow-1">
        <label class="form-label" for="RenamingForm.Library">Library</label>
        <select id="RenamingForm.Library" class="form-control form-select mb-3"></select>
        <label class="form-label" for="RenamingForm.Path">Path</label>
    <div class="input-group mb-2">
        <input id="RenamingForm.Path" type="text" class="form-control" />
        <button type="button" id="RenamingForm.OpenFolder" class="btn btn-outline-primary" title="Open Folder">
        <i class="bi bi-folder2-open"></i>
        </button> 
    </div>
    <div class="d-flex flex-row">
        <button id="RenamingForm.LoadFiles" type="button" class="btn btn-primary rounded-bottom-0 flex-grow-1">Load Files</button>
    </div>
</section>
<div class="flex-grow-1 position-relative mb-2 border rounded-top-0 rounded-bottom overflow-auto">
    <ul id="RenamingForm.TargetList" class="list-group pb-2 pt-0 position-absolute left-0 top-0 h-100 w-100"></ul>
</div>
</div>
<div class="d-flex flex-row justify-content-evenly">
<button type="button" id="RenamingForm.ApplyButton" class="btn btn-success w-25">Apply</button>
<button type="button" id="RenamingForm.DoneButton" class="btn btn-success w-25">Done</button>
<button type="button" id="RenamingForm.CancelButton" class="btn btn-danger w-25">Cancel</button>
</div>`;

export default async ( electronAPI: ElectronAPI ) => {
    const formElement = document.querySelector<HTMLFormElement>('#RenamingForm')!;
    formElement.innerHTML = formHtml;

    const librarySelector = formElement.querySelector<HTMLSelectElement>('#RenamingForm\\.Library');
    const pathInput = formElement.querySelector<HTMLInputElement>('#RenamingForm\\.Path');
    const fileListElement = formElement.querySelector<HTMLUListElement>('#RenamingForm\\.TargetList');

    const openFolderButton = formElement.querySelector<HTMLButtonElement>('#RenamingForm\\.OpenFolder');
    const loadFilesButton = formElement.querySelector<HTMLButtonElement>('#RenamingForm\\.LoadFiles');

    const applyButton = formElement.querySelector<HTMLButtonElement>('#RenamingForm\\.ApplyButton');
    const doneButton = formElement.querySelector<HTMLButtonElement>('#RenamingForm\\.DoneButton');
    const cancelButton = formElement.querySelector<HTMLButtonElement>('#RenamingForm\\.CancelButton');

    const libraries = await electronAPI.getLibraries();
    librarySelector.innerHTML = libraries.map( lib => /*html*/ `<option>${lib}</option>` ).join('\n');

    loadFilesButton.addEventListener('click', async () => {
        const state = {
            library: librarySelector.value,
            paths: [pathInput.value]
        };

        window.localStorage.setItem('renamer:state', JSON.stringify(state));
        let fileList = (await electronAPI.searchLibrary(state)).toSorted((a,b) => a.toLowerCase() > b.toLowerCase() ? 1 : -1 );

        const recurse = false;
        if( !recurse ) {
            let path = pathInput.value;
            if( !path.endsWith('/') ) path += '/';

            fileList = fileList.filter( file => {
                return !file.substring(path.length).includes('/');
            });
        }

        fileListElement.innerHTML = fileList.map( f => /*html*/ `
            <li class="list-group-item d-flex flex-column">
                <span class="source ps-2 fs-8 form-text">${f}</span>
                <span class="target ps-2 fs-8 form-text text-success d-none">${f}</span>
                <div class="input-group">
                    <input type="text" class="form-control" placeholder="Unchanged"/>
                    <button type="button" class="up-directory btn btn-outline-primary" tabIndex="-1" title="Move file into parent directory"><i class="bi bi-arrow-90deg-up"></i></button>
                    <button type="button" class="open-file btn btn-outline-primary" tabindex="-1" title="Open File"><i class="bi bi-folder2-open"></i></button>
                    <button type="button" class="clear-file btn btn-outline-danger" tabindex="-1" title="Clear Override"><i class="bi bi-x-lg"></i></button>
                </div>
            </li>
            ` ).join('\r\n');
        
        fileListElement.querySelectorAll<HTMLLIElement>('li').forEach( li => {
            const textInput = li.querySelector<HTMLInputElement>('input')!;
            const sourceSpan = li.querySelector<HTMLSpanElement>('.source')!;
            const targetSpan = li.querySelector<HTMLSpanElement>('.target')!;

            const sourceFull = sourceSpan.textContent;
            const source = {
                full: sourceFull,
                path: sourceFull.split(/[\/\\]/).slice(0,-1).join('/'),
                file: sourceFull.split(/[\/\\]/).at(-1),
                extension: '.' + sourceFull.split(/\..*?/).at(-1)
            };

            textInput.addEventListener('input', () => {
                if( !textInput.value ) targetSpan.textContent = sourceSpan.textContent;
                else {
                    targetSpan.textContent = transformPath( source, textInput.value );
                }

                targetSpan.classList.toggle('d-none', sourceSpan.textContent === targetSpan.textContent );
                const isError = targetSpan.textContent.startsWith("Error:");
                targetSpan.classList.toggle('text-danger', isError);
                targetSpan.classList.toggle('text-success', !isError);
            });

            sourceSpan.addEventListener('dblclick', ()=> {
                textInput.value = sourceSpan.textContent.split(/[\/\\]/).at(-1).split('.').slice(0,-1).join('');
            });

            const openFileButton = li.querySelector<HTMLButtonElement>('.open-file')!;
            openFileButton.addEventListener('click', () => {
                electronAPI.openFile( { file: sourceFull } );
            });

            li.querySelector<HTMLButtonElement>('.up-directory')
                ?.addEventListener('click', () => {
                    if( textInput.value ) textInput.value = '../' + textInput.value;
                    else textInput.value = '../{file}';
                    textInput.dispatchEvent( new Event('input' ) );
                });

            li.querySelector<HTMLButtonElement>('.clear-file')
                ?.addEventListener('click', () => {
                    textInput.value = '';
                    textInput.dispatchEvent( new Event('input' ) );
                });
        });
    });

    async function applyNames( exit: boolean ) {
        fileListElement.querySelectorAll<HTMLLIElement>('li').forEach( async li => {
            const oldName = li.querySelector('.source').textContent;
            const newName = li.querySelector('.target').textContent;
            if( oldName === newName ) return;
            else if( newName.startsWith('Error:') ) return;

            await electronAPI.renameFile({ oldName, newName });
        });

        if( exit ) electronAPI.cancelDialog();
        else {
            await new Promise(r => setTimeout(r, 2000)); 
            loadFilesButton.click();
        }
    }

    openFolderButton.addEventListener('click', () => {
        electronAPI.openFile({ file: pathInput.value });
    });

    cancelButton.addEventListener('click', electronAPI.cancelDialog);
    applyButton.addEventListener('click', async () => await applyNames(false) );
    doneButton.addEventListener('click', async () => await applyNames(true) );


    const initialState = window.localStorage.getItem('renamer:state');
    if( initialState ) {
        const initialStateOBJ: SearchLibraryRequest = JSON.parse(initialState);
        librarySelector.value = initialStateOBJ.library;
        pathInput.value = initialStateOBJ.paths?.at(0) ?? '';
    }
}

const invalidCharacters = ['<', '>', ':', '"', '|', '*'];
function transformPath( source: { full: string, file: string, path: string, extension: string }, input: string ) {
    const usedInvalidChars = [];
    for( const c of input ) {
        if( invalidCharacters.includes(c) ) {
            usedInvalidChars.push(c);
        }
    }

    if( usedInvalidChars.length ) {
        return `Error: Contains invalid characters: [${usedInvalidChars.join(', ')}]`;
    }

    if( !input.match(/[\/\\]/) ) input = './' + input;

    input = input.replaceAll( '{path}', source.path ).replaceAll( '{file}', source.file );
    if( !input.toLowerCase().endsWith(source.extension.toLowerCase()) ) {
        input += source.extension;
    }

    if( input.startsWith('.') ) {
        return decodeURIComponent( ( new URL( input.replaceAll('?', '%3F'), `https://localhost${source.path}/` )).pathname );
    } else {
        return input;
    }
}