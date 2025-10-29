import { html, WCFormElement } from 'wc-lib'

const template = html``;

export default class SiteHeaderElement extends WCFormElement {
    constructor() {
        super();
    }
}
customElements.define('site-header', SiteHeaderElement);