/**
 * This file will automatically be loaded by vite and run in the "renderer" context.
 * To learn more about the differences between the "main" and the "renderer" context in
 * Electron, visit:
 *
 * https://electronjs.org/docs/tutorial/process-model
 *
 * By default, Node.js integration in this file is disabled. When enabling Node.js integration
 * in a renderer process, please be aware of potential security implications. You can read
 * more about security risks here:
 *
 * https://electronjs.org/docs/tutorial/security
 *
 * To enable Node.js integration in this file, open up `main.ts` and enable the `nodeIntegration`
 * flag:
 *
 * ```
 *  // Create the browser window.
 *  mainWindow = new BrowserWindow({
 *    width: 800,
 *    height: 600,
 *    webPreferences: {
 *      nodeIntegration: true
 *    }
 *  });
 * ```
 */

import './index.css';

import 'bootstrap-icons/font/bootstrap-icons.scss'
import 'bootstrap/scss/bootstrap.scss'
import * as bootstrap from 'bootstrap';
import { AcceptDialogRequest, GetTagRequest, StartupParameters } from './Global';

console.log(
  '👋 This message is being logged by "renderer.ts", included via Vite',
);

type ElectronAPI = {
  getLibraries: () => Promise<string[]>,
  getTags: (parameters: GetTagRequest) => Promise<string[]>,
  selectFiles: () => Promise<string[]>,
  selectDirectories: () => Promise<string[]>,
  cancelDialog: () => Promise<void>,
  acceptDialog: (parameters: AcceptDialogRequest) => Promise<void>,
  getStartupParameters: () => Promise<StartupParameters>,
};

const electronAPI = (window as any).electronAPI as ElectronAPI;

const librarySelector = document.querySelector<HTMLSelectElement>('#Library');
const fileListElement = document.querySelector<HTMLUListElement>('#TargetList');
const tagAreaElement = document.querySelector<HTMLTextAreaElement>('#TagList');

const addFileButton = document.querySelector<HTMLButtonElement>('#AddFileButton');
const addFolderButton = document.querySelector<HTMLButtonElement>('#AddFolderButton');

const applyButton = document.querySelector<HTMLButtonElement>('#ApplyButton');
const doneButton = document.querySelector<HTMLButtonElement>('#DoneButton');
const cancelButton = document.querySelector<HTMLButtonElement>('#CancelButton');

function getParameters( exit: boolean ) {
  return {
    library: librarySelector.value,
    tags: tagAreaElement.value.split('\n').filter( tag => !!tag ),
    paths: [...fileListElement.querySelectorAll('li')]
      .map( item => item.querySelector('span').innerText ),
      exit: exit
  };
}

function addFiles( files: string[] ) {
  fileListElement.innerHTML += files.map( file => /*html*/ `
    <li class="list-group-item d-flex flex-row">
      <i class="bi bi-file-earmark"></i>
      <span>${file}</span>
      <button type="button" class="import-button btn background-transparent p-0 ms-2"><i class="bi bi-database-up text-primary"></i></button>
      <button type="button" class="delete-button btn background-transparent p-0 ms-2"><i class="bi bi-trash text-danger"></i></button>
    </li>`
  );

  fileListElement.querySelectorAll('li').forEach( listItem => {
    if( listItem.hasAttribute('hooked') ) return;

    const path = listItem.querySelector('span').textContent;
    const importButton = listItem.querySelector<HTMLButtonElement>('.import-button');
    const deleteButton = listItem.querySelector<HTMLButtonElement>('.delete-button');

    deleteButton.addEventListener('click', () => listItem.remove() );
    importButton.addEventListener('click', async () => {
      const getTags = electronAPI.getTags({ library: librarySelector.value, path});
      const currentTags = tagAreaElement.value.split('\n').filter( tag => !!tag ).map( tag => tag.toLowerCase() );
      const newTags = (await getTags).filter( tag => {
        return !!tag && !currentTags.includes( tag.toLowerCase() );
      });
      if( tagAreaElement.value && !tagAreaElement.value.endsWith('\n')) tagAreaElement.value += '\n';
      tagAreaElement.value += newTags.join('\n');
    });

    listItem.setAttribute('hooked', 'true');
  });
}

function addFolders( directories: string[] ) {
  fileListElement.innerHTML += directories.map( directory => /*html*/ `
    <li class="list-group-item d-flex flex-row">
      <i class="bi bi-folder me-2"></i>
      <span>${directory}</span>
      <button type="button" class="btn background-transparent p-0 ms-2"><i class="bi bi-trash text-danger"></i></button>
    </li>` );
}

addFileButton.addEventListener('click', async () => {
  const files = await electronAPI.selectFiles();
  addFiles(files);
});

addFolderButton.addEventListener('click', async () => {
  const directories = await electronAPI.selectDirectories();
  addFolders(directories);
});

cancelButton.addEventListener('click', electronAPI.cancelDialog);
applyButton.addEventListener('click', () => electronAPI.acceptDialog( getParameters(false) ) );
doneButton.addEventListener('click', () => electronAPI.acceptDialog( getParameters( true ) ) );

document.addEventListener('DOMContentLoaded', async() => {
  const libraries = await electronAPI.getLibraries();
  librarySelector.innerHTML = libraries.map( lib => /*html*/ `<option>${lib}</option>` ).join('\n');

  const startupParameters = await electronAPI.getStartupParameters();
  if( startupParameters.library && libraries.map( lib => lib.toLowerCase() ).includes( startupParameters.library.toLowerCase() ) ) {
    librarySelector.querySelectorAll<HTMLOptionElement>('option').forEach( item => {
      if( item.value.toLowerCase() == startupParameters.library ) {
        item.selected = true;
      }
    });
  }

  if( startupParameters.files ) {
    addFiles(startupParameters.files);
  } 

  if( startupParameters.folders ) {
    addFolders(startupParameters.folders);
  }

  if( startupParameters.tags ) {
    tagAreaElement.value = startupParameters.tags.join('\n');
  }
});