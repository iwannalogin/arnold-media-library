import { app, BrowserWindow, dialog, ipcMain, IpcMainEvent } from 'electron';
import path from 'node:path';
import { spawn } from 'node:child_process';
import started from 'electron-squirrel-startup';
import { AcceptDialogRequest, GetTagRequest, StartupParameters } from './Global';
import yargs from 'yargs';
import { hideBin } from 'yargs/helpers';
import { lstatSync, existsSync } from 'node:fs';

// Handle creating/removing shortcuts on Windows when installing/uninstalling.
if (started) {
  app.quit();
}

const createWindow = () => {
  // Create the browser window.
  const mainWindow = new BrowserWindow({
    width: 800,
    height: 600,
    webPreferences: {
      preload: path.join(__dirname, 'preload.js'),
    },
  });

  // and load the index.html of the app.
  if (MAIN_WINDOW_VITE_DEV_SERVER_URL) {
    mainWindow.loadURL(MAIN_WINDOW_VITE_DEV_SERVER_URL);
  } else {
    mainWindow.loadFile(
      path.join(__dirname, `../renderer/${MAIN_WINDOW_VITE_NAME}/index.html`),
    );
  }

  // Open the DevTools.
  mainWindow.webContents.openDevTools();
};

function forceArray( val: any ): string[] {
  if( !val ) return [];
  else if( typeof val === 'string' ) return [val];
  else if( Array.isArray(val) ) return val;
  return [];
}

async function getStartupParameters(): Promise<StartupParameters> {
  const argv = await yargs(hideBin(process.argv)).parseAsync();

  const paths = forceArray( argv['paths'] );
  const files: string[] = [];
  const folders: string[] = [];

  for( const p of paths ) {
    if( !existsSync(p) ) continue;

    if( lstatSync(p).isDirectory() ) {
      folders.push(p);
    } else {
      files.push(p);
    }
  }
  return {
    library: argv['library'] as string,
    tags: forceArray( argv['tags'] ),
    folders: folders,
    files: files
  };
}

async function getLibraries() {
  const arnold = spawn('arnold', ['library', 'list']);

  let accumulator = '';
  for await( const chunk of arnold.stdout ) {
    accumulator += chunk.toString();
  }
  return accumulator.split('\n').filter( lib => !!lib );
}

async function getTags({ library, path }: GetTagRequest ) {
  const arnold = spawn('arnold', ['meta', 'tags', library, path ]);

  let accumulator = '';
  for await ( const chunk of arnold.stdout ) {
    accumulator += chunk.toString();
  }
  return accumulator.split('\n').filter( tag => !!tag );
}

async function selectFiles() {
  return (await dialog.showOpenDialog({
    properties: ['openFile', 'multiSelections']
  }))?.filePaths ?? [];
}

async function selectDirectories() {
  return (await dialog.showOpenDialog({
    properties: ['openDirectory', 'multiSelections']
  }))?.filePaths ?? [];
}

async function cancelDialog() {
  app.quit();
}

async function acceptDialog({ library, tags, paths, exit }: AcceptDialogRequest ) {
  const arnold = spawn('arnold', ['tag', 'add', library, '--paths', ...paths, '--tags', ...tags]);
  await new Promise( resolve => arnold.on('close', resolve) );
  if( exit ) app.quit();
}

// This method will be called when Electron has finished
// initialization and is ready to create browser windows.
// Some APIs can only be used after this event occurs.
app.on('ready', ()=> {
  ipcMain.handle('get-startup', getStartupParameters );
  ipcMain.handle('get-libraries', getLibraries );
  ipcMain.handle('select-files', selectFiles );
  ipcMain.handle('select-directories', selectDirectories);
  ipcMain.handle('cancel-dialog', cancelDialog);
  ipcMain.handle('accept-dialog', (ev, params) => acceptDialog(params) );
  ipcMain.handle('get-tags', (ev, params) => getTags(params));
  createWindow();
});

// Quit when all windows are closed, except on macOS. There, it's common
// for applications and their menu bar to stay active until the user quits
// explicitly with Cmd + Q.
app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') {
    app.quit();
  }
});

app.on('activate', () => {
  // On OS X it's common to re-create a window in the app when the
  // dock icon is clicked and there are no other windows open.
  if (BrowserWindow.getAllWindows().length === 0) {
    createWindow();
  }
});

// In this file you can include the rest of your app's specific main process
// code. You can also put them in separate files and import them here.
