import './index.css';

import 'bootstrap-icons/font/bootstrap-icons.scss'
import 'bootstrap/scss/bootstrap.scss'
import * as bootstrap from 'bootstrap';
import TaggingForm from './forms/tagging'
import RenamingForm from './forms/renaming'
import { ElectronAPI } from './electron-api';

const electronAPI = (window as any).electronAPI as ElectronAPI;

document.addEventListener('DOMContentLoaded', async() => {
  const tabButtons = [...document.querySelectorAll<HTMLDivElement>('#tab-box > button')];
  const tabForms = [...document.querySelectorAll<HTMLFormElement>('form')];
  tabButtons.forEach( button => {
    const targetName = button.getAttribute('for');
    const targetForm = tabForms.find( f => f.id == targetName );

    button.addEventListener( 'click', ()=> {
      tabButtons.forEach( tb => {
        tb.toggleAttribute('active', tb === button );
      })

      tabForms.forEach( tf => {
        tf.toggleAttribute('active', tf === targetForm );
      });
    });
  });

  TaggingForm(electronAPI);
  RenamingForm(electronAPI);

  const formHider = document.querySelector('#FormHider');
  formHider.addEventListener('click', ()=>{
    document.querySelectorAll('.user-form').forEach( ele => {
      ele.classList.toggle('d-none');
    });
  })
});