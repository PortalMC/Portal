export default class {
    constructor(parent, name, folder) {
        this.parent = parent;
        this.name = name;
        this.folder = folder;
        this.path = folder ? `${parent}${name}/` : `${parent}${name}`;
    }

    toString() {
        return `Path(parent='${this.parent}', name='${this.name}', path='${this.path}', folder='${this.folder}')`;
    }
}